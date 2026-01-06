# Change: Make VShapeDrawer shadow instance-scoped

## Why
Static shadow shapes can outlive their COM session and lead to invalid COM object reuse across conversions.

## What Changes
- Replace the static shadow shape with an instance field.
- Keep text measurement behavior the same while isolating COM lifetime per drawer.

## Impact
- Affected specs: conversion-runtime (shadow cache scope)
- Affected code: md2visio/vsdx/@base/VShapeDrawer.cs
