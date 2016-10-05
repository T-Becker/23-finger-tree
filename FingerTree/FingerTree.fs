﻿namespace CmdQ

module Errors =
    let treeIsEmpty = "The input tree was empty."

type Node<'a> =
    | Node2 of 'a * 'a
    | Node3 of 'a * 'a * 'a

module Node =
    let ofList = function
        | [a; b] -> Node2(a, b)
        | [a; b; c] -> Node3(a, b, c)
        | _ -> failwith "Only lists of length 2 or 3 accepted!"

    let toList = function
        | Node2(a, b) -> [a; b]
        | Node3(a, b, c) -> [a; b; c]

type Digit<'a> =
    | One of 'a
    | Two of 'a * 'a
    | Three of 'a * 'a * 'a
    | Four of 'a * 'a * 'a * 'a

[<NoComparison>]
[<NoEquality>]
type FingerTree<'a> =
    | Empty
    | Single of 'a
    | Deep of Digit<'a> * FingerTree<Node<'a>> * Digit<'a>

module Digit =
    let ofList = function
        | [a] -> One(a)
        | [a; b] -> Two(a, b)
        | [a; b; c] -> Three(a, b, c)
        | [a; b; c; d] -> Four(a, b, c, d)
        | _ -> failwith "Only lists of length 1 to 4 accepted!"

    let toList = function
        | One a -> [a]
        | Two(a, b) -> [a; b]
        | Three(a, b, c) -> [a; b; c]
        | Four(a, b, c, d) -> [a; b; c; d]

    let append x = function
        | One a -> Two(a, x)
        | Two(a, b) -> Three(a, b, x)
        | Three(a, b, c) -> Four(a, b, c, x)
        | _ -> failwith "Cannot append to Digit.Four!"

    let prepend x = function
        | One a -> Two(x, a)
        | Two(a, b) -> Three(x, a, b)
        | Three(a, b, c) -> Four(x, a, b, c)
        | _ -> failwith "Cannot prepend to Digit.Four!"

    let promote = function
        | One a -> Single a
        | Two(a, b) -> Deep(One a, Empty, One b)
        | Three(a, b, c) -> Deep(Two(a, b), Empty, One(c))
        | Four(a, b, c, d) -> Deep(Two(a, b), Empty, Two(c, d))

type View<'a> = Nil | View of 'a * FingerTree<'a>

module Finger =
    let (|SplitFirst|_|) = function
        | Two(a, b) -> Some(a, One b)
        | Three(a, b, c) -> Some(a, Two(b, c))
        | Four(a, b, c, d) -> Some(a, Three(b, c, d))
        | _ -> None

    let (|SplitLast|_|) = function
        | Two(a, b) -> Some(One a, b)
        | Three(a, b, c) -> Some(Two(a, b), c)
        | Four(a, b, c, d) -> Some(Three(a, b, c), d)
        | _ -> None

    let rec viewl<'a> : FingerTree<'a> -> View<'a> = function
        | Empty -> Nil
        | Single x -> View(x, Empty)
        | Deep(One x, deeper, suffix) ->
            let rest =
                match viewl deeper with
                | Nil ->
                    suffix |> Digit.promote
                | View (node, rest) ->
                    let prefix = node |> Node.toList |> Digit.ofList
                    Deep(prefix, rest, suffix)
            View(x, rest)
        | Deep(SplitFirst(x, shorter), deeper, suffix) ->
            View(x, Deep(shorter, deeper, suffix))
        | _ -> failwith "Impossible!"

    let rec viewr<'a> : FingerTree<'a> -> View<'a> = function
        | Empty -> Nil
        | Single x -> View(x, Empty)
        | Deep(prefix, deeper, One x) ->
            let rest =
                match viewr deeper with
                | Nil ->
                    prefix |> Digit.promote
                | View (node, rest) ->
                    let suffix = node |> Node.toList |> Digit.ofList
                    Deep(prefix, rest, suffix)
            View(x, rest)
        | Deep(prefix, deeper, SplitLast(shorter, x)) ->
            View(x, Deep(prefix, deeper, shorter))
        | _ -> failwith "Impossible!"

    let empty = Empty

    let isEmpty tree =
        match viewl tree with
        | Nil -> true
        | _ -> false

    let head tree =
        match viewl tree with
        | View(h, _) -> h
        | _ -> invalidArg "tree" Errors.treeIsEmpty

    let tail tree =
        match viewl tree with
        | View(_, t) -> t
        | _ -> invalidArg "tree" Errors.treeIsEmpty

    let rec append<'a> (z:'a) : FingerTree<'a> -> FingerTree<'a> = function
        | Empty -> Single z
        | Single y -> Deep(One y, Empty, One z)
        | Deep(prefix, deeper, Four(v, w, x, y)) ->
            Deep(prefix, append (Node3(v, w, x)) deeper, Two(y, z))
        | Deep(prefix, deeper, suffix) ->
            Deep(prefix, deeper, suffix |> Digit.append z)

    let rec prepend<'a> (a:'a) : FingerTree<'a> -> FingerTree<'a> = function
        | Empty -> Single a
        | Single b -> Deep(One a, Empty, One b)
        | Deep(Four(b, c, d, e), deeper, suffix) ->
            Deep(Two(a, b), prepend (Node3(c, d, e)) deeper, suffix)
        | Deep(prefix, deeper, suffix) ->
            Deep(prefix |> Digit.prepend a, deeper, suffix)

module Exp =
    let namel =
        "Tobias Becker".ToCharArray()
        |> Array.fold (fun acc c -> acc |> Finger.append c) Finger.empty

    let namer =
        Finger.empty
        |> Array.foldBack (fun c acc -> acc |> Finger.prepend c) ("Tobias Becker".ToCharArray())


