using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace Bolt.Client
{
    public static class HttpHeadersExtensions
    {
        public static string GetHeaderValue(this HttpResponseHeaders headers, string header)
        {
            IEnumerable<string> values;

            if (headers.TryGetValues(header, out values))
            {
                return values.FirstOrDefault();
            }

            return null;
        }
    }
}