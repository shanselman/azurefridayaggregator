using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace AFAF
{
    public class DocsToDump
    {
        public static async Task DumpJsonFromDoc(Stream outputStream)
        {
            var urlMain = "/api/hierarchy/shows/azure-friday/episodes?page={0}&pageSize=30&orderBy=uploaddate%20desc";
            var urlBatch = "/api/video/public/v1/entries/batch?ids={0}";

            HttpClient client = new HttpClient();
            // Our "base" URL is Production
            client.BaseAddress = new Uri("https://docs.microsoft.com");
            int pageNumber = 0;
            int totalCount = 0;

            Dictionary<string, Episode> episodes = new Dictionary<string, Episode>();

            while (true)
            {
                string epUrl = String.Format(urlMain, pageNumber);
                string jsonString = await client.GetStringAsync(epUrl);
                Console.WriteLine($"Fetching {epUrl}");
                var jsonObject = JsonNode.Parse(jsonString);
                totalCount = (int)jsonObject["totalCount"].AsValue(); //don't need to do this twice

                JsonNode epNode = jsonObject["episodes"];
                if (epNode?.AsArray() != null && epNode.AsArray().Count == 0) break;

                StringBuilder batchEps = new StringBuilder();

                foreach (JsonObject item in epNode.AsArray())
                {
                    var ep = JsonSerializer.Deserialize<Episode>(item);

                    batchEps.Append(ep.entryId + ",");
                    episodes.Add(ep.entryId, ep);
                }

                string epDetailsUrl = String.Format(urlBatch, batchEps.ToString().TrimEnd(','));
                string jsonDetailsString = await client.GetStringAsync(epDetailsUrl);
                Console.WriteLine($"Fetching {epDetailsUrl}");

                var jsonDetailsObject = JsonNode.Parse(jsonDetailsString);
                foreach (JsonObject item in jsonDetailsObject.AsArray())
                {
                    var entry = item["entry"];
                    var publicVideo = entry["publicVideo"];
                    var thumbnailObj = publicVideo["thumbnailOtherSizes"];
                    var littleThumbnail = thumbnailObj["w800Url"].ToString();

                    string entryId = entry["id"].ToString();
                    string youTubeUrl = entry["youTubeUrl"].ToString();
                    episodes[entryId].thumbnailUrl = littleThumbnail;
                    episodes[entryId].youTubeUrl = youTubeUrl;
                }
                pageNumber++;
            }


            List<Episode> epList = episodes.Values.ToList<Episode>();

            //string dumpString = JsonSerializer.Serialize(epList, new JsonSerializerOptions() { WriteIndented = true });
            JsonSerializer.Serialize(outputStream, epList, new JsonSerializerOptions() { WriteIndented = true });
            //File.WriteAllText(), dumpString);
        }


    }


public record Episode
{
    public string title { get; init; }
    private string _url;
    public string url { get { return _url; } init { _url = "https://docs.microsoft.com" + value; } }
    public string description { get; init; }
    public string descriptionAsHtml { get { return Markdig.Markdown.ToHtml(description); } }
    public string entryId { get; init; }
    public DateTime uploadDate { get; init; }

    //from details
    public string youTubeUrl { get; set; }
    public string thumbnailUrl { get; set; }
}

}
