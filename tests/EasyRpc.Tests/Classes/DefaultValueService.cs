namespace EasyRpc.Tests.Classes
{
    public class DefaultValueService
    {
        public const string DefaultValueString = "-DefaultValue";

        public string SomeMethod(string baseString, string endString = DefaultValueString)
        {
            return baseString + endString;
        }

        public string NullDefaultValue(string stringValue, string defaultValue = null)
        {
            return stringValue + defaultValue;
        }
    }
}
