using EasyRpc.AspNetCore.Data;

namespace EasyRpc.AspNetCore.Middleware
{
    public interface IExposeMethodInformationCacheManager
    {
        IExposedMethodInformation GetExposedMethodInformation(string path, string method);

        void Configure(EndPointConfiguration endPoint);
    }

    public class ExposeMethodInformationCacheManager : IExposeMethodInformationCacheManager
    {
        private ImmutableHashTree<string, IExposedMethodInformation> _methodCache = 
            ImmutableHashTree<string, IExposedMethodInformation>.Empty;
        private EndPointConfiguration _endPointConfiguration;

        public void Configure(EndPointConfiguration endPoint)
        {
            _endPointConfiguration = endPoint;
        }

        public IExposedMethodInformation GetExposedMethodInformation(string path, string method)
        {
            var key = string.Concat(path, '*', method);

            return _methodCache.GetValueOrDefault(key) ?? FindMethod(key);
        }

        private IExposedMethodInformation FindMethod(string key)
        {
            if (_endPointConfiguration.Methods.TryGetValue(key, out var method))
            {
                ImmutableHashTree.ThreadSafeAdd(ref _methodCache, key, method);

                return method;
            }

            return null;
        }
    }
}
