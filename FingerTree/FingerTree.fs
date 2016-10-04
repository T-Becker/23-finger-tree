﻿namespace CmdQ

    type Node<'a> =
        | Node2 of 'a * 'a
        | Node3 of 'a * 'a * 'a

        static member OfList = function
            | [a; b] -> Node2(a, b)
            | [a; b; c] -> Node3(a, b, c)
            | _ -> failwith "Only lists of length 2 or 3 accepted!"

        member me.ToList () =
            match me with
            | Node2(a, b) -> [a; b]
            | Node3(a, b, c) -> [a; b; c]

    type Digit<'a> =
        | One of 'a
        | Two of 'a * 'a
        | Three of 'a * 'a * 'a
        | Four of 'a * 'a * 'a * 'a

        static member OfList = function
            | [a] -> One(a)
            | [a; b] -> Two(a, b)
            | [a; b; c] -> Three(a, b, c)
            | [a; b; c; d] -> Four(a, b, c, d)
            | _ -> failwith "Only lists of length 1 to 4 accepted!"

        member me.ToList () =
            match me with
            | One a -> [a]
            | Two(a, b) -> [a; b]
            | Three(a, b, c) -> [a; b; c]
            | Four(a, b, c, d) -> [a; b; c; d]

        member me.Append x =
            match me with
            | One a -> Two(a, x)
            | Two(a, b) -> Three(a, b, x)
            | Three(a, b, c) -> Four(a, b, c, x)
            | _ -> failwith "Cannot prepend to Digit.Four!"

        member me.Prepend x =
            match me with
            | One a -> Two(x, a)
            | Two(a, b) -> Three(x, a, b)
            | Three(a, b, c) -> Four(x, a, b, c)
            | _ -> failwith "Cannot prepend to Digit.Four!"

    [<NoComparison>]
    [<NoEquality>]
    type FingerTree<'a> =
        | Empty
        | Single of 'a
        | Deep of Digit<'a> * FingerTree<Node<'a>> * Digit<'a>

    type Digit<'a> with
        member me.Promote () =
            match me with
            | One a -> Single a
            | Two(a, b) -> Deep(One a, Empty, One b)
            | Three(a, b, c) -> Deep(One a, Empty, Two(b, c))
            | Four(a, b, c, d) -> Deep(Two(a, b), Empty, Two(c, d))

    type View<'a> = Nil | View of 'a * FingerTree<'a>

type private Polymorphy() =
    static member ViewL (this:FingerTree<'a>) : View<'a> =
        match this with
        | Empty -> Nil
        | Single x -> View(x, Empty)
        | Deep(One x, deeper, suffix) ->
            let rest =
                match Polymorphy.ViewL deeper with
                | Nil ->
                    suffix.Promote()
                | View (node:Node<'a>, rest) ->
                    let prefix = node.ToList() |> Digit<_>.OfList
                    Deep(prefix, rest, suffix)
            View(x, rest)
        | Deep(prefix, deeper, suffix) ->
            match prefix.ToList() with
            | x::xs ->
                View(x, Deep(Digit<_>.OfList xs, deeper, suffix))
            | _ -> invalidOp "Impossible!"
(*
    static member Append<'a> (z:'a) : FingerTree<'a> -> FingerTree<'a> = function
        | Empty -> Single z
        | Single y -> Deep(One y, Empty, One z)
        | Deep(prefix, deeper, Four(v, w, x, y)) ->
            Deep(prefix, Polymorphy.Append (Node3(v, w, x)) deeper, Two(y, z))
        | Deep(prefix, deeper, suffix) ->
            Deep(prefix, deeper, suffix.Append z)

    static member Prepend<'a> (a:'a) : FingerTree<'a> -> FingerTree<'a> = function
        | Empty -> Single a
        | Single b -> Deep(One a, Empty, One b)
        | Deep(Four(b, c, d, e), deeper, suffix) ->
            Deep(Two(a, b), Polymorphy.Prepend (Node3(c, d, e)) deeper, suffix)
        | Deep(prefix, deeper, suffix) ->
            Deep(prefix.Prepend a, deeper, suffix)
*)


module Finger =
    let rec viewl : FingerTree<'a> -> View<'a> = function
        | Empty -> Nil
        | Single x -> View(x, Empty)
        | Deep(One x, deeper(*:FingerTree<'a>/FingerTree<Node<'a>>*), suffix) ->
            let rest =
                match viewl deeper with
                | Nil ->
                    suffix.Promote()
                | View (node(*:Node<'a>*), rest) ->
                    let prefix = node.ToList() |> Digit<_>.OfList
                    Deep(prefix, rest, suffix)
            View(x, rest)
        | Deep(prefix, deeper, suffix) ->
            match prefix.ToList() with
            | x::xs ->
                View(x, Deep(Digit<_>.OfList xs, deeper, suffix))
            | _ -> failwith "Impossible!"

    let empty = Empty

    let rec append<'a> (z:'a) : FingerTree<'a> -> FingerTree<'a> = function
        | Empty -> Single z
        | Single y -> Deep(One y, Empty, One z)
        | Deep(prefix, deeper, Four(v, w, x, y)) ->
            Deep(prefix, append (Node3(v, w, x)) deeper, Two(y, z))
        | Deep(prefix, deeper, suffix) ->
            Deep(prefix, deeper, suffix.Append z)

    let rec prepend<'a> (a:'a) : FingerTree<'a> -> FingerTree<'a> = function
        | Empty -> Single a
        | Single b -> Deep(One a, Empty, One b)
        | Deep(Four(b, c, d, e), deeper, suffix) ->
            Deep(Two(a, b), prepend (Node3(c, d, e)) deeper, suffix)
        | Deep(prefix, deeper, suffix) ->
            Deep(prefix.Prepend a, deeper, suffix)

    type Foo<'a> = Foo of ('a -> 'a)





//module Exp =
//    let layer2 =
//        let isList = "is".ToCharArray() |> Array.toList
//        let prefix = Two(Node<_>.OfList isList, Node<_>.OfList isList)
//        let suffix = Two(Node<_>.OfList ['n'; 'o'; 't'], Node<_>.OfList ['a'; 't'])
//        Deep(prefix, Empty, suffix)
//
//    let layer1 =
//        let prefix = Digit<_>.OfList ['t'; 'h']
//        let suffix = Digit<_>.OfList ['r'; 'e'; 'e']
//        Deep(prefix, layer2 , suffix)
//
//    let name =
//        "Tobias Becker".ToCharArray()
//        |> Array.fold (fun acc c -> acc |> Finger.append c) Finger.empty
//
//
