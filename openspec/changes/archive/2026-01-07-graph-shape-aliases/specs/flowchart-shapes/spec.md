## ADDED Requirements
### Requirement: Flowchart shape aliases parsed
The system SHALL accept Mermaid flowchart shape aliases provided via @{ shape: ... } and map tri, diam, text, card, and h-cyl to supported shapes without raising unknown shape errors.

#### Scenario: Alias shapes accepted
- **WHEN** a flowchart node uses @{ shape: tri } (or diam/text/card/h-cyl)
- **THEN** parsing succeeds and the node resolves to a supported Visio master mapping.
