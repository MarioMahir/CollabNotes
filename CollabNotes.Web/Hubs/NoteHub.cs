using System.Security.Claims;
using CollabNotes.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CollabNotes.Web.Hubs;

[Authorize]
public class NoteHub : Hub
{
    private readonly ICollaborationService _collaborationService;

    public NoteHub(ICollaborationService collaborationService)
    {
        _collaborationService = collaborationService;
    }

    private string UserId => Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public override async Task OnConnectedAsync()
    {
        var noteIdValue = Context.GetHttpContext()?.Request.Query["noteId"].ToString();
        if (!Guid.TryParse(noteIdValue, out var noteId))
        {
            Context.Abort();
            return;
        }

        if (!await _collaborationService.CanAccessNoteAsync(noteId, UserId))
        {
            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, noteId.ToString());

        var displayName = Context.User!.FindFirstValue(ClaimTypes.Name) ?? UserId;
        var viewers = await _collaborationService.RegisterViewerAsync(noteId, Context.ConnectionId, UserId, displayName);

        await Clients.OthersInGroup(noteId.ToString()).SendAsync("ViewerJoined", new { userId = UserId, displayName });
        await Clients.Caller.SendAsync("PresenceSnapshot", viewers);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var left = _collaborationService.UnregisterViewer(Context.ConnectionId);
        if (left is not null)
        {
            await Clients.OthersInGroup(left.NoteId.ToString())
                .SendAsync("ViewerLeft", new { userId = left.UserId, displayName = left.DisplayName });
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task UpdateBlockAsync(Guid noteId, int blockIndex, string content)
    {
        try
        {
            var dto = await _collaborationService.UpdateBlockAsync(noteId, blockIndex, content, UserId);
            await Clients.OthersInGroup(noteId.ToString()).SendAsync("BlockUpdated", dto);
        }
        catch (UnauthorizedAccessException)
        {
            await Clients.Caller.SendAsync("EditRejected", blockIndex);
        }
    }

    public Task NotifyTypingAsync(Guid noteId, int blockIndex)
        => Clients.OthersInGroup(noteId.ToString()).SendAsync("UserTyping", new { userId = UserId, blockIndex });

    public Task NotifyTypingStoppedAsync(Guid noteId, int blockIndex)
        => Clients.OthersInGroup(noteId.ToString()).SendAsync("UserStoppedTyping", new { userId = UserId, blockIndex });
}
