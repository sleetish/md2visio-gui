# Change: Dispose drawer instances after build

## Why
Drawer instances now own COM resources; they must be disposed deterministically after drawing to avoid leaks and RPC failures.

## What Changes
- Remove the global shadow cleanup call from VFigureBuilder.
- Dispose each VDrawer instance after Draw completes.

## Impact
- Affected specs: conversion-runtime (drawer disposal)
- Affected code: md2visio/vsdx/@base/VFigureBuilder.cs and md2visio/vsdx/VBuilder*.cs
