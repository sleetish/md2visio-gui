# Change: Safely dispose VShapeDrawer shadow COM objects

## Why
Shadow shapes created for text measurement must be released safely to avoid COM errors or leaked Visio objects.

## What Changes
- Convert RemoveShadow to an instance method with COM exception handling.
- Release shadow COM objects and null the field in a finally block.
- Implement IDisposable on VShapeDrawer to ensure cleanup.

## Impact
- Affected specs: conversion-runtime (shadow cleanup)
- Affected code: md2visio/vsdx/@base/VShapeDrawer.cs
