using System.Threading.Tasks;
using System;
using System.IO;
using System.Text;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Company.FunctionApp
{
    public static class OnOrderRequestQueue
    {
        [FunctionName("OnOrderRequestQueue")]
        public static async Task RunAsync([QueueTrigger("orders", Connection = "AzureWebJobsStorage")]
            string myQueueItem,
            IBinder binder,
            ILogger log)
        {
            log.LogInformation(JsonConvert.SerializeObject(myQueueItem));

            var outputBlob =
                await binder.BindAsync<Stream>(new BlobAttribute($"ordersprocessed/{Environment.MachineName}.json",
                    FileAccess.Write));
            await outputBlob.WriteAsync(Encoding.UTF8.GetBytes(myQueueItem));
        }
    }
}