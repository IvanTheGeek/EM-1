# Event Modeling Experiment

An F# Bolero application for experimenting with Event Modeling visualizations.

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

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
