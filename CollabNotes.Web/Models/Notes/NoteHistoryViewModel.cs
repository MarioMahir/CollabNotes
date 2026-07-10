using CollabNotes.Application.Dtos;

namespace CollabNotes.Web.Models.Notes;

public class NoteHistoryViewModel
{
    public Guid NoteId { get; set; }
    public string NoteTitle { get; set; } = string.Empty;
    public IEnumerable<NoteSnapshotDto> Snapshots { get; set; } = [];
}
