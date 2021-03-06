﻿module CmdQ.FingerTree.Monoids

open System

/// Helper type to make any default-constructable type a singleton.
type Singleton<'a when 'a : (new : unit -> 'a)> private() =
    static let instance = new 'a()
    static member Instance = instance

/// A type that provides a zero element and an associative add method.
type IMonoid<'m> =
    abstract Zero:'m
    abstract Plus:'m -> 'm

/// A type that provides access to its measure.
type IMeasured<'m when 'm :> IMonoid<'m>> =
    abstract Measure:'m

/// Return the measure of a measured type.
let fmeasure (m:IMeasured<_>) = m.Measure

/// Associatively combine the measures of all elements in a list.
let mconcat monoids = monoids |> (List.map fmeasure >> List.reduce (fun a b -> a.Plus b))

module RandomAccess =
    [<Sealed>]
    type Size(value) =
        new () = Size 0

        member __.Value = value

        interface IMonoid<Size> with
            member __.Zero = Size()
            member __.Plus rhs = Size(value + rhs.Value)

        override __.ToString () = sprintf "Count = %i" value

    type Value<'a> =
        | Value of 'a

        interface IMeasured<Size> with
            member __.Measure = Size 1

        override me.ToString () =
            match me with
            | Value v -> v.ToString()

module PriorityQueue =
    [<Sealed>]
    type Priority(priority) =
        new () = Priority(Single.NegativeInfinity)

        member __.Priority = priority

        interface IMonoid<Priority> with
            member __.Zero = Priority()
            member __.Plus rhs = Priority(max priority rhs.Priority)

        override __.ToString () = sprintf "Priority = %f" priority

    type PriorityAndValue<'a> =
        {
            Priority : float32
            Value : 'a
        }
        interface IMeasured<Priority> with
            member me.Measure = Priority(me.Priority)

        override me.ToString () = sprintf "%O (priority %f)" me.Value me.Priority
