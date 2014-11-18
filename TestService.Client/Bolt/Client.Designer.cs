

using Bolt;
using Bolt.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestService.Core;
using TestService.Core.Parameters;

namespace TestService.Core
{
    public partial class PersonRepository : Channel, Bolt.IChannel, TestService.Core.IPersonRepository
    {
        public Person UpdatePerson(Person person)
        {
            var request = new UpdatePersonParameters();
            request.Person = person;
            return Send<Person, UpdatePersonParameters>(request, GetEndpoint(new MethodDescriptor("PersonRepository","UpdatePerson","PersonRepository/UpdatePerson")));
        }

        public Task<Person> UpdatePersonAsync(Person person)
        {
            var request = new UpdatePersonParameters();
            request.Person = person;
            return SendAsync<Person, UpdatePersonParameters>(request, GetEndpoint(new MethodDescriptor("PersonRepository","UpdatePerson","PersonRepository/UpdatePerson")));
        }

        public Task DoNothingAsAsync()
        {
            return SendAsync(Empty.Instance, GetEndpoint(new MethodDescriptor("PersonRepository","DoNothingAsAsync","PersonRepository/DoNothingAsAsync")));
        }

        public void DoNothing()
        {
            Send(Empty.Instance, GetEndpoint(new MethodDescriptor("PersonRepository","DoNothing","PersonRepository/DoNothing")));
        }

        public Task DoNothingWithComplexParameterAsAsync(List<Person> person)
        {
            var request = new DoNothingWithComplexParameterAsAsyncParameters();
            request.Person = person;
            return SendAsync(request, GetEndpoint(new MethodDescriptor("PersonRepository","DoNothingWithComplexParameterAsAsync","PersonRepository/DoNothingWithComplexParameterAsAsync")));
        }

        public void DoNothingWithComplexParameter(List<Person> person)
        {
            var request = new DoNothingWithComplexParameterParameters();
            request.Person = person;
            Send(request, GetEndpoint(new MethodDescriptor("PersonRepository","DoNothingWithComplexParameter","PersonRepository/DoNothingWithComplexParameter")));
        }

        public int GetSimpleType(int arg)
        {
            var request = new GetSimpleTypeParameters();
            request.Arg = arg;
            return Send<int, GetSimpleTypeParameters>(request, GetEndpoint(new MethodDescriptor("PersonRepository","GetSimpleType","PersonRepository/GetSimpleType")));
        }

        public Task GetSimpleTypeAsAsync(int arg)
        {
            var request = new GetSimpleTypeAsAsyncParameters();
            request.Arg = arg;
            return SendAsync(request, GetEndpoint(new MethodDescriptor("PersonRepository","GetSimpleTypeAsAsync","PersonRepository/GetSimpleTypeAsAsync")));
        }

        public Person GetSinglePerson(Person person)
        {
            var request = new GetSinglePersonParameters();
            request.Person = person;
            return Send<Person, GetSinglePersonParameters>(request, GetEndpoint(new MethodDescriptor("PersonRepository","GetSinglePerson","PersonRepository/GetSinglePerson")));
        }

        public Task<Person> GetSinglePersonAsAsync(Person person)
        {
            var request = new GetSinglePersonAsAsyncParameters();
            request.Person = person;
            return SendAsync<Person, GetSinglePersonAsAsyncParameters>(request, GetEndpoint(new MethodDescriptor("PersonRepository","GetSinglePersonAsAsync","PersonRepository/GetSinglePersonAsAsync")));
        }

        public List<Person> GetManyPersons(Person person)
        {
            var request = new GetManyPersonsParameters();
            request.Person = person;
            return Send<List<Person>, GetManyPersonsParameters>(request, GetEndpoint(new MethodDescriptor("PersonRepository","GetManyPersons","PersonRepository/GetManyPersons")));
        }

        public Task<List<Person>> GetManyPersonsAsAsync(Person person)
        {
            var request = new GetManyPersonsAsAsyncParameters();
            request.Person = person;
            return SendAsync<List<Person>, GetManyPersonsAsAsyncParameters>(request, GetEndpoint(new MethodDescriptor("PersonRepository","GetManyPersonsAsAsync","PersonRepository/GetManyPersonsAsAsync")));
        }
    }
}

