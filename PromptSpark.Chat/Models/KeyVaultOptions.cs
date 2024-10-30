namespace PromptSpark.Chat.Models;
/// <summary>
/// Represents the options for accessing Azure Key Vault.
/// </summary>
public class KeyVaultOptions
{
    /// <summary>
    /// Gets or sets the mode of accessing Azure Key Vault.
    /// </summary>
    public KeyVaultUsage Mode { get; set; }

    /// <summary>
    /// Gets or sets the URI of the Azure Key Vault.
    /// </summary>
    public string KeyVaultUri { get; set; }

    /// <summary>
    /// Gets or sets the client ID for authenticating with Azure Key Vault.
    /// </summary>
    public string ClientId { get; set; }

    /// <summary>
    /// Gets or sets the client secret for authenticating with Azure Key Vault.
    /// </summary>
    public string ClientSecret { get; set; }

    /// <summary>
    /// Initialize KeyVaultOptions.
    /// </summary>
    /// <param name="mode"></param>
    /// <param name="keyVaultUri"></param>
    /// <param name="clientId"></param>
    /// <param name="clientSecret"></param>
    public KeyVaultOptions(KeyVaultUsage mode, string keyVaultUri, string clientId, string clientSecret)
    {
        ArgumentNullException.ThrowIfNull(keyVaultUri);

        ArgumentNullException.ThrowIfNull(clientId);

        ArgumentNullException.ThrowIfNull(clientSecret);

        Mode = mode;
        KeyVaultUri = keyVaultUri;
        ClientId = clientId;
        ClientSecret = clientSecret;
    }
}
