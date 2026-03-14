module MyApp.Server.Index

open Microsoft.AspNetCore.Components
open Microsoft.AspNetCore.Components.Web
open Bolero
open Bolero.Html
open Bolero.Server.Html
open MyApp

let page = doctypeHtml {
    head {
        meta { attr.charset "UTF-8" }
        meta { attr.name "viewport"; attr.content "width=device-width, initial-scale=1.0" }
        title { "Bolero Application" }
        ``base`` { attr.href "/" }
        link { attr.rel "stylesheet"; attr.href "css/bulma.min.css" }
        link { attr.rel "stylesheet"; attr.href "css/index.css" }
        link { attr.rel "stylesheet"; attr.href "MyApp.Client.styles.css" }
    }
    body {
        nav {
            attr.``class`` "navbar is-dark"
            "role" => "navigation"
            attr.aria "label" "main navigation"
            div {
                attr.``class`` "navbar-brand"
                a {
                    attr.``class`` "navbar-item has-text-weight-bold is-size-5"
                    attr.href "https://fsbolero.io"
                    img { attr.style "height:40px"; attr.src "https://github.com/fsbolero/website/raw/master/src/Website/img/wasm-fsharp.png" }
                    "  Bolero"
                }
            }
        }
        div {
            attr.id "main"
#if SERVER_ONLY
            comp<Client.Main.MyApp> { attr.renderMode RenderMode.InteractiveServer }
#else
            comp<Client.Main.MyApp> { attr.renderMode RenderMode.InteractiveWebAssembly }
#endif
        }
        boleroScript
    }
}

[<Route "/{*path}">]
type Page() =
    inherit Bolero.Component()
    override _.Render() = page
