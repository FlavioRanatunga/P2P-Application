using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API_Library
{
    public class JobManager
    {
        private static readonly List<Job> jobs = new List<Job>();

        // Add a new job to the manager
        /*public void AddJob(string pythonCode, string clientIP, int clientPort)
        {
            Job newJob = new Job
            {
                JobId = jobs.Count + 1,
                PythonCode = pythonCode,
                ClientIP = clientIP,
                ClientPort = clientPort,
                State = JobStatus.Pending,
                Result = string.Empty
            };
            jobs.Add(newJob);
        }*/

        public void AddJob(string pythonCode, string hash, int clientPort)
        {
            Job newJob = new Job
            {
                JobId = jobs.Count + 1,
                PythonCode = pythonCode,
                Hash = hash,
                ClientIP = "127.0.0.1",
                ClientPort = clientPort,
                State = JobStatus.Pending,
                Result = string.Empty
            };
            jobs.Add(newJob);
        }

        // Get a job that is not yet completed (for processing by a client)
        public Job GetNextPendingJob()
        {
            Job nextJob = jobs.FirstOrDefault(j => j.State == JobStatus.Pending);
            if (nextJob != null)
            {
                nextJob.State = JobStatus.InProgress;
                Console.WriteLine($"Job {nextJob.JobId} status updated to In Progress");
            }
            return nextJob;
        }

        // Mark a job as completed and store the result
        public void CompleteJob(int jobId, string result)
        {
            Job job = jobs.FirstOrDefault(j => j.JobId == jobId);
            if (job != null)
            {
                job.State = JobStatus.Completed;
                job.Result = result;
            }
        }

        // Get the list of completed jobs
        public List<Job> GetCompletedJobs()
        {
            return jobs.Where(j => j.State == JobStatus.Completed).ToList();
        }

        // Get all jobs
        public List<Job> GetAllJobs()
        {
            return jobs;
        }

        public Job GetJob()
        {
            return jobs.FirstOrDefault();
        }
    }
}
