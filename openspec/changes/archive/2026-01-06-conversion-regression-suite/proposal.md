# Change: Regression conversion sweep for COM stability

## Why
We need a repeatable regression run across multiple diagram types to confirm the COM stability fixes.

## What Changes
- Run sequential GUI-path conversions for multiple sample inputs in one session.
- Capture logs and confirm output files are produced.

## Impact
- Affected specs: conversion-runtime (regression verification)
- Affected code: none (validation only)
