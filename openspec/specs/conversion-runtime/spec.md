# conversion-runtime Specification

## Purpose
Define runtime behavior and documentation requirements for conversion execution and COM usage.

## Requirements
### Requirement: Conversion entrypoint and threading documentation
The system SHALL provide developer-facing documentation listing conversion entrypoints and the threading model used for COM interactions.

#### Scenario: Documentation available
- **WHEN** a developer inspects the conversion runtime documentation
- **THEN** they can see GUI entrypoints, legacy/CLI entrypoints, and where COM sessions are created.

### Requirement: GUI conversions execute on STA
The system SHALL execute GUI conversion COM calls on a dedicated STA thread.

#### Scenario: GUI conversion uses STA
- **WHEN** a GUI conversion runs
- **THEN** COM automation occurs on an STA thread.
