using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EasyRpc.TestApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace EasyRpc.TestApp.Repositories
{
    public interface IPersonRepository
    {
        IEnumerable<PersonModel> All();

        int AddModel(PersonModel newModel);

        void UpdateModel(PersonModel updateModel);

        IEnumerable<PersonModel> SearchByLastName(string lastName);

        PersonModel GetPersonModel(int personId);
    }

    public class PersonRepository : IPersonRepository
    {
        private int _modelId = 1;
        private ConcurrentDictionary<int, PersonModel> _personModels;

        public PersonRepository()
        {
            _personModels = new ConcurrentDictionary<int, PersonModel>();

            _personModels.TryAdd(1, new PersonModel
            {
                PersonId = 1,
                LastName = "Doe",
                FirstName = "John",
                HomeAddress = new AddressModel
                {
                    Line1 = "123 Mocking Bird Lane",
                    City = "Denver",
                    State = "Colorado",
                    Country = "US",
                    PostalCode = "12345"
                }
            });
        }

        public int AddModel(PersonModel newModel)
        {
            var modelId = Interlocked.Increment(ref _modelId);

            newModel.PersonId = modelId;

            _personModels.AddOrUpdate(modelId, newModel, (i, model) => newModel);

            return _modelId;
        }

        public IEnumerable<PersonModel> All()
        {
            return _personModels.Values;
        }

        public PersonModel GetPersonModel(int personId)
        {
            _personModels.TryGetValue(personId, out var returnValue);

            return returnValue;
        }

        public IEnumerable<PersonModel> SearchByLastName(string lastName)
        {
            return _personModels.Values.Where(model =>
                string.Compare(lastName, model.LastName, StringComparison.CurrentCultureIgnoreCase) == 0);
        }

        public void UpdateModel(PersonModel updateModel)
        {
            _personModels.AddOrUpdate(updateModel.PersonId,
                i => throw new Exception($"Model with id {updateModel.PersonId} does not exist"),
                (i, model) => updateModel);
        }
    }
}
