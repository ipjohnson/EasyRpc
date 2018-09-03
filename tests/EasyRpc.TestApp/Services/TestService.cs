using System.Collections.Generic;
using EasyRpc.AspNetCore.Attributes;
using EasyRpc.TestApp.Models;
using EasyRpc.TestApp.Repositories;

namespace EasyRpc.TestApp.Services
{
    public interface ITestService
    {
        /// <summary>
        /// Int param
        /// </summary>
        /// <param name="intParam">integer parameter</param>
        /// <returns>int value</returns>
        int TestMethod(int intParam);
    }

    public class TestService : ITestService
    {
        public IEnumerable<PersonModel> All(IPersonRepository personRepository) => personRepository.All();

        /// <inheritdoc />
        public int TestMethod(int intParam)
        {
            return 1;
        }
    }
}
