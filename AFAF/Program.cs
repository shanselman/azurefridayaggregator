// See https://aka.ms/new-console-template for more information
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

var urlMain = "/api/hierarchy/shows/azure-friday/episodes?page={0}&pageSize=30&orderBy=uploaddate%20desc";
var urlBatch = "/api/video/public/v1/entries/batch?ids={0}";

HttpClient client = new HttpClient();
// Our "base" URL is Production
client.BaseAddress = new Uri("https://docs.microsoft.com");
int pageNumber = 0;
int totalCount = 0;

Dictionary<string, Episode> episodes =  new Dictionary<string, Episode>();


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
    batchEps.ToString().TrimEnd(','); //toss ending ,

    string epDetailsUrl = String.Format(urlBatch, batchEps);
    string jsonDetailsString = await client.GetStringAsync(epDetailsUrl);

    //get thumbnail and youtube url if they are there
    // go get each episode by guid and update with youtube and thumbnail


    pageNumber++;
}

Console.ReadLine();

//save a json file with all details (format=taco?)
// save to azure storage?




public record Episode
{
    public string title { get; init; }
    public string url { get; init; }
    public string description { get; init; }
    public string entryId { get; init; }
    public DateTime uploadDate { get; init; }

    //from details
    public string youTubeUrl { get; init; }
    public string thumbnailUrl { get; init; }
}

