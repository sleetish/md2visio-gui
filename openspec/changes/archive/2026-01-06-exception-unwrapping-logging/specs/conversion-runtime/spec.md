## ADDED Requirements
### Requirement: Conversion errors surface inner exceptions
The system SHALL include inner exception type and message details in conversion error logs and messages when present.

#### Scenario: Inner exception reported
- **WHEN** a conversion fails with a TargetInvocationException or inner exception
- **THEN** logs and error messages include the inner exception type and message.
