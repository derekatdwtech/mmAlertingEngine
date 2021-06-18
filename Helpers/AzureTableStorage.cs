using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace tempaast.alerting.helpers
{
    public class AzureTableStorage<T> where T : ITableEntity, new()
{
    private readonly CloudStorageAccount storageAccount;
    private readonly CloudTableClient tableClient;
    private readonly CloudTable table;

    public AzureTableStorage(string connectionString, string tableName)
    {
        // Retrieve the storage account from the connection string.
        storageAccount = CloudStorageAccount.Parse(connectionString);

        // Create the table client.
        tableClient = storageAccount.CreateCloudTableClient();

        // Retrieve a reference to the table.
        table = tableClient.GetTableReference(tableName);

        // Create the table if it doesn't exist.
        table.CreateIfNotExistsAsync().GetAwaiter().GetResult();
    }

    public async Task<T> Get(string partitionKey, string rowKey)
    {

        TableQuery<T> query = new TableQuery<T>().Where(
        TableQuery.CombineFilters(
            TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey),
            TableOperators.And,
            TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey))).Take(1);

        var results = new List<T>();

        TableContinuationToken continuationToken = null;
        do
        {
            var response = await table.ExecuteQuerySegmentedAsync(query, continuationToken);
            continuationToken = response.ContinuationToken;
            results.AddRange(response.Results);
        } while (continuationToken != null);

        return results.ToArray()[0];
    }

    public async Task<T> GetLatest() {
        DateTime now = DateTime.Now;

        TableQuery<T> query = new TableQuery<T>().Where(
            TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual, now.ToString())
        );

        var results = new List<T>();
        TableContinuationToken continuationToken = null;
        do {
            var response = await table.ExecuteQuerySegmentedAsync(query, continuationToken);
            continuationToken = response.ContinuationToken;
            results.AddRange(response.Results);
        } while(continuationToken != null);

        return results.ToArray()[0];
    }

    public async Task<List<T>> GetMany(TableQuery<T> query)
    {
        var results = new List<T>();

        TableContinuationToken continuationToken = null;
        do
        {
            var response = await table.ExecuteQuerySegmentedAsync(query, continuationToken);
            continuationToken = response.ContinuationToken;
            results.AddRange(response.Results);
        } while (continuationToken != null);

        return results;
    }

    public async Task<T> InsertOrUpdateAsync(T entity)
    {
        //Console.WriteLine($"{JsonConvert.SerializeObject(entity)}");
        var insertOrReplaceOperation = TableOperation.Insert(entity);
        var result = await table.ExecuteAsync(insertOrReplaceOperation);

        if (result == null || result.Result == null)
            return default(T);

        return (T)result.Result;
    }

    public async Task<T> Delete(T entity)
    {
        var deleteOperation = TableOperation.Delete(entity);
        var result = await table.ExecuteAsync(deleteOperation);
        return (T)result.Result;
    }

    public void BatchInsert(TableBatchOperation batchOperation)
    {
        table.ExecuteBatchAsync(batchOperation).GetAwaiter().GetResult();
    }
}
}