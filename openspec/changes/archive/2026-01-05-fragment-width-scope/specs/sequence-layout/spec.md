## ADDED Requirements
### Requirement: Fragment width scoped to involved participants
The system SHALL size fragment frames based on the participants referenced by messages, notes, and activations inside the fragment time window.

#### Scenario: Fragment width shrinks to content
- **WHEN** a fragment only involves a subset of participants
- **THEN** the fragment frame spans only those participants plus configured padding.
