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
        public async Task RunAsync(
            [TimerTrigger("0 3 * * *", RunOnStartup = true)] TimerInfo myTimer,
            ILogger log,
            [Blob("output//azurefriday.json", FileAccess.ReadWrite)] BlockBlobClient blobClient)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            Stream dumpJson = new MemoryStream();
            await AFAF.DocsToDump.DumpJsonFromDoc(dumpJson);
            dumpJson.Position = 0;
            await blobClient.UploadAsync(dumpJson, new BlobHttpHeaders { ContentType = "application/json" });
        }
    }
}
