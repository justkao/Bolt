using Bolt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using TestService.Contracts;

namespace TestService.Contracts.Parameters
{
    [DataContract]
    public partial class AddPersonAsyncParameters
    {
        [DataMember(Order = 1)]
        public Person Person { get; set; }
    }

    [DataContract]
    public partial class DeletePersonAsyncParameters
    {
        [DataMember(Order = 1)]
        public int PersonId { get; set; }
    }
}

namespace TestService.Contracts
{
    public interface IPersonRepositoryAsync : IPersonRepository
    {
        Task<string> GetServerNameAsync();
    }
}

