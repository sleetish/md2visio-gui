# Change: Add fragment padding controls

## Why
Fragment headers and section boundaries lack extra vertical padding, causing crowded layouts when fragments and long labels stack tightly.

## What Changes
- Add configurable fragment padding values and apply them during sequence parsing.
- Use configured padding for fragment frame top/bottom offsets.
- Update sequence layout context documentation to describe fragment padding behavior.

## Impact
- Affected specs: sequence-layout
- Affected code: md2visio/struc/sequence/SeqBuilder.cs, md2visio/vsdx/VDrawerSeq.cs, md2visio/default/sequence.yaml, docs/sequence-layout-context.md
