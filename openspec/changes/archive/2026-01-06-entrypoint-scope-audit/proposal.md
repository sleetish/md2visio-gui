# Change: Audit conversion entrypoints and threading model

## Why
We need a clear map of conversion entrypoints and where COM is invoked to scope STA fixes and disposal changes.

## What Changes
- Audit GUI entrypoints and conversion call chain to identify COM usage points.
- Confirm legacy/CLI entrypoints and whether they are compiled.
- Publish an entrypoint/threading report for follow-up changes.

## Impact
- Affected specs: conversion-runtime (entrypoint documentation)
- Affected code: docs/visio-com-entrypoints.md
