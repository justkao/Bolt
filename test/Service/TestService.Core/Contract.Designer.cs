using Bolt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using TestService.Core;

namespace TestService.Core
{
    public static class PersonRepositoryDescriptor
    {
        public static readonly MethodDescriptor UpdatePerson = new MethodDescriptor("PersonRepository","UpdatePerson","PersonRepository/UpdatePerson", typeof(TestService.Core.Parameters.UpdatePersonParameters));

        public static readonly MethodDescriptor DoNothingAsAsync = new MethodDescriptor("PersonRepository","DoNothingAsAsync","PersonRepository/DoNothingAsAsync", typeof(Bolt.Empty));

        public static readonly MethodDescriptor DoNothing = new MethodDescriptor("PersonRepository","DoNothing","PersonRepository/DoNothing", typeof(Bolt.Empty));

        public static readonly MethodDescriptor DoNothingWithComplexParameterAsAsync = new MethodDescriptor("PersonRepository","DoNothingWithComplexParameterAsAsync","PersonRepository/DoNothingWithComplexParameterAsAsync", typeof(TestService.Core.Parameters.DoNothingWithComplexParameterAsAsyncParameters));

        public static readonly MethodDescriptor DoNothingWithComplexParameter = new MethodDescriptor("PersonRepository","DoNothingWithComplexParameter","PersonRepository/DoNothingWithComplexParameter", typeof(TestService.Core.Parameters.DoNothingWithComplexParameterParameters));

        public static readonly MethodDescriptor GetSimpleType = new MethodDescriptor("PersonRepository","GetSimpleType","PersonRepository/GetSimpleType", typeof(TestService.Core.Parameters.GetSimpleTypeParameters));

        public static readonly MethodDescriptor GetSimpleTypeAsAsync = new MethodDescriptor("PersonRepository","GetSimpleTypeAsAsync","PersonRepository/GetSimpleTypeAsAsync", typeof(TestService.Core.Parameters.GetSimpleTypeAsAsyncParameters));

        public static readonly MethodDescriptor GetSinglePerson = new MethodDescriptor("PersonRepository","GetSinglePerson","PersonRepository/GetSinglePerson", typeof(TestService.Core.Parameters.GetSinglePersonParameters));

        public static readonly MethodDescriptor GetSinglePersonAsAsync = new MethodDescriptor("PersonRepository","GetSinglePersonAsAsync","PersonRepository/GetSinglePersonAsAsync", typeof(TestService.Core.Parameters.GetSinglePersonAsAsyncParameters));

        public static readonly MethodDescriptor GetManyPersons = new MethodDescriptor("PersonRepository","GetManyPersons","PersonRepository/GetManyPersons", typeof(TestService.Core.Parameters.GetManyPersonsParameters));

        public static readonly MethodDescriptor GetManyPersonsAsAsync = new MethodDescriptor("PersonRepository","GetManyPersonsAsAsync","PersonRepository/GetManyPersonsAsAsync", typeof(TestService.Core.Parameters.GetManyPersonsAsAsyncParameters));

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

