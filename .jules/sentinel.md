## Sentinel Journal
## 2024-05-28 - [CRITICAL] Prevent Path Traversal and RCE via Unvalidated Inputs
**Vulnerability:**
1. Path Traversal in `ConversionService.cs` (`BuildOutputPath`): User-provided `fileName` parameter was concatenated with `outputDir` directly. If the input contained path traversal sequences like `../../`, files could be created or overwritten in unauthorized directories.
2. Command Execution via `UseShellExecute = true` in `MainForm.cs`: Unvalidated user input (like link URL and output directory) was passed directly into `Process.Start` with `UseShellExecute = true`. An attacker could possibly exploit this by manipulating inputs to execute arbitrary shell commands.

**Learning:**
1. `System.IO.Path.Combine` doesn't automatically sanitize inputs in .NET; passing absolute paths or paths with traversal sequences as subsequent arguments overrides the base directory.
2. Directly piping unvalidated output into `Process.Start` with `UseShellExecute` enabled can inadvertently evaluate inputs in the host shell context and lead to OS command execution.

**Prevention:**
1. Always normalize paths by replacing backslash and forward slash to `Path.DirectorySeparatorChar` and extract only the filename via `Path.GetFileNameWithoutExtension()` before combining with standard user paths. Ensure to test for empty string cases after extraction and use defaults if empty.
2. When launching processes, disable `UseShellExecute` unless strictly needed. If required (e.g. `http` schema evaluation), ensure rigorous validation like `Uri.TryCreate` and checking `uri.Scheme`. For opening local folders, explicitly launch `explorer.exe`, safely construct `Arguments` by wrapping sanitized directory strings in double quotes, and trim trailing slashes.
