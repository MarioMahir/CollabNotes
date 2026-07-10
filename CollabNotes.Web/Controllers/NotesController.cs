using System.Security.Claims;
using CollabNotes.Application.Common;
using CollabNotes.Application.Interfaces;
using CollabNotes.Domain.Enums;
using CollabNotes.Web.Models.Notes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CollabNotes.Web.Controllers;

[Authorize]
public class NotesController : Controller
{
    private readonly INoteService _noteService;
    private readonly IFolderService _folderService;

    public NotesController(INoteService noteService, IFolderService folderService)
    {
        _noteService = noteService;
        _folderService = folderService;
    }

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task<IActionResult> Index(Guid? folderId)
    {
        var vm = new NotesIndexViewModel
        {
            Notes = await _noteService.GetNotesByFolderAsync(folderId, CurrentUserId),
            Folders = await _folderService.GetFoldersAsync(CurrentUserId),
            SelectedFolderId = folderId
        };

        return View(vm);
    }

    public async Task<IActionResult> Create(Guid? folderId)
    {
        var vm = new NoteFormViewModel
        {
            FolderId = folderId,
            Folders = await _folderService.GetFoldersAsync(CurrentUserId)
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NoteFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.Folders = await _folderService.GetFoldersAsync(CurrentUserId);
            return View(vm);
        }

        var note = await _noteService.CreateAsync(vm.Title, vm.FolderId, CurrentUserId);
        return RedirectToAction(nameof(Edit), new { id = note.Id });
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var note = await _noteService.GetByIdAsync(id, CurrentUserId);
        if (note is null)
        {
            return NotFound();
        }

        var vm = new NoteFormViewModel
        {
            Id = note.Id,
            Title = note.Title,
            Content = note.Content,
            FolderId = note.FolderId,
            ViewerRole = note.ViewerRole,
            Blocks = NoteBlockSplitter.Split(note.Content),
            Folders = await _folderService.GetFoldersAsync(CurrentUserId),
            Members = await _noteService.GetMembersAsync(id, CurrentUserId)
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, NoteFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.Folders = await _folderService.GetFoldersAsync(CurrentUserId);
            vm.Members = await _noteService.GetMembersAsync(id, CurrentUserId);
            vm.Blocks = NoteBlockSplitter.Split(vm.Content);
            return View(vm);
        }

        try
        {
            await _noteService.UpdateAsync(id, vm.Title, vm.Content, CurrentUserId);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> InviteUser(Guid id, string emailOrUsername, PermissionRole role)
    {
        try
        {
            await _noteService.InviteUserAsync(id, CurrentUserId, emailOrUsername, role);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            TempData["InviteError"] = ex.Message;
        }

        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _noteService.DeleteAsync(id, CurrentUserId);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFolder(string name)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            await _folderService.CreateAsync(name, CurrentUserId);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> History(Guid id)
    {
        var note = await _noteService.GetByIdAsync(id, CurrentUserId);
        if (note is null)
        {
            return NotFound();
        }

        try
        {
            var vm = new NoteHistoryViewModel
            {
                NoteId = id,
                NoteTitle = note.Title,
                Snapshots = await _noteService.GetSnapshotsAsync(id, CurrentUserId)
            };

            return View(vm);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    public async Task<IActionResult> SnapshotDetail(Guid id, Guid snapshotId)
    {
        var note = await _noteService.GetByIdAsync(id, CurrentUserId);
        if (note is null)
        {
            return NotFound();
        }

        try
        {
            var snapshot = await _noteService.GetSnapshotAsync(id, snapshotId, CurrentUserId);
            if (snapshot is null)
            {
                return NotFound();
            }

            var vm = new NoteHistoryViewModel
            {
                NoteId = id,
                NoteTitle = note.Title,
                Snapshots = [snapshot]
            };

            return View(vm);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}
