# Azure Friday Aggregator

The data pipeline behind [azurefriday.com](https://azurefriday.com). This Azure Function fetches all Azure Friday episode data from the Microsoft Learn API, generates podcast-compatible RSS feeds and a JSON export, and uploads them to Azure Blob Storage.

## How It Works

```
Microsoft Learn API                    Azure Function                      Blob Storage
──────────────────                    ──────────────                      ────────────
                                      Every 6 hours (+ HTTP trigger):
/api/hierarchy/shows/                                                     hanselstorage/output/
  azure-friday/episodes  ──────►  1. Paginate through all episodes  ──►  azurefriday.json
  (30 per page)                   2. Batch-fetch video details           azurefriday.rss
                                  3. Generate JSON + 2 RSS feeds         azurefridayaudio.rss
/api/video/public/v1/             4. Upload to blob storage
  entries/batch          ──────►     with Cache-Control headers
  (thumbnails, media URLs,
   captions, YouTube links)          Validation: refuses to upload
                                     if < 100 episodes returned
```

The [azure-friday](https://github.com/shanselman/azure-friday) web app reads `azurefriday.json` from blob storage. Podcast clients (Apple Podcasts, etc.) read the RSS feeds directly.

## Project Structure

```
azurefridayaggregator/
├── AFAF/                              # Console app for local testing
│   ├── Program.cs                     # Dumps to local files (dump.json, dump.xml, dumpaudio.xml)
│   └── AFAF.csproj
│
├── AzureFridayDocstoJSON/
│   └── AzureFridayDocstoJSON/         # Azure Function project (.NET 8, isolated worker v4)
│       ├── AzureDocsToAFJson.cs       # Function triggers (timer + HTTP) and blob upload
│       ├── DocsToDump.cs              # Core logic: API fetching, RSS generation, Episode model
│       ├── Program.cs                 # Azure Functions host
│       └── host.json
│
├── .github/workflows/
│   └── AzureFridayDocstoJSON.yml      # CI/CD: builds and deploys on push to master
│
├── azure friday conversion.txt        # API documentation and notes
└── broken stuff.txt                   # Known issues with legacy Channel 9 URLs
```

## Episode Data

Each episode contains:

| Field | Source | Example |
|-------|--------|---------|
| `title` | Hierarchy API | "Orchestrate your Agents with Microsoft Agent Framework" |
| `url` | Hierarchy API | `https://learn.microsoft.com/shows/azure-friday/...` |
| `description` | Hierarchy API | Markdown text (also rendered as HTML and plain text via Markdig) |
| `uploadDate` | Hierarchy API | `2026-02-03T17:42:48Z` |
| `entryId` | Hierarchy API | `b64fc5c9-b75e-47a0-b21f-68db6d977dac` |
| `thumbnailUrl` | Video Batch API | 800px wide JPG |
| `youTubeUrl` | Video Batch API | YouTube watch link |
| `audioUrl` | Video Batch API | MP4 audio file |
| `lowQualityVideoUrl` | Video Batch API | 640x360 MP4 |
| `mediumQualityVideoUrl` | Video Batch API | 1280x720 MP4 |
| `highQualityVideoUrl` | Video Batch API | 1920x1080 MP4 |
| `captionsUrlEnUs` | Video Batch API | VTT caption file (English) |
| `captionsUrlZhCn` | Video Batch API | VTT caption file (Chinese) |

## API Endpoints Used

| Endpoint | Purpose |
|----------|---------|
| `https://learn.microsoft.com/api/hierarchy/shows/azure-friday/episodes?page={n}&pageSize=30&orderBy=uploaddate%20desc` | Get episode list (paginated, 30 per page) |
| `https://learn.microsoft.com/api/video/public/v1/entries/batch?ids={id1},{id2},...` | Batch fetch video details (media URLs, thumbnails, captions) |

## Reliability Features

- **Minimum episode guard**: Refuses to upload if fewer than 100 episodes returned (protects against API outages overwriting good data with empty data)
- **Per-episode error handling**: Bad entries in the video batch API are logged and skipped, not fatal
- **Per-blob error handling**: Each upload (JSON, RSS, RSS Audio) is independent — if one fails, the others still upload. All failures are thrown as an AggregateException
- **Null-safe JSON parsing**: Missing thumbnails, captions, or YouTube URLs produce empty strings, not crashes
- **HTTP timeout**: 30-second timeout per API request
- **Duplicate protection**: Duplicate `entryId` values are logged and skipped via `TryAdd`
- **Structured logging**: All operations logged via `ILogger` for Application Insights visibility
- **Cache-Control headers**: All uploaded blobs include `public, max-age=300, must-revalidate`

## RSS Feed Features

Compatible with:
- **Apple Podcasts** (iTunes namespace: `itunes:author`, `itunes:image`, `itunes:category`, etc.)
- **Google Podcasts** (Google Play namespace)
- **Podtrac analytics** (media URLs wrapped with `dts.podtrac.com/redirect.mp3/`)

Feed metadata:
- Show: **Azure Friday** (video) / **Azure Friday (Audio)** (audio-only)
- Authors: Scott Hanselman, Rob Caron
- Category: Technology
- Language: en-us
- Type: episodic

## Running Locally

### Prerequisites

- .NET 8.0 SDK (or later)

### Console App (quick test, no Azure needed)

```bash
cd AFAF
dotnet run
```

Generates three local files:
- `dump.json` (~1.8 MB, 502 episodes)
- `dump.xml` (video RSS, ~1.1 MB)
- `dumpaudio.xml` (audio RSS, ~1.1 MB)

### Azure Function (local)

1. Copy `local.settings.sample.json` to `local.settings.json`
2. Add your Azure Storage connection string
3. Run with Azure Functions Core Tools:

```bash
cd AzureFridayDocstoJSON/AzureFridayDocstoJSON
func start
```

The timer trigger runs on startup. There's also an HTTP trigger at `/api/AzureDocsToPodcastRSSHttp` for manual invocation.

## Deployment

Automatically deployed via GitHub Actions on push to `master`:

1. Builds the .NET 8 project
2. Publishes to Azure Function App `AzureFridayDocstoJSON`
3. Runs on a timer: `0 0 */6 * * *` (every 6 hours)

### Required Secrets

- `AzureFridayDocstoJSON_FFFF` — Azure Function publish profile

## Dependencies

| Package | Purpose |
|---------|---------|
| `Markdig` | Convert Markdown episode descriptions to HTML and plain text |
| `System.ServiceModel.Syndication` | Generate RSS 2.0 feeds with iTunes/Google Play extensions |
| `Microsoft.Azure.Functions.Worker.*` | Azure Functions isolated worker model (v4) |
| `Azure.Storage.Blobs` | Upload generated files to Azure Blob Storage |
| `Microsoft.ApplicationInsights.WorkerService` | Structured logging and monitoring |
