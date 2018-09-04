using System.Collections.Generic;
using EasyRpc.TestApp.Models;
using EasyRpc.TestApp.Repositories;

namespace EasyRpc.TestApp.Services
{
    /// <summary>
    /// Service for dealing with PersonModel
    /// </summary>
    public class PersonService
    {
        private readonly IPersonRepository _personRepository;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="personRepository"></param>
        public PersonService(IPersonRepository personRepository)
        {
            _personRepository = personRepository;
        }

        /// <summary>
        /// Returns a list of all known person models
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PersonModel> All() => _personRepository.All();

        /// <summary>
        /// Add a person model to the service
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Add(PersonModel model) => _personRepository.AddModel(model);

        /// <summary>
        /// Update an existing model
        /// </summary>
        /// <param name="model"></param>
        public void Update(PersonModel model) => _personRepository.UpdateModel(model);

        /// <summary>
        /// Search for person by last name
        /// </summary>
        /// <param name="lastName"></param>
        /// <returns></returns>
        public IEnumerable<PersonModel> SearchByLastName(string lastName) =>
            _personRepository.SearchByLastName(lastName);
    }
}
