using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Entities;

namespace RepositoryLayer.Context
{
    public class FundooContext : DbContext
    {
        public FundooContext(DbContextOptions<FundooContext> options) : base(options)
        {
        }

        public DbSet<UserEntity> Users { get; set; }
        public DbSet<NoteEntity> Notes { get; set; }
        public DbSet<LabelEntity> Labels { get; set; }
        public DbSet<NoteLabelEntity> NoteLabels { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserEntity>()
                .HasIndex(u => u.Email)
                .IsUnique();
            modelBuilder.Entity<UserEntity>()
                .Property(u => u.GoogleId)
                .HasMaxLength(255);

            modelBuilder.Entity<UserEntity>()
                .HasIndex(u => u.GoogleId)
                .IsUnique();

            modelBuilder.Entity<UserEntity>()
                .Property(u => u.PhoneNumber)
                .IsRequired()
                .HasMaxLength(15);

            modelBuilder.Entity<NoteEntity>(b =>
            {
                b.HasKey(n => n.NoteId);
                b.Property(n => n.Title).IsRequired().HasMaxLength(200);
                b.Property(n => n.Colour).HasMaxLength(50);

                b.HasOne(n => n.User)
                    .WithMany()
                    .HasForeignKey(n => n.UserId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<LabelEntity>(b =>
            {
                b.HasKey(l => l.LabelId);
                b.Property(l => l.Name).IsRequired().HasMaxLength(50);
                b.Property(l => l.NormalizedName).IsRequired().HasMaxLength(50);
                b.Property(l => l.CreatedAt).IsRequired();

                // Ek user ke andar normalized naam unique. DB level par zaroori hai,
                // taaki do requests ek saath aayein tab bhi duplicate na bane.
                b.HasIndex(l => new { l.UserId, l.NormalizedName }).IsUnique();

                b.HasOne(l => l.User)
                    .WithMany()
                    .HasForeignKey(l => l.UserId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<NoteLabelEntity>(b =>
            {
                // Composite key: ek note par ek label sirf ek baar
                b.HasKey(nl => new { nl.NoteId, nl.LabelId });

                // Note delete -> uske NoteLabel rows delete
                b.HasOne(nl => nl.Note)
                    .WithMany(n => n.NoteLabels)
                    .HasForeignKey(nl => nl.NoteId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Label delete -> uske NoteLabel rows delete (notes safe rehte hain)
                b.HasOne(nl => nl.Label)
                    .WithMany(l => l.NoteLabels)
                    .HasForeignKey(nl => nl.LabelId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasIndex(nl => nl.LabelId);
            });
        }
    }
}