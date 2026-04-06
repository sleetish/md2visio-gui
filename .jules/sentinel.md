## 2024-05-20 - [Sentinel] Fix Path Traversal in ConversionService
**Vulnerability:** Path.Combine was used with untrusted `fileName` input in `BuildOutputPath`, which allowed an attacker to override the `outputDir` using absolute paths or path traversal (`../`) sequences, leading to arbitrary file write.
**Learning:** `System.IO.Path.Combine` doesn't sanitize inputs. If the second argument is an absolute path or contains path traversals, it can completely override the first argument. Cross-platform environments require normalizing slashes before sanitization.
**Prevention:** Always sanitize user-provided filename components by normalizing slashes and applying `Path.GetFileName()` before using them in `Path.Combine`. Provide a safe default if the sanitized string is empty.
