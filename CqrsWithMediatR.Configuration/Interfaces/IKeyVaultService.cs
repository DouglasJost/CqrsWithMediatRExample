using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CqrsWithMediatR.Configuration.Interfaces
{
    public interface IKeyVaultService
    {
        Task<string> GetSecretValueAsync(string secretName);
    }
}
