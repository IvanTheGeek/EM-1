module MyApp.Client.Main

open System
open Elmish
open Bolero
open Bolero.Html
#if !WASM_ONLY
open Bolero.Remoting
open Bolero.Remoting.Client
#endif
open Bolero.Templating.Client

/// Routing endpoints definition.
type Page =
    | [<EndPoint "/">] Home
    | [<EndPoint "/counter">] Counter
    | [<EndPoint "/data">] Data
    | [<EndPoint "/event-modeling">] EventModeling
    | [<EndPoint "/board">] BoardExperiment

/// The Elmish application's model.
type Model =
    {
        page: Page
        counter: int
        books: Book[] option
        error: string option
        username: string
        password: string
        signedInAs: option<string>
        signInFailed: bool
    }

and Book =
    {
        title: string
        author: string
        publishDate: DateTime
        isbn: string
    }

let initModel =
    {
        page = Home
        counter = 0
        books = None
        error = None
        username = ""
        password = ""
        signedInAs = None
        signInFailed = false
    }

#if !WASM_ONLY
/// Remote service definition.
type BookService =
    {
        /// Get the list of all books in the collection.
        getBooks: unit -> Async<Book[]>

        /// Add a book in the collection.
        addBook: Book -> Async<unit>

        /// Remove a book from the collection, identified by its ISBN.
        removeBookByIsbn: string -> Async<unit>

        /// Sign into the application.
        signIn : string * string -> Async<option<string>>

        /// Get the user's name, or None if they are not authenticated.
        getUsername : unit -> Async<string>

        /// Sign out from the application.
        signOut : unit -> Async<unit>
    }

    interface IRemoteService with
        member this.BasePath = "/books"
#endif

/// The Elmish application's update messages.
type Message =
    | SetPage of Page
    | Increment
    | Decrement
    | SetCounter of int
    | GetBooks
    | GotBooks of Book[]
    | SetUsername of string
    | SetPassword of string
    | ClearLoginForm
    | GetSignedInAs
    | RecvSignedInAs of option<string>
    | SendSignIn
    | RecvSignIn of option<string>
    | SendSignOut
    | RecvSignOut
    | Error of exn
    | ClearError

#if WASM_ONLY

let sampleBooks =
    [|
        { title = "The Fellowship of the Ring"; author = "J.R.R Tolkien";   publishDate = DateTime(1954, 7, 29);  isbn = "978-0345339706" }
        { title = "The Two Towers";             author = "J.R.R Tolkien";   publishDate = DateTime(1954, 11, 11); isbn = "978-0345339713" }
        { title = "The Return of the King";     author = "J.R.R Tolkien";   publishDate = DateTime(1955, 10, 20); isbn = "978-0345339737" }
    |]

let update message model =
    let onSignIn = function
        | Some _ -> Cmd.batch [ Cmd.ofMsg GetBooks; Cmd.ofMsg ClearLoginForm ]
        | None -> Cmd.none
    match message with
    | SetPage page ->
        { model with page = page }, Cmd.none

    | Increment ->
        { model with counter = model.counter + 1 }, Cmd.none
    | Decrement ->
        { model with counter = model.counter - 1 }, Cmd.none
    | SetCounter value ->
        { model with counter = value }, Cmd.none

    | GetBooks ->
        { model with books = Some sampleBooks }, Cmd.none
    | GotBooks books ->
        { model with books = Some books }, Cmd.none

    | SetUsername s ->
        { model with username = s }, Cmd.none
    | SetPassword s ->
        { model with password = s }, Cmd.none
    | ClearLoginForm ->
        { model with username = ""; password = "" }, Cmd.none

    | GetSignedInAs ->
        model, Cmd.none
    | RecvSignedInAs username ->
        { model with signedInAs = username }, onSignIn username

    | SendSignIn ->
        let result =
            if model.password = "password" && model.username <> ""
            then Some model.username
            else None
        { model with signedInAs = result; signInFailed = Option.isNone result },
        onSignIn result
    | RecvSignIn username ->
        { model with signedInAs = username; signInFailed = Option.isNone username },
        onSignIn username

    | SendSignOut ->
        { model with signedInAs = None; signInFailed = false; books = None }, Cmd.none
    | RecvSignOut ->
        { model with signedInAs = None; signInFailed = false }, Cmd.none

    | Error exn ->
        { model with error = Some exn.Message }, Cmd.none
    | ClearError ->
        { model with error = None }, Cmd.none

#else

let update remote message model =
    let onSignIn = function
        | Some _ -> Cmd.batch [ Cmd.ofMsg GetBooks; Cmd.ofMsg ClearLoginForm ]
        | None -> Cmd.none
    match message with
    | SetPage page ->
        { model with page = page }, Cmd.none

    | Increment ->
        { model with counter = model.counter + 1 }, Cmd.none
    | Decrement ->
        { model with counter = model.counter - 1 }, Cmd.none
    | SetCounter value ->
        { model with counter = value }, Cmd.none

    | GetBooks ->
        let cmd = Cmd.OfAsync.either remote.getBooks () GotBooks Error
        { model with books = None }, cmd
    | GotBooks books ->
        { model with books = Some books }, Cmd.none

    | SetUsername s ->
        { model with username = s }, Cmd.none
    | SetPassword s ->
        { model with password = s }, Cmd.none
    | ClearLoginForm ->
        { model with
            username = ""
            password = "" }, Cmd.none
    | GetSignedInAs ->
        model, Cmd.OfAuthorized.either remote.getUsername () RecvSignedInAs Error
    | RecvSignedInAs username ->
        { model with signedInAs = username }, onSignIn username
    | SendSignIn ->
        model, Cmd.OfAsync.either remote.signIn (model.username, model.password) RecvSignIn Error
    | RecvSignIn username ->
        { model with signedInAs = username; signInFailed = Option.isNone username }, onSignIn username
    | SendSignOut ->
        model, Cmd.OfAsync.either remote.signOut () (fun () -> RecvSignOut) Error
    | RecvSignOut ->
        { model with signedInAs = None; signInFailed = false }, Cmd.none

    | Error RemoteUnauthorizedException ->
        { model with error = Some "You have been logged out."; signedInAs = None }, Cmd.none
    | Error exn ->
        { model with error = Some exn.Message }, Cmd.none
    | ClearError ->
        { model with error = None }, Cmd.none

#endif
