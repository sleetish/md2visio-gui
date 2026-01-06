# Visio COM Entry Points and Threading

## GUI entrypoint
- `md2visio.GUI/Program.cs`: `Main` is marked with `[STAThread]`, sets the current thread to STA, and launches `MainForm`.

## GUI conversion call chain (COM usage)
- `MainForm.OnStartConversionClick` -> `ConversionService.ConvertAsync`
- `ConversionService.ConvertAsync` -> `ConversionService.Convert`
- `ConversionService.Convert` -> `Md2VisioConverter.Convert`
- `Md2VisioConverter.ConvertInternal` -> `new VisioSession(...)`
- `VisioSession` constructor -> `EnsureVisioApp` -> `new Visio.Application()`

## Threading model notes
- `ConversionService.ConvertAsync` uses `Task.Run`, so the conversion runs on a ThreadPool (MTA) thread today.
- UI updates from background work are marshaled via `InvokeRequired` in `MainForm.OnProgressChanged` and `MainForm.OnLogMessage`.

## Legacy / CLI / debug entrypoints
- `Services/ConversionService.cs` appears to be a legacy path and is not included in `md2visio.GUI` (file is outside the project folder).
- `md2visio` is a class library (`OutputType` is `Library`), and there is no `Program.cs` entrypoint.
- `md2visio/Properties/launchSettings.json` retains debug arguments, but there is no active CLI entrypoint in the project.

## COM session creation
- `VisioSession` owns the COM application lifecycle and creates the `Visio.Application` instance.
