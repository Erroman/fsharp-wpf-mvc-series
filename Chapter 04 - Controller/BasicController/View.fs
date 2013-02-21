﻿namespace FSharp.Windows

open System
open System.Windows

type IView<'Event> =
    inherit IObservable<'Event>

    abstract SetBindings : obj -> unit

[<AbstractClass>]
type View<'Event, 'Window when 'Window :> Window and 'Window : (new : unit -> 'Window)>(?window) = 

    let window = defaultArg window (new 'Window())

    member this.Window = window
    static member (?) (view : View<_, _>, name) = 
        match view.Window.FindName name with
        | null -> 
            match view.Window.TryFindResource name with
            | null -> invalidArg "Name" ("Cannot find child control or resource named: " + name)
            | resource -> unbox resource 
        | control -> unbox control
    
    interface IView<'Event> with
        member this.Subscribe observer = 
            let xs = this.EventStreams |> List.reduce Observable.merge 
            xs.Subscribe observer
        member this.SetBindings model = 
            window.DataContext <- model
            this.SetBindings model

    abstract EventStreams : IObservable<'Event> list
    abstract SetBindings : obj -> unit

[<AbstractClass>]
type XamlView<'Event>(resourceLocator) = 
    inherit View<'Event, Window>(resourceLocator |> Application.LoadComponent |> unbox)


