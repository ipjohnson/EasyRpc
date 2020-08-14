# EasyRpc
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

* Very fast performance as it takes advantage of System.Reflection.Emit to execute methods (faster than MVC)
* Services participate in Asp.Net Core dependency injection framework
* Integrates with Asp.Net Core authorization schemes including Roles & Polices
* Built in data context idea that can be used to fetch and save data into header
* Filter support similar to Asp.Net filter (not exactly the same as no controller is ever created)
* Support for request/response gzip compression, br compression
* Built in Swagger UI

### Example App
An example app can be found [here](https://github.com/ipjohnson/EasyRpc.AspNetCore.Sample).

### Build
[![Build status](https://ci.appveyor.com/api/projects/status/1sflvdvnetodybab?svg=true)](https://ci.appveyor.com/project/ipjohnson/easyrpc) [![Coverage Status](https://coveralls.io/repos/github/ipjohnson/EasyRpc/badge.svg?branch=master)](https://coveralls.io/github/ipjohnson/EasyRpc?branch=master)


