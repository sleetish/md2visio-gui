# Change: Add flowchart shape aliases in GNodeShape

## Why
Mermaid flowchart @{ shape: ... } aliases like tri, diam, text, card, and h-cyl currently trigger unknown shape errors.

## What Changes
- Add alias mappings for new flowchart shape names in GNodeShape.
- Preserve existing bracket-based shape parsing behavior.

## Impact
- Affected specs: flowchart-shapes
- Affected code: md2visio/struc/graph/GNodeShape.cs
