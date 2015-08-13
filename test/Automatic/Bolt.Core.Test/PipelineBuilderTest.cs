using System.Collections.Generic;
using System.Threading.Tasks;
using Bolt.Pipeline;
using Xunit;

namespace Bolt.Core.Test
{
    public class PipelineBuilderTest
    {
        [Fact]
        public async Task EnsurePropertExecutionOrder()
        {
            List<string> target = new List<string>();

            PipelineBuilder<ActionContextBase> builder = new PipelineBuilder<ActionContextBase>();
            PipelineResult<ActionContextBase> pipeline = builder.Use(new Middleware("first", target)).Use(new Middleware("second", target)).Build();

            await pipeline.Instance(new TestActionContext());

            Assert.Equal(4, target.Count);

            Assert.Equal("first", target[0]);
            Assert.Equal("second", target[1]);
            Assert.Equal("second", target[2]);
            Assert.Equal("first", target[3]);
        }

        public class Middleware : MiddlewareBase<ActionContextBase>
        {
            private readonly string _name;

            private readonly List<string> _target;

            public Middleware(string name, List<string> target)
            {
                _name = name;
                _target = target;
            }

            public override async Task Invoke(ActionContextBase context)
            {
                _target.Add(_name);
                await Next(context);
                _target.Add(_name);
            }
        }
    }
}
