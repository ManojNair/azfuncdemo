using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Company.FunctionApp
{
    public static class ProcessOrder
    {
        [FunctionName("ProcessOrder")]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequest req,
            [Queue("orders")] IAsyncCollector<Order> orderQueue,
            [Table("OrdersTable")] IAsyncCollector<TableOrder> tableStorage,
            ILogger log)
        {
            log.LogInformation("Executing ProcessOrder");
            var inputString = await new StreamReader(req.Body).ReadToEndAsync();
            log.LogInformation(inputString);

            var order = JsonConvert.DeserializeObject<Order>(inputString);
            var orderToReturn = new {order, Status = "Processing"};
            await orderQueue.AddAsync(order);

            var orderToInsert = new TableOrder()
            {
                PartitionKey = "orders",
                RowKey = Guid.NewGuid().ToString(),
                OrderId = order.OrderId,
                Description = order.Description,
                Amount = order.Amount
            };
            await tableStorage.AddAsync(orderToInsert);


            return new JsonResult(orderToReturn, new JsonSerializerSettings() {Formatting = Formatting.Indented});
        }
    }

    public class Order
    {
        public int OrderId { get; set; }
        public string Description { get; set; }
        public double Amount { get; set; }
    }

    public class TableOrder
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public int OrderId { get; set; }
        public string Description { get; set; }
        public double Amount { get; set; }
    }
}