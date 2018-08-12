namespace EasyRpc.AspNetCore.Documentation
{
    public class TemplateProvider
    {
        public void Test()
        {
            var strings = "Blah\\blah\\test.css";

            var index = strings.LastIndexOf('\\');
            index = strings.LastIndexOf('\\', index);
            var sub = strings.Substring(index);
        }
    }
}
