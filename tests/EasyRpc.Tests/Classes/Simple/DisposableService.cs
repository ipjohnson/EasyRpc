using System;

namespace EasyRpc.Tests.Classes.Simple
{
    public interface IDisposableService
    {
        event EventHandler<EventArgs> Disposing;
    }

    public class DisposableService : IDisposableService, IDisposable
    {
        public void Dispose()
        {
            if (Disposing != null)
            {
                Disposing(this, EventArgs.Empty);
            }
        }

        public event EventHandler<EventArgs> Disposing;
    }
}
