using Microsoft.EntityFrameworkCore;
using ProfApi.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ProfApi.DBcontext
{
    public class ProfDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Follower> Followers { get; set; }

        public ProfDbContext(DbContextOptions<ProfDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
              .Property(u => u.UserId)
              .ValueGeneratedNever();


            modelBuilder.Entity<Follower>()
                .HasKey(f => new { f.FollowerId, f.FollowingId });

            modelBuilder.Entity<Follower>()
                .HasOne(f => f.Followero)
                .WithMany(u => u.Following)
                .HasForeignKey(f => f.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Follower>()
                .HasOne(f => f.Following)
                .WithMany(u => u.Followers)
                .HasForeignKey(f => f.FollowingId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}