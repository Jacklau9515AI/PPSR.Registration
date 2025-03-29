using PPSR.Registration.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPSR.Registration.Application.Interfaces
{
    public interface IRegistrationRepository
    {
        Task<PpsrRegistration?> FindByVinAsync(string vin, CancellationToken cancellationToken);
        Task AddAsync(PpsrRegistration registration, CancellationToken cancellationToken);
        Task UpdateAsync(PpsrRegistration registration, CancellationToken cancellationToken);
    }
}
