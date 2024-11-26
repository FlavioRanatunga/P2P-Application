using System;
using System.ServiceModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client_App
{
    public class TcpHost
    {
        private ServiceHost sHost;
        public TaskManager TaskManagerInstance { get; private set; }

        public void StartService(int port)
        {
            string address = $"net.tcp://localhost:{port}/TaskService";
            TaskManagerInstance = new TaskManager();
            sHost = new ServiceHost(TaskManagerInstance, new Uri(address));
            NetTcpBinding binding = new NetTcpBinding();
            sHost.AddServiceEndpoint(typeof(ITaskManager), binding, "");

            sHost.Open();
            Console.WriteLine($"Service started at {address}");
        }

        public void StopService()
        {
            if (sHost != null)
            {
                sHost.Close();
                sHost = null;
            }
        }
    }
}
