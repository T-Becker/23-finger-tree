﻿module CmdQ.FingerTree.Error

open System

[<RequireQualifiedAccess>]
module Messages =
    let treeIsEmpty = "The input tree was empty."
    let digitAlreadyHas4Entries = "Cannot add to digit, because it already has maximum capacity."
    let onlyList2or3Accepted = "Only lists of length 2 or 3 accepted!"
    let onlyList1to4Accepted = "Only lists of length 1 to 4 accepted!"
    let splitPointNotFound = "Split point not found."
    let digitsCannotBeLongerThanFour = "Digits cannot be longer than 4."
    let indexOutOfRange = "The index is outside the legal range."
    let patternMatchImpossible = "Impossible!"

/// Raise an IndexOutOfRangeException with a message.
let inline invalidIndex msg =
    // This is inline, so that the stack trace is at the right location instead of here.
    IndexOutOfRangeException msg |> raise
