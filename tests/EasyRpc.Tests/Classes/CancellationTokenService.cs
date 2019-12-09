using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyRpc.Tests.Classes
{
    public class CancellationTokenService
    {
        public async Task<bool> Wait(int waitTime, CancellationToken token)
        {
            await Task.Delay(waitTime, token);

            return true;
        }
    }
}
