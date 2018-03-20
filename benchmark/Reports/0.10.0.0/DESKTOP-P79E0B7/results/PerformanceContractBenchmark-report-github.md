``` ini

BenchmarkDotNet=v0.10.13, OS=Windows 10 Redstone 3 [1709, Fall Creators Update] (10.0.16299.309)
Intel Core i5-4690K CPU 3.50GHz (Haswell), 1 CPU, 4 logical cores and 4 physical cores
Frequency=3415991 Hz, Resolution=292.7408 ns, Timer=TSC
.NET Core SDK=2.1.300-preview1-008174
  [Host]     : .NET Core 2.0.5 (CoreCLR 4.6.26020.03, CoreFX 4.6.26018.01), 64bit RyuJIT
  Job-HARMNA : .NET Core 2.0.5 (CoreCLR 4.6.26020.03, CoreFX 4.6.26018.01), 64bit RyuJIT

RemoveOutliers=False  Runtime=Core  Server=True  
Toolchain=.NET Core 2.0  LaunchCount=2  RunStrategy=Throughput  
TargetCount=10  WarmupCount=5  

```
|                  Method | UseMessagePack |        Mean |      Error |     StdDev |     Op/s |  Version | Rank |   Gen 0 | Allocated |
|------------------------ |--------------- |------------:|-----------:|-----------:|---------:|--------- |-----:|--------:|----------:|
|            **Method_Async** |          **False** |    **24.29 us** |  **0.0662 us** |  **0.0763 us** | **41,169.3** | **0.10.0.0** |    **2** |  **0.3357** |   **7.66 KB** |
|        Method_Int_Async |          False |    47.54 us |  0.3679 us |  0.4237 us | 21,032.9 | 0.10.0.0 |   15 |  1.5259 |  28.95 KB |
|      Method_Large_Async |          False |   658.53 us |  7.1698 us |  8.2567 us |  1,518.5 | 0.10.0.0 |   23 |  2.9297 |  65.73 KB |
|       Method_Many_Async |          False |    71.41 us |  0.2220 us |  0.2557 us | 14,003.0 | 0.10.0.0 |   19 |  1.2207 |  30.13 KB |
|     Method_Object_Async |          False |    68.46 us |  0.2683 us |  0.3090 us | 14,607.2 | 0.10.0.0 |   18 |  1.3428 |  30.07 KB |
|     Method_String_Async |          False |    49.80 us |  0.5381 us |  0.6196 us | 20,081.2 | 0.10.0.0 |   16 |  1.2817 |  28.95 KB |
|       Return_Ints_Async |          False |    44.77 us |  1.1508 us |  1.3252 us | 22,335.6 | 0.10.0.0 |   14 |  1.0376 |   7.87 KB |
|        Return_Int_Async |          False |    42.33 us |  0.6907 us |  0.7955 us | 23,624.9 | 0.10.0.0 |   12 |  1.0986 |   7.86 KB |
|      Return_Large_Async |          False |   452.65 us |  1.6778 us |  1.9322 us |  2,209.2 | 0.10.0.0 |   22 |  1.9531 |   7.87 KB |
|    Return_Objects_Async |          False |    83.19 us |  2.7182 us |  3.1303 us | 12,021.2 | 0.10.0.0 |   20 |  1.0986 |    7.9 KB |
|     Return_Object_Async |          False |    61.93 us |  2.6789 us |  3.0850 us | 16,146.9 | 0.10.0.0 |   17 |  0.9766 |   7.88 KB |
|    Return_Strings_Async |          False |    47.40 us |  2.3462 us |  2.7018 us | 21,095.9 | 0.10.0.0 |   15 |  1.0376 |    7.9 KB |
|     Return_String_Async |          False |    42.68 us |  0.5845 us |  0.6731 us | 23,427.7 | 0.10.0.0 |   12 |  1.0376 |   7.88 KB |
| Method_ThrowsErrorAsync |          False | 2,937.54 us | 43.3020 us | 49.8667 us |    340.4 | 0.10.0.0 |   25 | 11.7188 |   8.02 KB |
|            **Method_Async** |           **True** |    **24.24 us** |  **0.0283 us** |  **0.0326 us** | **41,255.8** | **0.10.0.0** |    **1** |  **0.3967** |   **7.66 KB** |
|        Method_Int_Async |           True |    34.88 us |  0.3364 us |  0.3874 us | 28,673.5 | 0.10.0.0 |    6 |  0.3662 |   8.21 KB |
|      Method_Large_Async |           True |   115.56 us |  0.2654 us |  0.3056 us |  8,653.6 | 0.10.0.0 |   21 |  0.9766 |  12.32 KB |
|       Method_Many_Async |           True |    43.82 us |  0.1335 us |  0.1537 us | 22,822.8 | 0.10.0.0 |   13 |  0.3662 |   8.27 KB |
|     Method_Object_Async |           True |    40.16 us |  0.0830 us |  0.0956 us | 24,902.8 | 0.10.0.0 |   10 |  0.3662 |   8.22 KB |
|     Method_String_Async |           True |    36.08 us |  0.3859 us |  0.4444 us | 27,717.6 | 0.10.0.0 |    8 |  0.3052 |   8.21 KB |
|       Return_Ints_Async |           True |    33.71 us |  0.1067 us |  0.1229 us | 29,663.6 | 0.10.0.0 |    5 |  0.3052 |   7.84 KB |
|        Return_Int_Async |           True |    32.21 us |  0.1226 us |  0.1412 us | 31,048.7 | 0.10.0.0 |    3 |  0.2441 |   7.83 KB |
|      Return_Large_Async |           True |   115.92 us |  1.3168 us |  1.5164 us |  8,626.7 | 0.10.0.0 |   21 |  0.8545 |   7.88 KB |
|    Return_Objects_Async |           True |    40.92 us |  0.7693 us |  0.8859 us | 24,440.7 | 0.10.0.0 |   11 |  0.3052 |    7.9 KB |
|     Return_Object_Async |           True |    36.66 us |  0.2101 us |  0.2420 us | 27,279.6 | 0.10.0.0 |    9 |  0.3052 |   7.88 KB |
|    Return_Strings_Async |           True |    35.10 us |  0.1393 us |  0.1604 us | 28,491.7 | 0.10.0.0 |    7 |  0.2441 |   7.88 KB |
|     Return_String_Async |           True |    33.25 us |  0.1126 us |  0.1297 us | 30,073.1 | 0.10.0.0 |    4 |  0.2441 |   7.86 KB |
| Method_ThrowsErrorAsync |           True | 2,862.74 us |  9.8431 us | 11.3353 us |    349.3 | 0.10.0.0 |   24 |  7.8125 |   8.02 KB |
