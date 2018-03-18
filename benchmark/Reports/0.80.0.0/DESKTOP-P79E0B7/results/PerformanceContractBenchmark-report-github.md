``` ini

BenchmarkDotNet=v0.10.13, OS=Windows 10 Redstone 3 [1709, Fall Creators Update] (10.0.16299.309)
Intel Core i5-4690K CPU 3.50GHz (Haswell), 1 CPU, 4 logical cores and 4 physical cores
Frequency=3415991 Hz, Resolution=292.7408 ns, Timer=TSC
.NET Core SDK=2.1.300-preview1-008174
  [Host]     : .NET Core 2.0.5 (CoreCLR 4.6.26020.03, CoreFX 4.6.26018.01), 64bit RyuJIT
  Job-WBODDK : .NET Core 2.0.5 (CoreCLR 4.6.26020.03, CoreFX 4.6.26018.01), 64bit RyuJIT

RemoveOutliers=False  Runtime=Core  Server=True  
Toolchain=.NET Core 2.0  LaunchCount=2  RunStrategy=Throughput  
TargetCount=10  WarmupCount=5  

```
|                  Method |        Mean |      Error |     StdDev |     Op/s |  Version | Rank |  Gen 0 | Allocated |
|------------------------ |------------:|-----------:|-----------:|---------:|--------- |-----:|-------:|----------:|
|            Method_Async |    24.74 us |  0.2365 us |  0.2723 us | 40,422.4 | 0.80.0.0 |    1 | 0.3357 |   7.66 KB |
|        Method_Int_Async |    48.41 us |  0.6938 us |  0.7990 us | 20,656.0 | 0.80.0.0 |    4 | 1.4648 |  28.76 KB |
|      Method_Large_Async |   630.68 us |  6.7108 us |  7.7282 us |  1,585.6 | 0.80.0.0 |   11 | 2.9297 |  64.13 KB |
|       Method_Many_Async |    68.47 us |  0.2151 us |  0.2477 us | 14,604.1 | 0.80.0.0 |    8 | 1.5869 |  29.45 KB |
|     Method_Object_Async |    64.88 us |  1.0586 us |  1.2191 us | 15,412.5 | 0.80.0.0 |    6 | 1.4648 |  29.04 KB |
|     Method_String_Async |    46.78 us |  0.6047 us |  0.6964 us | 21,374.8 | 0.80.0.0 |    4 | 1.5869 |  28.76 KB |
|       Return_Ints_Async |    47.78 us |  3.1270 us |  3.6011 us | 20,929.5 | 0.80.0.0 |    4 | 1.0986 |   7.87 KB |
|        Return_Int_Async |    42.92 us |  2.6300 us |  3.0288 us | 23,300.1 | 0.80.0.0 |    2 | 1.0986 |   7.86 KB |
|      Return_Large_Async |   438.58 us |  3.2401 us |  3.7313 us |  2,280.1 | 0.80.0.0 |   10 | 2.4414 |   7.87 KB |
|    Return_Objects_Async |    92.29 us |  1.9255 us |  2.2175 us | 10,835.8 | 0.80.0.0 |    9 | 1.0986 |    7.9 KB |
|     Return_Object_Async |    67.29 us |  2.0698 us |  2.3835 us | 14,861.2 | 0.80.0.0 |    7 | 1.0986 |   7.88 KB |
|    Return_Strings_Async |    50.87 us |  3.9931 us |  4.5984 us | 19,659.0 | 0.80.0.0 |    5 | 1.0986 |    7.9 KB |
|     Return_String_Async |    44.67 us |  1.8759 us |  2.1603 us | 22,384.5 | 0.80.0.0 |    3 | 1.0986 |   7.88 KB |
| Method_ThrowsErrorAsync | 3,495.09 us | 74.5113 us | 85.8073 us |    286.1 | 0.80.0.0 |   12 | 7.8125 |   8.02 KB |
