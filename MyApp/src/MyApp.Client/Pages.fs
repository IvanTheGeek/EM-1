module MyApp.Client.Pages

open System
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
        .CounterUrl(router.Link Counter)
        .DataUrl(router.Link Data)
        .EventModelingUrl(router.Link EventModeling)
        .Elt()

let eventModelingPage model dispatch =
    T.EventModeling().Elt()

let counterPage model dispatch =
    T.Counter()
        .Decrement(fun _ -> dispatch Decrement)
        .Increment(fun _ -> dispatch Increment)
        .Value(model.counter, fun v -> dispatch (SetCounter v))
        .Elt()

let dataPage model (username: string) dispatch =
    T.Data()
        .Reload(fun _ -> dispatch GetBooks)
        .Username(username)
        .SignOut(fun _ -> dispatch SendSignOut)
        .Rows(cond model.books <| function
            | None ->
                T.EmptyData().Elt()
            | Some books ->
                forEach books <| fun book ->
                    tr {
                        td { book.title }
                        td { book.author }
                        td { book.publishDate.ToString("yyyy-MM-dd") }
                        td { book.isbn }
                    })
        .Elt()

let signInPage model dispatch =
    T.SignIn()
        .Username(model.username, fun s -> dispatch (SetUsername s))
        .Password(model.password, fun s -> dispatch (SetPassword s))
        .SignIn(fun _ -> dispatch SendSignIn)
        .ErrorNotification(
            cond model.signInFailed <| function
            | false -> empty()
            | true ->
                T.ErrorNotification()
                    .HideClass("is-hidden")
                    .Text("Sign in failed. Use any username and the password \"password\".")
                    .Elt()
        )
        .Elt()

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
            menuItem model Counter "Counter"
            menuItem model Data "Download data"
            menuItem model EventModeling "Event Modeling"
            menuItem model BoardExperiment "Board Experiment"
        })
        .Body(
            cond model.page <| function
            | Home -> homePage model dispatch
            | Counter -> counterPage model dispatch
            | EventModeling -> eventModelingPage model dispatch
            | BoardExperiment -> EmBoard.boardExperimentPage model dispatch
            | Data ->
                cond model.signedInAs <| function
                | Some username -> dataPage model username dispatch
                | None -> signInPage model dispatch
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
#if WASM_ONLY
        Program.mkProgram (fun _ -> initModel, Cmd.none) update view
#else
        let bookService = this.Remote<BookService>()
        let update = update bookService
        Program.mkProgram (fun _ -> initModel, Cmd.ofMsg GetSignedInAs) update view
#endif
        |> Program.withRouter router
#if DEBUG
        |> Program.withHotReload
#endif
