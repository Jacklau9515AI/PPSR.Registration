using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPSR.Registration.Test.Ingestor.Mocks
{
    public static class SampleCsvGenerator
    {
        public static Stream ValidSingleRow()
        {
            var csv = new StringBuilder();
            csv.AppendLine("Grantor First Name,Grantor Middle Names,Grantor Last Name,VIN,Registration Start Date,Registration Duration,SPG ACN,SPG Org Name");
            csv.AppendLine("John,,Smith,ABC1234567890,2025-01-01,7,123456789,New Org Pty Ltd");
            return ToStream(csv.ToString());
        }

        public static Stream ValidMultipleRows()
        {
            var csv = new StringBuilder();
            csv.AppendLine("Make,Model,Year,VIN");
            csv.AppendLine("Toyota,Camry,2020,ABC1234567890");
            csv.AppendLine("Honda,Civic,2021,XYZ9876543210");
            return ToStream(csv.ToString());
        }

        public static Stream WithInvalidRow()
        {
            var csv = new StringBuilder();
            csv.AppendLine("Grantor First Name,Grantor Middle Names,Grantor Last Name,VIN,Registration Start Date,Registration Duration,SPG ACN,SPG Org Name");
            csv.AppendLine("John,,Smith,ABC1234567890,2025-01-01,7,123456789,New Org Pty Ltd"); // valid
            csv.AppendLine(",,Smith,ABC1234567890,,,123456789");                                // invalid
            return ToStream(csv.ToString());
        }

        public static Stream Empty()
        {
            return ToStream("");
        }

        private static Stream ToStream(string content)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            return new MemoryStream(bytes);
        }
    }
}
