













using Bolt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using TestService.Core;
using TestService.Core.Parameters;

namespace TestService.Core.Parameters
{
    [DataContract]
    public partial class UpdatePersonParameters
    {
        [DataMember(Order = 1)]
        public Person Person { get; set; }
    }

    [DataContract]
    public partial class UpdatePersonThatThrowsInvalidOperationExceptionParameters
    {
        [DataMember(Order = 1)]
        public Person Person { get; set; }
    }

    [DataContract]
    public partial class DoNothingWithComplexParameterAsAsyncParameters
    {
        [DataMember(Order = 1)]
        public List<Person> Person { get; set; }
    }

    [DataContract]
    public partial class DoNothingWithComplexParameterParameters
    {
        [DataMember(Order = 1)]
        public List<Person> Person { get; set; }
    }

    [DataContract]
    public partial class GetSimpleTypeParameters
    {
        [DataMember(Order = 1)]
        public int Arg { get; set; }
    }

    [DataContract]
    public partial class GetSimpleTypeAsAsyncParameters
    {
        [DataMember(Order = 1)]
        public int Arg { get; set; }
    }

    [DataContract]
    public partial class GetSinglePersonParameters
    {
        [DataMember(Order = 1)]
        public Person Person { get; set; }
    }

    [DataContract]
    public partial class GetSinglePersonAsAsyncParameters
    {
        [DataMember(Order = 1)]
        public Person Person { get; set; }
    }

    [DataContract]
    public partial class GetManyPersonsParameters
    {
        [DataMember(Order = 1)]
        public Person Person { get; set; }
    }

    [DataContract]
    public partial class GetManyPersonsAsAsyncParameters
    {
        [DataMember(Order = 1)]
        public Person Person { get; set; }
    }

}

namespace TestService.Core
{
    public partial class PersonRepositoryDescriptor : ContractDescriptor
    {
        public PersonRepositoryDescriptor() : base(typeof(TestService.Core.IPersonRepository))
        {
            UpdatePerson = Add("UpdatePerson", typeof(TestService.Core.Parameters.UpdatePersonParameters), typeof(IPersonRepository).GetTypeInfo().GetMethod("UpdatePerson"));
            UpdatePersonThatThrowsInvalidOperationException = Add("UpdatePersonThatThrowsInvalidOperationException", typeof(TestService.Core.Parameters.UpdatePersonThatThrowsInvalidOperationExceptionParameters), typeof(IPersonRepository).GetTypeInfo().GetMethod("UpdatePersonThatThrowsInvalidOperationException"));
            DoNothingAsAsync = Add("DoNothingAsAsync", typeof(Bolt.Empty), typeof(IPersonRepository).GetTypeInfo().GetMethod("DoNothingAsAsync"));
            DoNothing = Add("DoNothing", typeof(Bolt.Empty), typeof(IPersonRepository).GetTypeInfo().GetMethod("DoNothing"));
            DoNothingWithComplexParameterAsAsync = Add("DoNothingWithComplexParameterAsAsync", typeof(TestService.Core.Parameters.DoNothingWithComplexParameterAsAsyncParameters), typeof(IPersonRepository).GetTypeInfo().GetMethod("DoNothingWithComplexParameterAsAsync"));
            DoNothingWithComplexParameter = Add("DoNothingWithComplexParameter", typeof(TestService.Core.Parameters.DoNothingWithComplexParameterParameters), typeof(IPersonRepository).GetTypeInfo().GetMethod("DoNothingWithComplexParameter"));
            GetSimpleType = Add("GetSimpleType", typeof(TestService.Core.Parameters.GetSimpleTypeParameters), typeof(IPersonRepository).GetTypeInfo().GetMethod("GetSimpleType"));
            GetSimpleTypeAsAsync = Add("GetSimpleTypeAsAsync", typeof(TestService.Core.Parameters.GetSimpleTypeAsAsyncParameters), typeof(IPersonRepository).GetTypeInfo().GetMethod("GetSimpleTypeAsAsync"));
            GetSinglePerson = Add("GetSinglePerson", typeof(TestService.Core.Parameters.GetSinglePersonParameters), typeof(IPersonRepository).GetTypeInfo().GetMethod("GetSinglePerson"));
            GetSinglePersonAsAsync = Add("GetSinglePersonAsAsync", typeof(TestService.Core.Parameters.GetSinglePersonAsAsyncParameters), typeof(IPersonRepository).GetTypeInfo().GetMethod("GetSinglePersonAsAsync"));
            GetManyPersons = Add("GetManyPersons", typeof(TestService.Core.Parameters.GetManyPersonsParameters), typeof(IPersonRepository).GetTypeInfo().GetMethod("GetManyPersons"));
            GetManyPersonsAsAsync = Add("GetManyPersonsAsAsync", typeof(TestService.Core.Parameters.GetManyPersonsAsAsyncParameters), typeof(IPersonRepository).GetTypeInfo().GetMethod("GetManyPersonsAsAsync"));
            InnerOperation = Add("InnerOperation", typeof(Bolt.Empty), typeof(IPersonRepositoryInner).GetTypeInfo().GetMethod("InnerOperation"));
            InnerOperationExAsync = Add("InnerOperationExAsync", typeof(Bolt.Empty), typeof(IPersonRepositoryInner).GetTypeInfo().GetMethod("InnerOperationExAsync"));
            InnerOperation2 = Add("InnerOperation2", typeof(Bolt.Empty), typeof(IPersonRepositoryInner2).GetTypeInfo().GetMethod("InnerOperation2"));
            InnerOperationExAsync2 = Add("InnerOperationExAsync2", typeof(Bolt.Empty), typeof(IPersonRepositoryInner2).GetTypeInfo().GetMethod("InnerOperationExAsync2"));
        }

        public static readonly PersonRepositoryDescriptor Default = new PersonRepositoryDescriptor();

        public virtual ActionDescriptor UpdatePerson { get; set; }

        public virtual ActionDescriptor UpdatePersonThatThrowsInvalidOperationException { get; set; }

        public virtual ActionDescriptor DoNothingAsAsync { get; set; }

        public virtual ActionDescriptor DoNothing { get; set; }

        public virtual ActionDescriptor DoNothingWithComplexParameterAsAsync { get; set; }

        public virtual ActionDescriptor DoNothingWithComplexParameter { get; set; }

        public virtual ActionDescriptor GetSimpleType { get; set; }

        public virtual ActionDescriptor GetSimpleTypeAsAsync { get; set; }

        public virtual ActionDescriptor GetSinglePerson { get; set; }

        public virtual ActionDescriptor GetSinglePersonAsAsync { get; set; }

        public virtual ActionDescriptor GetManyPersons { get; set; }

        public virtual ActionDescriptor GetManyPersonsAsAsync { get; set; }

        public virtual ActionDescriptor InnerOperation { get; set; }

        public virtual ActionDescriptor InnerOperationExAsync { get; set; }

        public virtual ActionDescriptor InnerOperation2 { get; set; }

        public virtual ActionDescriptor InnerOperationExAsync2 { get; set; }
    }
}

