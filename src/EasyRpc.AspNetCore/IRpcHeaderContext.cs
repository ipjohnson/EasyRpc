namespace EasyRpc.AspNetCore
{
    /// <summary>
    /// Header context
    /// </summary>
    public interface IRpcHeaderContext
    {
        /// <summary>
        /// Get value from headers
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">optional key</param>
        /// <returns></returns>
        T GetValue<T>(string key = null) where T : class;

        /// <summary>
        /// Set value into header
        /// </summary>
        /// <typeparam name="T">tpye of value</typeparam>
        /// <param name="value">value</param>
        /// <param name="key">optional key</param>
        void SetValue<T>(T value, string key = null) where T : class;
    }
}
