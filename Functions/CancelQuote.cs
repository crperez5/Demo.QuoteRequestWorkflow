using System;
using System.Threading.Tasks;
using Demo.QuoteRequestWorkflow.Master;
using Demo.QuoteRequestWorkflow.Quotes;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;

namespace Demo.QuoteRequestWorkflow.Functions
{
    public static class CancelQuote
    {
        [FunctionName("CancelQuote")]
        public static async Task<bool> Run(
            [ActivityTrigger]Quote quote,
            [Table(nameof(Quote))]CloudTable table,
            ILogger logger)
        {
            logger.LogInformation($"Cancel Quote with id {quote.Id}");

            var client = table.AsClientFor<Quote>();

            var quoteFromDb = await client.GetAsync(quote.PartitionKey, quote.RowKey);

            if (quoteFromDb == null)
            {
                throw new Exception($"CancelQuote: Quote {quote.Id} not found!");
            }

            quoteFromDb.Status = Status.Cancelled;
            await client.ReplaceAsync(quoteFromDb);

            logger.LogInformation($"Quote with id {quote.Id} cancelled successful");
            return true;
        }
    }
}
