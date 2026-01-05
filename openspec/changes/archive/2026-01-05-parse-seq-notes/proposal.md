# Change: Parse sequence notes

## Why
Sequence notes appear in Mermaid inputs and must be parsed into the sequence model to drive layout and rendering.

## What Changes
- Parse single-line note syntax (left of/right of/over) in SeqBuilder.
- Capture note participants, position, and text into Sequence.Notes.

## Impact
- Affected specs: openspec/specs/sequence-layout/spec.md
- Affected code: md2visio/struc/sequence/SeqBuilder.cs; md2visio/struc/sequence/SeqNote.cs
