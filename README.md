# Event Modeling Experiment

An F# Bolero application for experimenting with Event Modeling visualizations.

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

## Build Modes

All commands run from the repository root.

### WASM + Server (default)

WASM client runs in the browser; server backend handles remoting (ModelService).

**Windows:**

```
dotnet build MyApp\src\MyApp.Server\MyApp.Server.fsproj
dotnet run --project MyApp\src\MyApp.Server\MyApp.Server.fsproj
```

**Linux / macOS:**

```
dotnet build MyApp/src/MyApp.Server/MyApp.Server.fsproj
dotnet run --project MyApp/src/MyApp.Server/MyApp.Server.fsproj
```

### Server Only

Server renders all UI via Blazor InteractiveServer. No WASM payload downloaded by the browser.

**Windows:**

```
dotnet build MyApp\src\MyApp.Server\MyApp.Server.fsproj /p:HostingMode=ServerOnly
dotnet run --project MyApp\src\MyApp.Server\MyApp.Server.fsproj /p:HostingMode=ServerOnly
```

**Linux / macOS:**

```
dotnet build MyApp/src/MyApp.Server/MyApp.Server.fsproj /p:HostingMode=ServerOnly
dotnet run --project MyApp/src/MyApp.Server/MyApp.Server.fsproj /p:HostingMode=ServerOnly
```

### WASM Only

Pure client-side WASM. Model data files are copied from `MyApp/src/MyApp.Server/data/model/`
into `wwwroot/data/model/` at build time. No server required — can be served from any static
file host.

**Windows:**

```
dotnet build MyApp\src\MyApp.Client\MyApp.Client.fsproj /p:HostingMode=WasmOnly
dotnet run --project MyApp\src\MyApp.Client\MyApp.Client.fsproj /p:HostingMode=WasmOnly
```

**Linux / macOS:**

```
dotnet build MyApp/src/MyApp.Client/MyApp.Client.fsproj /p:HostingMode=WasmOnly
dotnet run --project MyApp/src/MyApp.Client/MyApp.Client.fsproj /p:HostingMode=WasmOnly
```

## Publish

All publish commands output to `./publish/` at the repo root (already gitignored).

> **Important:** Always delete the `publish/` folder before re-publishing. Leaving stale files
> from a previous build causes Blazor WASM version-mismatch errors at runtime
> (`dotnet.js` / `dotnet.runtime.js` / `dotnet.native.js` hash conflicts).
>
> ```
> # Windows CMD
> rmdir /s /q publish
>
> # PowerShell / Linux / macOS
> rm -rf publish/
> ```

### WASM Client + Server Backend/Remoting (default)

**Windows:**

```
dotnet publish MyApp\src\MyApp.Server\MyApp.Server.fsproj -c Release -o publish
```

**Linux / macOS:**

```
dotnet publish MyApp/src/MyApp.Server/MyApp.Server.fsproj -c Release -o publish
```

Run with: `dotnet publish/MyApp.Server.dll`

### Server Only

**Windows:**

```
dotnet publish MyApp\src\MyApp.Server\MyApp.Server.fsproj -c Release -o publish /p:HostingMode=ServerOnly
```

**Linux / macOS:**

```
dotnet publish MyApp/src/MyApp.Server/MyApp.Server.fsproj -c Release -o publish -p:HostingMode=ServerOnly
```

Run with: `dotnet publish/MyApp.Server.dll`

### WASM Only

**Windows:**

```
dotnet publish MyApp\src\MyApp.Client\MyApp.Client.fsproj -c Release -o publish /p:HostingMode=WasmOnly
```

**Linux / macOS:**

```
dotnet publish MyApp/src/MyApp.Client/MyApp.Client.fsproj -c Release -o publish -p:HostingMode=WasmOnly
```

###### TIP

The `publish/wwwroot/` folder is a self-contained static site. Serve it with any static file
host (e.g.  publish/wwwroot`, nginx, GitHub Pages, Azure Static Web Apps).

If you use the dotnet server:

```
dotnet tool install --global dotnet-serve
```

serve locally with: (from repo root)
```
dotnet serve -d publish/wwwroot
```

---

## Project Structure

```
MyApp/
├── MyApp.sln
└── src/
    ├── MyApp.Client/          # Blazor/Bolero client (F#)
    │   ├── Main.fs            # Domain types, Messages, Effects, update
    │   ├── Pages.fs           # Page views, routing, MyApp component
    │   ├── em-board.fs        # Board experiment page
    │   ├── Startup.fs         # WASM bootstrap
    │   └── wwwroot/
    │       ├── main.html      # Bolero HTML templates
    │       └── em-board.html  # Board template
    │
    └── MyApp.Server/          # ASP.NET Core server
        ├── ModelService.fs    # Remote service: serves TOML model files
        ├── Index.fs           # Server-rendered page shell
        ├── Startup.fs         # Server configuration
        └── data/
            └── model/         # TOML event model specifications
```
