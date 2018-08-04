namespace EasyRpc.TestApp.FSharp

open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open EasyRpc.AspNetCore
open EasyRpc.TestApp.FSharp.Services

type Startup() =

    member this.ConfigureServices(services: IServiceCollection) =
        services.AddJsonRpc() |>ignore

    member this.Configure(app: IApplicationBuilder) =
        app.UseJsonRpc("/",
            fun api -> api.ExposeNamespaceContaining<Anchor>() |> ignore) |> ignore