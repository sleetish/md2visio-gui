## ADDED Requirements
### Requirement: Text measurement shadow is instance-scoped
The system SHALL scope the text-measurement shadow shape to each drawer instance to avoid cross-session COM reuse.

#### Scenario: Shadow shape isolated per drawer
- **WHEN** text size measurement is performed during drawing
- **THEN** the shadow shape is created and reused only within the drawer instance.
