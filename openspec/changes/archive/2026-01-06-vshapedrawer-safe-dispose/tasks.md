## 1. Implementation
- [x] 1.1 Convert RemoveShadow to an instance method and guard COM exceptions.
- [x] 1.2 Delete and release shadow COM objects in a finally block, then null the field.
- [x] 1.3 Implement IDisposable on VShapeDrawer and call RemoveShadow from Dispose.

## 2. Validation
- [x] 2.1 Run dotnet build md2visio.sln.
