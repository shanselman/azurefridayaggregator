# Local Settings Configuration

## ⚠️ SECURITY WARNING

**NEVER commit `local.settings.json` with real secrets!**

## Setup Instructions

1. **Copy the sample file**:
   ```bash
   cp local.settings.sample.json local.settings.json
   ```

2. **Add your connection strings**: Replace placeholders with actual values
   - `AzureWebJobsStorage`: Your Azure Storage connection string

3. **Verify it's ignored**: The file `local.settings.json` is in `.gitignore` to prevent accidental commits

## Best Practices

- ✅ **Use environment variables** for local development when possible
- ✅ **Use Azure Key Vault** for production secrets
- ✅ **Use Managed Identity** to avoid connection strings altogether
- ❌ **Never commit** `local.settings.json` to version control
- ❌ **Never share** connection strings via email, Slack, or screenshots

## Production Configuration

In Azure, configure these settings in the Function App portal:

1. Go to **Configuration** → **Application settings**
2. Add your settings there instead of using connection strings
3. Use **Managed Identity** for Azure Storage access:
   ```bash
   az functionapp identity assign --name AzureFridayDocstoJSON --resource-group <your-rg>
   az role assignment create --assignee <principal-id> --role "Storage Blob Data Contributor" --scope <storage-account-resource-id>
   ```

## More Information

See [SECURITY.md](../../SECURITY.md) for complete security guidelines.
