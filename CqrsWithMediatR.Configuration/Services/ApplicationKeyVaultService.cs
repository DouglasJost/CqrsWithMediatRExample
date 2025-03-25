using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using CqrsWithMediatR.Configuration.Enums;
using CqrsWithMediatR.Configuration.Interfaces;
using System;
using System.Threading.Tasks;

namespace CqrsWithMediatR.Configuration.Services
{
    public class ApplicationKeyVaultService : IKeyVaultService
    {
        // 
        // NOTE: ApplicationKeyVaultService implements IKeyVaultService, which is not the MyClassName:IMyClassName convention
        //       Therefore, ServiceRegistration.GetServices() will not include it when AddServicesWithDefaultConventions() is
        //       called by Program.cs to automatically register services to the DI container.
        //
        //       Because of this, ApplicationKeyVaultService:IKeyVaultService is manually added to the DI container as a singleton
        //       in Program.cs.
        //
        //       Also, note that the constructor takes keyVaultUrl and secretSource as inputs.  This is also handled in program.cs
        //       when IKeyVaultService is registered as a singleton.
        //

        private readonly SecretClient _secretClient;
        private SecretSource _secretSource;

        public ApplicationKeyVaultService(string keyVaultUrl, SecretSource secretSource)
        {
            if (string.IsNullOrWhiteSpace(keyVaultUrl))
            {
                throw new ArgumentNullException(nameof(keyVaultUrl), "Azure Key Vault URL cannot be null or empty.");
            }

            _secretClient = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
            _secretSource = secretSource;
        }

        public async Task<string> GetSecretValueAsync(string secretName)
        {
            if (string.IsNullOrWhiteSpace(secretName))
            {
                throw new ArgumentNullException(nameof(secretName), "Secret name cannot be null or empty.");
            }

            try 
            {
                string secretValue = string.Empty;
                switch(_secretSource)
                {
                    case SecretSource.EnvironmentVariable:
                    {
                        secretValue = GetEnvironmentVariable(secretName);
                        break;
                    }
                    case SecretSource.AzureKeyVault:
                    {
                        secretValue = await GetAzureKeyVaultSecretAsync(secretName);
                        break;
                    }
                    default:
                    {
                        secretValue = GetEnvironmentVariable(secretName);
                        break;
                    }
                }

                return secretValue;
            }
            catch (RequestFailedException ex) when (ex.Status == 403)
            {
                // Handle access denied specifically
                throw new UnauthorizedAccessException($"Access to the Azure Key Vault is denied. Unable to retrieve the secret '{secretName}'. Check Azure Active Directory permissions.", ex);
            }
            catch (RequestFailedException ex)
            {
                // Handle other Azure-related errors
                throw new InvalidOperationException($"An error occurred while retrieving the secret '{secretName}' from Azure Key Vault. Status Code: {ex.Status}.", ex);
            }
            catch (Exception ex)
            {
                // General error handling
                throw new InvalidOperationException($"An unexpected error occurred while retrieving the secret '{secretName}'.", ex);
            }
        }


        private string GetEnvironmentVariable(string secretName)
        {
            return Environment.GetEnvironmentVariable(secretName)
                ?? throw new InvalidOperationException($"The environment variable '{secretName}' is not set as an environment variable.");
        }

        private async Task<string> GetAzureKeyVaultSecretAsync(string secretName)
        {
            KeyVaultSecret secret = await _secretClient.GetSecretAsync(secretName);
            return secret?.Value ?? throw new InvalidOperationException($"The secret '{secretName}' is not found in Azure Key Vault.");
        }
    }
}
