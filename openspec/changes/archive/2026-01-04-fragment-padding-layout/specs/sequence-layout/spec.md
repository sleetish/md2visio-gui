## ADDED Requirements
### Requirement: Fragment padding configuration
The system SHALL apply configurable fragment padding for fragment headers, section boundaries, and fragment end spacing.

#### Scenario: Defaults used
- **WHEN** no fragment padding overrides are provided
- **THEN** fragment spacing uses defaults derived from message spacing.

#### Scenario: Custom padding used
- **WHEN** front matter provides fragment padding overrides
- **THEN** fragment spacing reflects the configured values.
