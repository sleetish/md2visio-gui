# Change: Postprocess flowchart text and h-cyl shapes

## Why
Flowchart text nodes should render as label-only, and horizontal cylinders need rotation to match Mermaid visuals.

## What Changes
- Add a postprocess hook in VDrawerG for text and h-cyl nodes.
- Ensure text nodes have no fill or line, and h-cyl nodes are rotated with readable text.

## Impact
- Affected specs: flowchart-shapes
- Affected code: md2visio/vsdx/VDrawerG.cs
