module MyApp.Client.EmBoard

open Bolero

type Board = Template<"wwwroot/em-board.html">

let boardExperimentPage _ _ =
    Board.Slice().Elt()
