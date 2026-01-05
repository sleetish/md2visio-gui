# Change: Include notes in vertical layout

## Why
Sequence notes add vertical extent; without accounting for them, the diagram can clip note boxes.

## What Changes
- Include note bounds when estimating vertical extent in sequence layout.

## Impact
- Affected specs: openspec/specs/sequence-layout/spec.md
- Affected code: md2visio/vsdx/VDrawerSeq.cs
