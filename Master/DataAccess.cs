using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace Demo.QuoteRequestWorkflow.Master
{
    public class DataAccess<T> where T : TableEntity, new()
    {
        private CloudTable _tableClient;

        public DataAccess(CloudTable ct)
        {
            _tableClient = ct;
        }

        public async Task InsertAsync(T item)
        {
            var operation = TableOperation.Insert(item);
            await _tableClient.ExecuteAsync(operation);
        }

        public async Task InsertManyAsync(T[] items)
        {
            var operation = new TableBatchOperation();
            foreach (var item in items)
            {
                operation.Insert(item);
            }
            await _tableClient.ExecuteBatchAsync(operation);
        }

        public async Task<T> GetAsync(string partition, string key)
        {
            var operation = TableOperation.Retrieve<T>(partition, key);
            var result = await _tableClient.ExecuteAsync(operation);
            var item = result.Result as T;
            return item;
        }

        public async Task ReplaceAsync(T item)
        {
            var operation = TableOperation.Replace(item);
            await _tableClient.ExecuteAsync(operation);
        }

        public async Task<T[]> GetAllAsync(string partition)
        {
            var query = new TableQuery<T>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey",
                    QueryComparisons.Equal, partition));
            var result = await _tableClient.ExecuteQuerySegmentedAsync(query, null);
            var list = result.Results.ToArray();
            
            return list;
        }
    }
}
