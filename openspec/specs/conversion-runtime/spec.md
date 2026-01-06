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

### Requirement: Text measurement shadow is instance-scoped
The system SHALL scope the text-measurement shadow shape to each drawer instance to avoid cross-session COM reuse.

#### Scenario: Shadow shape isolated per drawer
- **WHEN** text size measurement is performed during drawing
- **THEN** the shadow shape is created and reused only within the drawer instance.

### Requirement: Shadow COM cleanup is resilient
The system SHALL release text-measurement shadow COM objects safely, even when COM exceptions occur.

#### Scenario: Shadow cleanup tolerates COM errors
- **WHEN** shadow cleanup encounters COM or invalid COM object exceptions
- **THEN** cleanup completes without aborting the conversion flow.

### Requirement: Drawer instances are disposed after drawing
The system SHALL dispose each drawer instance after its Draw routine completes.

#### Scenario: Drawer disposed after render
- **WHEN** a builder finishes drawing a figure
- **THEN** its drawer instance is disposed before the document is saved.

### Requirement: Conversion errors surface inner exceptions
The system SHALL include inner exception type and message details in conversion error logs and messages when present.

#### Scenario: Inner exception reported
- **WHEN** a conversion fails with a TargetInvocationException or inner exception
- **THEN** logs and error messages include the inner exception type and message.
