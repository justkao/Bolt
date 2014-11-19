using Bolt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using TestService.Core;

namespace TestService.Core
{
    public class PersonRepositoryDescriptor : ContractDescriptor
    {
        public PersonRepositoryDescriptor() : base(typeof(TestService.Core.IPersonRepository))
        {
            UpdatePerson = Add("UpdatePerson", typeof(TestService.Core.Parameters.UpdatePersonParameters), typeof(IPersonRepository).GetTypeInfo().GetMethod("UpdatePerson"));
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
        }

        public static readonly PersonRepositoryDescriptor Instance = new PersonRepositoryDescriptor();

        public ActionDescriptor UpdatePerson { get; private set; }

        public ActionDescriptor DoNothingAsAsync { get; private set; }

        public ActionDescriptor DoNothing { get; private set; }

        public ActionDescriptor DoNothingWithComplexParameterAsAsync { get; private set; }

        public ActionDescriptor DoNothingWithComplexParameter { get; private set; }

        public ActionDescriptor GetSimpleType { get; private set; }

        public ActionDescriptor GetSimpleTypeAsAsync { get; private set; }

        public ActionDescriptor GetSinglePerson { get; private set; }

        public ActionDescriptor GetSinglePersonAsAsync { get; private set; }

        public ActionDescriptor GetManyPersons { get; private set; }

        public ActionDescriptor GetManyPersonsAsAsync { get; private set; }

        public ActionDescriptor InnerOperation { get; private set; }

        public ActionDescriptor InnerOperationExAsync { get; private set; }

    }
}

namespace TestService.Core.Parameters
{
    [DataContract]
    public partial class UpdatePersonParameters
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

namespace TestService.Core.Parameters
{
}

