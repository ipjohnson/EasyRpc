``` ini

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18362
AMD Ryzen Threadripper 1950X, 1 CPU, 32 logical and 16 physical cores
.NET Core SDK=3.1.100
  [Host] : .NET Core 3.1.0 (CoreCLR 4.700.19.56402, CoreFX 4.700.19.56404), X64 RyuJIT

Job=InProcess  Server=True  Toolchain=InProcessEmitToolchain  

```
|          Method |     Mean |     Error |    StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------- |---------:|----------:|----------:|-------:|------:|------:|----------:|
| DefaultEndPoint | 2.682 us | 0.0367 us | 0.0325 us | 0.0038 |     - |     - |     600 B |
