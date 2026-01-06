# flowchart-shapes Specification

## Purpose
Define supported flowchart shape names and how they map to Visio masters, along with rendering adjustments.

## Requirements
### Requirement: Flowchart shape master mapping documented
The system SHALL provide developer-facing documentation mapping supported flowchart shape names to Visio master names and any required transformations.

#### Scenario: Mapping available
- **WHEN** a developer reviews the flowchart shape documentation
- **THEN** they can identify the Visio master and rotation/postprocess notes for each supported shape.

### Requirement: Flowchart shape aliases parsed
The system SHALL accept Mermaid flowchart shape aliases provided via @{ shape: ... } and map tri, diam, text, card, and h-cyl to supported shapes without raising unknown shape errors.

#### Scenario: Alias shapes accepted
- **WHEN** a flowchart node uses @{ shape: tri } (or diam/text/card/h-cyl)
- **THEN** parsing succeeds and the node resolves to a supported Visio master mapping.
