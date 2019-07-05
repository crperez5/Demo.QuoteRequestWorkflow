using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Demo.QuoteRequestWorkflow.Quotes;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Table;

namespace Demo.QuoteRequestWorkflow.Master
{
    public static class Extensions
    {
        public static DataAccess<T> AsClientFor<T>(this CloudTable table)
            where T : TableEntity, new()
        {
            return new DataAccess<T>(table);
        }

        public static async Task<DurableOrchestrationStatus> FindJob(
            this DurableOrchestrationClient client,
            string workflowName,
            string quoteId)
        {
            var filter = new List<OrchestrationRuntimeStatus> {OrchestrationRuntimeStatus.Running};

            var instances = await client.GetStatusAsync(
                DateTime.MinValue,
                DateTime.UtcNow, 
                filter);

            foreach (var instance in instances)
            {
                if (instance.Input.ToObject<Quote>().Id == quoteId &&
                    instance.Name == workflowName)
                {
                    return instance;
                }
            }
            return null;
        }
    }
}
