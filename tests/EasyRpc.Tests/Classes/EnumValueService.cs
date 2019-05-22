using System;
using System.Collections.Generic;
using System.Text;

namespace EasyRpc.Tests.Classes
{
    public enum TestEnum
    {
        Value1 = 1,
        Value2 = 2
    }

    public interface IEnumValueService
    {
        TestEnum GetTestEnum(TestEnum t);
    }

    public class EnumValueService : IEnumValueService
    {
        public TestEnum GetTestEnum(TestEnum t)
        {
            return t;
        }
    }
}
