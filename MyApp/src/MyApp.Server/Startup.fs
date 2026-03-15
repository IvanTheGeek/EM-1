module MyApp.Server.Program

open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Bolero
open Bolero.Remoting.Server
open Bolero.Server
open MyApp
open Bolero.Templating.Server

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)

    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents()
#if !SERVER_ONLY
        .AddInteractiveWebAssemblyComponents()
#endif
    |> ignore
    builder.Services.AddServerSideBlazor() |> ignore
    builder.Services.AddBoleroRemoting<ModelService>() |> ignore
    builder.Services.AddBoleroComponents() |> ignore
#if DEBUG
    builder.Services.AddHotReload(templateDir = __SOURCE_DIRECTORY__ + "/../MyApp.Client") |> ignore
#endif

    let app = builder.Build()

#if !SERVER_ONLY
    if app.Environment.IsDevelopment() then
        app.UseWebAssemblyDebugging()
#endif

    app
        .UseStaticFiles()
        .UseRouting()
        .UseAntiforgery()
    |> ignore

#if DEBUG
    app.UseHotReload()
#endif
    app.MapBoleroRemoting() |> ignore
    app.MapRazorComponents<Index.Page>()
        .AddInteractiveServerRenderMode()
#if !SERVER_ONLY
        .AddInteractiveWebAssemblyRenderMode()
#endif
        .AddAdditionalAssemblies(typeof<Client.Pages.MyApp>.Assembly)
    |> ignore

    app.Run()
    0
