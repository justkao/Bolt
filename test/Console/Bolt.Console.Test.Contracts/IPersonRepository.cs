using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bolt.Console.Test.Contracts
{
    public interface IPersonRepository : IPersonRepositoryInner, IPersonRepositoryInner2
    {
        [AsyncOperation]
        Person UpdatePerson(Person person, CancellationToken cancellation);

        Person UpdatePersonThatThrowsInvalidOperationException(Person person);

        Task DoLongRunningOperationAsync(Person person, CancellationToken cancellation);

        Task DoLongRunningOperation2Async(CancellationToken cancellation);

        Task DoNothingAsAsync();

        void DoNothing();

        Task DoNothingWithComplexParameterAsAsync(List<Person> person);

        void DoNothingWithComplexParameter(List<Person> person);

        int GetSimpleType(int arg);

        Task GetSimpleTypeAsAsync(int arg);

        Person GetSinglePerson(Person person);

        Task<Person> GetSinglePersonAsAsync(Person person);

        List<Person> GetManyPersons(Person person);

        Task<List<Person>> GetManyPersonsAsAsync(Person person);
    }
}
