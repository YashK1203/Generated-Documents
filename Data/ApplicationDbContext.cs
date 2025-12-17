using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GeneratingDocs;

namespace GeneratingDocs.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Tables
        public DbSet<Employee> Employees { get; set; }
        public DbSet<GeneratedDocument> GeneratedDocuments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Explicit table names
            builder.Entity<Employee>().ToTable("Employees");
            builder.Entity<GeneratedDocument>().ToTable("GeneratedDocuments");

            // Relationship: One Employee → Many GeneratedDocuments
            builder.Entity<GeneratedDocument>()
                .HasOne(d => d.Employee)
                .WithMany()   // Employee DOES NOT have a Documents list
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Required Fields
            builder.Entity<Employee>()
                .Property(e => e.EmployeeNo)
                .IsRequired();

            builder.Entity<Employee>()
                .Property(e => e.Name)
                .IsRequired();

            // Auto-Set UTC CreatedAt if not set
            builder.Entity<Employee>()
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
        }
    }
}
