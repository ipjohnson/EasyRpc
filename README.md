# EasyRpc
Adds JSON-RPC support to AspNetCore

```
public void ConfigureServices(IServiceCollection services)
{
  services.AddJsonRpc();
}

public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
{
  ~
  app.UseJsonRPC("RpcApi", api =>
  {
     // Expose methods at /RpcApi/IntMath
     api.Expose<IntMathService>().As("IntMath");
  });
  
  app.UseMvc(routes => 
  ~
}

public class IntMathService
{
  public int Add(int a, int b)
  {
    return a + b;
  }
}
```

### Features

* Full implementation of [JSON-RPC 2.0](http://www.jsonrpc.org/specification) (parameters can be passed in by order or by name)
* Very fast performance as it takes advantage of System.Reflection.Emit to execute methods
* Services participate in Asp.Net Core dependency injection framework
* Integrates with Asp.Net Core authorization schemes including Roles & Polices
* Built in data context idea that can be used to fetch and save data into header
* Filter support similar to Asp.Net filter (not exactly the same as no controller is ever created)
* Validation support using DataAnnotations and/or FluentValidation
* Support for request/response gzip compression
* Built in documentation/execution UI

### Example App
An example app can be found [here](https://github.com/ipjohnson/EasyRpc.AspNetCore.Sample).

### Build
[![Build status](https://ci.appveyor.com/api/projects/status/1sflvdvnetodybab?svg=true)](https://ci.appveyor.com/project/ipjohnson/easyrpc) [![Coverage Status](https://coveralls.io/repos/github/ipjohnson/EasyRpc/badge.svg?branch=master)](https://coveralls.io/github/ipjohnson/EasyRpc?branch=master)


