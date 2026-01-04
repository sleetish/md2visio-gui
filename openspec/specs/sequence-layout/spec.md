# sequence-layout Specification

## Purpose
TBD - created by archiving change audit-seq-layout-context. Update Purpose after archive.
## Requirements
### Requirement: Sequence layout documentation
The system SHALL provide developer-facing documentation for sequence diagram layout inputs and draw order.

#### Scenario: Documentation available
- **WHEN** a developer inspects the documentation
- **THEN** they can find the layout parameter sources and draw order for sequence diagrams.

### Requirement: Sequence layout configuration loading
The system SHALL read sequence layout parameters from `config.sequence` and apply them to sequence diagram layout calculations.

#### Scenario: Defaults applied
- **WHEN** no user overrides are provided
- **THEN** layout uses default `config.sequence` values.

#### Scenario: User override applied
- **WHEN** front matter provides `config.sequence.messageSpacing`
- **THEN** layout uses the configured value for spacing.

### Requirement: Configurable message spacing in parsing
The system SHALL use `config.sequence.messageSpacing` when advancing sequence message positions during parsing.

#### Scenario: Default spacing used
- **WHEN** no user overrides are provided
- **THEN** parsing advances message Y positions using default `config.sequence.messageSpacing`.

#### Scenario: Custom spacing used
- **WHEN** front matter provides `config.sequence.messageSpacing`
- **THEN** parsing advances message Y positions using the configured spacing.

### Requirement: Dynamic label height measurement
The system SHALL measure message and fragment label heights and use them to size fragment header and section labels.

#### Scenario: Long fragment label
- **WHEN** a fragment label is longer than the default height
- **THEN** the label box height grows to fit the measured text.

#### Scenario: Message label measurement
- **WHEN** a message label is present
- **THEN** its measured height is recorded for layout calculations.

