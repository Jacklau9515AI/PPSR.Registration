using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPSR.Registration.Application.DTOs
{
    public class PpsrCsvUploadDto
    {
        public string GrantorFirstName { get; set; } = null!;
        public string? GrantorMiddleNames { get; set; }
        public string GrantorLastName { get; set; } = null!;
        public string VIN { get; set; } = null!;
        public DateTime RegistrationStartDate { get; set; }
        public int RegistrationDurationRaw { get; set; }
        public string SpgAcn { get; set; } = null!;
        public string SpgOrganizationName { get; set; } = null!;
    }
}
