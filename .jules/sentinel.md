
## 2026-05-19 - [Path Traversal in GUI Output Filename]
**Vulnerability:** The `ConversionService` GUI component accepted user-provided filenames without sanitization, concatenating them directly to the output directory path using `Path.Combine`. This allowed path traversal (e.g., `../../../Windows/System32/evil`), enabling a user to write files to arbitrary locations.
**Learning:** `Path.Combine` doesn't sanitize paths against traversal or absolute path overrides. User-provided filenames in GUI applications must be explicitly normalized and stripped to their base name before being used in file system operations.
**Prevention:** Sanitize user-provided filename components by replacing cross-platform directory separators and applying `System.IO.Path.GetFileName()` to extract only the file name before passing them to `Path.Combine`. Ensure a fallback filename is provided if the sanitized name is empty.
