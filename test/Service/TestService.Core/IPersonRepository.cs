using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

using Bolt;

namespace TestService.Core
{
    [ServiceContract]
    public interface IPersonRepository : IPersonRepositoryInner
    {
        [OperationContract]
        [AsyncOperation]
        Person UpdatePerson(Person person);

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
        List<Person> GetManyPersons(Person person);

        [OperationContract]
        Task<List<Person>> GetManyPersonsAsAsync(Person person);
    }
}
