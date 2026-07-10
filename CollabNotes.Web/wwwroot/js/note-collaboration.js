(function () {
    "use strict";

    var container = document.getElementById("note-collab");
    if (!container) {
        return;
    }

    var noteId = container.dataset.noteId;
    var viewerRole = container.dataset.viewerRole;
    var isReader = viewerRole === "Reader";

    var blocksContainer = document.getElementById("blocks-container");
    var addBlockBtn = document.getElementById("add-block-btn");
    var rosterList = document.getElementById("viewer-roster");
    var banner = document.getElementById("connection-banner");
    var contentHiddenField = document.getElementById("content-hidden-field");
    var editForm = document.getElementById("note-edit-form");

    var BLOCK_DEBOUNCE_MS = 300;
    var TYPING_SEND_THROTTLE_MS = 1500;
    var TYPING_STOP_TIMEOUT_MS = 2000;
    var TYPING_REMOTE_FALLBACK_MS = 2500;
    var HIGHLIGHT_MS = 1500;

    var saveTimers = {};
    var typingSendTimers = {};
    var typingStopTimers = {};
    var remoteTypingFallbackTimers = {};

    function showBanner(text, cssClass) {
        if (!banner) return;
        banner.textContent = text;
        banner.className = "connection-banner " + cssClass;
        banner.classList.remove("d-none");
    }

    function hideBanner() {
        if (!banner) return;
        banner.classList.add("d-none");
    }

    function blockElement(index) {
        return blocksContainer.querySelector('[data-block-index="' + index + '"]');
    }

    function renderRoster(viewers) {
        if (!rosterList) return;
        rosterList.innerHTML = "";
        if (!viewers || viewers.length === 0) {
            var li = document.createElement("li");
            li.className = "list-group-item text-muted";
            li.textContent = "Nadie más está viendo esta nota.";
            rosterList.appendChild(li);
            return;
        }
        viewers.forEach(function (v) {
            var li = document.createElement("li");
            li.className = "list-group-item";
            li.dataset.userId = v.userId;
            li.textContent = v.displayName + " está viendo esta nota";
            rosterList.appendChild(li);
        });
    }

    function addViewer(v) {
        if (!rosterList) return;
        var existing = rosterList.querySelector('[data-user-id="' + v.userId + '"]');
        if (existing) return;
        var empty = rosterList.querySelector(".text-muted");
        if (empty) empty.remove();
        var li = document.createElement("li");
        li.className = "list-group-item";
        li.dataset.userId = v.userId;
        li.textContent = v.displayName + " está viendo esta nota";
        rosterList.appendChild(li);
    }

    function removeViewer(userId) {
        if (!rosterList) return;
        var existing = rosterList.querySelector('[data-user-id="' + userId + '"]');
        if (existing) existing.remove();
        if (!rosterList.querySelector("li")) {
            renderRoster([]);
        }
    }

    function setTypingBadge(blockIndex, visible) {
        var block = blockElement(blockIndex);
        if (!block) return;
        var badge = block.querySelector(".typing-badge");
        if (!badge) return;
        badge.classList.toggle("d-none", !visible);
    }

    function flashHighlight(blockIndex) {
        var block = blockElement(blockIndex);
        if (!block) return;
        block.classList.add("block-recently-edited");
        setTimeout(function () {
            block.classList.remove("block-recently-edited");
        }, HIGHLIGHT_MS);
    }

    function syncHiddenContentField() {
        if (!contentHiddenField) return;
        var textareas = blocksContainer.querySelectorAll(".note-block-textarea");
        var values = [];
        textareas.forEach(function (t) {
            values.push(t.value);
        });
        contentHiddenField.value = values.join("\n\n");
    }

    if (editForm) {
        editForm.addEventListener("submit", syncHiddenContentField);
    }

    if (isReader) {
        blocksContainer.querySelectorAll(".note-block-textarea").forEach(function (t) {
            t.readOnly = true;
        });
        if (addBlockBtn) addBlockBtn.classList.add("d-none");
    }

    var connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/note?noteId=" + encodeURIComponent(noteId))
        .withAutomaticReconnect()
        .build();

    connection.onreconnecting(function () {
        showBanner("Reconectando…", "alert alert-warning");
    });

    connection.onreconnected(function () {
        hideBanner();
    });

    connection.onclose(function () {
        showBanner("Conexión perdida. Reintentando…", "alert alert-danger");
        setTimeout(startConnection, 3000);
    });

    connection.on("PresenceSnapshot", function (viewers) {
        renderRoster(viewers);
    });

    connection.on("ViewerJoined", function (info) {
        addViewer(info);
    });

    connection.on("ViewerLeft", function (info) {
        removeViewer(info.userId);
    });

    connection.on("BlockUpdated", function (dto) {
        var block = blockElement(dto.blockIndex);
        if (!block) return;
        var textarea = block.querySelector(".note-block-textarea");
        if (!textarea) return;
        if (document.activeElement !== textarea) {
            textarea.value = dto.content;
        }
        flashHighlight(dto.blockIndex);
    });

    connection.on("EditRejected", function (blockIndex) {
        var block = blockElement(blockIndex);
        if (block) {
            block.classList.add("block-recently-edited");
            setTimeout(function () {
                block.classList.remove("block-recently-edited");
            }, HIGHLIGHT_MS);
        }
    });

    connection.on("UserTyping", function (info) {
        setTypingBadge(info.blockIndex, true);
        clearTimeout(remoteTypingFallbackTimers[info.blockIndex]);
        remoteTypingFallbackTimers[info.blockIndex] = setTimeout(function () {
            setTypingBadge(info.blockIndex, false);
        }, TYPING_REMOTE_FALLBACK_MS);
    });

    connection.on("UserStoppedTyping", function (info) {
        clearTimeout(remoteTypingFallbackTimers[info.blockIndex]);
        setTypingBadge(info.blockIndex, false);
    });

    function startConnection() {
        connection.start()
            .then(hideBanner)
            .catch(function () {
                showBanner("No se pudo conectar. Reintentando…", "alert alert-danger");
                setTimeout(startConnection, 3000);
            });
    }

    function sendBlockUpdate(blockIndex, value) {
        connection.invoke("UpdateBlockAsync", noteId, blockIndex, value).catch(function () {
            // errors already surfaced via EditRejected / connection banner
        });
    }

    function sendTypingSignal(blockIndex) {
        connection.invoke("NotifyTypingAsync", noteId, blockIndex).catch(function () { });
    }

    function sendTypingStopped(blockIndex) {
        connection.invoke("NotifyTypingStoppedAsync", noteId, blockIndex).catch(function () { });
    }

    function wireBlockTextarea(textarea) {
        var blockIndex = parseInt(textarea.dataset.blockIndex, 10);

        textarea.addEventListener("input", function () {
            clearTimeout(saveTimers[blockIndex]);
            saveTimers[blockIndex] = setTimeout(function () {
                sendBlockUpdate(blockIndex, textarea.value);
            }, BLOCK_DEBOUNCE_MS);

            if (!typingSendTimers[blockIndex]) {
                sendTypingSignal(blockIndex);
                typingSendTimers[blockIndex] = setTimeout(function () {
                    typingSendTimers[blockIndex] = null;
                }, TYPING_SEND_THROTTLE_MS);
            }

            clearTimeout(typingStopTimers[blockIndex]);
            typingStopTimers[blockIndex] = setTimeout(function () {
                sendTypingStopped(blockIndex);
            }, TYPING_STOP_TIMEOUT_MS);
        });
    }

    if (!isReader) {
        blocksContainer.querySelectorAll(".note-block-textarea").forEach(wireBlockTextarea);

        if (addBlockBtn) {
            addBlockBtn.addEventListener("click", function () {
                var existing = blocksContainer.querySelectorAll(".note-block");
                var nextIndex = existing.length;

                var wrapper = document.createElement("div");
                wrapper.className = "note-block mb-2";
                wrapper.dataset.blockIndex = nextIndex;

                var badge = document.createElement("span");
                badge.className = "typing-badge d-none badge bg-info text-dark mb-1";
                badge.textContent = "Alguien está escribiendo…";

                var textarea = document.createElement("textarea");
                textarea.className = "form-control note-block-textarea";
                textarea.rows = 3;
                textarea.dataset.blockIndex = nextIndex;

                wrapper.appendChild(badge);
                wrapper.appendChild(textarea);
                blocksContainer.appendChild(wrapper);

                wireBlockTextarea(textarea);
                textarea.focus();
            });
        }
    }

    startConnection();
})();
