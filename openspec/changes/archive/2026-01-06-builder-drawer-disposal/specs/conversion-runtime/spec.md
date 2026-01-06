## ADDED Requirements
### Requirement: Drawer instances are disposed after drawing
The system SHALL dispose each drawer instance after its Draw routine completes.

#### Scenario: Drawer disposed after render
- **WHEN** a builder finishes drawing a figure
- **THEN** its drawer instance is disposed before the document is saved.
