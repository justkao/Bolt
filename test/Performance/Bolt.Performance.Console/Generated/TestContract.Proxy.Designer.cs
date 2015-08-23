//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.

//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Bolt.Client;
using Bolt.Performance.Contracts;


namespace Bolt.Performance.Contracts
{
    public partial class TestContractProxy : Bolt.Client.ProxyBase, Bolt.Performance.Contracts.ITestContract
    {
        public TestContractProxy(Bolt.Performance.Contracts.TestContractProxy proxy) : base(proxy)
        {
        }

        public TestContractProxy(Bolt.Client.Pipeline.IClientPipeline channel) : base(typeof(Bolt.Performance.Contracts.ITestContract), channel)
        {
        }

        public virtual Person UpdatePerson(Person person, CancellationToken cancellation)
        {
            return this.Send<Person>(__UpdatePersonAction, person, cancellation);
        }

        public virtual Task<Person> UpdatePersonAsync(Person person, CancellationToken cancellation)
        {
            return this.SendAsync<Person>(__UpdatePersonAction, person, cancellation);
        }

        public virtual Person UpdatePersonThatThrowsInvalidOperationException(Person person)
        {
            return this.Send<Person>(__UpdatePersonThatThrowsInvalidOperationExceptionAction, person);
        }

        public virtual Task<Person> UpdatePersonThatThrowsInvalidOperationExceptionAsync(Person person)
        {
            return this.SendAsync<Person>(__UpdatePersonThatThrowsInvalidOperationExceptionAction, person);
        }

        public virtual Task DoNothingAsAsync()
        {
            return this.SendAsync(__DoNothingAsAsyncAction);
        }

        public virtual void DoNothing()
        {
            this.Send(__DoNothingAction);
        }

        public virtual Task DoNothingAsync()
        {
            return this.SendAsync(__DoNothingAction);
        }

        public virtual Task DoNothingWithComplexParameterAsAsync(List<Person> person)
        {
            return this.SendAsync(__DoNothingWithComplexParameterAsAsyncAction, person);
        }

        public virtual void DoNothingWithComplexParameter(List<Person> person)
        {
            this.Send(__DoNothingWithComplexParameterAction, person);
        }

        public virtual Task DoNothingWithComplexParameterAsync(List<Person> person)
        {
            return this.SendAsync(__DoNothingWithComplexParameterAction, person);
        }

        public virtual void MethodWithManyArguments(List<Person> person, int intValue, string stringValue, DateTime dateValue, Person objectValue)
        {
            this.Send(__MethodWithManyArgumentsAction, person, intValue, stringValue, dateValue, objectValue);
        }

        public virtual Task MethodWithManyArgumentsAsync(List<Person> person, int intValue, string stringValue, DateTime dateValue, Person objectValue)
        {
            return this.SendAsync(__MethodWithManyArgumentsAction, person, intValue, stringValue, dateValue, objectValue);
        }

        public virtual Task MethodWithManyArgumentsAsAsync(List<Person> person, int intValue, string stringValue, DateTime dateValue, Person objectValue)
        {
            return this.SendAsync(__MethodWithManyArgumentsAsAsyncAction, person, intValue, stringValue, dateValue, objectValue);
        }

        public virtual int GetSimpleType(int arg)
        {
            return this.Send<int>(__GetSimpleTypeAction, arg);
        }

        public virtual Task<int> GetSimpleTypeAsync(int arg)
        {
            return this.SendAsync<int>(__GetSimpleTypeAction, arg);
        }

        public virtual Task GetSimpleTypeAsAsync(int arg)
        {
            return this.SendAsync(__GetSimpleTypeAsAsyncAction, arg);
        }

        public virtual Person GetSinglePerson(Person person)
        {
            return this.Send<Person>(__GetSinglePersonAction, person);
        }

        public virtual Task<Person> GetSinglePersonAsync(Person person)
        {
            return this.SendAsync<Person>(__GetSinglePersonAction, person);
        }

        public virtual Task<Person> GetSinglePersonAsAsync(Person person)
        {
            return this.SendAsync<Person>(__GetSinglePersonAsAsyncAction, person);
        }

        public virtual List<Person> GetManyPersons()
        {
            return this.Send<List<Person>>(__GetManyPersonsAction);
        }

        public virtual Task<List<Person>> GetManyPersonsAsync()
        {
            return this.SendAsync<List<Person>>(__GetManyPersonsAction);
        }

        public virtual Task<List<Person>> GetManyPersonsAsAsync()
        {
            return this.SendAsync<List<Person>>(__GetManyPersonsAsAsyncAction);
        }

        public virtual void Throws()
        {
            this.Send(__ThrowsAction);
        }

