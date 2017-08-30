using Microsoft.Extensions.Logging;

namespace EasyRpc.AspNetCore
{
    public static class EventIdCode
    {
        public static EventId DeserializeException = 51000;

        public static EventId SerializeException = 52000;

        public static EventId ActivationException = 53000;

        public static EventId ExecutionException = 54000;
    }
}
