## ADDED Requirements
### Requirement: Regression conversions cover multiple diagram types
The system SHALL provide a regression conversion sweep across multiple diagram types to validate COM stability.

#### Scenario: Multi-diagram regression run
- **WHEN** regression validation is performed
- **THEN** sequence and graph sample conversions complete in one session without RPC errors.
