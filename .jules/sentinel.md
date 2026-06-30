## 2024-05-24 - [CRITICAL] Fix Denial of Service in Custom JSON Parser
**Vulnerability:** A missing recursion depth limit in `MmdJsonObj` and `MmdJsonArray` custom JSON parsers allowed for StackOverflowExceptions when parsing deeply nested JSON data (e.g. `[[[[...]]]]` or `{{{{...}}}}`). This could crash the application (Denial of Service).
**Learning:** In modern .NET, a `StackOverflowException` cannot be caught using try-catch blocks and will immediately crash the entire application process. Custom recursive parsers are extremely susceptible to this.
**Prevention:** Always enforce a hardcoded maximum recursion depth limit (e.g., `MAX_DEPTH = 50`) and throw an `InvalidOperationException` (which is catchable) when exceeded.
