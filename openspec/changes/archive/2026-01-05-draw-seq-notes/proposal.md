# Change: Draw sequence notes

## Why
Parsed notes should be rendered in the Visio output to match Mermaid sequence semantics.

## What Changes
- Draw note shapes in VDrawerSeq based on parsed note position and participants.
- Integrate note drawing into the sequence draw order.

## Impact
- Affected specs: openspec/specs/sequence-layout/spec.md
- Affected code: md2visio/vsdx/VDrawerSeq.cs
