import sys

def patch_mmdjsonobj():
    file_path = "md2visio/struc/figure/MmdJsonObj.cs"
    with open(file_path, "r") as f:
        content = f.read()

    # Add MAX_DEPTH
    content = content.replace("internal class MmdJsonObj : ValueAccessor\n    {", "internal class MmdJsonObj : ValueAccessor\n    {\n        private const int MAX_DEPTH = 50;")

    # Replace constructors
    content = content.replace("public MmdJsonObj(StringBuilder textBuilder, int index)", "public MmdJsonObj(StringBuilder textBuilder, int index, int depth = 0)")
    content = content.replace("this.index = index;\n            Load(textBuilder);", "this.index = index;\n            Load(textBuilder, depth);")

    # Replace Load
    content = content.replace("public MmdJsonObj Load(string text)\n        {", "public MmdJsonObj Load(string text)\n        {")
    content = content.replace("return Load(new StringBuilder(text));", "return Load(new StringBuilder(text), 0);")

    content = content.replace("MmdJsonObj Load(StringBuilder textBuilder)", "MmdJsonObj Load(StringBuilder textBuilder, int depth)")
    content = content.replace("MmdJsonObj Load(StringBuilder textBuilder, int depth)\n        {\n            StringBuilder keyBuilder", "MmdJsonObj Load(StringBuilder textBuilder, int depth)\n        {\n            if (depth > MAX_DEPTH) throw new InvalidOperationException(\"Maximum JSON nesting depth exceeded.\");\n            StringBuilder keyBuilder")

    # Replace new MmdJsonObj calls inside Load
    content = content.replace("MmdJsonObj obj = new MmdJsonObj(textBuilder, index);", "MmdJsonObj obj = new MmdJsonObj(textBuilder, index, depth + 1);")
    content = content.replace("MmdJsonArray arr = new MmdJsonArray(textBuilder, index);", "MmdJsonArray arr = new MmdJsonArray(textBuilder, index, depth + 1);")

    # Replace UpdateWith
    content = content.replace("MmdJsonObj UpdateWith(MmdJsonObj json, StringBuilder path)", "MmdJsonObj UpdateWith(MmdJsonObj json, StringBuilder path, int depth = 0)")
    content = content.replace("MmdJsonObj UpdateWith(MmdJsonObj json, StringBuilder path, int depth = 0)\n        {\n            if (json == null) return this;", "MmdJsonObj UpdateWith(MmdJsonObj json, StringBuilder path, int depth = 0)\n        {\n            if (depth > MAX_DEPTH) throw new InvalidOperationException(\"Maximum JSON nesting depth exceeded.\");\n            if (json == null) return this;")
    content = content.replace("if (val is MmdJsonObj) UpdateWith((MmdJsonObj)val, new StringBuilder(path.ToString()));", "if (val is MmdJsonObj) UpdateWith((MmdJsonObj)val, new StringBuilder(path.ToString()), depth + 1);")

    with open(file_path, "w") as f:
        f.write(content)

def patch_mmdjsonarray():
    file_path = "md2visio/struc/figure/MmdJsonArray.cs"
    with open(file_path, "r") as f:
        content = f.read()

    # Add MAX_DEPTH
    content = content.replace("internal class MmdJsonArray : ValueAccessor, IEnumerable<object>\n    {", "internal class MmdJsonArray : ValueAccessor, IEnumerable<object>\n    {\n        private const int MAX_DEPTH = 50;")

    # Replace constructors
    content = content.replace("public MmdJsonArray(StringBuilder textBuilder, int index)", "public MmdJsonArray(StringBuilder textBuilder, int index, int depth = 0)")
    content = content.replace("this.index = index;\n            Load(textBuilder);", "this.index = index;\n            Load(textBuilder, depth);")

    # Replace Load
    content = content.replace("return Load(new StringBuilder(json));", "return Load(new StringBuilder(json), 0);")

    content = content.replace("MmdJsonArray Load(StringBuilder textBuilder)", "MmdJsonArray Load(StringBuilder textBuilder, int depth)")
    content = content.replace("MmdJsonArray Load(StringBuilder textBuilder, int depth)\n        {\n            StringBuilder item = new();", "MmdJsonArray Load(StringBuilder textBuilder, int depth)\n        {\n            if (depth > MAX_DEPTH) throw new InvalidOperationException(\"Maximum JSON nesting depth exceeded.\");\n            StringBuilder item = new();")

    # Replace new MmdJsonObj and MmdJsonArray calls inside Load
    content = content.replace("MmdJsonObj obj = new(textBuilder, index);", "MmdJsonObj obj = new MmdJsonObj(textBuilder, index, depth + 1);")
    content = content.replace("MmdJsonArray arr = new(textBuilder, index + 1);", "MmdJsonArray arr = new MmdJsonArray(textBuilder, index + 1, depth + 1);")

    with open(file_path, "w") as f:
        f.write(content)

patch_mmdjsonobj()
patch_mmdjsonarray()
