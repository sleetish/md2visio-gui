## ADDED Requirements
### Requirement: Dynamic label height measurement
The system SHALL measure message and fragment label heights and use them to size fragment header and section labels.

#### Scenario: Long fragment label
- **WHEN** a fragment label is longer than the default height
- **THEN** the label box height grows to fit the measured text.

#### Scenario: Message label measurement
- **WHEN** a message label is present
- **THEN** its measured height is recorded for layout calculations.
