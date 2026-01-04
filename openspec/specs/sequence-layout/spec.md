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

