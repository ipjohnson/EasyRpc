``` ini

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18362
AMD Ryzen Threadripper 1950X, 1 CPU, 32 logical and 16 physical cores
.NET Core SDK=3.1.100
  [Host] : .NET Core 3.1.0 (CoreCLR 4.700.19.56402, CoreFX 4.700.19.56404), X64 RyuJIT

Job=InProcess  Server=True  Toolchain=InProcessEmitToolchain  

```
|              Method |     Mean |    Error |   StdDev | Gen 0 | Gen 1 | Gen 2 | Allocated |
|-------------------- |---------:|---------:|---------:|------:|------:|------:|----------:|
|     ConcatBenchmark | 83.49 us | 1.898 us | 2.660 us |     - |     - |     - |   12.6 KB |
|     CarterPlainText | 72.16 us | 0.833 us | 0.739 us |     - |     - |     - |  12.59 KB |
|           PlainText | 66.45 us | 1.230 us | 1.150 us |     - |     - |     - |   10.5 KB |
|            NoParams | 67.49 us | 0.513 us | 0.429 us |     - |     - |     - |  10.71 KB |
|         GetOneParam | 69.46 us | 1.043 us | 0.924 us |     - |     - |     - |  10.72 KB |
| PlainTextAspRouting | 66.49 us | 1.315 us | 1.350 us |     - |     - |     - |  10.47 KB |
|  NoParamsAspRouting | 67.86 us | 0.963 us | 0.853 us |     - |     - |     - |  10.56 KB |
|      BlankBenchmark | 61.69 us | 1.214 us | 1.535 us |     - |     - |     - |   9.76 KB |
