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
    | [<EndPoint "/event-modeling">] EventModeling
    | [<EndPoint "/board">] BoardExperiment

/// The Elmish application's model.
type Model =
    {
        page: Page
        error: string option
    }

let initModel =
    {
        page = Home
        error = None
    }

#if !WASM_ONLY
/// Remote service definition for model data.
type ModelService =
    {
        /// List all model files (relative paths).
        getModelFiles: unit -> Async<string[]>

        /// Read the contents of a model file by relative path.
        getModelFile: string -> Async<string>
    }

    interface IRemoteService with
        member this.BasePath = "/model"
#endif

/// The Elmish application's update messages.
type Message =
    | SetPage of Page
    | Error of exn
    | ClearError

/// Minimal effects record — extend as features are added.
type Effects =
    {
        onError: exn -> Model -> Model * Cmd<Message>
    }

let defaultEffects : Effects =
    {
        onError = fun exn model ->
            { model with error = Some exn.Message }, Cmd.none
    }

let update (fx: Effects) message model =
    match message with
    | SetPage page ->
        { model with page = page }, Cmd.none
    | Error exn ->
        fx.onError exn model
    | ClearError ->
        { model with error = None }, Cmd.none
