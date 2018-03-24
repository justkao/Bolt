``` ini

BenchmarkDotNet=v0.10.13, OS=Windows 10 Redstone 3 [1709, Fall Creators Update] (10.0.16299.309)
Intel Core i5-4690K CPU 3.50GHz (Haswell), 1 CPU, 4 logical cores and 4 physical cores
Frequency=3415995 Hz, Resolution=292.7405 ns, Timer=TSC
.NET Core SDK=2.1.300-preview1-008174
  [Host]     : .NET Core 2.0.5 (CoreCLR 4.6.26020.03, CoreFX 4.6.26018.01), 64bit RyuJIT
  Job-JPMJEV : .NET Framework 4.6.1 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2633.0
  Job-PFIXIW : .NET Core 2.0.5 (CoreCLR 4.6.26020.03, CoreFX 4.6.26018.01), 64bit RyuJIT
  Job-NHSMXE : .NET Core 2.1.0-preview1-26216-03 (CoreCLR 4.6.26216.04, CoreFX 4.6.26216.02), 64bit RyuJIT

RemoveOutliers=False  Server=True  LaunchCount=2  
RunStrategy=Throughput  TargetCount=10  WarmupCount=5  

```
|                  Method | Runtime |     Toolchain | UseMessagePack |          Mean |         Error |        StdDev |       Op/s |   Version | Rank |      Gen 0 |      Gen 1 |     Gen 2 |    Allocated |
|------------------------ |-------- |-------------- |--------------- |--------------:|--------------:|--------------:|-----------:|---------- |-----:|-----------:|-----------:|----------:|-------------:|
|            **Method_Async** |     **Clr** |  **CsProjnet461** |          **False** |      **27.94 us** |     **0.2238 us** |     **0.2577 us** | **35,788.407** | **0.100.1.0** |    **6** |     **3.6316** |     **0.0305** |         **-** |     **11.22 KB** |
|        Method_Int_Async |     Clr |  CsProjnet461 |          False |      52.00 us |     0.5261 us |     0.6059 us | 19,230.298 | 0.100.1.0 |   25 |    16.3574 |     0.0610 |         - |     50.48 KB |
|      Method_Large_Async |     Clr |  CsProjnet461 |          False |     934.67 us |    19.3247 us |    22.2544 us |  1,069.895 | 0.100.1.0 |   46 |   128.9063 |          - |         - |     400.7 KB |
|       Method_Many_Async |     Clr |  CsProjnet461 |          False |      90.05 us |     0.2912 us |     0.3354 us | 11,105.001 | 0.100.1.0 |   33 |    17.2119 |     0.1221 |         - |     53.27 KB |
|     Method_Object_Async |     Clr |  CsProjnet461 |          False |      88.19 us |     0.3551 us |     0.4089 us | 11,338.990 | 0.100.1.0 |   32 |    17.2119 |     0.1221 |         - |     53.02 KB |
|     Method_String_Async |     Clr |  CsProjnet461 |          False |      51.58 us |     0.3751 us |     0.4320 us | 19,386.391 | 0.100.1.0 |   24 |    16.3574 |     0.0610 |         - |     50.48 KB |
|       Return_Ints_Async |     Clr |  CsProjnet461 |          False |      66.10 us |     1.1209 us |     1.2908 us | 15,128.674 | 0.100.1.0 |   28 |    16.8457 |     0.1221 |         - |     52.21 KB |
|        Return_Int_Async |     Clr |  CsProjnet461 |          False |      49.43 us |     0.6446 us |     0.7423 us | 20,231.931 | 0.100.1.0 |   23 |    16.8457 |     0.0610 |         - |     52.16 KB |
|      Return_Large_Async |     Clr |  CsProjnet461 |          False |     596.63 us |     6.6726 us |     7.6842 us |  1,676.084 | 0.100.1.0 |   44 |   120.1172 |          - |         - |    372.08 KB |
|    Return_Objects_Async |     Clr |  CsProjnet461 |          False |     107.43 us |     2.3426 us |     2.6977 us |  9,308.073 | 0.100.1.0 |   35 |    22.3389 |     0.1221 |         - |     68.94 KB |
|     Return_Object_Async |     Clr |  CsProjnet461 |          False |      82.37 us |     4.0573 us |     4.6724 us | 12,140.801 | 0.100.1.0 |   31 |    16.9678 |     0.1221 |         - |     52.49 KB |
|    Return_Strings_Async |     Clr |  CsProjnet461 |          False |      69.32 us |     1.8057 us |     2.0795 us | 14,426.664 | 0.100.1.0 |   29 |    16.8457 |     0.1221 |         - |     52.22 KB |
|     Return_String_Async |     Clr |  CsProjnet461 |          False |      48.78 us |     0.3658 us |     0.4213 us | 20,499.912 | 0.100.1.0 |   22 |    16.9067 |     0.0610 |         - |     52.17 KB |
| Method_ThrowsErrorAsync |     Clr |  CsProjnet461 |          False |     409.12 us |     4.7080 us |     5.4217 us |  2,444.274 | 0.100.1.0 |   41 |    62.9883 |          - |         - |    194.52 KB |
| Return_Very_Large_Async |     Clr |  CsProjnet461 |          False | 339,873.55 us | 4,206.6633 us | 4,844.3993 us |      2.942 | 0.100.1.0 |   62 | 30125.0000 | 10000.0000 | 3625.0000 | 160931.73 KB |
| Method_Very_Large_Async |     Clr |  CsProjnet461 |          False |  87,302.76 us | 1,552.7027 us | 1,788.0946 us |     11.454 | 0.100.1.0 |   59 |  8062.5000 |  2250.0000 | 1562.5000 |  37111.95 KB |
|            Method_Async |    Core | .NET Core 2.0 |          False |      27.53 us |     0.2248 us |     0.2589 us | 36,324.517 | 0.100.1.0 |    5 |     0.2441 |          - |         - |      7.66 KB |
|        Method_Int_Async |    Core | .NET Core 2.0 |          False |      53.28 us |     0.4247 us |     0.4891 us | 18,767.843 | 0.100.1.0 |   26 |     1.5869 |          - |         - |     29.98 KB |
|      Method_Large_Async |    Core | .NET Core 2.0 |          False |     941.61 us |     1.9679 us |     2.2662 us |  1,062.006 | 0.100.1.0 |   46 |    14.6484 |     0.9766 |         - |    280.91 KB |
|       Method_Many_Async |    Core | .NET Core 2.0 |          False |      71.30 us |     0.7465 us |     0.8597 us | 14,024.286 | 0.100.1.0 |   30 |     1.5869 |          - |         - |     32.45 KB |
|     Method_Object_Async |    Core | .NET Core 2.0 |          False |      69.71 us |     0.2390 us |     0.2752 us | 14,345.726 | 0.100.1.0 |   29 |     1.5869 |          - |         - |     32.39 KB |
|     Method_String_Async |    Core | .NET Core 2.0 |          False |      53.58 us |     0.3953 us |     0.4552 us | 18,662.404 | 0.100.1.0 |   26 |     1.4648 |          - |         - |     29.98 KB |
|       Return_Ints_Async |    Core | .NET Core 2.0 |          False |      53.55 us |     2.3719 us |     2.7315 us | 18,674.523 | 0.100.1.0 |   26 |     1.0986 |          - |         - |      7.87 KB |
|        Return_Int_Async |    Core | .NET Core 2.0 |          False |      41.98 us |     1.0670 us |     1.2288 us | 23,822.450 | 0.100.1.0 |   18 |     1.0376 |          - |         - |      7.86 KB |
|      Return_Large_Async |    Core | .NET Core 2.0 |          False |     567.80 us |     1.4710 us |     1.6940 us |  1,761.169 | 0.100.1.0 |   43 |     7.8125 |          - |         - |      7.87 KB |
|    Return_Objects_Async |    Core | .NET Core 2.0 |          False |     106.35 us |     3.0883 us |     3.5565 us |  9,402.750 | 0.100.1.0 |   35 |     1.4648 |          - |         - |       7.9 KB |
|     Return_Object_Async |    Core | .NET Core 2.0 |          False |      66.30 us |     3.1659 us |     3.6458 us | 15,082.237 | 0.100.1.0 |   28 |     1.0986 |          - |         - |      7.88 KB |
|    Return_Strings_Async |    Core | .NET Core 2.0 |          False |      52.93 us |     2.3262 us |     2.6789 us | 18,894.299 | 0.100.1.0 |   26 |     1.0986 |          - |         - |       7.9 KB |
|     Return_String_Async |    Core | .NET Core 2.0 |          False |      42.16 us |     0.6052 us |     0.6969 us | 23,719.027 | 0.100.1.0 |   18 |     1.0986 |          - |         - |      7.88 KB |
| Method_ThrowsErrorAsync |    Core | .NET Core 2.0 |          False |   3,001.80 us |    45.6585 us |    52.5804 us |    333.133 | 0.100.1.0 |   48 |    15.6250 |          - |         - |      8.02 KB |
| Return_Very_Large_Async |    Core | .NET Core 2.0 |          False | 245,898.93 us | 2,409.0509 us | 2,774.2663 us |      4.067 | 0.100.1.0 |   61 |   562.5000 |   375.0000 |  125.0000 |     30.25 KB |
| Method_Very_Large_Async |    Core | .NET Core 2.0 |          False |  84,638.26 us | 1,528.1683 us | 1,759.8408 us |     11.815 | 0.100.1.0 |   58 |   937.5000 |   812.5000 |  687.5000 |  27133.28 KB |
|            Method_Async |    Core | .NET Core 2.1 |          False |      23.94 us |     0.3128 us |     0.3602 us | 41,767.351 | 0.100.1.0 |    2 |     0.2747 |          - |         - |      7.09 KB |
|        Method_Int_Async |    Core | .NET Core 2.1 |          False |      52.80 us |     0.7994 us |     0.9206 us | 18,937.787 | 0.100.1.0 |   26 |     1.5259 |          - |         - |     29.34 KB |
|      Method_Large_Async |    Core | .NET Core 2.1 |          False |     920.70 us |     3.0127 us |     3.4694 us |  1,086.127 | 0.100.1.0 |   45 |    12.6953 |     0.9766 |         - |    280.26 KB |
|       Method_Many_Async |    Core | .NET Core 2.1 |          False |      69.09 us |     0.3386 us |     0.3899 us | 14,472.878 | 0.100.1.0 |   29 |     1.4648 |          - |         - |      31.8 KB |
|     Method_Object_Async |    Core | .NET Core 2.1 |          False |      67.65 us |     0.5646 us |     0.6502 us | 14,782.179 | 0.100.1.0 |   28 |     1.5869 |          - |         - |     31.74 KB |
|     Method_String_Async |    Core | .NET Core 2.1 |          False |      51.62 us |     0.4539 us |     0.5227 us | 19,370.465 | 0.100.1.0 |   24 |     1.6479 |          - |         - |     29.33 KB |
|       Return_Ints_Async |    Core | .NET Core 2.1 |          False |      48.79 us |     0.1761 us |     0.2028 us | 20,493.977 | 0.100.1.0 |   22 |     1.0986 |          - |         - |      7.25 KB |
|        Return_Int_Async |    Core | .NET Core 2.1 |          False |      35.52 us |     0.6005 us |     0.6915 us | 28,153.451 | 0.100.1.0 |   13 |     1.0376 |          - |         - |      7.21 KB |
|      Return_Large_Async |    Core | .NET Core 2.1 |          False |     551.62 us |     2.8502 us |     3.2823 us |  1,812.858 | 0.100.1.0 |   42 |    12.6953 |          - |         - |      7.26 KB |
|    Return_Objects_Async |    Core | .NET Core 2.1 |          False |     102.48 us |     1.4188 us |     1.6338 us |  9,758.122 | 0.100.1.0 |   34 |     1.8311 |          - |         - |      7.29 KB |
|     Return_Object_Async |    Core | .NET Core 2.1 |          False |      61.99 us |     0.7311 us |     0.8420 us | 16,130.413 | 0.100.1.0 |   27 |     1.0986 |          - |         - |      7.27 KB |
|    Return_Strings_Async |    Core | .NET Core 2.1 |          False |      48.56 us |     0.6795 us |     0.7825 us | 20,591.653 | 0.100.1.0 |   22 |     1.0376 |          - |         - |      7.29 KB |
|     Return_String_Async |    Core | .NET Core 2.1 |          False |      35.16 us |     0.3494 us |     0.4024 us | 28,445.091 | 0.100.1.0 |   13 |     1.0376 |          - |         - |      7.22 KB |
| Method_ThrowsErrorAsync |    Core | .NET Core 2.1 |          False |   3,373.25 us |     8.4889 us |     9.7759 us |    296.450 | 0.100.1.0 |   50 |    11.7188 |          - |         - |       7.3 KB |
| Return_Very_Large_Async |    Core | .NET Core 2.1 |          False | 240,153.86 us | 2,583.7936 us | 2,975.5003 us |      4.164 | 0.100.1.0 |   60 |   437.5000 |   250.0000 |   62.5000 |     29.62 KB |
| Method_Very_Large_Async |    Core | .NET Core 2.1 |          False |  81,751.63 us |   532.3345 us |   613.0372 us |     12.232 | 0.100.1.0 |   57 |   937.5000 |   750.0000 |  687.5000 |  27131.71 KB |
|            **Method_Async** |     **Clr** |  **CsProjnet461** |           **True** |      **27.11 us** |     **0.1109 us** |     **0.1277 us** | **36,882.728** | **0.100.1.0** |    **3** |     **3.6316** |     **0.0305** |         **-** |     **11.22 KB** |
|        Method_Int_Async |     Clr |  CsProjnet461 |           True |      43.55 us |     0.0538 us |     0.0619 us | 22,961.096 | 0.100.1.0 |   20 |     3.9673 |     0.0610 |         - |     12.34 KB |
|      Method_Large_Async |     Clr |  CsProjnet461 |           True |     109.67 us |     0.1341 us |     0.1545 us |  9,118.151 | 0.100.1.0 |   36 |    13.0615 |     0.1221 |         - |     40.51 KB |
|       Method_Many_Async |     Clr |  CsProjnet461 |           True |      46.04 us |     0.0507 us |     0.0584 us | 21,719.397 | 0.100.1.0 |   21 |     4.0894 |     0.0610 |         - |      12.7 KB |
|     Method_Object_Async |     Clr |  CsProjnet461 |           True |      44.63 us |     0.1024 us |     0.1179 us | 22,406.360 | 0.100.1.0 |   21 |     4.0283 |     0.0610 |         - |     12.48 KB |
|     Method_String_Async |     Clr |  CsProjnet461 |           True |      43.77 us |     0.1550 us |     0.1785 us | 22,846.943 | 0.100.1.0 |   20 |     3.9673 |     0.0610 |         - |     12.33 KB |
|       Return_Ints_Async |     Clr |  CsProjnet461 |           True |      42.96 us |     0.0973 us |     0.1120 us | 23,278.876 | 0.100.1.0 |   19 |     4.0283 |     0.0610 |         - |     12.43 KB |
|        Return_Int_Async |     Clr |  CsProjnet461 |           True |      42.39 us |     0.5984 us |     0.6891 us | 23,590.483 | 0.100.1.0 |   18 |     3.9673 |     0.0610 |         - |     12.22 KB |
|      Return_Large_Async |     Clr |  CsProjnet461 |           True |     114.02 us |     0.1754 us |     0.2020 us |  8,770.491 | 0.100.1.0 |   39 |    13.1836 |     0.1221 |         - |     40.87 KB |
|    Return_Objects_Async |     Clr |  CsProjnet461 |           True |      46.99 us |     0.4326 us |     0.4981 us | 21,281.308 | 0.100.1.0 |   21 |     4.4556 |     0.0610 |         - |     13.87 KB |
|     Return_Object_Async |     Clr |  CsProjnet461 |           True |      45.31 us |     1.8815 us |     2.1667 us | 22,067.932 | 0.100.1.0 |   21 |     4.0894 |     0.0610 |         - |      12.6 KB |
|    Return_Strings_Async |     Clr |  CsProjnet461 |           True |      44.47 us |     0.3067 us |     0.3533 us | 22,485.915 | 0.100.1.0 |   21 |     4.0894 |     0.0610 |         - |     12.66 KB |
|     Return_String_Async |     Clr |  CsProjnet461 |           True |      46.27 us |     2.5889 us |     2.9813 us | 21,613.343 | 0.100.1.0 |   21 |     3.9673 |     0.0610 |         - |     12.25 KB |
| Method_ThrowsErrorAsync |     Clr |  CsProjnet461 |           True |     381.01 us |     7.0058 us |     8.0678 us |  2,624.636 | 0.100.1.0 |   40 |    43.9453 |          - |         - |    136.25 KB |
| Return_Very_Large_Async |     Clr |  CsProjnet461 |           True |  62,007.01 us |   825.8025 us |   950.9954 us |     16.127 | 0.100.1.0 |   56 |  3000.0000 |  2000.0000 | 1000.0000 |  31492.85 KB |
| Method_Very_Large_Async |     Clr |  CsProjnet461 |           True |   8,993.36 us |   165.2167 us |   190.2638 us |    111.193 | 0.100.1.0 |   53 |   890.6250 |   859.3750 |  500.0000 |   4579.82 KB |
|            Method_Async |    Core | .NET Core 2.0 |           True |      27.37 us |     0.0928 us |     0.1069 us | 36,537.634 | 0.100.1.0 |    4 |     0.2441 |          - |         - |      7.67 KB |
|        Method_Int_Async |    Core | .NET Core 2.0 |           True |      37.89 us |     0.3752 us |     0.4321 us | 26,392.777 | 0.100.1.0 |   14 |     0.3052 |          - |         - |      8.22 KB |
|      Method_Large_Async |    Core | .NET Core 2.0 |           True |     113.63 us |     0.4710 us |     0.5424 us |  8,800.304 | 0.100.1.0 |   38 |     0.9766 |          - |         - |     12.32 KB |
|       Method_Many_Async |    Core | .NET Core 2.0 |           True |      43.56 us |     0.5384 us |     0.6200 us | 22,954.648 | 0.100.1.0 |   20 |     0.2441 |          - |         - |      8.27 KB |
|     Method_Object_Async |    Core | .NET Core 2.0 |           True |      40.03 us |     0.2125 us |     0.2447 us | 24,982.556 | 0.100.1.0 |   17 |     0.3662 |          - |         - |      8.22 KB |
|     Method_String_Async |    Core | .NET Core 2.0 |           True |      38.29 us |     0.0571 us |     0.0658 us | 26,114.408 | 0.100.1.0 |   14 |     0.3052 |          - |         - |      8.22 KB |
|       Return_Ints_Async |    Core | .NET Core 2.0 |           True |      37.09 us |     0.2699 us |     0.3108 us | 26,964.069 | 0.100.1.0 |   13 |     0.3052 |          - |         - |      7.86 KB |
|        Return_Int_Async |    Core | .NET Core 2.0 |           True |      34.51 us |     0.1204 us |     0.1386 us | 28,980.507 | 0.100.1.0 |   12 |     0.2441 |          - |         - |      7.84 KB |
|      Return_Large_Async |    Core | .NET Core 2.0 |           True |     112.17 us |     0.9707 us |     1.1179 us |  8,914.654 | 0.100.1.0 |   37 |     0.8545 |          - |         - |      7.88 KB |
|    Return_Objects_Async |    Core | .NET Core 2.0 |           True |      42.41 us |     0.9544 us |     1.0991 us | 23,576.625 | 0.100.1.0 |   18 |     0.3662 |          - |         - |       7.9 KB |
|     Return_Object_Async |    Core | .NET Core 2.0 |           True |      39.43 us |     0.4160 us |     0.4791 us | 25,362.857 | 0.100.1.0 |   16 |     0.3052 |          - |         - |      7.88 KB |
|    Return_Strings_Async |    Core | .NET Core 2.0 |           True |      38.84 us |     0.3877 us |     0.4465 us | 25,749.844 | 0.100.1.0 |   15 |     0.3052 |          - |         - |       7.9 KB |
|     Return_String_Async |    Core | .NET Core 2.0 |           True |      36.09 us |     0.1731 us |     0.1994 us | 27,707.412 | 0.100.1.0 |   13 |     0.2441 |          - |         - |      7.87 KB |
| Method_ThrowsErrorAsync |    Core | .NET Core 2.0 |           True |   2,933.02 us |     7.8546 us |     9.0453 us |    340.946 | 0.100.1.0 |   47 |    11.7188 |          - |         - |      8.02 KB |
| Return_Very_Large_Async |    Core | .NET Core 2.0 |           True |  46,288.68 us | 1,111.9846 us | 1,280.5630 us |     21.604 | 0.100.1.0 |   55 |  1625.0000 |  1562.5000 | 1562.5000 |      8.41 KB |
| Method_Very_Large_Async |    Core | .NET Core 2.0 |           True |   8,227.64 us |    98.9420 us |   113.9417 us |    121.542 | 0.100.1.0 |   52 |   390.6250 |   375.0000 |  343.7500 |   1325.61 KB |
|            Method_Async |    Core | .NET Core 2.1 |           True |      20.43 us |     1.9596 us |     2.2567 us | 48,953.595 | 0.100.1.0 |    1 |     0.2747 |          - |         - |      7.09 KB |
|        Method_Int_Async |    Core | .NET Core 2.1 |           True |      36.05 us |     2.3040 us |     2.6532 us | 27,735.588 | 0.100.1.0 |   13 |     0.2441 |          - |         - |      7.61 KB |
|      Method_Large_Async |    Core | .NET Core 2.1 |           True |     106.23 us |     0.1531 us |     0.1763 us |  9,413.150 | 0.100.1.0 |   35 |     1.0986 |          - |         - |      11.7 KB |
|       Method_Many_Async |    Core | .NET Core 2.1 |           True |      42.53 us |     0.6208 us |     0.7149 us | 23,511.703 | 0.100.1.0 |   18 |     0.3052 |          - |         - |      7.66 KB |
|     Method_Object_Async |    Core | .NET Core 2.1 |           True |      35.18 us |     0.3077 us |     0.3544 us | 28,428.722 | 0.100.1.0 |   13 |     0.3052 |          - |         - |       7.6 KB |
|     Method_String_Async |    Core | .NET Core 2.1 |           True |      36.58 us |     1.4212 us |     1.6366 us | 27,333.620 | 0.100.1.0 |   13 |     0.3662 |          - |         - |       7.6 KB |
|       Return_Ints_Async |    Core | .NET Core 2.1 |           True |      30.72 us |     0.7535 us |     0.8678 us | 32,554.121 | 0.100.1.0 |    9 |     0.2441 |          - |         - |      7.21 KB |
|        Return_Int_Async |    Core | .NET Core 2.1 |           True |      28.59 us |     0.1894 us |     0.2181 us | 34,976.915 | 0.100.1.0 |    7 |     0.2747 |          - |         - |      7.21 KB |
|      Return_Large_Async |    Core | .NET Core 2.1 |           True |     107.08 us |     0.8141 us |     0.9376 us |  9,338.527 | 0.100.1.0 |   35 |     1.0986 |          - |         - |      7.27 KB |
|    Return_Objects_Async |    Core | .NET Core 2.1 |           True |      38.10 us |     0.7112 us |     0.8190 us | 26,248.372 | 0.100.1.0 |   14 |     0.3052 |          - |         - |      7.26 KB |
|     Return_Object_Async |    Core | .NET Core 2.1 |           True |      32.13 us |     0.2746 us |     0.3162 us | 31,127.352 | 0.100.1.0 |   11 |     0.2441 |          - |         - |      7.23 KB |
|    Return_Strings_Async |    Core | .NET Core 2.1 |           True |      31.28 us |     0.1567 us |     0.1804 us | 31,965.004 | 0.100.1.0 |   10 |     0.2441 |          - |         - |      7.24 KB |
|     Return_String_Async |    Core | .NET Core 2.1 |           True |      29.49 us |     0.1238 us |     0.1425 us | 33,908.826 | 0.100.1.0 |    8 |     0.2441 |          - |         - |      7.22 KB |
| Method_ThrowsErrorAsync |    Core | .NET Core 2.1 |           True |   3,345.16 us |     9.5686 us |    11.0192 us |    298.940 | 0.100.1.0 |   49 |    11.7188 |          - |         - |      7.31 KB |
| Return_Very_Large_Async |    Core | .NET Core 2.1 |           True |  43,663.14 us |   914.8607 us | 1,053.5548 us |     22.903 | 0.100.1.0 |   54 |  1500.0000 |  1437.5000 | 1375.0000 |      7.79 KB |
| Method_Very_Large_Async |    Core | .NET Core 2.1 |           True |   7,764.38 us |    79.7173 us |    91.8026 us |    128.793 | 0.100.1.0 |   51 |   375.0000 |   343.7500 |  343.7500 |    1324.8 KB |
