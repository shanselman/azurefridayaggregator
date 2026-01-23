# Security Policy

## Reporting a Security Vulnerability

If you discover a security vulnerability in this project, please report it by emailing the maintainer or opening a private security advisory on GitHub.

## Leaked Azure Functions Key - Rotation Instructions

**⚠️ IMMEDIATE ACTION REQUIRED**: An Azure Functions key has been leaked and must be rotated immediately.

**Leaked Key**: `<REDACTED - See original incident report>`  
**Endpoint**: `https://azurefridaydocstojson.azurewebsites.net`

### Step 1: Rotate the Key in Azure Portal

1. **Navigate to Azure Portal**: Go to https://portal.azure.com
2. **Find your Function App**: Search for `AzureFridayDocstoJSON` (or your function app name)
3. **Access Function Keys**:
   - Go to **Settings** → **Configuration** → **Function keys** (or)
   - Go to **Functions** → Select function → **Function Keys**
4. **Regenerate the leaked key**:
   - Find the key named in the leak (e.g., `default`, `master`, or custom key name)
   - Click **Regenerate** or **Delete** the compromised key
   - If deleted, create a new key with a different name
5. **Update any applications** that use this key to use the new key value

### Step 2: Revoke Access Using the Old Key

After regenerating the key in Azure, the old compromised key will be immediately invalidated and cannot be used to access your Azure Functions.

### Step 3: Verify the Key is Rotated

Test that the old key no longer works:

```bash
# This should now return 401 Unauthorized
curl -s -H "x-functions-key: <OLD_COMPROMISED_KEY>" \
  "https://azurefridaydocstojson.azurewebsites.net/admin/host/status"
```

Test that the new key works:

```bash
# This should return 200 OK with your new key
curl -s -H "x-functions-key: <YOUR_NEW_KEY>" \
  "https://azurefridaydocstojson.azurewebsites.net/admin/host/status"
```

### Step 4: Update GitHub Secrets (if applicable)

If this key was used in GitHub Actions workflows or other CI/CD pipelines:

1. Go to your repository **Settings** → **Secrets and variables** → **Actions**
2. Update any secrets that contain the old function key
3. Re-run any failed workflows with the new key

### Step 5: Monitor for Unauthorized Access

1. Check Azure Function App logs for any unauthorized access attempts
2. Review Application Insights for suspicious activity
3. Set up alerts for failed authentication attempts

## Best Practices for Managing Secrets

### DO ✅

- **Use Azure Key Vault** for storing sensitive keys and connection strings
- **Use Managed Identities** when possible to avoid using keys altogether
- **Use GitHub Secrets** for storing keys needed in CI/CD pipelines
- **Rotate keys regularly** (every 90 days recommended)
- **Use different keys** for development, staging, and production environments
- **Limit key permissions** to the minimum required (use function-level keys instead of host keys when possible)
- **Enable secret scanning** in your repository (GitHub Advanced Security)

### DON'T ❌

- **Never commit secrets** to source control (even in private repositories)
- **Never hardcode API keys** in source code
- **Never share keys** in public forums, documentation, or screenshots
- **Never use the same key** across multiple environments
- **Never log secrets** in application logs or error messages
- **Never send secrets** via email, Slack, or other unsecured channels

## Enabling GitHub Secret Scanning

To prevent future secret leaks, enable GitHub's secret scanning feature:

### For Public Repositories (Free)

1. GitHub automatically scans public repositories for secrets
2. You'll receive alerts when secrets are detected
3. Enable push protection: **Settings** → **Code security and analysis** → **Push protection**

### For Private Repositories (Requires GitHub Advanced Security)

1. Go to **Settings** → **Code security and analysis**
2. Enable **Secret scanning**
3. Enable **Push protection** to prevent commits containing secrets
4. Optionally enable **Dependency review** and **Code scanning** with CodeQL

### Custom Secret Patterns (Advanced)

If you need to detect organization-specific secrets:

1. Go to **Organization Settings** → **Code security and analysis** → **Secret scanning**
2. Click **New custom pattern**
3. Define your pattern using regex (for Azure Functions keys, GitHub already has built-in patterns)

**Note**: GitHub natively detects Azure service credentials, including Function keys, so additional configuration is typically not needed.

## If You Accidentally Commit a Secret

If a secret is accidentally committed to the repository:

1. **Rotate the secret immediately** in Azure
2. **Remove the secret from git history** using:
   ```bash
   git filter-branch --force --index-filter \
     "git rm --cached --ignore-unmatch <file-with-secret>" \
     --prune-empty --tag-name-filter cat -- --all
   ```
   Or use [BFG Repo-Cleaner](https://rtyley.github.io/bfg-repo-cleaner/)
3. **Force push** to update remote repository
4. **Verify the secret is removed** from all branches and history

## Additional Resources

- [Azure Functions Security Best Practices](https://learn.microsoft.com/en-us/azure/azure-functions/security-concepts)
- [Azure Key Vault Documentation](https://learn.microsoft.com/en-us/azure/key-vault/)
- [GitHub Secret Scanning](https://docs.github.com/en/code-security/secret-scanning/about-secret-scanning)
- [Managing Secrets in Azure Functions](https://learn.microsoft.com/en-us/azure/azure-functions/functions-how-to-use-azure-function-app-settings)

## Contact

For security-related questions or concerns, please contact the repository maintainers.
