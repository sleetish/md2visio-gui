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

### Requirement: Configurable message spacing in parsing
The system SHALL use `config.sequence.messageSpacing` when advancing sequence message positions during parsing.

#### Scenario: Default spacing used
- **WHEN** no user overrides are provided
- **THEN** parsing advances message Y positions using default `config.sequence.messageSpacing`.

#### Scenario: Custom spacing used
- **WHEN** front matter provides `config.sequence.messageSpacing`
- **THEN** parsing advances message Y positions using the configured spacing.

### Requirement: Dynamic label height measurement
The system SHALL measure message, fragment, and note label heights and use them to size fragment header and section labels as well as note boxes.

#### Scenario: Long fragment label
- **WHEN** a fragment label is longer than the default height
- **THEN** the label box height grows to fit the measured text.

#### Scenario: Message label measurement
- **WHEN** a message label is present
- **THEN** its measured height is recorded for layout calculations.

#### Scenario: Note label measurement
- **WHEN** a note label is present
- **THEN** its measured height is recorded for layout calculations.

### Requirement: Fragment padding configuration
The system SHALL apply configurable fragment padding for fragment headers, section boundaries, and fragment end spacing.

#### Scenario: Defaults used
- **WHEN** no fragment padding overrides are provided
- **THEN** fragment spacing uses defaults derived from message spacing.

#### Scenario: Custom padding used
- **WHEN** front matter provides fragment padding overrides
- **THEN** fragment spacing reflects the configured values.

### Requirement: Vertical layout includes fragment label bounds
The system SHALL include fragment header/section label heights and frame bounds when estimating sequence diagram vertical extent.

#### Scenario: Fragment labels enlarge height
- **WHEN** fragment header or section labels are taller than default spacing
- **THEN** the vertical layout extends to include the label bounds.

#### Scenario: Fragment end padding extends height
- **WHEN** fragment end padding moves the fragment bottom below the last message
- **THEN** the diagram bottom accounts for the fragment frame bound.

### Requirement: Sequence layout validation samples
The system SHALL provide sample sequence diagrams that exercise long labels and fragment nesting for layout validation.

#### Scenario: Long-label sample available
- **WHEN** a developer reviews sequence layout validation samples
- **THEN** they can find a long-label sample covering fragment headers and message labels.

#### Scenario: Multi-fragment sample available
- **WHEN** a developer inspects validation samples
- **THEN** they can find a sample that includes multiple fragment types (e.g., alt/loop).

### Requirement: Sequence notes model available
The system SHALL provide a sequence note model and a notes collection on the sequence data structure for layout and rendering.

#### Scenario: Notes collection exposed
- **WHEN** a developer inspects the sequence data model
- **THEN** they can access a notes collection with note position, participants, text, and label metadata.

### Requirement: Sequence notes are parsed
The system SHALL parse single-line note statements and record note position, participants, and text in the sequence model.

#### Scenario: Note parsed into sequence model
- **WHEN** a Mermaid note statement is present in a sequence diagram
- **THEN** a note entry is added with its participants, position, and text.

### Requirement: Fragment width scoped to involved participants
The system SHALL size fragment frames based on the participants referenced by messages, notes, and activations inside the fragment time window.

#### Scenario: Fragment width shrinks to content
- **WHEN** a fragment only involves a subset of participants
- **THEN** the fragment frame spans only those participants plus configured padding.

### Requirement: Sequence notes are rendered
The system SHALL render sequence notes at the specified position relative to participants.

#### Scenario: Note drawn at position
- **WHEN** a note is parsed for a sequence diagram
- **THEN** a note box is drawn at left, right, or over the referenced participants.

