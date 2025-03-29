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
            var csvStream = SampleCsvGenerator.ValidSingleRow();

            var existing = new PpsrRegistration
            {
                VIN = "ABC1234567890",
                GrantorFirstName = "John",
                GrantorLastName = "Smith",
                SpgAcn = "123456789",
                SpgOrganizationName = "Old Org",
                RegistrationStartDate = new DateTime(2020, 1, 1),
                Duration = RegistrationDuration.SevenYears
            };

            PpsrRegistration? updatedEntity = null;

            var mockRepo = new Mock<IRegistrationRepository>();
            mockRepo.Setup(r => r.FindByVinAsync("ABC1234567890", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(existing);

            mockRepo.Setup(r => r.UpdateAsync(It.IsAny<PpsrRegistration>(), It.IsAny<CancellationToken>()))
                    .Callback<PpsrRegistration, CancellationToken>((entity, _) => updatedEntity = entity)
                    .Returns(Task.CompletedTask);

            var service = new BatchRegistrationService(mockRepo.Object);

            // Act
            var result = await service.ProcessCsvUploadAsync(csvStream, CancellationToken.None);

            // Assert
            Assert.Equal(1, result.SubmittedRecords);
            Assert.Equal(1, result.UpdatedRecords);
            Assert.Equal(0, result.AddedRecords);
            Assert.Equal(1, result.ProcessedRecords);
            Assert.NotNull(updatedEntity);
            Assert.Equal("New Org Pty Ltd", updatedEntity!.SpgOrganizationName); // Make sure it's updated
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
