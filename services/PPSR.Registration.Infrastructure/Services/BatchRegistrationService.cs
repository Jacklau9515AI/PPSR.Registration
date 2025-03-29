using Microsoft.EntityFrameworkCore;
using PPSR.Registration.Application.DTOs;
using PPSR.Registration.Application.Interfaces;
using PPSR.Registration.Domain.Entities;
using PPSR.Registration.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPSR.Registration.Infrastructure.Services
{
    public class BatchRegistrationService : IBatchRegistrationService
    {
        private readonly IRegistrationRepository _repository;

        public BatchRegistrationService(IRegistrationRepository repository)
        {
            _repository = repository;
        }

        public async Task<BatchUploadResult> ProcessCsvUploadAsync(Stream csvFileStream, CancellationToken cancellationToken)
        {
            var result = new BatchUploadResult();
            using var reader = new StreamReader(csvFileStream);
            var lineIndex = 0;

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync(cancellationToken);
                if (lineIndex++ == 0 || string.IsNullOrWhiteSpace(line)) continue; // Skip header

                var columns = line.Split(',');

                if (columns.Length < 8)
                {
                    result.InvalidRecords++;
                    continue;
                }

                try
                {
                    var dto = new PpsrCsvUploadDto
                    {
                        GrantorFirstName = columns[0].Trim(),
                        GrantorMiddleNames = string.IsNullOrWhiteSpace(columns[1]) ? null : columns[1].Trim(),
                        GrantorLastName = columns[2].Trim(),
                        VIN = columns[3].Trim(),
                        RegistrationStartDate = ParseDate(columns[4].Trim()),
                        RegistrationDurationRaw = ParseDurationRaw(columns[5].Trim()),
                        SpgAcn = columns[6].Trim().Replace(" ", ""),
                        SpgOrganizationName = columns[7].Trim()
                    };

                    var existing = await _repository.FindByVinAsync(dto.VIN, cancellationToken);
                    
                    if (existing != null &&
                        existing.GrantorFirstName == dto.GrantorFirstName &&
                        existing.GrantorLastName == dto.GrantorLastName &&
                        existing.SpgAcn == dto.SpgAcn)
                    {
                        // Update path
                        existing.RegistrationStartDate = dto.RegistrationStartDate.Date;
                        existing.Duration = ConvertToEnum(dto.RegistrationDurationRaw);
                        existing.GrantorMiddleNames = dto.GrantorMiddleNames;
                        existing.SpgOrganizationName = dto.SpgOrganizationName;

                        await _repository.UpdateAsync(existing, cancellationToken);
                        result.UpdatedRecords++;
                    }
                    else
                    {
                        // Add path
                        var newRecord = new PpsrRegistration
                        {
                            GrantorFirstName = dto.GrantorFirstName,
                            GrantorMiddleNames = dto.GrantorMiddleNames,
                            GrantorLastName = dto.GrantorLastName,
                            VIN = dto.VIN,
                            RegistrationStartDate = dto.RegistrationStartDate.Date,
                            Duration = ConvertToEnum(dto.RegistrationDurationRaw),
                            SpgAcn = dto.SpgAcn,
                            SpgOrganizationName = dto.SpgOrganizationName
                        };

                        await _repository.AddAsync(newRecord, cancellationToken);
                        result.AddedRecords++;
                    }
                }
                catch
                {
                    result.InvalidRecords++;
                }
            }

            if (lineIndex == 0)
            {
                result.SubmittedRecords = 0;
            }
            else
            {
                result.SubmittedRecords = lineIndex - 1;
            }
            return result;
        }

        private static DateTime ParseDate(string dateStr)
        {
            return DateTime.ParseExact(dateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        private static int ParseDurationRaw(string raw)
        {
            return string.IsNullOrWhiteSpace(raw) ? 0 : (int)double.Parse(raw);
        }

        private static RegistrationDuration ConvertToEnum(int value)
        {
            return value switch
            {
                7 => RegistrationDuration.SevenYears,
                25 => RegistrationDuration.TwentyFiveYears,
                _ => RegistrationDuration.NoEndDate
            };
        }
    }
}
