using VaultSharp;
using VaultSharp.Core;
using VaultSharp.V1.AuthMethods.Token;

namespace TeamTasks.Identity.Api.Common;

/// <summary>
/// Represents the vault service class.
/// </summary>
public sealed class VaultService
{
    private readonly IVaultClient _vaultClient;

    /// <summary>
    /// Initializes a new instance of <see cref="VaultService"/>.
    /// </summary>
    /// <param name="configuration">The vault configuration.</param>
    public VaultService(IConfiguration configuration)
    {
        var vaultAddress = configuration["Vault:Address"];
        var vaultToken = configuration["Vault:Token"];
        
        var authMethod = new TokenAuthMethodInfo(vaultToken);
        _vaultClient = new VaultClient(new VaultClientSettings(vaultAddress, authMethod));
    }

    /// <summary>
    /// Get vault secret with specified secret path.
    /// </summary>
    /// <param name="secretPath">The secret path.</param>
    /// <returns>Returns the secret value.</returns>
    /// <exception cref="Exception">The get secret exception.</exception>
    public async Task<string?> GetSecretAsync(string secretPath)
    {
        try
        {
            var secret = await  _vaultClient.V1.Secrets.KeyValue.V1.ReadSecretAsync(secretPath);
            return secret.Data["value"].ToString();
        }
        catch (VaultApiException ex)
        {
            // Log and handle the exception as needed
            throw new Exception($"Failed to read secret from path: {secretPath}. Error: {ex.Message}", ex);
        }
    }
}