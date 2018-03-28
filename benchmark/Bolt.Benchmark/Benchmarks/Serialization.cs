using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains;
using BenchmarkDotNet.Toolchains.CsProj;
using BenchmarkDotNet.Toolchains.InProcess;
using BenchmarkDotNet.Validators;
using Bolt.Benchmark.Contracts;
using Bolt.Performance.Core.Benchmark;
using Bolt.Serialization;
using Bolt.Serialization.MessagePack;

namespace Bolt.Benchmark.Benchmarks
{
    [RankColumn]
    [Config(typeof(Config))]
    public class Serialization
    {
        public enum SerializerType
        {
            Json,
            MessagePack
        }

        private class Config : ConfigBase
        {
            public Config()
            {
                Add(JitOptimizationsValidator.FailOnError);
                AddJob(Job.Core, InProcessToolchain.Instance);
            }

            private void AddJob(Job job, IToolchain toolchain)
            {
                Add(job.With(toolchain)
                    .WithRemoveOutliers(false)
                    .With(RunStrategy.Throughput));
            }
        }

        private ISerializer _serializer;
        private List<Person> _result;
        private MemoryStream _resultSerialized;
        private MemoryStream _buffer;

        private object[] _parameters;
        private List<ParameterMetadata> _parameterMetadata;
        private MemoryStream _parametersSerialized;

        [Params(SerializerType.Json, SerializerType.MessagePack)]
        public SerializerType Serializer { get; set; }

        [GlobalSetup]
        public async Task GlobalSetupAsync()
        {
            _buffer = new MemoryStream(new byte[100000]);

            switch (Serializer)
            {
                case SerializerType.Json:
                    _serializer = new JsonSerializer();
                    break;
                case SerializerType.MessagePack:
                    _serializer = new MessagePackSerializer();
                    break;
                default:
                    break;
            }

            _result = Enumerable.Range(0, 15).Select(v => Person.Create(v)).ToList();
            _resultSerialized = new MemoryStream();
            await _serializer.WriteAsync(_resultSerialized, _result.GetType(), _result);

            _parametersSerialized = new MemoryStream();
            _parameters = new object[] { 56, "dummy", new[] { Person.Create(1), Person.Create(2) }, DateTime.UtcNow, true };
            _parameterMetadata = _parameters.Select((p, i) => new ParameterMetadata(p.GetType(), i.ToString(CultureInfo.InvariantCulture))).ToList();
            await _serializer.WriteParametersAsync(_parametersSerialized, _parameterMetadata, _parameters);
        }

        [Benchmark]
        public Task SerializeResult()
        {
            _buffer.Seek(0, SeekOrigin.Begin);
            return _serializer.WriteAsync(_buffer, _result.GetType(), _result);
        }

        [Benchmark]
        public Task DeserializeResult()
        {
            _resultSerialized.Seek(0, SeekOrigin.Begin);
            return _serializer.ReadAsync(_resultSerialized, _result.GetType());
        }

        [Benchmark]
        public Task SerializeParameters()
        {
            _buffer.Seek(0, SeekOrigin.Begin);
            return _serializer.WriteParametersAsync(_buffer, _parameterMetadata, _parameters);
        }

        [Benchmark]
        public Task DeserializeParameters()
        {
            _parametersSerialized.Seek(0, SeekOrigin.Begin);
            return _serializer.ReadParametersAsync(_parametersSerialized, _parameterMetadata);
        }
    }
}
