## ADDED Requirements
### Requirement: GUI conversions execute on STA
The system SHALL execute GUI conversion COM calls on a dedicated STA thread.

#### Scenario: GUI conversion uses STA
- **WHEN** a GUI conversion runs
- **THEN** COM automation occurs on an STA thread.
