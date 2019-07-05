using System;
using System.Threading.Tasks;
using Demo.QuoteRequestWorkflow.Master;
using Demo.QuoteRequestWorkflow.Quotes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;

namespace Demo.QuoteRequestWorkflow.Functions
{
    public class GetQuote
    {
        [FunctionName("GetQuote")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "quotes/{quoteId}")] HttpRequest req,
            string quoteId,
            [Table(nameof(Quote))]CloudTable table,
            ILogger logger)
        {
            var quote = new Quote() {Id = quoteId};
            var client = table.AsClientFor<Quote>();

            var quoteFromDb = await client.GetAsync(quote.PartitionKey, quote.RowKey);

            if (quoteFromDb == null)
            {
                throw new Exception($"CancelQuote: Quote {quote.Id} not found!");
            }

            return new OkObjectResult(quoteFromDb);
        }
    }
}
