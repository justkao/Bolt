using Bolt.Common;
using Microsoft.AspNet.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Bolt.Server
{
    public class EmptyParameterBinder : IParameterBinder
    {
        public async Task<T> BindParametersAsync<T>(ServerActionContext context)
        {
            return default(T);
        }
    }
}