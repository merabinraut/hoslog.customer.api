using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hoslog.customer.common.Cache
{
    public interface IRedisCacheService
    {
        void Subscribe(string channel, Action<string, string> messageHandler);
        Task PublishAsync(string channel, string message);
        Task ClearCacheAsync();
        Task ClearInstanceCacheAsync();
    }
}
