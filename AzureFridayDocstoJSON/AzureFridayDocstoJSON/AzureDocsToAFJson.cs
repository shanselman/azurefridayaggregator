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
            [TimerTrigger("0 3 * * *", RunOnStartup = true)]TimerInfo myTimer,
            ILogger log,
            [Blob("output//azurefriday.json", FileAccess.Write)] Stream dumpJson)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            await AFAF.DocsToDump.DumpJsonFromDoc(dumpJson);
        }
    }
}
