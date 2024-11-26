using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API_Library
{

    public enum Status
    {
        Idle,
        Busy,
        Stopped
    }

    public class Client
    {
        public int Id { get; set; }  
        public string IPAddress { get; set; }  
        public int Port { get; set; }  
        public Status State { get; set; }
        public int NoOfCompletedTasks { get; set; }
    }

}
