# Azure Friday Aggregator

An automated tool that aggregates Azure Friday show episodes from Microsoft Docs and generates podcast-compatible RSS feeds and JSON exports.

## What It Does

This project fetches episode metadata from the Microsoft Docs API for the [Azure Friday](https://docs.microsoft.com/en-us/shows/azure-friday/) show hosted by Scott Hanselman. It then:

1. **Fetches episode data** from the Microsoft Docs Hierarchy API (paginated, 30 episodes per page)
2. **Enriches episodes** with video/audio URLs, thumbnails, captions, and YouTube links via the Docs Video API
3. **Generates outputs**:
   - `azurefriday.json` - JSON dump of all episodes
   - `azurefriday.rss` - Video podcast RSS feed (iTunes/Google Play compatible)
   - `azurefridayaudio.rss` - Audio-only podcast RSS feed

The outputs are uploaded to Azure Blob Storage and available at:
- https://hanselstorage.blob.core.windows.net/output/azurefriday.json

## Project Structure

```
azurefridayaggregator/
├── AFAF/                              # Console app for local testing
│   ├── Program.cs                     # Entry point - dumps to local files
│   └── AFAF.csproj                    # References the shared library
│
├── AzureFridayDocstoJSON/
│   └── AzureFridayDocstoJSON/         # Azure Function project
│       ├── AzureDocsToAFJson.cs       # Timer-triggered function (runs daily at 10:05 AM UTC)
│       ├── DocsToDump.cs              # Core logic - API calls & feed generation
│       ├── Program.cs                 # Azure Functions worker host
│       └── AzureFridayDocstoJSON.csproj
│
└── .github/workflows/
    └── AzureFridayDocstoJSON.yml      # CI/CD - deploys to Azure Functions on push to master
```

## How It Works

### Data Flow

```
Microsoft Docs API ──► Fetch Episodes ──► Enrich with Media URLs ──► Generate Outputs
      │                                                                     │
      │  /api/hierarchy/shows/azure-friday/episodes                         ├─► JSON
      │  /api/video/public/v1/entries/batch                                 ├─► RSS (Video)
      └────────────────────────────────────────────────────────────────────►└─► RSS (Audio)
```

### API Endpoints Used

| Endpoint | Purpose |
|----------|---------|
| `https://docs.microsoft.com/api/hierarchy/shows/azure-friday/episodes?page={n}&pageSize=30` | Get episode list (title, description, URL, entry ID) |
| `https://docs.microsoft.com/api/video/public/v1/entries/batch?ids={ids}` | Batch fetch video details (thumbnail, audio/video URLs, captions) |

### Episode Data Collected

- `title`, `description`, `url`, `uploadDate`
- `thumbnailUrl` (800px wide)
- `youTubeUrl`
- `audioUrl`, `lowQualityVideoUrl`, `mediumQualityVideoUrl`, `highQualityVideoUrl`
- `captionsUrlEnUs`, `captionsUrlZhCn`

## Running Locally

### Prerequisites

- .NET 8.0 SDK
- Azure Storage account (for Azure Function) or local files (for console app)

### Console App (AFAF)

```bash
cd AFAF
dotnet run
```

This generates three local files:
- `dump.json`
- `dump.xml` (video RSS)
- `dumpaudio.xml` (audio RSS)

### Azure Function (Local)

1. Copy `local.settings.sample.json` to `local.settings.json`
2. Add your Azure Storage connection string (see [LOCAL_SETTINGS.md](AzureFridayDocstoJSON/AzureFridayDocstoJSON/LOCAL_SETTINGS.md) for details)
3. Run with Azure Functions Core Tools:

```bash
cd AzureFridayDocstoJSON/AzureFridayDocstoJSON
func start
```

⚠️ **Security Note**: Never commit `local.settings.json` - it's in `.gitignore` to protect your secrets.

## Deployment

The Azure Function deploys automatically via GitHub Actions when pushing to `master`. It:

1. Builds the .NET 8 project
2. Publishes to Azure Function App `AzureFridayDocstoJSON`
3. Runs on a timer schedule: `5 10 * * *` (daily at 10:05 AM UTC)

### Required Secrets

- `AzureFridayDocstoJSON_FFFF` - Azure Function publish profile

## RSS Feed Features

The generated RSS feeds are compatible with:
- Apple Podcasts (iTunes)
- Google Podcasts
- Podtrac analytics (URLs wrapped with `dts.podtrac.com/redirect.mp3/`)

Feed metadata includes:
- Show: **Azure Friday**
- Authors: Scott Hanselman, Rob Caron
- Category: Technology
- Language: en-us

## Dependencies

| Package | Purpose |
|---------|---------|
| `Markdig` | Convert Markdown descriptions to HTML/plain text |
| `System.ServiceModel.Syndication` | Generate RSS 2.0 feeds |
| `Microsoft.Azure.Functions.Worker.*` | Azure Functions isolated worker model |
| `Azure.Storage.Blobs` | Upload to Azure Blob Storage |

## Security

⚠️ **Important**: This project uses Azure Functions with API keys for authentication. 

### Key Management Best Practices

- **Never commit API keys or secrets** to the repository
- **Use GitHub Secrets** for storing sensitive values needed in CI/CD
- **Rotate keys regularly** (recommended every 90 days)
- **Enable secret scanning** in repository settings
- **Use Azure Key Vault** for production secrets when possible

If you accidentally leak a key, **immediately**:
1. Rotate the key in Azure Portal (Function App → Settings → Function Keys)
2. Update any services using the old key
3. Review the [SECURITY.md](SECURITY.md) file for detailed instructions

See [SECURITY.md](SECURITY.md) for complete security guidelines and key rotation procedures.

## Notes

- The Microsoft Docs API has a page size limit of 30 episodes
- Old Channel 9 URLs redirect to the new Docs video player
- Some legacy video embeds may be broken (see `broken stuff.txt`)
