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
        }
    }
}