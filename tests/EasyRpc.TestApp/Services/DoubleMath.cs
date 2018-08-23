using System;
using EasyRpc.TestApp.Filter;

namespace EasyRpc.TestApp.Services
{
    /// <summary>
    /// Services associated with doing double math
    /// </summary>
    public class DoubleMath
    {
        public double Add(double x, double y)
        {
            return x + y;
        }

        [ExecuteFilter]
        public double Subtract(double x, double y)
        {
            return x - y;
        }

        public DateTime Test(DateTime input)
        {
            return input;
        }

    }
}
