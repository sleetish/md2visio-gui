# Change: Run GUI conversion on a dedicated STA thread

## Why
Visio COM automation requires STA, but GUI conversions currently execute on a ThreadPool (MTA) thread.

## What Changes
- Add an STA thread wrapper for GUI conversion execution.
- Route ConvertAsync through the STA wrapper to keep COM calls on STA.

## Impact
- Affected specs: conversion-runtime (STA execution requirement)
- Affected code: md2visio.GUI/Services/ConversionService.cs
