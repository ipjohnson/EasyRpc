``` ini

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18362
AMD Ryzen Threadripper 1950X, 1 CPU, 32 logical and 16 physical cores
.NET Core SDK=3.1.100
  [Host] : .NET Core 3.1.0 (CoreCLR 4.700.19.56402, CoreFX 4.700.19.56404), X64 RyuJIT

Job=InProcess  Server=True  Toolchain=InProcessEmitToolchain  

```
|              Method |     Mean |    Error |   StdDev | Gen 0 | Gen 1 | Gen 2 | Allocated |
|-------------------- |---------:|---------:|---------:|------:|------:|------:|----------:|
|     ConcatBenchmark | 89.90 us | 1.870 us | 5.455 us |     - |     - |     - |  12.54 KB |
|     CarterPlainText | 77.19 us | 1.527 us | 3.119 us |     - |     - |     - |  12.53 KB |
|           PlainText | 70.18 us | 1.394 us | 3.031 us |     - |     - |     - |  10.48 KB |
|            NoParams | 69.88 us | 1.395 us | 3.206 us |     - |     - |     - |  10.68 KB |
|         GetOneParam | 71.64 us | 1.413 us | 3.518 us |     - |     - |     - |  10.69 KB |
| PlainTextAspRouting | 70.42 us | 1.399 us | 3.781 us |     - |     - |     - |  10.48 KB |
|  NoParamsAspRouting | 70.33 us | 1.401 us | 3.565 us |     - |     - |     - |  10.53 KB |
|      BlankBenchmark | 61.24 us | 1.217 us | 2.771 us |     - |     - |     - |    9.7 KB |
