using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace EasyRpc.AspNetCore
{
    public interface IAssetsConfiguration
    {
        /// <summary>
        /// Apply authorization to this set of assets
        /// </summary>
        /// <param name="role"></param>
        /// <param name="policy"></param>
        /// <returns></returns>
        IAssetsConfiguration Authorize(string role = null, string policy = null);

        /// <summary>
        /// Set max cache age
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        IAssetsConfiguration HttpMaxCacheAge(TimeSpan timeSpan);

        /// <summary>
        /// Set max cache age per file using delegate
        /// </summary>
        /// <param name="cacheAction"></param>
        /// <returns></returns>
        IAssetsConfiguration HttpSetCache(Action<HttpContext, string> cacheAction);

        /// <summary>
        /// Set NO Cache on asset
        /// </summary>
        /// <returns></returns>
        IAssetsConfiguration HttpNoCache();

        /// <summary>
        /// Set the in memory cache size for these assets. 
        /// </summary>
        /// <param name="enable"></param>
        /// <returns></returns>
        IAssetsConfiguration InMemoryCache(bool enable = true);

        /// <summary>
        /// Set last modified date in response (true by default)
        /// </summary>
        /// <param name="lastModified"></param>
        /// <returns></returns>
        IAssetsConfiguration SetLastModified(bool lastModified = true);

        /// <summary>
        /// Apply ETag to response (true by default)
        /// </summary>
        /// <param name="etag"></param>
        /// <returns></returns>
        IAssetsConfiguration SetETag(bool etag = true);

        /// <summary>
        /// Set fallback file name, this is useful for SPA
        /// </summary>
        /// <param name="fallback"></param>
        /// <returns></returns>
        IAssetsConfiguration Fallback(string fallback);

        /// <summary>
        /// Max file size to cache in MB (default 1MB)
        /// </summary>
        /// <param name="fileSize"></param>
        /// <returns></returns>
        IAssetsConfiguration MaxFileCacheSize(double fileSize);

        /// <summary>
        /// Filter assets
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        IAssetsConfiguration Where(Func<string, bool> where);
    }
}
