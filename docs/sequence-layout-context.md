# Sequence Layout Context

## Scope
This document summarizes current sequence diagram layout inputs and draw order for debugging and refactoring. It does not describe intended behavior changes.

## Layout units and scale
- Layout units are defined as `mm * LayoutScale` with `LayoutScale = 15.0`.
- VDrawerSeq converts layout units to Visio units via `Mm()` and `Inches()`.

## Configuration sources
- Default values live in `md2visio/default/sequence.yaml` under `config.sequence`.
- Defaults are loaded through `ConfigDefaults` and `MmdFrontMatter`.
- `VDrawerSeq.LoadConfiguration()` maps `config.sequence` values to layout fields and converts mm to layout units.
- `SeqBuilder` decrements `currentY` using `MESSAGE_SPACING` (layout units).

## Vertical layout
- `CalculateVerticalLayout()` sets `diagramStartY` using `topY`, `participantHeight`, and `messageSpacing / 2`.
- `minRelativeY` is computed from message and activation Y values only.
- Fragment label heights and section labels are not considered in height estimation.

## Fragment drawing
- `DrawFragments()` adds frame padding using `messageSpacing / 4`.
- Fragment header label uses `labelWidth = 600` and `labelHeight = 250` (layout units).
- Section labels reuse `labelHeight` and are anchored at `sectionY`.

## Message drawing
- `DrawRegularMessage()` attaches message text directly to the line shape.
- `DrawSelfCallMessage()` renders a separate text shape offset by `selfCallTextOffset`.

## References
- `md2visio/vsdx/VDrawerSeq.cs`
- `md2visio/struc/sequence/SeqBuilder.cs`
- `md2visio/default/sequence.yaml`
