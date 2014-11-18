using Bolt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using TestService.Core;

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

