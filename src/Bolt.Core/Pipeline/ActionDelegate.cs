using System.Threading.Tasks;

namespace Bolt.Pipeline
{
    public delegate Task ActionDelegate<in T>(T context) where T : ActionContextBase;
}