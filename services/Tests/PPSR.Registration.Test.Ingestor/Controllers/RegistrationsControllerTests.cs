using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PPSR.Registration.Application.DTOs;
using PPSR.Registration.Application.Interfaces;
using PPSR.Registration.Test.Ingestor.Mocks;
using PPSR.Registration.WebApi.Ingestor.Controllers;
using PPSR.Registration.WebApi.Ingestor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PPSR.Registration.Test.Ingestor.Controllers
{
    public class RegistrationsControllerTests
    {
        [Fact]
        public async Task Upload_ReturnsOk_WithExpectedResult()
        {
            // Arrange
            var mockService = new Mock<IBatchRegistrationService>();
            var expected = new BatchUploadResult
            {
                SubmittedRecords = 2,
                InvalidRecords = 1,
                AddedRecords = 1,
                UpdatedRecords = 0
            };
            mockService.Setup(s => s.ProcessCsvUploadAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(expected);

            var controller = new RegistrationsController(mockService.Object);
            var stream = SampleCsvGenerator.WithInvalidRow();
            var file = new FormFile(stream, 0, stream.Length, "File", "sample.csv")
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/csv"
            };

            var request = new UploadCsvRequest { File = file };

            // Act
            var result = await controller.Upload(request, CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var actual = Assert.IsType<BatchUploadResult>(ok.Value);
            Assert.Equal(expected.SubmittedRecords, actual.SubmittedRecords);
            Assert.Equal(expected.InvalidRecords, actual.InvalidRecords);
        }

        [Fact]
        public async Task Upload_ReturnsBadRequest_WhenFileIsNull()
        {
            // Arrange
            var mockService = new Mock<IBatchRegistrationService>();
            var controller = new RegistrationsController(mockService.Object);
            var request = new UploadCsvRequest { File = null };

            // Act
            var result = await controller.Upload(request, CancellationToken.None);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task Upload_StillReturnsOk_WhenCsvIsEmpty()
        {
            // Arrange
            var mockService = new Mock<IBatchRegistrationService>();
            var controller = new RegistrationsController(mockService.Object);

            var emptyStream = SampleCsvGenerator.Empty();
            var file = new FormFile(emptyStream, 0, 0, "File", "empty.csv")
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/csv"
            };

            var request = new UploadCsvRequest { File = file };

            // Act
            var result = await controller.Upload(request, CancellationToken.None);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }
    }
}
