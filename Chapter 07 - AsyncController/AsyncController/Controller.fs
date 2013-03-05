﻿namespace FSharp.Windows

open System.ComponentModel

type EventHandler<'Model> = 
    | Sync of ('Model -> unit)
    | Async of ('Model -> Async<unit>)

type IController<'Events, 'Model when 'Model :> INotifyPropertyChanged> =

    abstract InitModel : 'Model -> unit
    abstract Dispatcher : ('Events -> EventHandler<'Model>)

[<AbstractClass>]
type Controller<'Events, 'Model when 'Model :> INotifyPropertyChanged>() =

    interface IController<'Events, 'Model> with
        member this.InitModel model = this.InitModel model
        member this.Dispatcher = this.Dispatcher

    abstract InitModel : 'Model -> unit
    abstract Dispatcher : ('Events -> EventHandler<'Model>)

    static member Create callback = {
        new IController<'Events, 'Model> with
            member this.InitModel _ = ()
            member this.Dispatcher = callback
    } 

[<AbstractClass>]
type SyncController<'Events, 'Model when 'Model :> INotifyPropertyChanged>() =
    inherit Controller<'Events, 'Model>()

    abstract Dispatcher : ('Events -> 'Model -> unit)
    override this.Dispatcher = fun e -> Sync(this.Dispatcher e)

[<AbstractClass>]
type AsyncInitController<'Events, 'Model when 'Model :> INotifyPropertyChanged>() =
    inherit Controller<'Events, 'Model>()

    abstract InitModel : 'Model -> Async<unit>
    override this.InitModel model = model |> this.InitModel |> Async.StartImmediate

    static member inline Create(controller : ^Controller) = {
        new IController<'Events, 'Model> with
            member this.InitModel model = (^Controller : (member InitModel : 'Model -> Async<unit>) (controller, model)) |> Async.StartImmediate
            member this.Dispatcher = (^Controller : (member Dispatcher : ('Events -> EventHandler<'Model>)) controller)
    } 
