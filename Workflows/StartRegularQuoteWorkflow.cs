using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Demo.QuoteRequestWorkflow.Workflows
{
    public static class StartRegularQuoteWorkflow
    {
        [FunctionName("StartRegularQuoteWorkflow")]
        public static async Task Run(
            [ActivityTrigger]string quoteId,
            [OrchestrationClient]DurableOrchestrationClient client,
            ILogger logger)
        {
            logger.LogInformation($"Starting StartRegularQuoteWorkflow for quote with id {quoteId}");

            await client.StartNewAsync(nameof(QuoteRegularWorkflow), quoteId);
        }
    }
}
