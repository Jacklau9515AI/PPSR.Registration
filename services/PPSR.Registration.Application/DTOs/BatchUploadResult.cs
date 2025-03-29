using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPSR.Registration.Application.DTOs
{
    public class BatchUploadResult
    {
        public int SubmittedRecords { get; set; }
        public int InvalidRecords { get; set; }
        public int ProcessedRecords => AddedRecords + UpdatedRecords;
        public int AddedRecords { get; set; }
        public int UpdatedRecords { get; set; }
    }
}
