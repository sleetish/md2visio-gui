## ADDED Requirements
### Requirement: Vertical layout includes fragment label bounds
The system SHALL include fragment header/section label heights and frame bounds when estimating sequence diagram vertical extent.

#### Scenario: Fragment labels enlarge height
- **WHEN** fragment header or section labels are taller than default spacing
- **THEN** the vertical layout extends to include the label bounds.

#### Scenario: Fragment end padding extends height
- **WHEN** fragment end padding moves the fragment bottom below the last message
- **THEN** the diagram bottom accounts for the fragment frame bound.
