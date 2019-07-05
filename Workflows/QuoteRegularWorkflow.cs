using System.Threading.Tasks;
using Demo.QuoteRequestWorkflow.Master;
using Demo.QuoteRequestWorkflow.Quotes;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;

namespace Demo.QuoteRequestWorkflow.Workflows
{
    public static class QuoteRegularWorkflow
    {
        [FunctionName("QuoteRegularWorkflow")]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context,
            ILogger logger)
        {
            var quoteId = context.GetInput<string>();

            logger.LogInformation($"Start of QuoteRegularWorkflow for quote with id {quoteId}");

            await context.CallActivityAsync(nameof(UpdateQuoteStatus), (quoteId, Status.InProgress));
            await context.CallActivityAsync(nameof(UpdateQuoteStatus), (quoteId, Status.Delivered));
        }

        [FunctionName(nameof(UpdateQuoteStatus))]
        public static async Task UpdateQuoteStatus(
            [ActivityTrigger](string quoteId, Status quoteStatus) payload,
            [Table(nameof(Quote))]CloudTable quoteTable,
            ILogger logger)
        {
            // Simulating time
            await Task.Delay(2000);

            var quoteClient = quoteTable.AsClientFor<Quote>();
            var quoteKey = new Quote { Id = payload.quoteId };
            var quote = await quoteClient.GetAsync(quoteKey.PartitionKey, quoteKey.RowKey);
            quote.Status = payload.quoteStatus;
            await quoteClient.ReplaceAsync(quote);
        }
    }
}