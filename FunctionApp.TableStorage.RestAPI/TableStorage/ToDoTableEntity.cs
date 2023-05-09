using System;
using Azure;
using Azure.Data.Tables;

namespace FunctionApp.TableStorage.RestAPI.TableStorage
{
    // Need to inherit TableEntity
    public class ToDoTableEntity : ITableEntity
    {
        public DateTime CreatedTime { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }

        public string PartitionKey { get; set; }

        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }

        public ETag ETag { get; set; }
    }
}
