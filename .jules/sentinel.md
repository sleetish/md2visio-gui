## 2024-04-15 - Path Traversal in File Output Generation
**Vulnerability:** The application used `Path.Combine(outputDir, fileName)` where `fileName` was directly derived from user input or source file names. If a file was named `../../../secret.vsdx`, it would traverse outside the intended `outputDir`.
**Learning:** `System.IO.Path.Combine()` does not sanitize inputs against directory traversal (e.g., `..`) and will override the base path if the second argument is absolute.
**Prevention:** Always sanitize components before combining by normalizing slashes, extracting the base filename using `Path.GetFileName()`, and falling back to a safe default if the result is empty.

## 2024-04-15 - Path Traversal leading to Arbitrary Command Execution via ShellExecute
**Vulnerability:** The application passed user input `_outputDirTextBox.Text` directly to `Process.Start` with `UseShellExecute = true` to open an output directory.
**Learning:** Calling `Process.Start` with a user-controlled path and `UseShellExecute = true` can be leveraged to execute arbitrary executables if the path is manipulated, as Windows Shell relies on file associations and path resolution.
**Prevention:** When opening directories, always use `UseShellExecute = false`, explicitly invoke `explorer.exe` as the `FileName`, and pass the fully resolved and sanitized path (e.g., `Path.GetFullPath()`) as an argument enclosed in double quotes (making sure to trim trailing backslashes so they don't escape the closing quote).
