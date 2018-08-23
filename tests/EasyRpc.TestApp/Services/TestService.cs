using System.Collections.Generic;
using EasyRpc.AspNetCore.Attributes;
using EasyRpc.TestApp.Models;
using EasyRpc.TestApp.Repositories;

namespace EasyRpc.TestApp.Services
{
    public class TestService
    {
        public IEnumerable<PersonModel> All(IPersonRepository personRepository) => personRepository.All();
    }
}
