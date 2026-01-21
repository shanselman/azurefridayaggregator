using AFAF;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AzureFridayDocstoJSON
{
    public class AzureDocsToAFJson
    {
        private readonly ILogger<AzureDocsToAFJson> _logger;

        public AzureDocsToAFJson(ILogger<AzureDocsToAFJson> logger)
        {
            _logger = logger;
        }

        [Function("AzureDocsToPodcastRSS")]
        public async Task RunJsonAsync(
            [TimerTrigger("5 10 * * *", RunOnStartup = true)] TimerInfo myTimer,
            [BlobInput("output//azurefriday.json")] BlockBlobClient blobJsonClient,
            [BlobInput("output//azurefriday.rss")] BlockBlobClient blobRssClient,
            [BlobInput("output//azurefridayaudio.rss")] BlockBlobClient blobRssAudioClient
            )
        {
            _logger.LogInformation("AzureDocsToPodcastRSS timer trigger fired at: {time}", DateTime.UtcNow);
            await RunCoreAsync(blobJsonClient, blobRssClient, blobRssAudioClient);
        }

        [Function("AzureDocsToPodcastRSSHttp")]
        public async Task<string> RunHttpAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req,
            [BlobInput("output//azurefriday.json")] BlockBlobClient blobJsonClient,
            [BlobInput("output//azurefriday.rss")] BlockBlobClient blobRssClient,
            [BlobInput("output//azurefridayaudio.rss")] BlockBlobClient blobRssAudioClient
            )
        {
            _logger.LogInformation("AzureDocsToPodcastRSS HTTP trigger fired at: {time}", DateTime.UtcNow);
            await RunCoreAsync(blobJsonClient, blobRssClient, blobRssAudioClient);
            return "Done!";
        }

        private async Task RunCoreAsync(BlockBlobClient blobJsonClient, BlockBlobClient blobRssClient, BlockBlobClient blobRssAudioClient)
        {
            _logger.LogInformation("Fetching episode list...");
            List<Episode> episodes = await DocsToDump.GetEpisodeList();
            _logger.LogInformation("Got {count} episodes", episodes.Count);

            _logger.LogInformation("Uploading JSON...");
            Stream dumpJson = new MemoryStream();
                await AFAF.DocsToDump.DumpDoc(dumpJson, episodes, AFAF.Format.Json);
            dumpJson.Position = 0;
            await blobJsonClient.UploadAsync(dumpJson, new BlobHttpHeaders { ContentType = "application/json" });

            _logger.LogInformation("Uploading RSS...");
            Stream dumpRss = new MemoryStream();
                await AFAF.DocsToDump.DumpDoc(dumpRss, episodes, AFAF.Format.Rss);
            dumpRss.Position = 0;
            await blobRssClient.UploadAsync(dumpRss, new BlobHttpHeaders { ContentType = "application/rss+xml" });

            _logger.LogInformation("Uploading RSS Audio...");
            Stream dumpRssAudio = new MemoryStream();
                await AFAF.DocsToDump.DumpDoc(dumpRssAudio, episodes, AFAF.Format.RssAudio);
            dumpRssAudio.Position = 0;
            await blobRssAudioClient.UploadAsync(dumpRssAudio, new BlobHttpHeaders { ContentType = "application/rss+xml" });
            
            _logger.LogInformation("AzureDocsToPodcastRSS completed successfully!");
        }
    }
}
