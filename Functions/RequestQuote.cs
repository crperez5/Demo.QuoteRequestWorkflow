using System;
using System.IO;
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
using Newtonsoft.Json;

namespace Demo.QuoteRequestWorkflow.Functions
{
    public static class RequestQuote
    {
        [FunctionName("RequestQuote")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "request")] HttpRequest req,
            [Table(nameof(Quote))]CloudTable quotes,
            [OrchestrationClient]DurableOrchestrationClient starter,
            ILogger log)
        {
            log.LogInformation("A new quote has been requested.");


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string description = data?.description;

            if (string.IsNullOrEmpty(description))
            {
                log.LogError("A new quote has been requested but no description provided.");
                return new BadRequestObjectResult("Description required.");
            }

            DateTime dueDate = data?.dueDate;
            if (dueDate < DateTime.UtcNow)
            {
                log.LogError("A new quote has been requested but due time is in the past.");
                return new BadRequestObjectResult("Due time must be set to future time.");
            }

            var quotesClient = quotes.AsClientFor<Quote>();

            var newQuote = new Quote
            {
                Description = description,
                DueDate = dueDate,
                Id = Guid.NewGuid().ToString(),
                Status = Status.AwaitingReview,
            };

            await quotesClient.InsertAsync(newQuote);

            log.LogInformation($"A new quote has been created with id {newQuote.Id}");

            var instanceId = await starter.StartNewAsync(nameof(QuoteRequestConfirmationWorkflow), newQuote);

            log.LogInformation($"Started {nameof(QuoteRequestConfirmationWorkflow)} orchestration with ID = '{instanceId}'.");

            return new CreatedResult($"http://localhost:7071/api/quotes/{newQuote.Id}", newQuote);
        }
    }
}
