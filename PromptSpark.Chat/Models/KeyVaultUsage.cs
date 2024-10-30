
namespace PromptSpark.Chat.Models;
/// <summary>
/// Key Vault Usage
/// </summary>
public enum KeyVaultUsage
{
    /// <summary>
    /// Use local secret store
    /// </summary>
    UseLocalSecretStore,

    /// <summary>
    /// Use client secret
    /// </summary>
    UseClientSecret,

    /// <summary>
    /// Use Managed Service Identity (MSI)
    /// </summary>
    UseMsi
}
