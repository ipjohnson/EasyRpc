using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyRpc.TestApp.Models;
using EasyRpc.TestApp.Repositories;

namespace EasyRpc.TestApp.Services
{
    public class TestService
    {

        public IEnumerable<PersonModel> All(IPersonRepository personRepository) => personRepository.All();
    }
}
