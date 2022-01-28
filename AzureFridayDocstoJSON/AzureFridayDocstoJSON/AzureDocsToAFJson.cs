using AFAF;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AzureFridayDocstoJSON
{
    public class AzureDocsToAFJson
    {
        [FunctionName("AzureDocsToPodcastRSS")]
        public async Task RunJsonAsync(
            [TimerTrigger("5 10 * * *", RunOnStartup = false)] TimerInfo myTimer,
            ILogger log,
            [Blob("output//azurefriday.json", FileAccess.ReadWrite)] BlockBlobClient blobJsonClient,
            [Blob("output//azurefriday.rss", FileAccess.ReadWrite)] BlockBlobClient blobRssClient,
            [Blob("output//azurefridayaudio.rss", FileAccess.ReadWrite)] BlockBlobClient blobRssAudioClient
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
