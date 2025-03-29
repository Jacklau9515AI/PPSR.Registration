using Microsoft.AspNetCore.Mvc;
using PPSR.Registration.Application.DTOs;
using PPSR.Registration.Application.Interfaces;
using PPSR.Registration.WebApi.Ingestor.Models;

namespace PPSR.Registration.WebApi.Ingestor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationsController : ControllerBase
    {
        private readonly IBatchRegistrationService _service;

        public RegistrationsController(IBatchRegistrationService service)
        {
            _service = service;
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<BatchUploadResult>> Upload([FromForm] UploadCsvRequest request, CancellationToken cancellationToken)
        {
            if (request.File == null || request.File.Length == 0)
                return BadRequest("CSV file is required.");

            var result = await _service.ProcessCsvUploadAsync(request.File.OpenReadStream(), cancellationToken);
            return Ok(result);
        }
    }
}
