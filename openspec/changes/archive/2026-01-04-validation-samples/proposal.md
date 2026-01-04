# Change: Validate sequence layout samples

## Why
Recent sequence layout spacing updates need a clear regression target that exercises long labels and fragment stacking, so future changes can be validated consistently.

## What Changes
- Add a long-label, multi-fragment Mermaid sample for sequence layout validation.
- Document validation sample references in the sequence layout context doc.
- Run the existing test suite to confirm baseline behavior.

## Impact
- Affected specs: sequence-layout
- Affected code: md2visio/test/sequence_fragments_longtext.md, docs/sequence-layout-context.md
