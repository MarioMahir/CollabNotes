using System.ComponentModel.DataAnnotations;
using CollabNotes.Application.Dtos;
using CollabNotes.Domain.Enums;

namespace CollabNotes.Web.Models.Notes;

public class NoteFormViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(200, ErrorMessage = "El título no puede superar los 200 caracteres.")]
    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public Guid? FolderId { get; set; }

    public IEnumerable<FolderDto> Folders { get; set; } = [];

    public PermissionRole ViewerRole { get; set; }

    public IEnumerable<NotePermissionDto> Members { get; set; } = [];

    public IReadOnlyList<string> Blocks { get; set; } = [];
}
