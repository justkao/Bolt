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
using System.Text;
using System.Threading.Tasks;

using Bolt.Server.IntegrationTest.Core;


namespace Bolt.Server.IntegrationTest.Core
{
    public partial interface ITestContractStateFullAsync : ITestContractStateFull
    {
        Task SetStateAsync(string state);

        Task<string> GetStateAsync();

        Task NextCallWillFailProxyAsync();

        Task<string> GetSessionIdAsync();
    }
}