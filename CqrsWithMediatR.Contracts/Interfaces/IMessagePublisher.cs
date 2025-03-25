using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CqrsWithMediatR.Contracts.Interfaces
{
    public interface IMessagePublisher
    {
        Task SendMessageAsync<T>(T message) where T : class;
    }
}
