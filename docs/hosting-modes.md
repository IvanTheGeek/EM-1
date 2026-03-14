# MyApp Hosting Modes

This application supports three hosting modes, controlled by the MSBuild
property `HostingMode`. Each mode produces a different deployment artifact and
serves a different use-case.

| Mode | Property value | What runs in the browser | Server required? |
|------|---------------|-------------------------|-----------------|
| **WASM + Server** (default) | *(none)* | WebAssembly client | Yes (ASP.NET Core) |
| **WASM-only** | `WasmOnly` | WebAssembly client | No (static files) |
| **Server-only** | `ServerOnly` | Blazor Server (SignalR) | Yes (ASP.NET Core) |

---

## 1. WASM + Server (default / hosted)

The original mode. The ASP.NET Core server hosts the Blazor WebAssembly
client, provides Bolero Remoting services (books, auth), and pre-renders
the initial HTML.

### Build & run (development)

```bash
cd MyApp
dotnet run --project src/MyApp.Server
```

### Publish for production

```bash
cd MyApp
dotnet publish src/MyApp.Server/MyApp.Server.fsproj -c Release -o publish/hosted
```

Deploy the `publish/hosted/` directory to any host that supports ASP.NET
Core (Azure App Service, a Linux VM with Kestrel behind a reverse proxy,
Docker, etc.).

### What you get

- Server-side book data (loaded from `data/books.json`)
- Cookie-based authentication
- Full Bolero Remoting between client and server
- WebAssembly downloaded to the browser on first visit

---

## 2. WASM-only (static hosting)

A fully standalone WebAssembly application. No server is required after
the initial download. Books are hardcoded sample data and authentication
is a simple in-memory check (password = `"password"`).

### Build & publish

```bash
cd MyApp
dotnet publish src/MyApp.Client/MyApp.Client.fsproj -c Release -p:HostingMode=WasmOnly -o publish/wasm
```

### Deploy

Upload **everything inside `publish/wasm/wwwroot/`** to any static file
host:

- **GitHub Pages** — push the `wwwroot/` contents to the `gh-pages` branch
- **Netlify / Vercel** — point to the `wwwroot/` folder
- **Azure Static Web Apps** — deploy the `wwwroot/` folder
- **AWS S3 + CloudFront** — upload to an S3 bucket with static hosting
- **Any web server** — Nginx, Apache, Caddy serving static files

### SPA routing

Because the app uses client-side routing, configure your host to serve
`index.html` for all paths that don't match a physical file. Examples:

**Netlify** — add a `_redirects` file in `wwwroot/`:
```
/*    /index.html   200
```

**Nginx:**
```nginx
location / {
    try_files $uri $uri/ /index.html;
}
```

**Azure Static Web Apps** — add `staticwebapp.config.json`:
```json
{
  "navigationFallback": {
    "rewrite": "/index.html",
    "exclude": ["/_framework/*", "/css/*"]
  }
}
```

### Offline capability

Once the browser has downloaded all the WASM/JS/CSS assets, the app runs
entirely in-browser with no network calls. All CSS (Bulma) is bundled
locally. No PWA service worker is installed — the browser's standard HTTP
cache handles repeat visits.

---

## 3. Server-only (Blazor Server)

The Blazor component runs on the server and the browser receives UI
updates over a SignalR WebSocket connection. No WebAssembly is downloaded
to the client. This gives the fastest initial load but requires a
persistent connection to the server.

### Build & run (development)

```bash
cd MyApp
dotnet run --project src/MyApp.Server -p:HostingMode=ServerOnly
```

### Publish for production

```bash
cd MyApp
dotnet publish src/MyApp.Server/MyApp.Server.fsproj -c Release -p:HostingMode=ServerOnly -o publish/server
```

Deploy the `publish/server/` directory the same way as the hosted mode
(any ASP.NET Core host).

### What you get

- Fastest initial page load (no WASM download)
- Full server-side book data and authentication
- Requires a persistent WebSocket connection to the server
- All UI interactions round-trip through SignalR

---

## How the conditional compilation works

The `HostingMode` MSBuild property drives `#if` preprocessor directives:

| Property | Client gets define | Server gets define |
|----------|-------------------|-------------------|
| *(default)* | *(none)* | *(none)* |
| `WasmOnly` | `WASM_ONLY` | *(not built)* |
| `ServerOnly` | *(none)* | `SERVER_ONLY` |

### Key conditional blocks

| File | Condition | Effect |
|------|-----------|--------|
| `Client/Main.fs` | `WASM_ONLY` | Uses hardcoded sample books and local auth instead of Bolero Remoting |
| `Client/Startup.fs` | `WASM_ONLY` | Registers `MyApp` as root component; skips `AddBoleroRemoting` |
| `Server/Index.fs` | `SERVER_ONLY` | Uses `RenderMode.InteractiveServer` instead of `InteractiveWebAssembly` |
| `Server/Startup.fs` | `SERVER_ONLY` | Skips WebAssembly component registration and debugging |

---

## Quick reference

```bash
# Default: WASM + Server
dotnet publish src/MyApp.Server/MyApp.Server.fsproj -c Release -o publish/hosted

# WASM-only (static hosting, offline-capable)
dotnet publish src/MyApp.Client/MyApp.Client.fsproj -c Release -p:HostingMode=WasmOnly -o publish/wasm

# Server-only (Blazor Server, no WASM download)
dotnet publish src/MyApp.Server/MyApp.Server.fsproj -c Release -p:HostingMode=ServerOnly -o publish/server
```
