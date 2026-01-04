## ADDED Requirements
### Requirement: Sequence layout configuration loading
The system SHALL read sequence layout parameters from `config.sequence` and apply them to sequence diagram layout calculations.

#### Scenario: Defaults applied
- **WHEN** no user overrides are provided
- **THEN** layout uses default `config.sequence` values.

#### Scenario: User override applied
- **WHEN** front matter provides `config.sequence.messageSpacing`
- **THEN** layout uses the configured value for spacing.
