# Change: Propagate configurable message spacing

## Why
Sequence parsing uses a hard-coded message spacing constant, which can diverge from the configured layout spacing.

## What Changes
- Read `config.sequence.messageSpacing` in `SeqBuilder` and apply it when advancing message Y positions.
- Update sequence layout context documentation to reflect the configurable parsing spacing.

## Impact
- Affected specs: sequence-layout
- Affected code: md2visio/struc/sequence/SeqBuilder.cs, docs/sequence-layout-context.md
