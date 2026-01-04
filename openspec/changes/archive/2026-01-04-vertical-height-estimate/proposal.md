# Change: Include fragment label heights in vertical layout estimate

## Why
Fragment header and section labels can be taller than the default spacing, but the vertical layout estimate ignores those bounds and fragment frame bottoms, causing the diagram height to be underestimated.

## What Changes
- Account for fragment header and section label heights when computing the minimum relative Y.
- Include fragment frame bottom bounds in the height estimate.
- Update sequence layout context documentation to describe the fragment-aware estimate.

## Impact
- Affected specs: sequence-layout
- Affected code: md2visio/vsdx/VDrawerSeq.cs, docs/sequence-layout-context.md
