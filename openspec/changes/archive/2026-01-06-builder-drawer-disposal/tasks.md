## 1. Implementation
- [x] 1.1 Remove the VShapeDrawer.RemoveShadow call from VFigureBuilder.Build.
- [x] 1.2 Dispose VDrawerSeq after Draw().
- [x] 1.3 Dispose VDrawerG after Draw().
- [x] 1.4 Dispose VDrawerCls after Draw().
- [x] 1.5 Dispose VDrawerJo after Draw().
- [x] 1.6 Dispose VDrawerPie after Draw().
- [x] 1.7 Dispose VDrawerPac after Draw() and keep SortedNodes assignment intact.
- [x] 1.8 Dispose VDrawerXy after Draw(), updating helper usage if needed.
- [x] 1.9 Verify no VShapeDrawer.RemoveShadow calls remain.

## 2. Validation
- [x] 2.1 Run dotnet build md2visio.sln.
- [x] 2.2 Run one GUI-path conversion and confirm output file exists.
