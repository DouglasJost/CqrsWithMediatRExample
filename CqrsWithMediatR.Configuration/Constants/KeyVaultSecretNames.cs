using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CqrsWithMediatR.Configuration.Constants
{
    public static class KeyVaultSecretNames
    {
        public static readonly string Azure_KeyVault_Url = "Azure-KeyVault-Url";
        public static readonly string Azure_Service_Bus_Namespace = "Azure-Service-Bus-Namespace";
        public static readonly string Azure_Service_Bus_QueueName = "Azure-Service-Bus-QueueName";
        public static readonly string Authentication_Audience = "Authentication-Audience";
        public static readonly string Authentication_Issuer = "Authentication-Issuer";
        public static readonly string Authentication_SecretForKey = "Authentication-SecretForKey";
    }
}
