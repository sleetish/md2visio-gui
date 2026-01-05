# Change: Measure sequence note label height

## Why
Sequence notes affect layout and fragment sizing, but note label heights are not measured yet.

## What Changes
- Measure note label heights alongside message and fragment labels.
- Store note label height for layout calculations.

## Impact
- Affected specs: openspec/specs/sequence-layout/spec.md
- Affected code: md2visio/vsdx/VDrawerSeq.cs
