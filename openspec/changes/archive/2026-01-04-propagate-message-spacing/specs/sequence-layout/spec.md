## ADDED Requirements
### Requirement: Configurable message spacing in parsing
The system SHALL use `config.sequence.messageSpacing` when advancing sequence message positions during parsing.

#### Scenario: Default spacing used
- **WHEN** no user overrides are provided
- **THEN** parsing advances message Y positions using default `config.sequence.messageSpacing`.

#### Scenario: Custom spacing used
- **WHEN** front matter provides `config.sequence.messageSpacing`
- **THEN** parsing advances message Y positions using the configured spacing.
