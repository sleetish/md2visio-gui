## 2024-11-20 - Path Traversal in Path.Combine
**Vulnerability:** User-provided filename wasn't sanitized before being used in `System.IO.Path.Combine`. If the user provided an absolute path or path traversal sequences (`../`), `Path.Combine` would override the base path and write outside the intended output directory.
**Learning:** `System.IO.Path.Combine` does not automatically sanitize its arguments. The second argument can override the entire path if it's considered absolute (e.g. starting with `/` or `C:\`).
**Prevention:** Always rigorously sanitize untrusted input before appending it via `Path.Combine`. Replace cross-platform directory separator characters, apply `Path.GetFileName()`, and fall back to a safe default if the result is empty.
