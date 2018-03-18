using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bolt.Benchmark.Contracts
{
    public class PerformanceContractImplementation : IPerformanceContract
    {
        private static readonly Task Completed = Task.FromResult(true);

        private static readonly Task<int> CompletedInt = Task.FromResult(1);

        private static readonly Task<string> CompletedString = Task.FromResult("dummy string");

        private static readonly Task<Person> CompletedPerson = Task.FromResult(Person.Create(10));

        private static readonly Task<IEnumerable<int>> CompletedInts =
            Task.FromResult((IEnumerable<int>) Enumerable.Range(0, 5).ToList());

        private static readonly Task<IEnumerable<string>> CompletedStrings =
            Task.FromResult((IEnumerable<string>) Enumerable.Range(0, 5).Select(v => "dummy_" + v).ToList());

        private static readonly Task<IEnumerable<Person>> CompletedObjects =
            Task.FromResult((IEnumerable<Person>) Enumerable.Range(0, 5).Select(Person.Create).ToList());

        private static readonly Task<IEnumerable<Person>> CompletedLarge =
            Task.FromResult((IEnumerable<Person>) Enumerable.Range(0, 100).Select(Person.Create).ToList());

        public Task Method_Async()
        {
            return Completed;
        }

        public Task Method_Int_Async(int value)
        {
            return Completed;
        }

        public Task Method_Many_Async(int intValue, string stringValue, DateTime dateValue, Person objectValue)
        {
            return Completed;
        }

        public Task Method_Large_Async(List<Person> largeObject)
        {
            return Completed;
        }

        public Task Method_Object_Async(Person value)
        {
            return Completed;
        }

        public Task Method_String_Async(string value)
        {
            return Completed;
        }

        public Task<int> Return_Int_Async()
        {
            return CompletedInt;
        }

        public Task<IEnumerable<int>> Return_Ints_Async()
        {
            return CompletedInts;
        }

        public Task<Person> Return_Object_Async()
        {
            return CompletedPerson;
        }

        public Task<IEnumerable<Person>> Return_Objects_Async()
        {
            return CompletedObjects;
        }

        public Task<string> Return_String_Async()
        {
            return CompletedString;
        }

        public Task<IEnumerable<string>> Return_Strings_Async()
        {
            return CompletedStrings;
        }

        public Task<IEnumerable<Person>> Return_Large_Async()
        {
            return CompletedLarge;
        }

        public Task Method_ThrowsErrorAsync()
        {
            try
            {
                throw new InvalidOperationException("Forced inner error");
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Forced outer error", e);
            }
        }
    }
}