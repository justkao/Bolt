using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace build
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Current Directory: {0}", Directory.GetCurrentDirectory());

            string job = args.FirstOrDefault();
            BoltBuild build = new BoltBuild();
            if (string.IsNullOrEmpty(job))
            {
                build.Build();
            }
            else
            {
                MethodInfo found = build.GetType()
                    .GetTypeInfo()
                    .DeclaredMethods.FirstOrDefault(m => string.Equals(m.Name, job, StringComparison.OrdinalIgnoreCase) && m.GetParameters().Length == 0);
                if (found == null)
                {
                    throw new InvalidOperationException($"Job - '{job}' not found.");
                }

                found.Invoke(build, null);
            }
        }
    }
}
