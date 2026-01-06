## 1. Implementation
- [x] 1.1 Add a RunOnStaThread helper using Thread + TaskCompletionSource to run Convert on STA.
- [x] 1.2 Update ConvertAsync to use the STA wrapper instead of Task.Run.
- [x] 1.3 Confirm UI callbacks still marshal via InvokeRequired without changes.

## 2. Validation
- [x] 2.1 Run two consecutive GUI-path conversions for the same input file.
