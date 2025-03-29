using PPSR.Registration.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPSR.Registration.Domain.Entities
{
    public class PpsrRegistration
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string GrantorFirstName { get; set; } = null!;
        public string? GrantorMiddleNames { get; set; }
        public string GrantorLastName { get; set; } = null!;
        public string VIN { get; set; } = null!;
        public DateTime RegistrationStartDate { get; set; }
        public RegistrationDuration Duration { get; set; }
        public string SpgAcn { get; set; } = null!;
        public string SpgOrganizationName { get; set; } = null!;
    }
}
