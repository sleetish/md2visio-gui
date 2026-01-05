## ADDED Requirements
### Requirement: Sequence notes are parsed
The system SHALL parse single-line note statements and record note position, participants, and text in the sequence model.

#### Scenario: Note parsed into sequence model
- **WHEN** a Mermaid note statement is present in a sequence diagram
- **THEN** a note entry is added with its participants, position, and text.
