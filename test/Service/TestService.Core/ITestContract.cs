using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

using Bolt;

namespace TestService.Core
{
    [ServiceContract]
    public interface ITestContract : IInnerTestContract, IInnerTestContract2
    {
        [OperationContract]
        [AsyncOperation]
        Person UpdatePerson(Person person, CancellationToken cancellation);

        [OperationContract]
        Person UpdatePersonThatThrowsInvalidOperationException(Person person);

        [OperationContract]
        Task DoNothingAsAsync();

        [OperationContract]
        void DoNothing();

        [OperationContract]
        Task DoNothingWithComplexParameterAsAsync(List<Person> person);

        [OperationContract]
        void DoNothingWithComplexParameter(List<Person> person);

        [OperationContract]
        int GetSimpleType(int arg);

        [OperationContract]
        Task GetSimpleTypeAsAsync(int arg);

        [OperationContract]
        Person GetSinglePerson(Person person);

        [OperationContract]
        Task<Person> GetSinglePersonAsAsync(Person person);

        [OperationContract]
        List<Person> GetManyPersons();

        [OperationContract]
        Task<List<Person>> GetManyPersonsAsAsync(Person person);

        [OperationContract]
        void Throws();

        [OperationContract]
        void ThrowsCustom();
    }
}
