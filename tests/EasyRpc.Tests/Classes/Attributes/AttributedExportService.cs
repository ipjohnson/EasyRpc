using Grace.DependencyInjection.Attributes;

namespace EasyRpc.Tests.Classes.Attributes
{
    public interface IAttributedExportService
    {
        IAttributeBasicService BasicService { get; }
    }

    [Export(typeof(IAttributedExportService))]
    public class AttributedExportService : IAttributedExportService
    {
        public IAttributeBasicService BasicService
        {
            get
            {
                return new AttributeBasicService(); 
            }
        }
    }
}
