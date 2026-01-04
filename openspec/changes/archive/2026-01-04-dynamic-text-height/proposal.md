# Change: Compute dynamic text heights

## Why
Fixed label heights cause crowded sequence diagrams when message or fragment labels are long, especially with CJK text.

## What Changes
- Measure message, fragment, and section label heights using Visio text metrics.
- Store measured heights for layout and use them to size fragment labels.
- Update sequence layout context documentation to reflect dynamic label sizing.

## Impact
- Affected specs: sequence-layout
- Affected code: md2visio/vsdx/VDrawerSeq.cs, md2visio/struc/sequence/SeqMessage.cs, md2visio/struc/sequence/SeqFragment.cs, docs/sequence-layout-context.md
