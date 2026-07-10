using CollabNotes.Application.Dtos;

namespace CollabNotes.Web.Models.Notes;

public class NotesIndexViewModel
{
    public IEnumerable<NoteDto> Notes { get; set; } = [];
    public IEnumerable<FolderDto> Folders { get; set; } = [];
    public Guid? SelectedFolderId { get; set; }
}
