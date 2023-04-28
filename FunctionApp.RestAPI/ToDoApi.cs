using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

// GOAL: Expose a REST STYLE api for managing todo list items

// GET     /api/todo       -> gets all todo list items
// GET     /api/todo/{id}  -> get a single todo list item by id
// POST    /api/todo       -> create a new todo list item
// PUT     /api/todo/{id}  -> update a new todo list item
// DELETE  /api/todo/{id}  -> delete a single todo list item

// Problem: Using function names as routes (default behaviour) will make the api inconsistent
// Solution: Specifying a route instead of using the function name as a route

namespace FunctionApp.RestAPI
{
    public static class ToDoApi
    {
        static List<Todo> items = new List<Todo>();

        [FunctionName("CreateTodo")]
        public static async Task<IActionResult> CreateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo")] HttpRequest req,
            ILogger logger)
        {
            logger.LogInformation("Creating a new todo item");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var todoCreateModel = JsonConvert.DeserializeObject<ToDoCreateModel>(requestBody);
            Todo todo = new Todo(Guid.NewGuid().ToString(), DateTime.Now, todoCreateModel.TaskDescription, false);
            items.Add(todo);

            logger.LogInformation($"Todo item with id {todo.Id} created");

            //does not calculate the route correctly so it is unusable now
            //return new CreatedAtRouteResult(new { id = todo.Id }, todo);
            
            return new OkObjectResult(todo);
        }


        [FunctionName("GetTodos")]
        public static IActionResult GetTodos(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo")] HttpRequest req,
            ILogger logger)
        {
            logger.LogInformation("Retrieving all the todo items");
            return new OkObjectResult(items);
        }

        [FunctionName("GetTodo")]
        public static IActionResult GetTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo/{id}")] HttpRequest req,
            ILogger log,
            string id)
        {
            log.LogInformation($"Retrieving a todo item with id {id}");
            var todo = items.FirstOrDefault(x => x.Id == id);

            return todo is null
                ? new NotFoundResult()
                : new OkObjectResult(todo);
        }

        [FunctionName("UpdateTodo")]
        public static async Task<IActionResult> UpdateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todo/{id}")] HttpRequest req,
            ILogger log,
            string id)
        {
            log.LogInformation($"Updating todo item with id {id}");

            var todo = items.FirstOrDefault(items => items.Id == id);

            if (todo is null)
            {
                return new NotFoundResult();
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var toDoUpdateModel = JsonConvert.DeserializeObject<ToDoUpdateModel>(requestBody);
            var taskDescription = !string.IsNullOrEmpty(toDoUpdateModel.TaskDescription)
                                        ? toDoUpdateModel.TaskDescription
                                        : todo.TaskDescription;

            var updatedTodo = todo with
            {
                TaskDescription = taskDescription,
                IsCompleted = toDoUpdateModel.IsCompleted
            };

            items.Remove(todo);
            items.Add(updatedTodo);

            return new OkObjectResult(updatedTodo);
        }


        [FunctionName("DeleteTodo")]
        public static IActionResult DeleteTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "todo/{id}")] HttpRequest req,
            ILogger logger,
            string id)
        {
            logger.LogInformation($"Deleting a record with id {id}");

            var todo = items.FirstOrDefault(items => items.Id == id);
            
            if (todo is null)
            {
                return new NotFoundResult();
            }

            items.Remove(todo);
            return new OkResult();
        }
    }


    record Todo(string Id, DateTime CreatedTime, string TaskDescription, bool IsCompleted);

    record ToDoCreateModel(string TaskDescription);

    record ToDoUpdateModel(string TaskDescription, bool IsCompleted);
}
