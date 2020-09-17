namespace EasyRpc.Tests.Classes.Simple
{
    public interface IImportArrayService
    {
        IMultipleService[] Array { get; }
    }

    public class ImportArrayService : IImportArrayService
    {
        public ImportArrayService(IMultipleService[] array)
        {
            Array = array;
        }

        public IMultipleService[] Array { get; }
    }
}
