``` ini

BenchmarkDotNet=v0.10.13, OS=Windows 10 Redstone 3 [1709, Fall Creators Update] (10.0.16299.309)
Intel Core i5-4690K CPU 3.50GHz (Haswell), 1 CPU, 4 logical cores and 4 physical cores
Frequency=3415991 Hz, Resolution=292.7408 ns, Timer=TSC
.NET Core SDK=2.1.300-preview1-008174
  [Host]     : .NET Core 2.0.5 (CoreCLR 4.6.26020.03, CoreFX 4.6.26018.01), 64bit RyuJIT
  Job-FHCPMP : .NET Core 2.0.5 (CoreCLR 4.6.26020.03, CoreFX 4.6.26018.01), 64bit RyuJIT

RemoveOutliers=False  Runtime=Core  Server=True  
Toolchain=.NET Core 2.0  LaunchCount=2  RunStrategy=Throughput  
TargetCount=10  WarmupCount=5  

```
|                  Method | UseMessagePack |          Mean |         Error |        StdDev |       Op/s |  Version | Rank |     Gen 0 |     Gen 1 |     Gen 2 |   Allocated |
|------------------------ |--------------- |--------------:|--------------:|--------------:|-----------:|--------- |-----:|----------:|----------:|----------:|------------:|
|            **Method_Async** |          **False** |      **24.37 us** |     **0.3399 us** |     **0.3914 us** | **41,030.389** | **0.10.2.0** |    **2** |    **0.3052** |         **-** |         **-** |     **7.66 KB** |
|        Method_Int_Async |          False |      54.84 us |     0.2774 us |     0.3195 us | 18,233.707 | 0.10.2.0 |   13 |    1.2207 |         - |         - |    29.98 KB |
|      Method_Large_Async |          False |     941.43 us |     0.8801 us |     1.0136 us |  1,062.218 | 0.10.2.0 |   21 |   14.6484 |    0.9766 |         - |   280.12 KB |
|       Method_Many_Async |          False |      76.04 us |     0.2327 us |     0.2679 us | 13,150.128 | 0.10.2.0 |   17 |    1.3428 |         - |         - |    32.44 KB |
|     Method_Object_Async |          False |      71.26 us |     0.4273 us |     0.4921 us | 14,033.309 | 0.10.2.0 |   16 |    1.3428 |         - |         - |    32.38 KB |
|     Method_String_Async |          False |      54.38 us |     0.2369 us |     0.2728 us | 18,388.810 | 0.10.2.0 |   12 |    1.2817 |         - |         - |    29.98 KB |
|       Return_Ints_Async |          False |      57.02 us |     2.6424 us |     3.0430 us | 17,537.172 | 0.10.2.0 |   14 |    1.0986 |         - |         - |     7.87 KB |
|        Return_Int_Async |          False |      43.12 us |     0.8191 us |     0.9433 us | 23,191.945 | 0.10.2.0 |   11 |    1.0376 |         - |         - |     7.87 KB |
|      Return_Large_Async |          False |     584.97 us |     2.6891 us |     3.0968 us |  1,709.500 | 0.10.2.0 |   20 |    7.8125 |         - |         - |     7.87 KB |
|    Return_Objects_Async |          False |     105.29 us |     3.6991 us |     4.2599 us |  9,497.897 | 0.10.2.0 |   18 |    1.4648 |         - |         - |      7.9 KB |
|     Return_Object_Async |          False |      69.75 us |     3.2862 us |     3.7844 us | 14,337.246 | 0.10.2.0 |   16 |    1.0986 |         - |         - |     7.88 KB |
|    Return_Strings_Async |          False |      58.85 us |     2.0599 us |     2.3722 us | 16,993.001 | 0.10.2.0 |   15 |    1.0986 |         - |         - |      7.9 KB |
|     Return_String_Async |          False |      42.50 us |     0.7419 us |     0.8543 us | 23,531.010 | 0.10.2.0 |   10 |    1.0986 |         - |         - |     7.88 KB |
| Method_ThrowsErrorAsync |          False |   2,885.89 us |     4.2454 us |     4.8890 us |    346.514 | 0.10.2.0 |   23 |   15.6250 |         - |         - |     8.02 KB |
| Return_Very_Large_Async |          False | 245,890.33 us | 1,698.0264 us | 1,955.4495 us |      4.067 | 0.10.2.0 |   27 |  437.5000 |  187.5000 |   62.5000 |    30.25 KB |
| Method_Very_Large_Async |          False |  82,218.82 us |   629.3214 us |   724.7274 us |     12.163 | 0.10.2.0 |   26 | 1187.5000 |  875.0000 |  750.0000 | 27055.51 KB |
|            **Method_Async** |           **True** |      **24.01 us** |     **0.2128 us** |     **0.2450 us** | **41,646.156** | **0.10.2.0** |    **1** |    **0.3662** |         **-** |         **-** |     **7.67 KB** |
|        Method_Int_Async |           True |      32.91 us |     0.4146 us |     0.4775 us | 30,383.915 | 0.10.2.0 |    3 |    0.3052 |         - |         - |      8.2 KB |
|      Method_Large_Async |           True |     116.37 us |     0.2450 us |     0.2821 us |  8,593.572 | 0.10.2.0 |   19 |    0.9766 |         - |         - |    12.32 KB |
|       Method_Many_Async |           True |      41.63 us |     0.0481 us |     0.0554 us | 24,021.282 | 0.10.2.0 |    9 |    0.3662 |         - |         - |     8.27 KB |
|     Method_Object_Async |           True |      37.91 us |     0.1348 us |     0.1553 us | 26,376.832 | 0.10.2.0 |    7 |    0.3662 |         - |         - |     8.22 KB |
|     Method_String_Async |           True |      35.41 us |     0.6767 us |     0.7793 us | 28,241.074 | 0.10.2.0 |    5 |    0.3662 |         - |         - |      8.2 KB |
|       Return_Ints_Async |           True |      36.80 us |     0.7497 us |     0.8634 us | 27,172.475 | 0.10.2.0 |    6 |    0.3052 |         - |         - |     7.86 KB |
|        Return_Int_Async |           True |      34.74 us |     0.6733 us |     0.7754 us | 28,785.143 | 0.10.2.0 |    4 |    0.2441 |         - |         - |     7.85 KB |
|      Return_Large_Async |           True |     116.19 us |     1.1460 us |     1.3198 us |  8,606.387 | 0.10.2.0 |   19 |    0.8545 |         - |         - |     7.88 KB |
|    Return_Objects_Async |           True |      42.22 us |     0.9316 us |     1.0729 us | 23,685.282 | 0.10.2.0 |   10 |    0.3662 |         - |         - |      7.9 KB |
|     Return_Object_Async |           True |      39.06 us |     0.6392 us |     0.7361 us | 25,603.181 | 0.10.2.0 |    8 |    0.3052 |         - |         - |     7.88 KB |
|    Return_Strings_Async |           True |      38.20 us |     0.8732 us |     1.0056 us | 26,175.053 | 0.10.2.0 |    7 |    0.3052 |         - |         - |     7.89 KB |
|     Return_String_Async |           True |      36.92 us |     0.2074 us |     0.2388 us | 27,086.627 | 0.10.2.0 |    6 |    0.3052 |         - |         - |     7.87 KB |
| Method_ThrowsErrorAsync |           True |   2,867.60 us |     6.5798 us |     7.5773 us |    348.724 | 0.10.2.0 |   22 |    7.8125 |         - |         - |     8.02 KB |
| Return_Very_Large_Async |           True |  45,477.19 us |   991.5604 us | 1,141.8823 us |     21.989 | 0.10.2.0 |   25 | 1250.0000 | 1187.5000 | 1187.5000 |     8.41 KB |
| Method_Very_Large_Async |           True |   8,101.70 us |   101.2858 us |   116.6409 us |    123.431 | 0.10.2.0 |   24 |  375.0000 |  343.7500 |  343.7500 |   1325.7 KB |
