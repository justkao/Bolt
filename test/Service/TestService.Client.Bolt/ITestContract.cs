using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Bolt;

namespace TestService.Core
{
    public interface ITestContract : IInnerTestContract, IInnerTestContract2
    {
        [AsyncOperation]
        Person UpdatePerson(Person person, CancellationToken cancellation);

        Person UpdatePersonThatThrowsInvalidOperationException(Person person);

        Task DoNothingAsAsync();

        void DoNothing();

        Task DoNothingWithComplexParameterAsAsync(List<Person> person);

        void DoNothingWithComplexParameter(List<Person> person);

        int GetSimpleType(int arg);

        Task GetSimpleTypeAsAsync(int arg);

        Person GetSinglePerson(Person person);

        Task<Person> GetSinglePersonAsAsync(Person person);

        List<Person> GetManyPersons();

        Task<List<Person>> GetManyPersonsAsAsync(Person person);

        void Throws();

        void ThrowsCustom();
    }
}
