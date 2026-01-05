## MODIFIED Requirements
### Requirement: Dynamic label height measurement
The system SHALL measure message, fragment, and note label heights and use them to size fragment header and section labels as well as note boxes.

#### Scenario: Long fragment label
- **WHEN** a fragment label is longer than the default height
- **THEN** the label box height grows to fit the measured text.

#### Scenario: Message label measurement
- **WHEN** a message label is present
- **THEN** its measured height is recorded for layout calculations.

#### Scenario: Note label measurement
- **WHEN** a note label is present
- **THEN** its measured height is recorded for layout calculations.
