using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TestService.Core
{
    public class PersonRepository : IPersonRepository
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

        public Task DoNothingAsAsync()
        {
            return Task.FromResult(0);
        }

        public void DoNothing()
        {
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
            return new Random().Next();
        }

        public Task GetSimpleTypeAsAsync(int arg)
        {
            return Task.FromResult(new Random().Next());
        }

        public Person GetSinglePerson(Person person)
        {
            Console.WriteLine(GetHashCode());

            return Person.Create(0);
        }

        public Task<Person> GetSinglePersonAsAsync(Person person)
        {
            return Task.FromResult(Person.Create(0));
        }

        public List<Person> GetManyPersons(Person person)
        {
            return Enumerable.Range(0, 100).Select(Person.Create).ToList();
        }

        public Task<List<Person>> GetManyPersonsAsAsync(Person person)
        {
            return Task.FromResult(Enumerable.Range(0, 100).Select(Person.Create).ToList());
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
    }
}