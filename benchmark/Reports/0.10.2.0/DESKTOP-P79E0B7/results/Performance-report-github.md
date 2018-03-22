``` ini

BenchmarkDotNet=v0.10.13, OS=Windows 10 Redstone 3 [1709, Fall Creators Update] (10.0.16299.309)
Intel Core i5-4690K CPU 3.50GHz (Haswell), 1 CPU, 4 logical cores and 4 physical cores
Frequency=3415991 Hz, Resolution=292.7408 ns, Timer=TSC
.NET Core SDK=2.1.300-preview1-008174
  [Host]     : .NET Core 2.0.5 (CoreCLR 4.6.26020.03, CoreFX 4.6.26018.01), 64bit RyuJIT
  Job-NWFPVK : .NET Framework 4.6.1 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2633.0
  Job-ZVPMMM : .NET Core 2.0.5 (CoreCLR 4.6.26020.03, CoreFX 4.6.26018.01), 64bit RyuJIT
  Job-SWBMOF : .NET Core 2.1.0-preview1-26216-03 (CoreCLR 4.6.26216.04, CoreFX 4.6.26216.02), 64bit RyuJIT

RemoveOutliers=False  Server=True  LaunchCount=2  
RunStrategy=Throughput  TargetCount=10  WarmupCount=5  

```
|                  Method | Runtime |     Toolchain | UseMessagePack |          Mean |         Error |        StdDev |        Median |       Op/s |  Version | Rank |      Gen 0 |     Gen 1 |     Gen 2 |   Allocated |
|------------------------ |-------- |-------------- |--------------- |--------------:|--------------:|--------------:|--------------:|-----------:|--------- |-----:|-----------:|----------:|----------:|------------:|
|            **Method_Async** |     **Clr** |  **CsProjnet461** |          **False** |      **25.14 us** |     **0.2057 us** |     **0.2369 us** |      **25.12 us** | **39,778.154** | **0.10.2.0** |    **4** |     **3.6621** |    **0.0305** |         **-** |     **11565 B** |
|        Method_Int_Async |     Clr |  CsProjnet461 |          False |      53.57 us |     0.4047 us |     0.4661 us |      53.66 us | 18,668.688 | 0.10.2.0 |   32 |    16.3574 |    0.0610 |         - |     51692 B |
|      Method_Large_Async |     Clr |  CsProjnet461 |          False |     959.14 us |    16.9466 us |    19.5158 us |     946.39 us |  1,042.602 | 0.10.2.0 |   54 |   128.9063 |         - |         - |    410377 B |
|       Method_Many_Async |     Clr |  CsProjnet461 |          False |      91.67 us |     0.0876 us |     0.1009 us |      91.67 us | 10,908.920 | 0.10.2.0 |   39 |    17.2119 |    0.1221 |         - |     54539 B |
|     Method_Object_Async |     Clr |  CsProjnet461 |          False |      90.44 us |     0.1075 us |     0.1238 us |      90.42 us | 11,057.579 | 0.10.2.0 |   38 |    17.2119 |    0.1221 |         - |     54428 B |
|     Method_String_Async |     Clr |  CsProjnet461 |          False |      52.27 us |     0.0893 us |     0.1028 us |      52.26 us | 19,129.699 | 0.10.2.0 |   31 |    16.3574 |    0.0610 |         - |     51694 B |
|       Return_Ints_Async |     Clr |  CsProjnet461 |          False |      69.93 us |     2.0607 us |     2.3731 us |      71.18 us | 14,300.444 | 0.10.2.0 |   34 |    16.8457 |    0.1221 |         - |     53476 B |
|        Return_Int_Async |     Clr |  CsProjnet461 |          False |      49.97 us |     1.4634 us |     1.6853 us |      49.38 us | 20,010.314 | 0.10.2.0 |   30 |    16.8457 |    0.0610 |         - |     53418 B |
|      Return_Large_Async |     Clr |  CsProjnet461 |          False |     611.33 us |     9.8563 us |    11.3505 us |     610.15 us |  1,635.785 | 0.10.2.0 |   51 |   120.1172 |         - |         - |    381025 B |
|    Return_Objects_Async |     Clr |  CsProjnet461 |          False |     108.44 us |     4.1229 us |     4.7480 us |     107.18 us |  9,221.685 | 0.10.2.0 |   43 |    22.7051 |    0.1221 |         - |     71833 B |
|     Return_Object_Async |     Clr |  CsProjnet461 |          False |      82.75 us |     4.3618 us |     5.0230 us |      80.69 us | 12,083.921 | 0.10.2.0 |   37 |    16.9678 |    0.1221 |         - |     53770 B |
|    Return_Strings_Async |     Clr |  CsProjnet461 |          False |      68.99 us |     2.8926 us |     3.3311 us |      67.69 us | 14,494.624 | 0.10.2.0 |   34 |    16.8457 |    0.1221 |         - |     53481 B |
|     Return_String_Async |     Clr |  CsProjnet461 |          False |      50.10 us |     0.6958 us |     0.8013 us |      49.86 us | 19,958.745 | 0.10.2.0 |   30 |    17.1509 |    0.0610 |         - |     54306 B |
| Method_ThrowsErrorAsync |     Clr |  CsProjnet461 |          False |     399.27 us |     3.6906 us |     4.2501 us |     399.17 us |  2,504.550 | 0.10.2.0 |   48 |    62.9883 |         - |         - |    198634 B |
| Return_Very_Large_Async |     Clr |  CsProjnet461 |          False | 327,202.93 us | 3,237.2275 us | 3,727.9957 us | 326,252.18 us |      3.056 | 0.10.2.0 |   70 | 29562.5000 | 9500.0000 | 3375.0000 | 164663105 B |
| Method_Very_Large_Async |     Clr |  CsProjnet461 |          False |  87,017.31 us | 2,135.4610 us | 2,459.1998 us |  86,998.12 us |     11.492 | 0.10.2.0 |   67 |  8250.0000 | 2250.0000 | 1562.5000 |  38001404 B |
|            Method_Async |    Core | .NET Core 2.0 |          False |      24.01 us |     0.0987 us |     0.1137 us |      23.98 us | 41,653.133 | 0.10.2.0 |    2 |     0.3357 |         - |         - |      7840 B |
|        Method_Int_Async |    Core | .NET Core 2.0 |          False |      53.77 us |     0.1927 us |     0.2219 us |      53.74 us | 18,598.470 | 0.10.2.0 |   32 |     1.2817 |         - |         - |     30704 B |
|      Method_Large_Async |    Core | .NET Core 2.0 |          False |     940.12 us |     0.7402 us |     0.8524 us |     939.84 us |  1,063.694 | 0.10.2.0 |   53 |    13.6719 |    0.9766 |         - |    286843 B |
|       Method_Many_Async |    Core | .NET Core 2.0 |          False |      74.52 us |     0.3577 us |     0.4119 us |      74.53 us | 13,418.908 | 0.10.2.0 |   36 |     1.3428 |         - |         - |     33216 B |
|     Method_Object_Async |    Core | .NET Core 2.0 |          False |      71.07 us |     0.3942 us |     0.4539 us |      71.00 us | 14,070.509 | 0.10.2.0 |   35 |     1.2207 |         - |         - |     33160 B |
|     Method_String_Async |    Core | .NET Core 2.0 |          False |      53.30 us |     0.3179 us |     0.3661 us |      53.27 us | 18,761.683 | 0.10.2.0 |   32 |     1.2817 |         - |         - |     30696 B |
|       Return_Ints_Async |    Core | .NET Core 2.0 |          False |      54.72 us |     2.7185 us |     3.1307 us |      54.92 us | 18,273.882 | 0.10.2.0 |   32 |     1.0986 |         - |         - |      8056 B |
|        Return_Int_Async |    Core | .NET Core 2.0 |          False |      41.19 us |     0.4984 us |     0.5740 us |      41.33 us | 24,275.547 | 0.10.2.0 |   19 |     1.0376 |         - |         - |      8053 B |
|      Return_Large_Async |    Core | .NET Core 2.0 |          False |     553.46 us |     2.7943 us |     3.2180 us |     554.48 us |  1,806.825 | 0.10.2.0 |   50 |     8.7891 |         - |         - |      8056 B |
|    Return_Objects_Async |    Core | .NET Core 2.0 |          False |     102.24 us |     4.4247 us |     5.0954 us |      99.73 us |  9,780.448 | 0.10.2.0 |   41 |     1.4648 |         - |         - |      8088 B |
|     Return_Object_Async |    Core | .NET Core 2.0 |          False |      68.27 us |     1.6149 us |     1.8597 us |      67.52 us | 14,648.038 | 0.10.2.0 |   34 |     1.0986 |         - |         - |      8072 B |
|    Return_Strings_Async |    Core | .NET Core 2.0 |          False |      55.57 us |     2.4198 us |     2.7867 us |      56.08 us | 17,996.822 | 0.10.2.0 |   32 |     1.0986 |         - |         - |      8088 B |
|     Return_String_Async |    Core | .NET Core 2.0 |          False |      41.84 us |     0.4103 us |     0.4725 us |      42.00 us | 23,901.361 | 0.10.2.0 |   21 |     1.0986 |         - |         - |      8066 B |
| Method_ThrowsErrorAsync |    Core | .NET Core 2.0 |          False |   3,000.61 us |    29.6022 us |    34.0899 us |   2,999.12 us |    333.265 | 0.10.2.0 |   56 |    11.7188 |         - |         - |      8208 B |
| Return_Very_Large_Async |    Core | .NET Core 2.0 |          False | 246,116.19 us | 2,470.2544 us | 2,844.7484 us | 245,766.04 us |      4.063 | 0.10.2.0 |   69 |   500.0000 |  250.0000 |  125.0000 |     30979 B |
| Method_Very_Large_Async |    Core | .NET Core 2.0 |          False |  82,383.63 us |   652.0115 us |   750.8573 us |  82,403.04 us |     12.138 | 0.10.2.0 |   66 |   937.5000 |  750.0000 |  625.0000 |  27704488 B |
|            Method_Async |    Core | .NET Core 2.1 |          False |      22.43 us |     4.0288 us |     4.6396 us |      22.78 us | 44,579.531 | 0.10.2.0 |    2 |     0.3662 |         - |         - |      7274 B |
|        Method_Int_Async |    Core | .NET Core 2.1 |          False |      48.97 us |     0.1313 us |     0.1513 us |      48.96 us | 20,421.313 | 0.10.2.0 |   30 |     1.8311 |         - |         - |     30072 B |
|      Method_Large_Async |    Core | .NET Core 2.1 |          False |     914.61 us |     0.9748 us |     1.1226 us |     914.44 us |  1,093.358 | 0.10.2.0 |   52 |    11.7188 |    0.9766 |         - |    286179 B |
|       Method_Many_Async |    Core | .NET Core 2.1 |          False |      71.25 us |     0.4630 us |     0.5332 us |      71.28 us | 14,035.227 | 0.10.2.0 |   35 |     1.5869 |         - |         - |     32584 B |
|     Method_Object_Async |    Core | .NET Core 2.1 |          False |      70.52 us |     0.3059 us |     0.3523 us |      70.69 us | 14,180.169 | 0.10.2.0 |   34 |     1.7090 |         - |         - |     32528 B |
|     Method_String_Async |    Core | .NET Core 2.1 |          False |      49.16 us |     0.5272 us |     0.6071 us |      49.02 us | 20,343.163 | 0.10.2.0 |   30 |     2.0142 |         - |         - |     30064 B |
|       Return_Ints_Async |    Core | .NET Core 2.1 |          False |      49.18 us |     0.4931 us |     0.5678 us |      49.22 us | 20,333.896 | 0.10.2.0 |   30 |     1.0986 |         - |         - |      7426 B |
|        Return_Int_Async |    Core | .NET Core 2.1 |          False |      37.12 us |     0.1572 us |     0.1810 us |      37.18 us | 26,939.274 | 0.10.2.0 |   14 |     1.0376 |         - |         - |      7389 B |
|      Return_Large_Async |    Core | .NET Core 2.1 |          False |     550.86 us |     3.5338 us |     4.0696 us |     551.61 us |  1,815.343 | 0.10.2.0 |   49 |    13.6719 |    0.9766 |         - |      7432 B |
|    Return_Objects_Async |    Core | .NET Core 2.1 |          False |      99.62 us |     0.9951 us |     1.1460 us |      99.60 us | 10,038.381 | 0.10.2.0 |   40 |     1.8311 |         - |         - |      7464 B |
|     Return_Object_Async |    Core | .NET Core 2.1 |          False |      60.96 us |     0.7739 us |     0.8912 us |      61.24 us | 16,403.480 | 0.10.2.0 |   33 |     1.2207 |         - |         - |      7448 B |
|    Return_Strings_Async |    Core | .NET Core 2.1 |          False |      48.81 us |     0.1517 us |     0.1747 us |      48.78 us | 20,489.701 | 0.10.2.0 |   29 |     1.0986 |         - |         - |      7460 B |
|     Return_String_Async |    Core | .NET Core 2.1 |          False |      35.76 us |     0.0783 us |     0.0902 us |      35.75 us | 27,961.953 | 0.10.2.0 |   11 |     1.0376 |         - |         - |      7396 B |
| Method_ThrowsErrorAsync |    Core | .NET Core 2.1 |          False |   3,304.40 us |     9.3485 us |    10.7657 us |   3,301.42 us |    302.626 | 0.10.2.0 |   58 |    11.7188 |         - |         - |      7480 B |
| Return_Very_Large_Async |    Core | .NET Core 2.1 |          False | 241,961.47 us | 2,716.1538 us | 3,127.9265 us | 242,200.43 us |      4.133 | 0.10.2.0 |   68 |   500.0000 |  312.5000 |  125.0000 |     30373 B |
| Method_Very_Large_Async |    Core | .NET Core 2.1 |          False |  80,689.43 us |   468.2348 us |   539.2198 us |  80,739.42 us |     12.393 | 0.10.2.0 |   65 |  1250.0000 | 1062.5000 |  812.5000 |  27703616 B |
|            **Method_Async** |     **Clr** |  **CsProjnet461** |           **True** |            **NA** |            **NA** |            **NA** |            **NA** |         **NA** | **0.10.2.0** |    **?** |        **N/A** |       **N/A** |       **N/A** |         **N/A** |
|        Method_Int_Async |     Clr |  CsProjnet461 |           True |      43.37 us |     1.1115 us |     1.2800 us |      43.85 us | 23,057.165 | 0.10.2.0 |   22 |     3.9673 |    0.0610 |         - |     12666 B |
|      Method_Large_Async |     Clr |  CsProjnet461 |           True |     110.36 us |     0.8747 us |     1.0073 us |     110.52 us |  9,061.277 | 0.10.2.0 |   43 |    13.0615 |    0.1221 |         - |     41484 B |
|       Method_Many_Async |     Clr |  CsProjnet461 |           True |      46.60 us |     0.4028 us |     0.4639 us |      46.59 us | 21,457.789 | 0.10.2.0 |   27 |     4.1504 |    0.0610 |         - |     13086 B |
|     Method_Object_Async |     Clr |  CsProjnet461 |           True |      45.24 us |     0.0905 us |     0.1042 us |      45.23 us | 22,106.716 | 0.10.2.0 |   25 |     4.0283 |    0.0610 |         - |     12863 B |
|     Method_String_Async |     Clr |  CsProjnet461 |           True |      44.14 us |     0.5376 us |     0.6191 us |      44.15 us | 22,654.502 | 0.10.2.0 |   23 |     3.9673 |    0.0610 |         - |     12651 B |
|       Return_Ints_Async |     Clr |  CsProjnet461 |           True |      44.89 us |     0.2274 us |     0.2619 us |      44.98 us | 22,278.659 | 0.10.2.0 |   24 |     4.0283 |    0.0610 |         - |     12863 B |
|        Return_Int_Async |     Clr |  CsProjnet461 |           True |      42.93 us |     0.3717 us |     0.4280 us |      42.94 us | 23,295.415 | 0.10.2.0 |   22 |     3.9673 |    0.0610 |         - |     12672 B |
|      Return_Large_Async |     Clr |  CsProjnet461 |           True |     116.39 us |     0.2402 us |     0.2766 us |     116.34 us |  8,591.538 | 0.10.2.0 |   46 |    13.1836 |    0.1221 |         - |     41633 B |
|    Return_Objects_Async |     Clr |  CsProjnet461 |           True |      47.74 us |     0.1321 us |     0.1522 us |      47.78 us | 20,946.291 | 0.10.2.0 |   28 |     4.5166 |    0.0610 |         - |     14315 B |
|     Return_Object_Async |     Clr |  CsProjnet461 |           True |      45.80 us |     0.1791 us |     0.2063 us |      45.77 us | 21,833.960 | 0.10.2.0 |   26 |     4.0894 |    0.0610 |         - |     13047 B |
|    Return_Strings_Async |     Clr |  CsProjnet461 |           True |      45.23 us |     0.5334 us |     0.6142 us |      45.53 us | 22,107.339 | 0.10.2.0 |   25 |     4.1504 |    0.0610 |         - |     13144 B |
|     Return_String_Async |     Clr |  CsProjnet461 |           True |      44.32 us |     0.2699 us |     0.3109 us |      44.35 us | 22,565.541 | 0.10.2.0 |   23 |     4.0283 |    0.0610 |         - |     12776 B |
| Method_ThrowsErrorAsync |     Clr |  CsProjnet461 |           True |     360.79 us |     1.1885 us |     1.3687 us |     361.45 us |  2,771.730 | 0.10.2.0 |   47 |    42.4805 |         - |         - |    135162 B |
| Return_Very_Large_Async |     Clr |  CsProjnet461 |           True |  62,370.74 us |   160.6693 us |   185.0270 us |  62,337.79 us |     16.033 | 0.10.2.0 |   64 |  3000.0000 | 2000.0000 | 1000.0000 |  32247168 B |
| Method_Very_Large_Async |     Clr |  CsProjnet461 |           True |   8,740.74 us |   364.1002 us |   419.2983 us |   8,624.05 us |    114.407 | 0.10.2.0 |   61 |   890.6250 |  843.7500 |  500.0000 |   4689884 B |
|            Method_Async |    Core | .NET Core 2.0 |           True |      24.44 us |     0.1041 us |     0.1199 us |      24.44 us | 40,908.496 | 0.10.2.0 |    3 |     0.3357 |         - |         - |      7849 B |
|        Method_Int_Async |    Core | .NET Core 2.0 |           True |      32.85 us |     0.3350 us |     0.3858 us |      32.85 us | 30,438.352 | 0.10.2.0 |    9 |     0.3662 |         - |         - |      8402 B |
|      Method_Large_Async |    Core | .NET Core 2.0 |           True |     115.89 us |     0.6001 us |     0.6911 us |     115.80 us |  8,628.590 | 0.10.2.0 |   45 |     0.9766 |         - |         - |     12616 B |
|       Method_Many_Async |    Core | .NET Core 2.0 |           True |      41.61 us |     0.1002 us |     0.1154 us |      41.61 us | 24,034.237 | 0.10.2.0 |   20 |     0.3052 |         - |         - |      8472 B |
|     Method_Object_Async |    Core | .NET Core 2.0 |           True |      38.95 us |     0.0865 us |     0.0996 us |      38.93 us | 25,676.271 | 0.10.2.0 |   18 |     0.3662 |         - |         - |      8415 B |
|     Method_String_Async |    Core | .NET Core 2.0 |           True |      34.80 us |     0.0905 us |     0.1042 us |      34.77 us | 28,737.835 | 0.10.2.0 |   11 |     0.3052 |         - |         - |      8396 B |
|       Return_Ints_Async |    Core | .NET Core 2.0 |           True |      36.55 us |     0.2531 us |     0.2914 us |      36.57 us | 27,362.942 | 0.10.2.0 |   13 |     0.3052 |         - |         - |      8046 B |
|        Return_Int_Async |    Core | .NET Core 2.0 |           True |      34.48 us |     0.0677 us |     0.0780 us |      34.49 us | 29,000.689 | 0.10.2.0 |   10 |     0.3052 |         - |         - |      8036 B |
|      Return_Large_Async |    Core | .NET Core 2.0 |           True |     115.21 us |     1.0141 us |     1.1679 us |     115.53 us |  8,679.781 | 0.10.2.0 |   44 |     0.8545 |         - |         - |      8064 B |
|    Return_Objects_Async |    Core | .NET Core 2.0 |           True |      42.28 us |     1.0602 us |     1.2209 us |      42.37 us | 23,654.454 | 0.10.2.0 |   21 |     0.3052 |         - |         - |      8095 B |
|     Return_Object_Async |    Core | .NET Core 2.0 |           True |      38.20 us |     0.2822 us |     0.3250 us |      38.29 us | 26,180.375 | 0.10.2.0 |   16 |     0.3052 |         - |         - |      8070 B |
|    Return_Strings_Async |    Core | .NET Core 2.0 |           True |      37.65 us |     0.4299 us |     0.4950 us |      37.77 us | 26,562.044 | 0.10.2.0 |   15 |     0.3052 |         - |         - |      8085 B |
|     Return_String_Async |    Core | .NET Core 2.0 |           True |      35.95 us |     0.2526 us |     0.2909 us |      35.93 us | 27,817.725 | 0.10.2.0 |   12 |     0.2441 |         - |         - |      8062 B |
| Method_ThrowsErrorAsync |    Core | .NET Core 2.0 |           True |   2,910.88 us |     8.7254 us |    10.0482 us |   2,913.42 us |    343.539 | 0.10.2.0 |   55 |     7.8125 |         - |         - |      8216 B |
| Return_Very_Large_Async |    Core | .NET Core 2.0 |           True |  45,552.49 us |   813.1438 us |   936.4175 us |  45,398.41 us |     21.953 | 0.10.2.0 |   63 |  1250.0000 | 1187.5000 | 1125.0000 |      8608 B |
| Method_Very_Large_Async |    Core | .NET Core 2.0 |           True |   8,124.77 us |    71.7705 us |    82.6510 us |   8,113.61 us |    123.080 | 0.10.2.0 |   60 |   390.6250 |  375.0000 |  375.0000 |   1357154 B |
|            Method_Async |    Core | .NET Core 2.1 |           True |      19.07 us |     0.3678 us |     0.4235 us |      19.05 us | 52,430.162 | 0.10.2.0 |    1 |     0.3967 |         - |         - |      7243 B |
|        Method_Int_Async |    Core | .NET Core 2.1 |           True |      34.34 us |     0.6962 us |     0.8017 us |      34.02 us | 29,123.515 | 0.10.2.0 |   10 |     0.2441 |         - |         - |      7792 B |
|      Method_Large_Async |    Core | .NET Core 2.1 |           True |     106.98 us |     0.2204 us |     0.2539 us |     107.00 us |  9,347.658 | 0.10.2.0 |   43 |     0.9766 |         - |         - |     11984 B |
|       Method_Many_Async |    Core | .NET Core 2.1 |           True |      41.80 us |     0.3144 us |     0.3621 us |      41.86 us | 23,924.925 | 0.10.2.0 |   21 |     0.3662 |         - |         - |      7840 B |
|     Method_Object_Async |    Core | .NET Core 2.1 |           True |      38.72 us |     0.3452 us |     0.3975 us |      38.82 us | 25,826.520 | 0.10.2.0 |   17 |     0.3052 |         - |         - |      7783 B |
|     Method_String_Async |    Core | .NET Core 2.1 |           True |      35.21 us |     1.8992 us |     2.1872 us |      35.16 us | 28,398.191 | 0.10.2.0 |   11 |     0.4272 |         - |         - |      7783 B |
|       Return_Ints_Async |    Core | .NET Core 2.1 |           True |      29.72 us |     0.1787 us |     0.2058 us |      29.75 us | 33,646.199 | 0.10.2.0 |    6 |     0.2747 |         - |         - |      7380 B |
|        Return_Int_Async |    Core | .NET Core 2.1 |           True |      28.89 us |     0.7731 us |     0.8903 us |      28.90 us | 34,610.601 | 0.10.2.0 |    5 |     0.2747 |         - |         - |      7378 B |
|      Return_Large_Async |    Core | .NET Core 2.1 |           True |     105.74 us |     1.2510 us |     1.4406 us |     105.76 us |  9,456.736 | 0.10.2.0 |   42 |     0.9766 |         - |         - |      7440 B |
|    Return_Objects_Async |    Core | .NET Core 2.1 |           True |      37.78 us |     0.6515 us |     0.7503 us |      37.71 us | 26,466.621 | 0.10.2.0 |   15 |     0.3052 |         - |         - |      7426 B |
|     Return_Object_Async |    Core | .NET Core 2.1 |           True |      32.56 us |     0.8344 us |     0.9609 us |      32.36 us | 30,715.821 | 0.10.2.0 |    9 |     0.3052 |         - |         - |      7402 B |
|    Return_Strings_Async |    Core | .NET Core 2.1 |           True |      31.09 us |     0.4200 us |     0.4837 us |      31.12 us | 32,161.058 | 0.10.2.0 |    8 |     0.3052 |         - |         - |      7412 B |
|     Return_String_Async |    Core | .NET Core 2.1 |           True |      30.33 us |     0.9255 us |     1.0658 us |      30.67 us | 32,972.974 | 0.10.2.0 |    7 |     0.2747 |         - |         - |      7395 B |
| Method_ThrowsErrorAsync |    Core | .NET Core 2.1 |           True |   3,274.57 us |    15.8746 us |    18.2812 us |   3,273.75 us |    305.384 | 0.10.2.0 |   57 |     7.8125 |         - |         - |      7488 B |
| Return_Very_Large_Async |    Core | .NET Core 2.1 |           True |  44,202.45 us |   870.3619 us | 1,002.3100 us |  44,382.39 us |     22.623 | 0.10.2.0 |   62 |  1187.5000 | 1125.0000 | 1125.0000 |      7976 B |
| Method_Very_Large_Async |    Core | .NET Core 2.1 |           True |   7,659.27 us |    89.7688 us |   103.3779 us |   7,666.47 us |    130.561 | 0.10.2.0 |   59 |   343.7500 |  320.3125 |  320.3125 |   1356602 B |

Benchmarks with issues:
  Performance.Method_Async: Job-NWFPVK(RemoveOutliers=False, Runtime=Clr, Server=True, Toolchain=CsProjnet461, LaunchCount=2, RunStrategy=Throughput, TargetCount=10, WarmupCount=5) [UseMessagePack=True]
