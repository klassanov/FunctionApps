using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Diagnostics;

namespace FunctionApp.HTTP
{
    public static class HttpExampleFunction
    {
        [FunctionName("HttpExample")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var stopwatch = Stopwatch.StartNew();

            log.LogInformation("C# HTTP trigger function processed a request.");

            //string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            if (string.IsNullOrEmpty(requestBody)){
                return new OkObjectResult("This HTTP triggered function executed successfully, but please pass a json object in the request body");
            }

            var person = JsonConvert.DeserializeObject<Person>(requestBody);
            string responseMessage = string.IsNullOrEmpty(person.Name)
                ? "This HTTP triggered function executed successfully. Pass a name in the request body for a personalized response."
                : $"Hello, {person.Name}. Your age is {person.Age}. This HTTP triggered function executed successfully.";

            log.LogInformation($"The request has been processed in {stopwatch.ElapsedMilliseconds} ms");

            return new OkObjectResult(responseMessage);
        }
    }

    public record class Person(string Name, int Age);
}
