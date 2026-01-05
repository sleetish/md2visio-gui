## MODIFIED Requirements
### Requirement: Sequence layout validation samples
The system SHALL provide sample sequence diagrams that exercise long labels, fragment nesting, and note rendering for layout validation.

#### Scenario: Long-label sample available
- **WHEN** a developer reviews sequence layout validation samples
- **THEN** they can find a long-label sample covering fragment headers and message labels.

#### Scenario: Multi-fragment sample available
- **WHEN** a developer inspects validation samples
- **THEN** they can find a sample that includes multiple fragment types (e.g., alt/loop).

#### Scenario: Note layout sample available
- **WHEN** a developer inspects validation samples
- **THEN** they can find a sample that includes notes scoped to participants.
