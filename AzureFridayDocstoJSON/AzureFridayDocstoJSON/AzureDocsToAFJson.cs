using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AzureFridayDocstoJSON
{
    public class AzureDocsToAFJson
    {
        [FunctionName("AzureDocsToAFJson")]
        public async Task RunJsonAsync(
            [TimerTrigger("0 10 * * *", RunOnStartup = true)] TimerInfo myTimer,
            ILogger log,
            [Blob("output//azurefriday.json", FileAccess.ReadWrite)] BlockBlobClient blobClient)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            Stream dump = new MemoryStream();
                await AFAF.DocsToDump.DumpDoc(dump, AFAF.Format.Json);
            dump.Position = 0;

            await blobClient.UploadAsync(dump, new BlobHttpHeaders { ContentType = "application/json" });
        }

        [FunctionName("AzureDocsToAFRss")]
        public async Task RunRssAsync(
            [TimerTrigger("0 11 * * *", RunOnStartup = true)] TimerInfo myTimer,
            ILogger log,
            [Blob("output//azurefriday.rss", FileAccess.ReadWrite)] BlockBlobClient blobClient)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            Stream dump = new MemoryStream();
            await AFAF.DocsToDump.DumpDoc(dump, AFAF.Format.Rss);
            dump.Position = 0;

            await blobClient.UploadAsync(dump, new BlobHttpHeaders { ContentType = "application/rss+xml" });
        }

    }
}
