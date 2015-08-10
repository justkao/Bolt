using System;
using System.Threading.Tasks;
using Bolt.Core;

namespace Bolt
{
    public delegate Task ActionDelegate<in T>(T context) where T : ActionContextBase;
}