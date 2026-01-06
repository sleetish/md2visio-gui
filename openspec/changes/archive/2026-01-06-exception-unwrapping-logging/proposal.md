# Change: Surface inner exceptions and avoid duplicate log prefixes

## Why
GUI logs currently hide inner exception details and may duplicate severity prefixes, making COM failures harder to diagnose.

## What Changes
- Unwrap TargetInvocationException/InnerException to surface root cause details.
- Avoid double-prefixing GUI log messages when a level tag is already present.

## Impact
- Affected specs: conversion-runtime (error reporting)
- Affected code: md2visio/Api/Md2VisioConverter.cs, md2visio.GUI/Services/ConversionService.cs
