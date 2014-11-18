using System.Collections.Generic;
using System.Threading.Tasks;

using Bolt;

namespace TestService.Contracts
{
    public interface IPersonRepository
    {
        void Init();

        [AsyncOperation]
        string GetServerName();

        Task<List<Person>> GetPersonsAsync();

        Task<int> AddPersonAsync(Person person);

        Task DeletePersonAsync(int personId);
    }
}
