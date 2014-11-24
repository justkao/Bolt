using System;
using System.IO;

namespace Bolt.Console.Test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Configuration config = Configuration.Load(Environment.CurrentDirectory, File.ReadAllText("Configuration.json"));

            config.Generate();
        }
    }
}
