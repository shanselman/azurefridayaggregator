# Key Rotation Incident Response - Azure Functions

**Date**: 2026-01-23  
**Severity**: HIGH  
**Status**: REMEDIATION IN PROGRESS  

## Incident Summary

An Azure Functions key for the `AzureFridayDocstoJSON` function app was exposed publicly. The key value has been redacted from this document for security purposes.

**Key Details**:
- **Leaked Key**: `<REDACTED>`
- **Exposure Method**: Public curl command example
- **Discovery Date**: 2026-01-23

## Impact Assessment

- **Resource Affected**: Azure Function App `AzureFridayDocstoJSON`
- **Endpoint**: `https://azurefridaydocstojson.azurewebsites.net`
- **Key Type**: Function Host Key (provides admin-level access)
- **Potential Impact**: 
  - Unauthorized users could trigger function executions
  - Access to admin endpoints for monitoring function status
  - Potential increase in Azure costs due to unauthorized invocations
  - Access to blob storage outputs (if function has write permissions)

## Repository Security Verification

✅ **Repository is CLEAN** - The leaked key was verified to NOT be present in:
- Current codebase
- Git history
- Configuration files
- Documentation files
- GitHub Actions workflows

## Immediate Actions Required

### 1. Rotate the Key in Azure Portal (DO THIS FIRST)

**Steps to rotate the compromised key:**

1. Log into [Azure Portal](https://portal.azure.com)
2. Navigate to the Function App: `AzureFridayDocstoJSON`
3. Go to **Settings** → **Configuration** → **App keys** or **Function keys**
4. Identify the compromised key and click **Regenerate** or **Delete**
5. Copy the new key value securely
6. Update any downstream services or applications using this key

**Alternative via Azure CLI:**

```bash
# List all function keys
az functionapp keys list --name AzureFridayDocstoJSON --resource-group <your-resource-group>

# Delete the compromised key
az functionapp keys delete --name AzureFridayDocstoJSON --resource-group <your-resource-group> --key-name <key-name>

# Create a new key
az functionapp keys set --name AzureFridayDocstoJSON --resource-group <your-resource-group> --key-name <new-key-name> --key-type functionKeys
```

### 2. Verify Old Key is Invalidated

After rotation, test that the old key no longer works:

```bash
# Should return 401 Unauthorized
curl -v -H "x-functions-key: <OLD_COMPROMISED_KEY>" \
  "https://azurefridaydocstojson.azurewebsites.net/admin/host/status"
```

Expected response:
```
HTTP/1.1 401 Unauthorized
```

### 3. Update GitHub Secrets (if applicable)

If the key was stored in GitHub Secrets for CI/CD:

```bash
# Using GitHub CLI
gh secret set AZURE_FUNCTION_KEY --body "<new-key-value>"
```

Or via GitHub UI:
1. Go to Repository → Settings → Secrets and variables → Actions
2. Update or create secret for the new function key
3. Name it appropriately (e.g., `AZURE_FUNCTION_KEY_PROD`)

### 4. Monitor for Unauthorized Access

Check Azure Function logs for suspicious activity:

```bash
# Via Azure CLI - get recent invocations
az monitor activity-log list --resource-group <your-resource-group> \
  --start-time 2026-01-20T00:00:00Z --offset 3d \
  --query "[?contains(resourceId, 'AzureFridayDocstoJSON')]"
```

Or in Azure Portal:
1. Navigate to Function App → **Monitoring** → **Application Insights**
2. Check for unusual patterns in:
   - Request volume
   - Failed requests (401/403 errors)
   - Geographic distribution of requests
   - Time patterns (requests at unusual hours)

### 5. Enable Enhanced Security

Configure additional security measures:

```bash
# Enable HTTPS only
az functionapp update --name AzureFridayDocstoJSON \
  --resource-group <your-resource-group> \
  --set httpsOnly=true

# Enable managed identity (to reduce reliance on keys)
az functionapp identity assign --name AzureFridayDocstoJSON \
  --resource-group <your-resource-group>
```

## Preventive Measures Implemented

The following files have been added to the repository to prevent future incidents:

1. **SECURITY.md** - Comprehensive security policy with key rotation procedures
2. **README.md** - Updated with security best practices section
3. **KEY_ROTATION_INCIDENT.md** - This incident response document
4. **.gitignore** - Updated to exclude local.settings.json with secrets

## Recommended Follow-up Actions

1. **Enable Azure Key Vault**: Store function keys in Azure Key Vault instead of using them directly
2. **Implement Managed Identity**: Where possible, use managed identities to eliminate keys
3. **Set up Alerts**: Configure Azure Monitor alerts for:
   - High number of 401 responses
   - Unusual function invocation patterns
   - Changes to function keys
4. **Regular Key Rotation**: Establish a policy to rotate keys every 90 days
5. **Security Audit**: Review all Azure resources for exposed credentials
6. **Enable GitHub Advanced Security**: Turn on secret scanning and push protection

## Additional Resources

- [Azure Functions Security Best Practices](https://learn.microsoft.com/en-us/azure/azure-functions/security-concepts)
- [Secure Azure Functions HTTP triggers](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-http-webhook-trigger?tabs=python-v2%2Cisolated-process%2Cnodejs-v4%2Cfunctionsv2&pivots=programming-language-csharp#secure-an-http-endpoint-in-production)
- [Azure Key Vault integration](https://learn.microsoft.com/en-us/azure/azure-functions/functions-identity-access-azure-sql-with-managed-identity)

## Incident Timeline

| Time | Action | Status |
|------|--------|--------|
| 2026-01-23 02:35 | Leaked key reported | ✅ Confirmed |
| 2026-01-23 02:37 | Repository verified clean | ✅ Complete |
| 2026-01-23 02:40 | Security documentation created | ✅ Complete |
| TBD | Azure key rotation | ⏳ Pending - Manual action required |
| TBD | Verification testing | ⏳ Pending |
| TBD | Monitoring review | ⏳ Pending |

## Sign-off

Once the key has been rotated and verified:

- [ ] Old key confirmed invalidated
- [ ] New key tested and working
- [ ] GitHub Secrets updated (if applicable)
- [ ] No unauthorized access detected in logs
- [ ] Alerts configured for future monitoring
- [ ] Security measures documented and communicated

---

**Next Steps**: Execute the key rotation in Azure Portal as described in Section 1 above.
