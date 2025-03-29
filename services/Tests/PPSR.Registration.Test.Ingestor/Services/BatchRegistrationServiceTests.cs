using Moq;
using PPSR.Registration.Application.Interfaces;
using PPSR.Registration.Domain.Entities;
using PPSR.Registration.Domain.Enums;
using PPSR.Registration.Infrastructure.Services;
using PPSR.Registration.Test.Ingestor.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPSR.Registration.Test.Ingestor.Services
{
    public class BatchRegistrationServiceTests
    {
        [Fact]
        public async Task ProcessCsvUploadAsync_ShouldAdd_WhenRecordIsValidAndDoesNotExist()
        {
            // Arrange
            var csvStream = SampleCsvGenerator.ValidSingleRow();

            var mockRepo = new Mock<IRegistrationRepository>();
            mockRepo.Setup(r => r.FindByVinAsync("ABC1234567890", It.IsAny<CancellationToken>()))
                    .ReturnsAsync((PpsrRegistration?)null); // Simulate no match

            mockRepo.Setup(r => r.AddAsync(It.IsAny<PpsrRegistration>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

            var service = new BatchRegistrationService(mockRepo.Object);

            // Act
            var result = await service.ProcessCsvUploadAsync(csvStream, CancellationToken.None);

            // Assert
            Assert.Equal(1, result.SubmittedRecords);
            Assert.Equal(1, result.AddedRecords);
            Assert.Equal(0, result.UpdatedRecords);
            Assert.Equal(1, result.ProcessedRecords);
            Assert.Equal(0, result.InvalidRecords);
        }

        [Fact]
        public async Task ProcessCsvUploadAsync_ShouldUpdate_WhenVinMatchesAndRecordExists()
        {
            // Arrange
            var csv = new StringBuilder();
            csv.AppendLine("Grantor First Name,Grantor Middle Names,Grantor Last Name,VIN,Registration start date,Registration duration,SPG ACN,SPG Organization Name");
            csv.AppendLine("John,,Smith,ABC1234567890,2025-01-01,7,123456789,New Org Pty Ltd");
            var csvStream = new MemoryStream(Encoding.UTF8.GetBytes(csv.ToString()));

            var existing = new PpsrRegistration
            {
                VIN = "ABC1234567890",
                GrantorFirstName = "John",
                GrantorLastName = "Smith",
                SpgAcn = "123456789",
                SpgOrganizationName = "Old Name"
            };

            var mockRepo = new Mock<IRegistrationRepository>();
            mockRepo.Setup(r => r.FindByUniqueKeyAsync("ABC1234567890", "John", "Smith", "123456789", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(existing);

            mockRepo.Setup(r => r.UpdateAsync(It.IsAny<PpsrRegistration>(), It.IsAny<CancellationToken>()))
                    .Callback<PpsrRegistration, CancellationToken>((updated, _) =>
                    {
                        Assert.Equal("New Org Pty Ltd", updated.SpgOrganizationName);
                        Assert.Equal(RegistrationDuration.SevenYears, updated.Duration);
                    })
                    .Returns(Task.CompletedTask);

            var service = new BatchRegistrationService(mockRepo.Object);

            // Act
            var result = await service.ProcessCsvUploadAsync(csvStream, CancellationToken.None);

            // Assert
            Assert.Equal(1, result.SubmittedRecords);
            Assert.Equal(1, result.UpdatedRecords);
            Assert.Equal(0, result.AddedRecords);
            Assert.Equal(0, result.InvalidRecords);
        }
    

        [Fact]
        public async Task ProcessCsvUploadAsync_ShouldSkipInvalidRows()
        {
            // Arrange
            var csvStream = SampleCsvGenerator.WithInvalidRow(); // one valid, one broken

            var mockRepo = new Mock<IRegistrationRepository>();
            mockRepo.Setup(r => r.FindByVinAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((PpsrRegistration?)null);
            mockRepo.Setup(r => r.AddAsync(It.IsAny<PpsrRegistration>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

            var service = new BatchRegistrationService(mockRepo.Object);

            // Act
            var result = await service.ProcessCsvUploadAsync(csvStream, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.SubmittedRecords); // header skipped
            Assert.Equal(1, result.AddedRecords);
            Assert.Equal(1, result.InvalidRecords);
            Assert.Equal(1, result.ProcessedRecords);
        }

        [Fact]
        public async Task ProcessCsvUploadAsync_ShouldReturnZero_WhenCsvIsEmpty()
        {
            // Arrange
            var stream = SampleCsvGenerator.Empty();
            var mockRepo = new Mock<IRegistrationRepository>();
            var service = new BatchRegistrationService(mockRepo.Object);

            // Act
            var result = await service.ProcessCsvUploadAsync(stream, CancellationToken.None);

            // Assert
            Assert.Equal(0, result.SubmittedRecords);
            Assert.Equal(0, result.AddedRecords);
            Assert.Equal(0, result.UpdatedRecords);
            Assert.Equal(0, result.InvalidRecords);
        }

        [Fact]
        public async Task ProcessCsvUploadAsync_ShouldCountLineEvenWhenHeaderOnly()
        {
            // Arrange
            var csv = "Grantor First Name,Grantor Middle Names,Grantor Last Name,VIN,Registration Start Date,Registration Duration,SPG ACN,SPG Org Name\n";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

            var mockRepo = new Mock<IRegistrationRepository>();
            var service = new BatchRegistrationService(mockRepo.Object);

            // Act
            var result = await service.ProcessCsvUploadAsync(stream, CancellationToken.None);

            // Assert
            Assert.Equal(0, result.SubmittedRecords);
            Assert.Equal(0, result.AddedRecords);
            Assert.Equal(0, result.UpdatedRecords);
            Assert.Equal(0, result.InvalidRecords);
        }

        [Fact]
        public async Task ProcessCsvUploadAsync_ShouldHandleException_PerRow()
        {
            // Arrange
            var badCsv = "John,,Smith,INVALID_DATE,abc,,123456789,Org Pty Ltd\n";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes("Grantor First Name,Grantor Middle Names,Grantor Last Name,VIN,Registration Start Date,Registration Duration,SPG ACN,SPG Org Name\n" + badCsv));

            var mockRepo = new Mock<IRegistrationRepository>();
            var service = new BatchRegistrationService(mockRepo.Object);

            // Act
            var result = await service.ProcessCsvUploadAsync(stream, CancellationToken.None);

            // Assert
            Assert.Equal(1, result.SubmittedRecords);
            Assert.Equal(0, result.AddedRecords);
            Assert.Equal(0, result.UpdatedRecords);
            Assert.Equal(1, result.InvalidRecords);
        }
    }
}
