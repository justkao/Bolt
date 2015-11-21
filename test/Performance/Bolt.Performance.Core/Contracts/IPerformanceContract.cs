using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Bolt.Performance.Contracts
{
    [ServiceContract]
    public interface IPerformanceContract
    {
        [OperationContract]
        Task Method_Async();

        [OperationContract]
        Task Method_Int_Async(int value);

        [OperationContract]
        Task Method_String_Async(string value);

        [OperationContract]
        Task Method_Object_Async(Person value);

        [OperationContract]
        Task Method_Many_Async(int intValue, string stringValue, DateTime dateValue, Person objectValue);

        [OperationContract]
        Task<int> Return_Int_Async();

        [OperationContract]
        Task<string> Return_String_Async();

        [OperationContract]
        Task<Person> Return_Object_Async();

        [OperationContract]
        Task<IEnumerable<Person>> Return_Objects_Async();

        [OperationContract]
        Task<IEnumerable<int>> Return_Ints_Async();

        [OperationContract]
        Task<IEnumerable<string>> Return_Strings_Async();
    }
}
