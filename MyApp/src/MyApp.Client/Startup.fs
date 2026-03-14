namespace MyApp.Client

open Microsoft.AspNetCore.Components.WebAssembly.Hosting
#if !WASM_ONLY
open Bolero.Remoting.Client
#endif

module Program =

    [<EntryPoint>]
    let Main args =
        let builder = WebAssemblyHostBuilder.CreateDefault(args)
#if WASM_ONLY
        builder.RootComponents.Add<Main.MyApp>("#main")
#else
        builder.Services.AddBoleroRemoting(builder.HostEnvironment) |> ignore
#endif
        builder.Build().RunAsync() |> ignore
        0
