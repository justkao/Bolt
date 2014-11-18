using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bolt
{
    public abstract class ExecutionContextBase
    {
        protected ExecutionContextBase(MethodDescriptor methodDescriptor)
        {
            MethodDescriptor = methodDescriptor;
        }

        public MethodDescriptor MethodDescriptor { get; private set; }
    }
}
