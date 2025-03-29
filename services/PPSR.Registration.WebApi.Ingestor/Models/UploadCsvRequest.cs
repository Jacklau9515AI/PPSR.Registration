namespace PPSR.Registration.WebApi.Ingestor.Models
{
    public class UploadCsvRequest
    {
        public IFormFile File { get; set; } = null!;
    }
}
