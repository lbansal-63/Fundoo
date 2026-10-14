using Microsoft.EntityFrameworkCore;
using ModelLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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