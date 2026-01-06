## ADDED Requirements
### Requirement: Text nodes render as label-only
The system SHALL render flowchart nodes with shape "text" without fill or line so only the label is visible.

#### Scenario: Text node styling
- **WHEN** a flowchart node uses @{ shape: text }
- **THEN** the Visio shape shows only the label with no border or fill.

### Requirement: Horizontal cylinder rendered
The system SHALL render flowchart nodes with shape "h-cyl" as a horizontal cylinder with readable text.

#### Scenario: h-cyl orientation
- **WHEN** a flowchart node uses @{ shape: h-cyl }
- **THEN** the Visio shape appears as a horizontal cylinder and its text remains horizontal.
