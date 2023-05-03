using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace FunctionApp.TableStorage.RestAPI.TableStorage
{
    // Need to inherit TableEntity
    public class ToDoTableEntity : TableEntity
    {
        public DateTime CreatedTime { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
    }
}
