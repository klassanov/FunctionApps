using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace FunctionApp.Queue.RestAPI
{
    public class QueueListener
    {
        [FunctionName("QueueListener")]
        public void Run(
            //We get the JSON serialized item
            //[QueueTrigger(queueName: "todos", Connection = "AzureWebJobStorage")] string myQueueItem,

            //We ask Azure functions to deserialize the item for us
            [QueueTrigger(queueName: "todos", Connection = "AzureWebJobStorage")] Todo todo,
            ILogger log)
        {
            //log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
        }
    }
}
