# Change: Load sequence layout configuration

## Why
Sequence layout spacing is currently hard-coded and ignores `sequence.yaml`, making it impossible to tune spacing through configuration.

## What Changes
- Map `config.sequence.*` values to `VDrawerSeq` layout fields with `LayoutScale` conversion.
- Update sequence layout context documentation to reflect configuration binding.

## Impact
- Affected specs: sequence-layout
- Affected code: md2visio/vsdx/VDrawerSeq.cs, docs/sequence-layout-context.md
