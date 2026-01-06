## ADDED Requirements
### Requirement: Shadow COM cleanup is resilient
The system SHALL release text-measurement shadow COM objects safely, even when COM exceptions occur.

#### Scenario: Shadow cleanup tolerates COM errors
- **WHEN** shadow cleanup encounters COM or invalid COM object exceptions
- **THEN** cleanup completes without aborting the conversion flow.
