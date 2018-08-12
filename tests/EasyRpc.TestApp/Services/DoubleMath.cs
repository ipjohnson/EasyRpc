using System;

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
