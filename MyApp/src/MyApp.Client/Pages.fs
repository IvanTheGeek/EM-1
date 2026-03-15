module MyApp.Client.Pages

open Elmish
open Bolero
open Bolero.Html
#if !WASM_ONLY
open Bolero.Remoting
open Bolero.Remoting.Client
#endif
open Bolero.Templating.Client
open MyApp.Client.Main

/// Connects the routing system to the Elmish application.
let router = Router.infer SetPage (fun model -> model.page)

type T = Template<"wwwroot/main.html">

let homePage model dispatch =
    T.Home()
        .EventModelingUrl(router.Link EventModeling)
        .Elt()

let eventModelingPage model dispatch =
    T.EventModeling().Elt()

let menuItem (model: Model) (page: Page) (text: string) =
    T.MenuItem()
        .Active(if model.page = page then "is-active" else "")
        .Url(router.Link page)
        .Text(text)
        .Elt()

let view model dispatch =
    T()
        .Menu(concat {
            menuItem model Home "Home"
            menuItem model EventModeling "Event Modeling"
            menuItem model BoardExperiment "Board Experiment"
        })
        .Body(
            cond model.page <| function
            | Home -> homePage model dispatch
            | EventModeling -> eventModelingPage model dispatch
            | BoardExperiment -> EmBoard.boardExperimentPage model dispatch
        )
        .Error(
            cond model.error <| function
            | None -> empty()
            | Some err ->
                T.ErrorNotification()
                    .Text(err)
                    .Hide(fun _ -> dispatch ClearError)
                    .Elt()
        )
        .Elt()

type MyApp() =
    inherit ProgramComponent<Model, Message>()

    override _.CssScope = CssScopes.MyApp

    override this.Program =
        let update = update defaultEffects
        Program.mkProgram (fun _ -> initModel, Cmd.none) update view
        |> Program.withRouter router
#if DEBUG
        |> Program.withHotReload
#endif
