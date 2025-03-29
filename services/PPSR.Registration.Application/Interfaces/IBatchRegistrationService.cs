using PPSR.Registration.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPSR.Registration.Application.Interfaces
{
    public interface IBatchRegistrationService
    {
        Task<BatchUploadResult> ProcessCsvUploadAsync(Stream csvFileStream, CancellationToken cancellationToken);
    }
}
