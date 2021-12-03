// See https://aka.ms/new-console-template for more information
using System.Text.Json;
using System.Text.Json.Nodes;

var urlMain = "/api/hierarchy/shows/azure-friday/episodes?page={0}&pageSize=30&orderBy=uploaddate%20desc";
var urlBatch = "/api/video/public/v1/entries/batch?ids={0}";

HttpClient client = new HttpClient();
// Our "base" URL is Production
client.BaseAddress = new Uri("https://docs.microsoft.com");
int pageNumber = 0;

string jsonString = await client.GetStringAsync(String.Format(urlMain, pageNumber));
var jsonObject = JsonNode.Parse(jsonString);
var totalCount = (int)jsonObject["totalCount"].AsValue();

foreach(JsonObject item in jsonObject["episodes"].AsArray())
{
    var show = JsonSerializer.Deserialize<Show>(item);
    Console.WriteLine(show.title);
}
Console.ReadLine();












public record Show
{
    public string title { get; init; }
    public string url { get; init; }
    public string description { get; init; }
    public string entryId { get; init; }
}


