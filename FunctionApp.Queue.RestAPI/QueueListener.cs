using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace FunctionApp.Queue.RestAPI
{
    public class QueueListener
    {
        //It is also possible to use a Strean directly in the binding
        [FunctionName("QueueListener")]
        public async Task Run(

            //This way We get the JSON serialized item, so we will need to deserialize it
            //[QueueTrigger(queueName: "todos", Connection = "AzureWebJobStorage")] string myQueueItem,

            //This way We ask Azure functions to deserialize the item for us
            [QueueTrigger(queueName: "todos", Connection = "AzureWebJobsStorage")] Todo todo,
            [Blob(blobPath: "todos", Connection = "AzureWebJobsStorage")] BlobContainerClient containerClient,
            ILogger log)
        {
            containerClient.CreateIfNotExists();
            var unicodeEncoding = new UnicodeEncoding();
            var message = $"Created a new task: {todo.TaskDescription}";
            var bytes = unicodeEncoding.GetBytes(message);
            var client = containerClient.GetBlobClient($"{todo.Id}.txt");

            using (var blobStream = new MemoryStream())
            {
                await blobStream.WriteAsync(bytes);
                blobStream.Position = 0;
                await client.UploadAsync(blobStream);
            }
                       
            log.LogInformation($"C# Queue trigger function processed: {todo.TaskDescription}");
        }
    }
}
