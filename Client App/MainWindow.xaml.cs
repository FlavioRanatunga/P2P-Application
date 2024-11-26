using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using RestSharp; 
using Newtonsoft.Json; 
using IronPython.Hosting;
using API_Library;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Collections.Generic;
using System.Net.Sockets;
using System.ServiceModel;
using System.Security.Cryptography;
using System.Text;



namespace Client_App
{
    public partial class MainWindow : Window
    {
        private int jobsDone = 0;
        private Thread serverThread;
        private Thread networkingThread;
        private readonly RestClient client;
        private const string URL = "http://localhost:5210";
        private int portNumber;
        private TcpHost tcpServerHost = new TcpHost();
        private List<Client> registeredClients = new List<Client>();
        private TaskManager taskManagerInstance;
        private int clientId;


        public MainWindow()
        {
            InitializeComponent();
            client = new RestClient(URL);
        }

        private void InsertFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Python files (*.py)|*.py|All files (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                PythonCodeTextBox.Text = File.ReadAllText(openFileDialog.FileName);
            }
        }

        private void InsertCodeButton_Click(object sender, RoutedEventArgs e)
        {
            string pythonCode = PythonCodeTextBox.Text;
            if (!string.IsNullOrWhiteSpace(pythonCode))
            {
                // Add the Python code as a job
                AddPythonJob(pythonCode);
                MessageBox.Show("Python code added as a job.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                PythonCodeTextBox.Clear();
            }
            else
            {
                MessageBox.Show("Please enter Python code in the text box.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StartServerThread()
        {
            serverThread = new Thread(() =>
            {
                tcpServerHost = new TcpHost();
                tcpServerHost.StartService(portNumber);
                taskManagerInstance = tcpServerHost.TaskManagerInstance;
                Console.WriteLine($"TaskManagerInstance assigned: {taskManagerInstance != null}");
            });
            serverThread.IsBackground = true;
            serverThread.Start();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            StopServer();
        }

        private void StopServer()
        {
            if (tcpServerHost != null)
            {
                tcpServerHost.StopService();
                RestRequest request = new RestRequest($"api/clients/updateStopped/{clientId}", Method.Post);
                RestResponse response = client.Execute(request);
                Console.WriteLine("Server stopped.");
            }
        }

        private void StartNetworkingThread()
        {
            networkingThread = new Thread(async () =>
            {
                await NetworkingTaskMethod();
            });
            networkingThread.IsBackground = true;
            networkingThread.Start();
        }

        private async Task NetworkingTaskMethod()
        {
            // Simulate networking work
            while (true)
            {
                // Request the list of registered clients
                var clientsRequest = new RestRequest("api/clients", Method.Get);
                RestResponse clientsResponse = await client.ExecuteAsync(clientsRequest);

                if (clientsResponse.IsSuccessful)
                {
                    registeredClients = JsonConvert.DeserializeObject<List<Client>>(clientsResponse.Content);
                    // Use the registered clients list for networking purposes
                    foreach (var registeredClient in registeredClients)
                    {
                        Console.WriteLine($"Got Registered Client: {registeredClient.IPAddress}:{registeredClient.Port}");
                        if (registeredClient.State != Status.Stopped)
                        {
                            await ConnectToClient(registeredClient);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Failed to retrieve registered clients.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                await Task.Delay(5000); // Simulate networking delay
            }
        }

        private async Task ConnectToClient(Client cClient)
        {
            try
            {
                // Replace with actual IP address of the client application
                string ownIPAddress = "127.0.0.1";
                int ownPort = portNumber;

                // Check if the client is trying to connect to its own IP and port
                if (cClient.Port == ownPort)
                {
                    //Console.WriteLine("Skipping connection to own IP and port.");
                    return;
                }

                string address = $"net.tcp://localhost:{cClient.Port}/TaskService";
                var binding = new NetTcpBinding();
                var endpointAddress = new EndpointAddress(address);

                var channelFactory = new ChannelFactory<ITaskManager>(binding, endpointAddress);
                var taskManager = channelFactory.CreateChannel();

                Console.WriteLine($"Connected to client {cClient.IPAddress}:{cClient.Port}");
                //taskManager.AddJob(PythonCodeTextBox.Text, ownPort);
                Job nextJob = taskManager.GetNextJob();
                if (nextJob != null)
                {
                    Console.WriteLine($"Processing Job ID: {nextJob.JobId}");
                    RestRequest req = new RestRequest("api/clients/taskProcessingUpdate/" + clientId, Method.Post);
                    RestResponse response = await client.ExecuteAsync(req);
                    //RestResponse res = await client.ExecuteAsync(req);
                    // Add logic to process the job
                    // For example, execute the Python code and update the job status
                    //string result = ExecuteJob(nextJob.PythonCode);
                    string result = ExecuteJob(nextJob.PythonCode, nextJob.Hash);
                    Console.WriteLine($"Job ID: {nextJob.JobId} completed with result: {result}");
                    taskManager.UpdateJobStatus(nextJob.JobId, result);
                    req = new RestRequest("api/clients/taskCompleteUpdate/" + clientId, Method.Post);
                    response = await client.ExecuteAsync(req);
                }
                else
                {
                    Console.WriteLine("No pending jobs available.");
                }

                //and process them
                // For example, you can send a request to the client to get jobs
                // and then process the jobs if available
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to connect to client {cClient.IPAddress}:{cClient.Port}. Error: {ex.Message}");
            }
        }

        /*private string ExecuteJob(string pythonCode)
        {
            try
            {
                var engine = Python.CreateEngine();
                var scope = engine.CreateScope();
                var source = engine.CreateScriptSourceFromString(pythonCode);
                var result = source.Execute(scope);
                return result?.ToString() ?? "No result";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing Python code: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }*/

        /*private string ExecuteJob(string base64EncodedCode, string expectedHash)
        {
            if (!VerifySha256Hash(base64EncodedCode, expectedHash))
            {
                Console.WriteLine("Hash verification failed. The code may have been tampered with.");
                return "Hash verification failed.";
            }

            string pythonCode = DecodeFromBase64(base64EncodedCode);

            try
            {
                var engine = Python.CreateEngine();
                var scope = engine.CreateScope();
                var source = engine.CreateScriptSourceFromString(pythonCode);
                var result = source.Execute(scope);
                return result?.ToString() ?? "No result";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing Python code: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }*/

        private string ExecuteJob(string base64EncodedCode, string expectedHash)
        {
            // Inline hash verification
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(base64EncodedCode));
                string hashOfInput = BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();

                if (!StringComparer.OrdinalIgnoreCase.Equals(hashOfInput, expectedHash))
                {
                    Console.WriteLine("Hash verification failed. The code may have been tampered with.");
                    return "Hash verification failed.";
                }
            }

            string pythonCode = DecodeFromBase64(base64EncodedCode);

            try
            {
                var engine = Python.CreateEngine();
                var scope = engine.CreateScope();
                var source = engine.CreateScriptSourceFromString(pythonCode);
                var result = source.Execute(scope);
                return result?.ToString() ?? "No result";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing Python code: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }



        private async void RegisterPortButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(PortNumberTextBox.Text, out portNumber))
            {
                PortNumberLabel.Content = $"Port Number: {portNumber}";

                var clientInfo = new
                {
                    IPAddress = "127.0.0.1", // Replace with actual IP address
                    Port = portNumber
                };

                var request = new RestRequest("api/clients/register", Method.Post);
                request.AddJsonBody(clientInfo);

                RestResponse response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    var cClient = JsonConvert.DeserializeObject<Client>(response.Content);
                    clientId = cClient.Id; // Store the client ID
                    MessageBox.Show("Client registered successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    StartServerThread();
                    StartNetworkingThread();
                    PortNumberTextBox.Visibility = Visibility.Collapsed;
                    RegisterPortButton.Visibility = Visibility.Collapsed;
                    // Request the list of registered clients
                    var clientsRequest = new RestRequest("api/clients", Method.Get);
                    RestResponse clientsResponse = await client.ExecuteAsync(clientsRequest);

                    if (clientsResponse.IsSuccessful)
                    {
                        registeredClients = JsonConvert.DeserializeObject<List<Client>>(clientsResponse.Content);
                      
                        // Use the registered clients list for networking purposes
                        foreach (var registeredClient in registeredClients)
                        {
                            Console.WriteLine($"Registered Client: {registeredClient.IPAddress}:{registeredClient.Port}");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Failed to retrieve registered clients.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Failed to register client.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid port number.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
            }


        }

        private void AddPythonJob(string pythonCode)
        {
            string base64EncodedCode = EncodeToBase64(pythonCode);

            // Compute the SHA-256 hash in place
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(base64EncodedCode));
                string hash = BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();

                // Send the base64EncodedCode and hash to the server
                taskManagerInstance.AddJob(base64EncodedCode, hash, portNumber);
                Console.WriteLine("Python code added as a job.");
            }
        }

        private string EncodeToBase64(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        private string DecodeFromBase64(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }       

        private async void GetClientStatusButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var request = new RestRequest($"api/Clients/{clientId}", Method.Get);
                var response = await client.ExecuteAsync<Client>(request);

                if (response.IsSuccessful && response.Data != null)
                {
                    var clientData = response.Data;
                    StatusLabel.Content = $"Status: {clientData.State}";
                    JobsDoneLabel.Content = $"Jobs Done: {clientData.NoOfCompletedTasks}";
                }
                else
                {
                    MessageBox.Show("Failed to retrieve client status.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
    }

}