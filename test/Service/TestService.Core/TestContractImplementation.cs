using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TestService.Core
{
    public class TestContractImplementation : ITestContract
    {
        public Person UpdatePerson(Person person, CancellationToken cancellation)
        {
            return null;
        }

        public Person UpdatePersonThatThrowsInvalidOperationException(Person person)
        {
            throw new InvalidOperationException("test message", new ArgumentOutOfRangeException("inner message"));
        }

        public Task DoLongRunningOperationAsync(Person person, CancellationToken cancellation)
        {
            return Task.Delay(TimeSpan.FromMinutes(1), cancellation);
        }

        public Task DoLongRunningOperation2Async(CancellationToken cancellation)
        {
            return Task.Delay(TimeSpan.FromMinutes(1), cancellation);
        }

        public async Task LongRunningOperation2Async(CancellationToken cancellation)
        {
            await Task.Delay(TimeSpan.FromMinutes(1), cancellation);
        }

        public Task DoNothingAsAsync()
        {
            return Task.Delay(200);
        }

        public void DoNothing()
        {
            Thread.Sleep(200);
        }

        public Task DoNothingWithComplexParameterAsAsync(List<Person> person)
        {
            return Task.FromResult(0);
        }

        public void DoNothingWithComplexParameter(List<Person> person)
        {
        }

        public int GetSimpleType(int arg)
        {
            return arg;
        }

        public Task GetSimpleTypeAsAsync(int arg)
        {
            return Task.FromResult(new Random().Next());
        }

        public Person GetSinglePerson(Person person)
        {
            return Person.Create(0);
        }

        public Task<Person> GetSinglePersonAsAsync(Person person)
        {
            return Task.FromResult(Person.Create(0));
        }

        public List<Person> GetManyPersons()
        {
            return Enumerable.Range(0, 100).Select(Person.Create).ToList();
        }

        public Task<List<Person>> GetManyPersonsAsAsync(Person person)
        {
            return Task.FromResult(Enumerable.Range(0, 100).Select(Person.Create).ToList());
        }

        public virtual void ThrowsCustom()
        {
        }

        public void Throws()
        {
            Exception inner;
            try
            {
                throw new NotSupportedException("Another message");
            }
            catch (Exception e)
            {
                inner = e;
            }

            throw new InvalidOperationException("This is forced error message.", inner);
        }

        public void InnerOperation()
        {
        }

        public Task InnerOperationExAsync()
        {
            return Task.FromResult(0);
        }

        public void InnerOperation2()
        {
            throw new NotImplementedException();
        }

        public Task InnerOperationExAsync2()
        {
            throw new NotImplementedException();
        }

        public Task<string> InnerOperation3()
        {
            return Task.FromResult("InnerOperation3");
        }
    }
}