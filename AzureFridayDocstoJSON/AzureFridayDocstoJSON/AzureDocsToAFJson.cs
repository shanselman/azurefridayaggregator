using AFAF;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;

namespace AzureFridayDocstoJSON
{
    public class AzureDocsToAFJson
    {
        [Function("AzureDocsToPodcastRSS")]
        public async Task RunJsonAsync(
            [TimerTrigger("5 10 * * *", RunOnStartup = true)] TimerInfo myTimer,
            ILogger log,
            [BlobInput("output//azurefriday.json")] BlockBlobClient blobJsonClient,
            [BlobInput("output//azurefriday.rss")] BlockBlobClient blobRssClient,
            [BlobInput("output//azurefridayaudio.rss")] BlockBlobClient blobRssAudioClient
            )
        {
            List<Episode> episodes = await DocsToDump.GetEpisodeList();

            Stream dumpJson = new MemoryStream();
                await AFAF.DocsToDump.DumpDoc(dumpJson, episodes, AFAF.Format.Json);
            dumpJson.Position = 0;
            await blobJsonClient.UploadAsync(dumpJson, new BlobHttpHeaders { ContentType = "application/json" });

            Stream dumpRss = new MemoryStream();
                await AFAF.DocsToDump.DumpDoc(dumpRss, episodes, AFAF.Format.Rss);
            dumpRss.Position = 0;
            await blobRssClient.UploadAsync(dumpRss, new BlobHttpHeaders { ContentType = "application/rss+xml" });

            Stream dumpRssAudio = new MemoryStream();
                await AFAF.DocsToDump.DumpDoc(dumpRssAudio, episodes, AFAF.Format.RssAudio);
            dumpRssAudio.Position = 0;
            await blobRssAudioClient.UploadAsync(dumpRssAudio, new BlobHttpHeaders { ContentType = "application/rss+xml" });
        }
    }
}
