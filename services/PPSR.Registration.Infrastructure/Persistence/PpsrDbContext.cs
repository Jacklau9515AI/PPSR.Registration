using Microsoft.EntityFrameworkCore;
using PPSR.Registration.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace PPSR.Registration.Infrastructure.Persistence
{
    public class PpsrDbContext : DbContext
    {
        public DbSet<PpsrRegistration> Registrations => Set<PpsrRegistration>();

        public PpsrDbContext(DbContextOptions<PpsrDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<PpsrRegistration>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.GrantorFirstName).HasMaxLength(35).IsRequired();
                entity.Property(e => e.GrantorMiddleNames).HasMaxLength(75);
                entity.Property(e => e.GrantorLastName).HasMaxLength(35).IsRequired();

                entity.Property(e => e.VIN).HasMaxLength(17).IsRequired();
                entity.Property(e => e.RegistrationStartDate).IsRequired();

                entity.Property(e => e.SpgAcn).HasMaxLength(9).IsRequired();
                entity.Property(e => e.SpgOrganizationName).HasMaxLength(75).IsRequired();

                entity.Property(e => e.Duration)
                    .HasConversion<int>();

                entity.HasIndex(e => new
                {
                    e.GrantorFirstName,
                    e.GrantorLastName,
                    e.VIN,
                    e.SpgAcn
                }).IsUnique();
            });
        }
    }
}
