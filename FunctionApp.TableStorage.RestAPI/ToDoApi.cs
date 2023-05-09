using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Azure.Data.Tables;
using FunctionApp.TableStorage.RestAPI.TableStorage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

// GOAL: Expose a REST STYLE api for managing todo list items

// GET     /api/todo       -> gets all todo list items
// GET     /api/todo/{id}  -> get a single todo list item by id
// POST    /api/todo       -> create a new todo list item
// PUT     /api/todo/{id}  -> update a new todo list item
// DELETE  /api/todo/{id}  -> delete a single todo list item

// Problem: Using function names as routes (default behaviour) will make the api inconsistent
// Solution: Specifying a route instead of using the function name as a route

namespace FunctionApp.TableStorage.RestAPI
{
    public static class ToDoApi
    {

        [FunctionName("CreateTodo")]
        public static async Task<IActionResult> CreateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo")] HttpRequest req,
            [Table(tableName: "todos", Connection = "AzureWebJobsStorage")] IAsyncCollector<ToDoTableEntity> todoTable,
            ILogger logger)
        {
            logger.LogInformation("Creating a new todo item");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var todoCreateModel = JsonConvert.DeserializeObject<ToDoCreateModel>(requestBody);
            Todo todo = new Todo(Guid.NewGuid().ToString(), DateTime.Now, todoCreateModel.TaskDescription, false);
            await todoTable.AddAsync(todo.ToTableEntity());

            logger.LogInformation($"Todo item with id {todo.Id} created");

            //does not calculate the route correctly so it is unusable now
            //return new CreatedAtRouteResult(new { id = todo.Id }, todo);

            return new OkObjectResult(todo);
        }


        [FunctionName("GetTodos")]
        public static async Task<IActionResult> GetTodos(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo")] HttpRequest req,
            [Table(tableName: Constants.ToDoTableName, Connection = "AzureWebJobsStorage")] TableClient tableClient,
            ILogger logger)
        {
            //var items = entities.Select(x => x.ToTodo()).ToList();

            //logger.LogInformation("Retrieving all the todo items");
            //return new OkObjectResult(items);
            var items = new List<Todo>();
            var queryResults = tableClient.QueryAsync<ToDoTableEntity>();
            await foreach (ToDoTableEntity entity in queryResults)
            {
               items.Add(entity.ToTodo());
            }

            return new OkObjectResult(items);
        }

        //[FunctionName("GetTodo")]
        //public static IActionResult GetTodo(
        //    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo/{id}")] HttpRequest req,
        //    [Table(tableName: Constants.ToDoTableName, partitionKey: Constants.ToDoPartitionKey, rowKey: "{id}", Connection = "AzureWebJobsStorage")] ToDoTableEntity todoTable,
        //    ILogger log,
        //    string id)
        //{
        //    log.LogInformation($"Retrieving a todo item with id {id}");

        //    var todo = todoTable.ToTodo();

        //    return todo is null
        //        ? new NotFoundResult()
        //        : new OkObjectResult(todo);
        //}

        //[FunctionName("UpdateTodo")]
        //public static async Task<IActionResult> UpdateTodo(
        //    [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todo/{id}")] HttpRequest req,
        //    [Table(tableName: Constants.ToDoTableName, Connection = "AzureWebJobsStorage")] CloudTable todoTable,
        //    ILogger log,
        //    string id)
        //{
        //    log.LogInformation($"Updating todo item with id {id}");

        //    var findOperation = TableOperation.Retrieve<ToDoTableEntity>(partitionKey: Constants.ToDoPartitionKey, rowkey:id);
        //    var findResult = await todoTable.ExecuteAsync(findOperation);
        //    var todoEntity = findResult.Result as ToDoTableEntity;
        //    if (todoEntity is null)
        //    {
        //        return new NotFoundResult();
        //    }

        //    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        //    var toDoUpdateModel = JsonConvert.DeserializeObject<ToDoUpdateModel>(requestBody);
        //    var taskDescription = !string.IsNullOrEmpty(toDoUpdateModel.TaskDescription)
        //                                ? toDoUpdateModel.TaskDescription
        //                                : todoEntity.Description;

        //    todoEntity.Description = taskDescription;
        //    todoEntity.IsCompleted = toDoUpdateModel.IsCompleted;

        //    var replaceOperation = TableOperation.Replace(todoEntity);
        //    await todoTable.ExecuteAsync(replaceOperation);

        //    return new OkObjectResult(todoEntity.ToTodo());
        //}


        //[FunctionName("DeleteTodo")]
        //public static async Task<IActionResult> DeleteTodo(
        //    [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "todo/{id}")] HttpRequest req,
        //    [Table(tableName: Constants.ToDoTableName, Connection = "AzureWebJobsStorage")] CloudTable todoTable,
        //    ILogger logger,
        //    string id)
        //{
        //    logger.LogInformation($"Deleting a record with id {id}");

        //    var deleteOperation = TableOperation.Delete(new TableEntity
        //    {
        //        PartitionKey = Constants.ToDoPartitionKey,
        //        RowKey = id,
        //        ETag = "*"
        //    });

        //    try
        //    {
        //        var deleteResult =await todoTable.ExecuteAsync(deleteOperation);
        //    }
        //    catch (StorageException ex) when (ex.RequestInformation.HttpStatusCode == (int) HttpStatusCode.NotFound)
        //    {
        //        logger.LogError(new EventId(1), ex, $"Error during deletion of entity with id {id}");
        //        return new NotFoundResult();
        //    }

        //    return new OkResult();
        //}
    }


    public record Todo(string Id, DateTime CreatedTime, string TaskDescription, bool IsCompleted);

    public record ToDoCreateModel(string TaskDescription);

    public record ToDoUpdateModel(string TaskDescription, bool IsCompleted);
}
