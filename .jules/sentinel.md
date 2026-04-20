## 2026-04-20 - [CRITICAL] Prevent shell execution via user-controlled output path in GUI

**Vulnerability:** Shell execution vulnerability in `md2visio.GUI/Forms/MainForm.cs` where `Process.Start` with `UseShellExecute = true` is used to open the user-provided output directory (`_outputDirTextBox.Text`). If a malicious user supplies a crafted path containing executable instructions, it could be executed by the system shell when opening the directory.

**Learning:** When attempting to open a directory or file path using `Process.Start` in .NET, passing a user-controlled path directly to `FileName` while `UseShellExecute = true` passes the string directly to the OS shell, which is inherently dangerous and equivalent to command injection for paths.

**Prevention:** To safely open directories from user-controlled paths, strictly use `explorer.exe` (or the respective platform's file browser) as the `FileName` with `UseShellExecute = false`. The user-controlled path should be passed as an argument properly enclosed in quotes, with any trailing slashes trimmed to prevent the final quote from being escaped. Additionally, always normalize paths with `Path.GetFullPath()` and verify their existence using `Directory.Exists()`.
