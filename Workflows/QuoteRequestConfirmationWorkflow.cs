using System.Threading;
using System.Threading.Tasks;
using Demo.QuoteRequestWorkflow.Functions;
using Demo.QuoteRequestWorkflow.Master;
using Demo.QuoteRequestWorkflow.Quotes;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Demo.QuoteRequestWorkflow.Workflows
{
    public static class QuoteRequestConfirmationWorkflow
    {
        [FunctionName("QuoteRequestConfirmationWorkflow")]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context,
            ILogger logger)
        {
            var quote = context.GetInput<Quote>();
            logger.LogInformation($"Start of QuoteRequestConfirmationWorkflow with due date set to {quote.DueDate}");

            using (var timeoutCts = new CancellationTokenSource())
            {
                var quoteRequestConfirmedEvent = context.WaitForExternalEvent(Constants.QUOTE_REQUEST_CONFIRMED);

                var durableTimeout = context.CreateTimer(quote.DueDate, timeoutCts.Token);

                var winner = await Task.WhenAny(quoteRequestConfirmedEvent, durableTimeout);

                if (winner == quoteRequestConfirmedEvent)
                {
                    timeoutCts.Cancel();
                    logger.LogInformation($"QuoteRequest with id {quote.Id} confirmed.");

                    await context.CallActivityAsync(nameof(StartRegularQuoteWorkflow), quote.Id);
                }
                else
                {
                    logger.LogInformation("QuoteRequest confirmation timed out.");

                    await context.CallActivityAsync(nameof(CancelQuote), quote);
                }
            }
        }
    }
}