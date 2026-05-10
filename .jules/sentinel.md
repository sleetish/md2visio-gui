## 2025-02-28 - [Path Traversal in ConversionService]
**Vulnerability:** The `ConversionService` combined the user-provided filename with an output directory using `Path.Combine` without sanitizing the input. This allowed a malicious user to craft a filename with path traversal characters (e.g., `../../../file.vsdx`) or absolute paths (e.g., `C:\Windows\System32\file.vsdx`) to write files outside of the intended directory.
**Learning:** `Path.Combine` doesn't sanitize inputs and absolute paths will overwrite the base path.
**Prevention:** Always sanitize user-provided filename components before using `Path.Combine`. Normalize cross-platform slashes, use `Path.GetFileName()` to extract only the filename component, and provide a safe fallback string if the resulting filename is empty.
