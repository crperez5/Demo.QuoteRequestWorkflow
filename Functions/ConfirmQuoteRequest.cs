using System.Threading.Tasks;
using Demo.QuoteRequestWorkflow.Master;
using Demo.QuoteRequestWorkflow.Quotes;
using Demo.QuoteRequestWorkflow.Workflows;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;

namespace Demo.QuoteRequestWorkflow.Functions
{
    public static class ConfirmQuoteRequest
    {
        [FunctionName("ConfirmQuoteRequest")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "confirm/{quoteId}")] HttpRequest req,
            string quoteId,
            [Table(nameof(Quote))]CloudTable quotes,
            [OrchestrationClient]DurableOrchestrationClient durableClient,
            ILogger log)
        {
            var quotesClient = quotes.AsClientFor<Quote>();
            var quote = new Quote { Id = quoteId };

            var quoteFromDb = await quotesClient.GetAsync(quote.PartitionKey, quote.RowKey);

            if (quoteFromDb == null)
            {
                log.LogError($"Quote with id {quoteId} not found.");
                return new NotFoundObjectResult("Quote not found.");
            }

            var instance = await durableClient.FindJob(nameof(QuoteRequestConfirmationWorkflow), quoteId);

            if (instance == null)
            {
                log.LogInformation($"{nameof(QuoteRequestConfirmationWorkflow)} not found for quote {quoteId}.");
                return new NotFoundResult();
            }

            log.LogInformation(
                $"{nameof(QuoteRequestConfirmationWorkflow)} with id {instance.InstanceId} found for quote {quoteId}.");

            await durableClient.RaiseEventAsync(instance.InstanceId, Constants.QUOTE_REQUEST_CONFIRMED, null);

            return new OkResult();
        }
    }
}
