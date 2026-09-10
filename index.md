# EasyRpc

> ## EasyRpc is retired
>
> EasyRpc is no longer maintained. The last release targets ASP.NET Core on .NET 5, and there
> will be no further releases or fixes. The published NuGet packages stay up so existing builds
> keep working.
>
> New work should use [Hardened](https://ipjohnson-org.github.io/Hardened.Docs), which replaces
> it. Hardened does the same job with source generators instead of runtime reflection: routing,
> dependency injection, model binding and configuration are all generated during the build, so
> there is no startup scan and the result runs under Native AOT. Handlers are still plain
> classes with plain methods.
>
> - Source: [ipjohnson/Hardened.Framework](https://github.com/ipjohnson/Hardened.Framework)
> - Documentation: [ipjohnson-org.github.io/Hardened.Docs](https://ipjohnson-org.github.io/Hardened.Docs)
>
> The rest of this page describes EasyRpc as it was left.

Adds rpc service support to AspNetCore

```
public void ConfigureServices(IServiceCollection services)
{
  services.AddRpcServices();
}

public void Configure(IApplicationBuilder app)
{
  app.UseRpcServices(api =>
  {
     // simple web method at /Status
     api.GetMethod("/Status", () => new { status = "Ok"});

     // Expose methods at /IntMath
     api.Expose<IntMathService>().As("IntMath");
  });
}

public class IntMathService
{
  // expose web api POST /IntMath/Add expecting {"a":int,"b":int}
  public int Add(int a, int b)
  {
    return a + b;
  }
}
```

### Features

EasyRpc allow developers to write business related classes and host them as remote procedure calls.
In essence developers focus on writing services that fullfil requirements vs. writing RESTful services 
that require the developer to think about which verbs they want to use. 

* Performs faster than MVC as seen in [these](https://www.techempower.com/benchmarks/#section=data-r19&hw=ph&test=json&p=zik0zj-zik0zj-zijocf-zik0zj-v&c=6) 3rd party benchmarks
* Services participate in Asp.Net Core dependency injection framework
* Integrates with Asp.Net Core authorization schemes including Roles & Polices
* Built in data context idea that can be used to fetch and save data into header
* Filter support similar to Asp.Net filter (not exactly the same as no controller is ever created)
* Support for request/response gzip compression, br compression
* Built in Swagger UI
