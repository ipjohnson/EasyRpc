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

### Example App

### Build
[![Build status](https://ci.appveyor.com/api/projects/status/1sflvdvnetodybab?svg=true)](https://ci.appveyor.com/project/ipjohnson/easyrpc) [![Coverage Status](https://coveralls.io/repos/github/ipjohnson/EasyRpc/badge.svg?branch=master)](https://coveralls.io/github/ipjohnson/EasyRpc?branch=master)


