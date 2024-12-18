using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RBPv1._1.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RBPv1._1
{
    public class RBPdbContext: IdentityDbContext<IdentityUser>
    {
        private readonly string _connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=RBP;Integrated Security=True;";

        public RBPdbContext(DbContextOptions<RBPdbContext> options)
        : base(options)
        { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_connectionString).LogTo(Console.WriteLine, LogLevel.Information);
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IdentityUserLogin<string>>()
           .HasKey(login => new { login.LoginProvider, login.ProviderKey });
            modelBuilder.Entity<IdentityUserRole<string>>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });
            modelBuilder.Entity<IdentityUserToken<string>>()
       .HasKey(ut => new { ut.UserId, ut.LoginProvider, ut.Name });
            modelBuilder.Entity<Workload>().HasKey(w=>w.WorkloadID);
            modelBuilder.Entity<Groups>().HasKey(g=>g.GroupId);
            modelBuilder.Entity<Teachers>().HasKey(t=>t.TeacherID);
            modelBuilder.Entity<Workload>().HasOne(w => w.groups).WithMany(g => g.Workload).HasForeignKey(w=>w.GroupID);
            modelBuilder.Entity<Workload>().HasOne(w => w.teachers).WithMany(t => t.Workload).HasForeignKey(w => w.TeacherID);
            modelBuilder.Entity<Workload>().Property(w => w.Payment).HasColumnType("decimal(10,2)");
        }

        public DbSet<Groups> Groups { get; set; }
        public DbSet<Teachers> Teachers { get; set; }
        public DbSet<Workload> Workloads { get; set; }
    }
}
