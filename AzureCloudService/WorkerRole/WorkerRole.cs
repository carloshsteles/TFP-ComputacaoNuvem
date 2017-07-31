using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System.IO;
using System.Text;

namespace WorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        static CloudQueue cloudQueueOne;
        static CloudQueue cloudQueueTwo;

        // Connection to QueueOne and QueueTwo
        public static void ConnectToStorageQueue()
        {
            var connectionString = "DefaultEndpointsProtocol=https;AccountName=storageposgraduacao;AccountKey=EZ4Ea+feAr+nCVpsZ5rYuJCw2xSsDspv6Z7Is7k/BA86oleQQnOdlUIp2xglqwX/7uQoBs1VjVumhenpEtAE8A==;EndpointSuffix=core.windows.net";
            CloudStorageAccount cloudStorageAccount;

            if (!CloudStorageAccount.TryParse(connectionString, out cloudStorageAccount))
            {
                Console.WriteLine("Expected connection string 'Azure Storage Account to be a valid Azure Storage Connection String.");
            }

            var cloudQueueClient = cloudStorageAccount.CreateCloudQueueClient();
            cloudQueueOne = cloudQueueClient.GetQueueReference("queueone");
            cloudQueueTwo = cloudQueueClient.GetQueueReference("queuetwo");

            cloudQueueOne.CreateIfNotExists();
            cloudQueueTwo.CreateIfNotExists();
        }

        //Send message to QueueTwo
        public void SendMessageToQueueTwo(String MessageText)
        {
            var message = new CloudQueueMessage(MessageText);

            cloudQueueTwo.AddMessage(message);

        }

        //Get message form QueueOne
        public void GetMessageFromQueueOne()
        {
            CloudQueueMessage cloudQueueMessage = cloudQueueOne.GetMessage();

            if (cloudQueueMessage == null)
            {
                return;
            }
            Trace.TraceInformation("Get message from QueueOne, send to Firebase and put to QueueTwo");
            Trace.TraceInformation(cloudQueueMessage.AsString);
            var retorno = SendMessageToFirebase(cloudQueueMessage.AsString);
            
            if (retorno)
            {
                SendMessageToQueueTwo(cloudQueueMessage.AsString);
                cloudQueueOne.DeleteMessage(cloudQueueMessage);
            }
            else
                return;
                        
        }

        public Boolean SendMessageToFirebase(String message)
        {

            // Create a request using a URL that can receive a post. 
            WebRequest request = WebRequest.Create("http://fcm.googleapis.com/fcm/send");
            // Set the Method property of the request to POST.
            request.Method = "POST";
            // Create POST data and convert it to a byte array.
            string postData = message;
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            // Set the ContentType property of the WebRequest.
            request.ContentType = "application/json";
            request.Headers.Add("Authorization","key=AAAAn5ZvqF0:APA91bEuDgVaqp78r1HiBQ5Y2SNULpO2VIvHSD2CI9jiI6k7eHi3JGcayH62UzJe_sGiIJBHFxskVIzipFXRvweVOyOcNSX3UOLlb7Ladn75VmKqDAu-8G64nFXQxn7tP_0NhaBF10cd");
            // Set the ContentLength property of the WebRequest.
            request.ContentLength = byteArray.Length;
            // Get the request stream.
            Stream dataStream = request.GetRequestStream();
            // Write the data to the request stream.
            dataStream.Write(byteArray, 0, byteArray.Length);
            // Close the Stream object.
            dataStream.Close();
            // Get the response.
            WebResponse response = request.GetResponse();
            // Display the status.
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            // Get the stream containing content returned by the server.
            dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = reader.ReadToEnd();
            // Display the content.
            Console.WriteLine(responseFromServer);
            // Clean up the streams.
            reader.Close();
            dataStream.Close();
            response.Close();

            Trace.TraceInformation(responseFromServer);

            Boolean retorno = false;
            string id = "message_id";

            if (responseFromServer.Contains(id)) 
                retorno = true;
            else
                retorno = false;

            return retorno;

        }


        public override void Run()
        {
            Trace.TraceInformation("WorkerRole is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Definir o número máximo de conexões simultâneas
            ServicePointManager.DefaultConnectionLimit = 12;

            // Para obter informações sobre como tratar as alterações de configuração
            // veja o tópico do MSDN em https://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Trace.TraceInformation("WorkerRole has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("WorkerRole is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("WorkerRole has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {

            ConnectToStorageQueue();
            // TODO: substitua o item a seguir pela sua própria lógica.
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Working");
                GetMessageFromQueueOne();
                await Task.Delay(10000);
                                
            }
        }
    }
}
