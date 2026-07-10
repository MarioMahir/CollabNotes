using System.ComponentModel.DataAnnotations;
using CollabNotes.Application.Dtos;

namespace CollabNotes.Web.Models.Notes;

public class NoteFormViewModel
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public Guid? FolderId { get; set; }

    public IEnumerable<FolderDto> Folders { get; set; } = [];
}
