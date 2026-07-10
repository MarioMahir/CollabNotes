using CollabNotes.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CollabNotes.Infrastructure.Persistence.Configurations;

public class NoteSnapshotConfiguration : IEntityTypeConfiguration<NoteSnapshot>
{
    public void Configure(EntityTypeBuilder<NoteSnapshot> builder)
    {
        builder.HasKey(s => s.Id);
        builder.HasIndex(s => new { s.NoteId, s.CreatedAtUtc });

        builder.HasOne(s => s.Note)
            .WithMany()
            .HasForeignKey(s => s.NoteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