        public virtual Task ThrowsAsync()
        {
            return this.SendAsync(__ThrowsAction);
        }

        public virtual void ThrowsCustom()
        {
            this.Send(__ThrowsCustomAction);
        }

        public virtual Task ThrowsCustomAsync()
        {
            return this.SendAsync(__ThrowsCustomAction);
        }

        public virtual void InnerOperation()
        {
            this.Send(__InnerOperationAction);
        }

        public virtual Task InnerOperationAsync()
        {
            return this.SendAsync(__InnerOperationAction);
        }

        public virtual Task<string> InnerOperation3()
        {
            return this.SendAsync<string>(__InnerOperation3Action);
        }

        public virtual Task InnerOperationExAsync()
        {
            return this.SendAsync(__InnerOperationExAsyncAction);
        }
        public virtual void InnerOperation2()
        {
            this.Send(__InnerOperation2Action);
        }

        public virtual Task InnerOperation2Async()
        {
            return this.SendAsync(__InnerOperation2Action);
        }

        public virtual Task InnerOperationExAsync2()
        {
            return this.SendAsync(__InnerOperationExAsync2Action);
        }

        private static readonly MethodInfo __UpdatePersonAction = typeof(ITestContract).GetMethod(nameof(ITestContract.UpdatePerson));
        private static readonly MethodInfo __UpdatePersonThatThrowsInvalidOperationExceptionAction = typeof(ITestContract).GetMethod(nameof(ITestContract.UpdatePersonThatThrowsInvalidOperationException));
        private static readonly MethodInfo __DoNothingAsAsyncAction = typeof(ITestContract).GetMethod(nameof(ITestContract.DoNothingAsAsync));
        private static readonly MethodInfo __DoNothingAction = typeof(ITestContract).GetMethod(nameof(ITestContract.DoNothing));
        private static readonly MethodInfo __DoNothingWithComplexParameterAsAsyncAction = typeof(ITestContract).GetMethod(nameof(ITestContract.DoNothingWithComplexParameterAsAsync));
        private static readonly MethodInfo __DoNothingWithComplexParameterAction = typeof(ITestContract).GetMethod(nameof(ITestContract.DoNothingWithComplexParameter));
        private static readonly MethodInfo __MethodWithManyArgumentsAction = typeof(ITestContract).GetMethod(nameof(ITestContract.MethodWithManyArguments));
        private static readonly MethodInfo __MethodWithManyArgumentsAsAsyncAction = typeof(ITestContract).GetMethod(nameof(ITestContract.MethodWithManyArgumentsAsAsync));
        private static readonly MethodInfo __GetSimpleTypeAction = typeof(ITestContract).GetMethod(nameof(ITestContract.GetSimpleType));
        private static readonly MethodInfo __GetSimpleTypeAsAsyncAction = typeof(ITestContract).GetMethod(nameof(ITestContract.GetSimpleTypeAsAsync));
        private static readonly MethodInfo __GetSinglePersonAction = typeof(ITestContract).GetMethod(nameof(ITestContract.GetSinglePerson));
        private static readonly MethodInfo __GetSinglePersonAsAsyncAction = typeof(ITestContract).GetMethod(nameof(ITestContract.GetSinglePersonAsAsync));
        private static readonly MethodInfo __GetManyPersonsAction = typeof(ITestContract).GetMethod(nameof(ITestContract.GetManyPersons));
        private static readonly MethodInfo __GetManyPersonsAsAsyncAction = typeof(ITestContract).GetMethod(nameof(ITestContract.GetManyPersonsAsAsync));
        private static readonly MethodInfo __ThrowsAction = typeof(ITestContract).GetMethod(nameof(ITestContract.Throws));
        private static readonly MethodInfo __ThrowsCustomAction = typeof(ITestContract).GetMethod(nameof(ITestContract.ThrowsCustom));
        private static readonly MethodInfo __InnerOperationAction = typeof(IInnerTestContract).GetMethod(nameof(IInnerTestContract.InnerOperation));
        private static readonly MethodInfo __InnerOperation3Action = typeof(IInnerTestContract).GetMethod(nameof(IInnerTestContract.InnerOperation3));
        private static readonly MethodInfo __InnerOperationExAsyncAction = typeof(IInnerTestContract).GetMethod(nameof(IInnerTestContract.InnerOperationExAsync));
        private static readonly MethodInfo __InnerOperation2Action = typeof(IInnerTestContract2).GetMethod(nameof(IInnerTestContract2.InnerOperation2));
        private static readonly MethodInfo __InnerOperationExAsync2Action = typeof(IInnerTestContract2).GetMethod(nameof(IInnerTestContract2.InnerOperationExAsync2));
    }
}