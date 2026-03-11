## 2025-02-17 - [CRITICAL] Fix Denial of Service (DoS) via Stack Overflow in Custom JSON Parsers
**Vulnerability:** The application utilizes custom recursive JSON parsers (`MmdJsonObj` and `MmdJsonArray`) to handle diagram generation data. The parsers recursively instantiated new objects without imposing any depth restriction on nesting elements. When given excessively nested JSON strings (e.g., `{{{{{...}}}}}`), the recursion would consume the application's call stack entirely, resulting in a `StackOverflowException` and a complete Denial of Service (DoS) crash of the application logic.

**Learning:** When developing custom recursive structure parsers (e.g., custom JSON or YAML deserializers), relying solely on input string length does not adequately protect against recursive depth exploits. The application will fault before reading to the end of an arbitrary large payload.

**Prevention:** Always enforce a hardcoded maximum recursion depth limit inside custom recursive parsers (e.g., `MAX_DEPTH = 50`). Track the recursion depth parameter in every new recursive instantiation and immediately throw a controlled runtime exception (e.g., `ArgumentException`) if the limit is exceeded.
