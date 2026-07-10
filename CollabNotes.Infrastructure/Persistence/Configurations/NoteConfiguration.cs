using CollabNotes.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollabNotes.Infrastructure.Persistence.Configurations;

public class NoteConfiguration : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> builder)
    {
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Title).IsRequired().HasMaxLength(200);
        builder.Property(n => n.OwnerId).IsRequired();

        builder.HasOne(n => n.Folder)
            .WithMany(f => f.Notes)
            .HasForeignKey(n => n.FolderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(n => n.Permissions)
            .WithOne(p => p.Note)
            .HasForeignKey(p => p.NoteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
