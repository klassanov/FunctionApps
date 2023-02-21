using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace FunctionApp.Chaining
{
    public static class FunctionChainingExample
    {


        [FunctionName(nameof(OrchestratorFunction))]
        public static async Task<List<string>> OrchestratorFunction([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            var generatedNumber = await context.CallActivityAsync<int>(nameof(GenerateNumber), null);
            outputs.Add(await context.CallActivityAsync<string>(nameof(PrintNumber), generatedNumber));

            generatedNumber = await context.CallActivityAsync<int>(nameof(GenerateNumber), null);
            outputs.Add(await context.CallActivityAsync<string>(nameof(PrintNumber), generatedNumber));

            // returns ["Generated number is: x", "Generated number is: y"]
            // Follow the statusQueryGetUri in the response of the call to see the output
            return outputs;
        }




        [FunctionName(nameof(PrintNumber))]
        public static string PrintNumber([ActivityTrigger] int generatedNumber, ILogger log)
        {
            log.LogInformation($"Generated number: {generatedNumber}.");
            return $"Generated number: {generatedNumber}";
        }



        [FunctionName(nameof(GenerateNumber))]
        public static int GenerateNumber([ActivityTrigger] ILogger log)
        {
            Random r = new Random();
            return r.Next(1, 10);
        }



        [FunctionName(nameof(HttpStart))]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync(nameof(OrchestratorFunction), null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }

    }
}