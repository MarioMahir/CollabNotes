using CollabNotes.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollabNotes.Infrastructure.Persistence.Configurations;

public class NotePermissionConfiguration : IEntityTypeConfiguration<NotePermission>
{
    public void Configure(EntityTypeBuilder<NotePermission> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.UserId).IsRequired();
        builder.HasIndex(p => new { p.NoteId, p.UserId }).IsUnique();
    }
}
