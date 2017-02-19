# EasyRpc
Adds JSON-RPC support to AspNetCore

```
public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
{
  ~
  app.UseJsonRPC("RpcApi", api =>
  {
     // Expose method at /RpcApi/IntMath
     api.Expose<IntMathService>().As("IntMath");
     
     // Expose methods at /RpcApi/Strings require user to have SomePolicy
     api.Expose<StringService>().As("Strings").Authorize(policy: "SomePolicy");
     
     // Expose all types in this assembly in the namespace MyProject.Services
     api.ExposeThisAssembly().ByType().As(NameMethod).Where(TypesThat.AreInNamespace("MyProject.Services");
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
