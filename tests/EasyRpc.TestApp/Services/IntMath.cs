namespace EasyRpc.TestApp.Services
{
    /// <summary>
    /// Integer math service
    /// </summary>
    public class IntMath
    {
        /// <summary>
        /// Add two integers together
        /// </summary>
        /// <param name="x">first integer value</param>
        /// <param name="y">second integer value</param>
        /// <returns>x + y</returns>
        public int Add(int x, int y)
        {
            return x + y;
        }

        /// <summary>
        /// Subtract two integers
        /// </summary>
        /// <param name="x">first integer value</param>
        /// <param name="y">second integer value</param>
        /// <returns></returns>
        public int Subtract(int x, int y)
        {
            return x - y;
        }

        /// <summary>
        /// Muliply two integers together
        /// </summary>
        /// <param name="x">first integer value</param>
        /// <param name="y">second integer value</param>
        /// <returns></returns>
        public int Multiply(int x, int y)
        {
            return x * y;
        }
    }
}
