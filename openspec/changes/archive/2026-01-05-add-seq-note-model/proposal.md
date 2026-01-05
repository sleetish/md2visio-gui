# Change: Add sequence note model

## Why
Sequence fragments need note-aware layout, which requires a first-class note model in the sequence data structures.

## What Changes
- Add a SeqNote model to represent note position, participants, text, and layout metadata.
- Store notes on Sequence for later parsing and layout phases.

## Impact
- Affected specs: openspec/specs/sequence-layout/spec.md
- Affected code: md2visio/struc/sequence/Sequence.cs; md2visio/struc/sequence/SeqNote.cs
