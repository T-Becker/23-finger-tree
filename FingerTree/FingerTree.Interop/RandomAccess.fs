﻿namespace CmdQ.FingerTree.Interop

open CmdQ.FingerTree
open CmdQ.FingerTree.Monoidal
open CmdQ.FingerTree.Monoids.RandomAccess
open System
open System.Collections.Generic

type ImmutableList<'T>(source:seq<'T>) =
    let mutable tree = source |> RandomAccess.ofSeq

    new () = ImmutableList<'T>(Seq.empty:seq<'T>)

    new (source:ImmutableList<_>) as this =
        ImmutableList()
        then
            this.Tree <- source.Tree

    member internal __.Tree
        with get () = tree
        and set value = tree <- value

    member __.Head = tree |> RandomAccess.head

    member __.Last = tree |> RandomAccess.last

    member __.Tail () = ImmutableList(Tree = RandomAccess.tail tree)

    member __.ButLast () = ImmutableList(Tree = RandomAccess.butLast tree)

    member me.IsEmpty = RandomAccess.isEmpty tree

    abstract Item : index:int -> 'T with get, set
    default __.Item
        with get index = tree |> RandomAccess.item index
        and set _ _ = invalidOp "Finger tree is read-only."

    // interface IEnumerable<'T> with

    member __.GetEnumerator () =
        let s = tree |> RandomAccess.toSeq
        s.GetEnumerator()

    interface IEnumerable<'T> with
        member me.GetEnumerator () = me.GetEnumerator()
        member me.GetEnumerator () = me.GetEnumerator() :> System.Collections.IEnumerator

    // interface ICollection<'T> with

    member __.Count = tree |> RandomAccess.length

    member __.IsReadOnly = true

    member me.CopyTo(array:'T [], arrayIndex) =
        if isNull array then
            ArgumentNullException("array") |> raise
        if arrayIndex < 0 then
            ArgumentOutOfRangeException("arrayIndex", "Index cannot be negative.") |> raise
        if array.Length - arrayIndex < me.Count then
            ArgumentException("The destination array does not have enough room to store everything.") |> raise
        let rec loop i tree =
            match FingerTree.viewl tree with
            | View(Value head, Lazy tail) ->
                array.[i] <- head
                loop (i + 1) tail
            | Nil ->
                assert(i = me.Count)
        loop 0 tree

[<Sealed>]
type MutableList<'T>(source:seq<'T>) =
    inherit ImmutableList<'T>(source)

    new () = MutableList(Seq.empty:seq<'T>)

    new (source:ImmutableList<_>) as this =
        MutableList()
        then
            this.Tree <- source.Tree

    // interface IList<'T> with

    override me.Item
        with get index = base.[index]
        and set index value = me.Tree <- RandomAccess.set me.Tree index value

    member me.Add item = me.Tree <- me.Tree |> RandomAccess.append item

    member me.Clear () = me.Tree <- RandomAccess.empty

    member me.Insert(index, item) = me.Tree <- me.Tree |> RandomAccess.insertAt index item

    member me.RemoveAt index = me.Tree <- me.Tree |> RandomAccess.removeIndex index

    interface IList<'T> with
        member me.Count = me.Count
        member me.Item
            with get index = (me :> ImmutableList<_>).[index]
            and set index v = me.[index] <- v
        member me.Add item = me.Add item
        member me.Clear () = me.Clear()
        member me.Contains item = failwith "Not implemented yet"
        member me.CopyTo(array, arrayIndex) = me.CopyTo(array, arrayIndex)
        member me.GetEnumerator () = me.GetEnumerator()
        member me.GetEnumerator () = (me :> System.Collections.IEnumerable).GetEnumerator()
        member me.IndexOf item = failwith "Not implemented yet"
        member me.Insert(index, item) = me.Insert(index, item)
        member me.IsReadOnly = false
        member me.Remove item = failwith "Not implemented yet"
        member me.RemoveAt index = me.RemoveAt index

namespace CmdQ.FingerTree.Interop.Extensions
    open CmdQ.FingerTree
    open CmdQ.FingerTree.Interop
    open System.Runtime.CompilerServices

    [<AutoOpen>]
    module private Helpers =
        type Which<'T> = {
            Tree : RandomAccess.Tree<'T>
            Mutable : bool
        }

        let extract (tree:ImmutableList<_>) =
            match tree with
            | :? MutableList<_> -> { Tree = tree.Tree; Mutable = true }
            | _ -> { Tree = tree.Tree; Mutable = false }

        let doAndPack f tree =
            let which = extract tree
            let re = f which.Tree
            if which.Mutable then
                MutableList(Tree = re) :> ImmutableList<_>
            else
                ImmutableList(Tree = re)

    [<Extension>]
    type RandomAccess =
        [<Extension>]
        static member Append(tree:ImmutableList<_>, item) =
            tree |> doAndPack (RandomAccess.append item)

        [<Extension>]
        static member Prepend(tree:ImmutableList<_>, item) =
            tree |> doAndPack (RandomAccess.prepend item)

        [<Extension>]
        static member Concat(tree:ImmutableList<_>, rhs:ImmutableList<_>) =
            tree |> doAndPack (fun lhs -> RandomAccess.concat lhs rhs.Tree)

        [<Extension>]
        static member Set(tree:ImmutableList<_>, index, value) =
            tree |> doAndPack (fun tree -> RandomAccess.set tree index value)

        [<Extension>]
        static member InsertAt(tree:ImmutableList<_>, index, value) =
            tree |> doAndPack (RandomAccess.insertAt index value)

        [<Extension>]
        static member RemoveIndex(tree:ImmutableList<_>, index) =
            tree |> doAndPack (RandomAccess.removeIndex index)