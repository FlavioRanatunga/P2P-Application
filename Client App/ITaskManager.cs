using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using API_Library;

namespace Client_App
{
    [ServiceContract]
    internal interface ITaskManager
    {
        [OperationContract]
        void AddJobb(string jobName, int port);

        [OperationContract]
        List<Job> GetJobs();

        [OperationContract]
        void UpdateJobStatus(int id, string res);

        [OperationContract]
        Job GetNextJob();

        [OperationContract]
        void AddJob(string base64EncodedCode, string hash, int portNumber);
    }
}
