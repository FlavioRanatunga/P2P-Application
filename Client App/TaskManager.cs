using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using API_Library;

namespace Client_App
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
    public class TaskManager : ITaskManager
    {
        private readonly JobManager jobManager = new JobManager();

        public void AddJobb(string pythonCode, int port)
        {
            
            string clientIP = "127.0.0.1"; 
            int clientPort = port; 
            jobManager.AddJob(pythonCode, clientIP, clientPort);
            Console.WriteLine($"Job added: {pythonCode}");
        }

        public void AddJob(string base64EncodedCode, string hash, int portNumber)
        {            
            int clientPort = portNumber; 
            jobManager.AddJob(base64EncodedCode, hash, clientPort);
            
        }

        public List<Job> GetJobs()
        {
            return jobManager.GetAllJobs();
        }

        public void UpdateJobStatus(int id, string result)
        {
            jobManager.CompleteJob(id, result);
            Console.WriteLine($"Job {id} status updated to Completed with result {result}");
        }

        public Job GetNextJob()
        {
            return jobManager.GetNextPendingJob();
        }
    }
}
