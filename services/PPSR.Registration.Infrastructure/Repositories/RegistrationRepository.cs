using Microsoft.EntityFrameworkCore;
using PPSR.Registration.Application.Interfaces;
using PPSR.Registration.Domain.Entities;
using PPSR.Registration.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPSR.Registration.Infrastructure.Repositories
{
    public class RegistrationRepository : IRegistrationRepository
    {
        private readonly PpsrDbContext _context;

        public RegistrationRepository(PpsrDbContext context)
        {
            _context = context;
        }

        public async Task<PpsrRegistration?> FindByVinAsync(string vin, CancellationToken cancellationToken)
        {
            return await _context.Registrations
                .FirstOrDefaultAsync(r => r.VIN == vin, cancellationToken);
        }

        public async Task AddAsync(PpsrRegistration registration, CancellationToken cancellationToken)
        {
            _context.Registrations.Add(registration);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(PpsrRegistration registration, CancellationToken cancellationToken)
        {
            _context.Registrations.Update(registration);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
