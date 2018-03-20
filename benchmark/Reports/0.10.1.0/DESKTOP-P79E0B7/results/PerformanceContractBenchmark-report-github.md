``` ini

BenchmarkDotNet=v0.10.13, OS=Windows 10 Redstone 3 [1709, Fall Creators Update] (10.0.16299.309)
Intel Core i5-4690K CPU 3.50GHz (Haswell), 1 CPU, 4 logical cores and 4 physical cores
Frequency=3415991 Hz, Resolution=292.7408 ns, Timer=TSC
.NET Core SDK=2.1.300-preview1-008174
  [Host]     : .NET Core 2.0.5 (CoreCLR 4.6.26020.03, CoreFX 4.6.26018.01), 64bit RyuJIT
  Job-TQGBII : .NET Core 2.0.5 (CoreCLR 4.6.26020.03, CoreFX 4.6.26018.01), 64bit RyuJIT

RemoveOutliers=False  Runtime=Core  Server=True  
Toolchain=.NET Core 2.0  LaunchCount=2  RunStrategy=Throughput  
TargetCount=10  WarmupCount=5  

```
|                  Method | UseMessagePack |        Mean |      Error |     StdDev |     Op/s |  Version | Rank |   Gen 0 |  Gen 1 | Allocated |
|------------------------ |--------------- |------------:|-----------:|-----------:|---------:|--------- |-----:|--------:|-------:|----------:|
|            **Method_Async** |          **False** |    **24.47 us** |  **0.2047 us** |  **0.2357 us** | **40,872.6** | **0.10.1.0** |    **1** |  **0.3052** |      **-** |   **7.66 KB** |
|        Method_Int_Async |          False |    55.54 us |  0.5727 us |  0.6595 us | 18,005.4 | 0.10.1.0 |   14 |  1.2817 |      - |  29.98 KB |
|      Method_Large_Async |          False |   950.52 us |  5.5383 us |  6.3779 us |  1,052.1 | 0.10.1.0 |   22 | 14.6484 | 0.9766 | 280.12 KB |
|       Method_Many_Async |          False |    77.35 us |  0.3014 us |  0.3470 us | 12,928.1 | 0.10.1.0 |   17 |  1.3428 |      - |  32.44 KB |
|     Method_Object_Async |          False |    73.29 us |  0.2498 us |  0.2877 us | 13,645.0 | 0.10.1.0 |   16 |  1.3428 |      - |  32.38 KB |
|     Method_String_Async |          False |    54.69 us |  0.3664 us |  0.4220 us | 18,285.5 | 0.10.1.0 |   13 |  1.2817 |      - |  29.98 KB |
|       Return_Ints_Async |          False |    53.93 us |  2.1229 us |  2.4447 us | 18,541.7 | 0.10.1.0 |   13 |  1.0986 |      - |   7.87 KB |
|        Return_Int_Async |          False |    41.39 us |  0.5137 us |  0.5916 us | 24,159.1 | 0.10.1.0 |   10 |  1.0376 |      - |   7.86 KB |
|      Return_Large_Async |          False |   572.23 us |  2.6222 us |  3.0197 us |  1,747.5 | 0.10.1.0 |   21 |  7.8125 |      - |   7.87 KB |
|    Return_Objects_Async |          False |   105.51 us |  3.9838 us |  4.5877 us |  9,477.4 | 0.10.1.0 |   18 |  1.4648 |      - |    7.9 KB |
|     Return_Object_Async |          False |    67.50 us |  2.1379 us |  2.4621 us | 14,815.1 | 0.10.1.0 |   15 |  1.0986 |      - |   7.88 KB |
|    Return_Strings_Async |          False |    53.56 us |  1.5555 us |  1.7914 us | 18,671.2 | 0.10.1.0 |   13 |  1.1597 |      - |    7.9 KB |
|     Return_String_Async |          False |    42.09 us |  0.5660 us |  0.6518 us | 23,760.5 | 0.10.1.0 |   11 |  1.0986 |      - |   7.88 KB |
| Method_ThrowsErrorAsync |          False | 2,954.84 us | 39.3152 us | 45.2755 us |    338.4 | 0.10.1.0 |   24 | 15.6250 |      - |   8.02 KB |
|            **Method_Async** |           **True** |    **24.51 us** |  **0.1380 us** |  **0.1590 us** | **40,798.7** | **0.10.1.0** |    **1** |  **0.3052** |      **-** |   **7.67 KB** |
|        Method_Int_Async |           True |    35.75 us |  0.1754 us |  0.2020 us | 27,975.0 | 0.10.1.0 |    4 |  0.3662 |      - |   8.21 KB |
|      Method_Large_Async |           True |   116.10 us |  0.1723 us |  0.1984 us |  8,613.5 | 0.10.1.0 |   20 |  0.9766 |      - |  12.32 KB |
|       Method_Many_Async |           True |    43.19 us |  0.1588 us |  0.1829 us | 23,153.4 | 0.10.1.0 |   12 |  0.3052 |      - |   8.27 KB |
|     Method_Object_Async |           True |    39.54 us |  0.2419 us |  0.2786 us | 25,291.4 | 0.10.1.0 |    9 |  0.3662 |      - |   8.22 KB |
|     Method_String_Async |           True |    35.51 us |  0.2195 us |  0.2528 us | 28,162.8 | 0.10.1.0 |    3 |  0.3662 |      - |   8.21 KB |
|       Return_Ints_Async |           True |    36.87 us |  0.2087 us |  0.2404 us | 27,122.1 | 0.10.1.0 |    6 |  0.3052 |      - |   7.86 KB |
|        Return_Int_Async |           True |    34.34 us |  0.1442 us |  0.1661 us | 29,120.4 | 0.10.1.0 |    2 |  0.3052 |      - |   7.84 KB |
|      Return_Large_Async |           True |   114.58 us |  0.6902 us |  0.7949 us |  8,727.9 | 0.10.1.0 |   19 |  0.8545 |      - |   7.88 KB |
|    Return_Objects_Async |           True |    41.48 us |  0.8826 us |  1.0164 us | 24,108.5 | 0.10.1.0 |   10 |  0.3052 |      - |    7.9 KB |
|     Return_Object_Async |           True |    38.81 us |  0.4441 us |  0.5115 us | 25,764.5 | 0.10.1.0 |    8 |  0.3052 |      - |   7.88 KB |
|    Return_Strings_Async |           True |    38.22 us |  0.2222 us |  0.2559 us | 26,161.9 | 0.10.1.0 |    7 |  0.3052 |      - |   7.89 KB |
|     Return_String_Async |           True |    36.42 us |  0.2355 us |  0.2712 us | 27,454.1 | 0.10.1.0 |    5 |  0.3052 |      - |   7.87 KB |
| Method_ThrowsErrorAsync |           True | 2,908.51 us | 17.0055 us | 19.5836 us |    343.8 | 0.10.1.0 |   23 | 11.7188 |      - |   8.02 KB |
