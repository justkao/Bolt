using System.Collections.Generic;
using System.Threading.Tasks;
using Bolt.Pipeline;
using Xunit;

namespace Bolt.Core.Test
{
    public class DelegeatedMiddlewareTest
    {
        [Fact]
        public async Task EnsurePropertExecutionOrder()
        {
            List<string> target = new List<string>();

            PipelineBuilder<ActionContextBase> builder = new PipelineBuilder<ActionContextBase>();
            PipelineResult<ActionContextBase> pipeline = builder.Use(
                async (next, ctxt) =>
                    {
                        target.Add("first");
                        await next(ctxt);
                        target.Add("third");
                    }).Use(
                        (next, ctxt) =>
                            {
                                target.Add("second");
                                return next(ctxt);
                            }).Build();

            await pipeline.Instance(new TestActionContext());

            Assert.Equal(3, target.Count);

            Assert.Equal("first", target[0]);
            Assert.Equal("second", target[1]);
            Assert.Equal("third", target[2]);
        }

        [Fact]
        public async Task EnsurePropertExecutionOrder_2()
        {
            List<string> target = new List<string>();

            PipelineBuilder<ActionContextBase> builder = new PipelineBuilder<ActionContextBase>();
            PipelineResult<ActionContextBase> pipeline = builder.Use(
                async (next, ctxt) =>
                    {
                        await next(ctxt);
                        target.Add("third");
                    }).Use(async (next, ctxt) =>
                            {
                                target.Add("first");
                                await next(ctxt);
                                target.Add("second");
                            }).Build();

            await pipeline.Instance(new TestActionContext());

            Assert.Equal(3, target.Count);

            Assert.Equal("first", target[0]);
            Assert.Equal("second", target[1]);
            Assert.Equal("third", target[2]);
        }
    }
}