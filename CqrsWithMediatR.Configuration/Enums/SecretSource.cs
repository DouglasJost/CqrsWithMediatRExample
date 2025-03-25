using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CqrsWithMediatR.Configuration.Enums
{
    public enum SecretSource
    {
        EnvironmentVariable,
        AzureKeyVault,
    }
}
