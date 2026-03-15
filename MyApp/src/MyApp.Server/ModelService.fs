namespace MyApp.Server

open System.IO
open Microsoft.AspNetCore.Hosting
open Bolero.Remoting
open Bolero.Remoting.Server
open MyApp

type ModelService(ctx: IRemoteContext, env: IWebHostEnvironment) =
    inherit RemoteHandler<Client.Main.ModelService>()

    let dataDir = Path.Combine(env.ContentRootPath, "data", "model")

    override this.Handler =
        {
            getModelFiles = fun () -> async {
                if Directory.Exists(dataDir) then
                    return
                        Directory.GetFiles(dataDir, "*.toml", SearchOption.AllDirectories)
                        |> Array.map (fun f -> Path.GetRelativePath(dataDir, f).Replace('\\', '/'))
                else
                    return [||]
            }

            getModelFile = fun path -> async {
                let fullPath = Path.GetFullPath(Path.Combine(dataDir, path))
                // Guard against path traversal
                if not (fullPath.StartsWith(Path.GetFullPath(dataDir))) then
                    return failwith "Invalid path"
                elif not (File.Exists(fullPath)) then
                    return failwith $"File not found: {path}"
                else
                    return! File.ReadAllTextAsync(fullPath) |> Async.AwaitTask
            }
        }
