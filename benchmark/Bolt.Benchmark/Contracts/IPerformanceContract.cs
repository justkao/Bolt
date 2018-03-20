using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bolt.Benchmark.Contracts
{
    public interface IPerformanceContract
    {
        Task Method_Async();

        Task Method_Int_Async(int value);

        Task Method_String_Async(string value);

        Task Method_Object_Async(Person value);

        Task Method_Many_Async(int intValue, string stringValue, DateTime dateValue, Person objectValue);

        Task Method_Large_Async(List<Person> largeObject);

        Task<int> Return_Int_Async();

        Task<string> Return_String_Async();

        Task<Person> Return_Object_Async();

        Task<IEnumerable<Person>> Return_Objects_Async();

        Task<IEnumerable<int>> Return_Ints_Async();

        Task<IEnumerable<string>> Return_Strings_Async();

        Task<IEnumerable<Person>> Return_Large_Async();

        Task<IEnumerable<Person>> Return_Large_Cached_Async(int count);

        Task Method_ThrowsErrorAsync();
    }
}
