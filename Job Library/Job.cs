using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API_Library
{
    public enum JobStatus
    {
        Pending,
        InProgress,
        Completed
    }
    public class Job
    {
        public int JobId { get; set; } 
        public string PythonCode { get; set; } 
        public string Hash { get; set; }
        public string Result { get; set; } 
        public JobStatus State { get; set; } 
        public string ClientIP { get; set; } 
        public int ClientPort { get; set; }
    }

}
