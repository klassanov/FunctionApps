using FunctionApp.Queue.RestAPI;

namespace FunctionApp.Queue.RestAPI.TableStorage
{
    public static class Mappings
    {
        public static ToDoTableEntity ToTableEntity(this Todo todo)
        {
            return new ToDoTableEntity()
            {
                PartitionKey = Constants.ToDoPartitionKey,
                RowKey = todo.Id,
                CreatedTime = todo.CreatedTime,
                Description = todo.TaskDescription,
                IsCompleted = todo.IsCompleted,
            };
        }

        public static Todo ToTodo(this ToDoTableEntity entity)
        {
            return new Todo(entity.RowKey, entity.CreatedTime, entity.Description, entity.IsCompleted);
        }
    }
}
