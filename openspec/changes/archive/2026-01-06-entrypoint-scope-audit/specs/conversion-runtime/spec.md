## ADDED Requirements
### Requirement: Conversion entrypoint and threading documentation
The system SHALL provide developer-facing documentation listing conversion entrypoints and the threading model used for COM interactions.

#### Scenario: Documentation available
- **WHEN** a developer inspects the conversion runtime documentation
- **THEN** they can see GUI entrypoints, legacy/CLI entrypoints, and where COM sessions are created.
