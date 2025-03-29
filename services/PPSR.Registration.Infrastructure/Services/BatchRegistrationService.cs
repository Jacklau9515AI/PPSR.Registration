using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using PPSR.Registration.Application.DTOs;
using PPSR.Registration.Application.Interfaces;
using PPSR.Registration.Domain.Entities;
using PPSR.Registration.Domain.Enums;
using System.Formats.Asn1;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

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

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
                MissingFieldFound = null,
                BadDataFound = null,
            };

            using var csv = new CsvReader(reader, config);
            var records = csv.GetRecords<dynamic>().ToList();

            int line = 1;

            foreach (var record in records)
            {
                line++;
                try
                {
                    var row = (IDictionary<string, object>)record;

                    var originalVin = row["VIN"]?.ToString();
                    var originalAcn = row["SPG ACN"]?.ToString();

                    var normalizedVin = NormalizeVin(originalVin);
                    var normalizedAcn = NormalizeAcn(originalAcn);

                    var dto = new PpsrCsvUploadDto
                    {
                        GrantorFirstName = CleanName(row["Grantor First Name"]?.ToString()),
                        GrantorMiddleNames = string.IsNullOrWhiteSpace(row["Grantor Middle Names"]?.ToString())
                         ? null : CleanName(row["Grantor Middle Names"]?.ToString()),
                        GrantorLastName = CleanName(row["Grantor Last Name"]?.ToString()),
                        VIN = NormalizeVin(row["VIN"]?.ToString()),
                        RegistrationStartDate = ParseDateSmart(row["Registration start date"]?.ToString() ?? throw new("Missing start date")),
                        RegistrationDurationRaw = ParseDurationRaw(row["Registration duration"]?.ToString()),
                        SpgAcn = NormalizeAcn(row["SPG ACN"]?.ToString()),
                        SpgOrganizationName = row["SPG Organization Name"]?.ToString()?.Trim() ?? ""
                    };

                    if (string.IsNullOrWhiteSpace(dto.VIN) || string.IsNullOrWhiteSpace(dto.GrantorFirstName) || string.IsNullOrWhiteSpace(dto.GrantorLastName))
                    {
                        result.InvalidRecords++;
                        result.WarningMessages.Add($"Line {line}: Missing required fields (VIN or Grantor names)");
                        continue;
                    }

                    if (!string.Equals(originalVin, normalizedVin, StringComparison.Ordinal))
                        result.WarningMessages.Add($"Line {line}: VIN was normalized to '{normalizedVin}'");

                    if (!string.Equals(originalAcn, normalizedAcn, StringComparison.Ordinal))
                        result.WarningMessages.Add($"Line {line}: SPG ACN was normalized to '{normalizedAcn}'");

                    var existing = await _repository.FindByUniqueKeyAsync(dto.VIN, dto.GrantorFirstName, dto.GrantorLastName, dto.SpgAcn, cancellationToken);
                    if (existing != null)
                    {
                        existing.GrantorFirstName = dto.GrantorFirstName;
                        existing.GrantorMiddleNames = dto.GrantorMiddleNames;
                        existing.GrantorLastName = dto.GrantorLastName;
                        existing.RegistrationStartDate = dto.RegistrationStartDate;
                        existing.Duration = ConvertToEnum(dto.RegistrationDurationRaw);
                        existing.SpgAcn = dto.SpgAcn;
                        existing.SpgOrganizationName = dto.SpgOrganizationName;

                        await _repository.UpdateAsync(existing, cancellationToken);
                        result.UpdatedRecords++;
                    }
                    else
                    {
                        var entity = new PpsrRegistration
                        {
                            GrantorFirstName = dto.GrantorFirstName,
                            GrantorMiddleNames = dto.GrantorMiddleNames,
                            GrantorLastName = dto.GrantorLastName,
                            VIN = dto.VIN,
                            RegistrationStartDate = dto.RegistrationStartDate,
                            Duration = ConvertToEnum(dto.RegistrationDurationRaw),
                            SpgAcn = dto.SpgAcn,
                            SpgOrganizationName = dto.SpgOrganizationName
                        };

                        await _repository.AddAsync(entity, cancellationToken);
                        result.AddedRecords++;
                    }
                }
                catch (Exception ex)
                {
                    result.InvalidRecords++;
                    result.WarningMessages.Add($"Line {line}: {ex.Message}");
                }
            }

            result.SubmittedRecords = records.Count;
            result.ProcessedAt = DateTime.UtcNow;
            return result;
        }

        private static DateTime ParseDateSmart(string input)
        {
            var formats = new[] { "yyyy-MM-dd", "dd/MM/yyyy", "MM/dd/yyyy", "d/M/yyyy", "dd-MM-yyyy", "d MMM yyyy", "dd MMM yyyy" };
            if (DateTime.TryParseExact(input, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                return DateTime.SpecifyKind(dt, DateTimeKind.Utc);

            if (DateTime.TryParse(input, out var fallback))
                return DateTime.SpecifyKind(fallback, DateTimeKind.Utc);

            throw new FormatException($"Unrecognized date format: {input}");
        }

        private static int ParseDurationRaw(string? raw)
        {
            return int.TryParse(raw, out var val) ? val : 0;
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

        private static string CleanName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "";

            return Regex.Replace(name.Replace(",", " "), @"[^a-zA-Z\s]", "").Trim();
        }

        private static string NormalizeVin(string? vin)
        {
            return vin?.Trim().ToUpperInvariant() ?? "";
        }

        private static string NormalizeAcn(string? acn)
        {
            return acn?.Replace(" ", "").Trim() ?? "";
        }
    }
}
