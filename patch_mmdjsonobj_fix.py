import sys

def patch_mmdjsonobj():
    file_path = "md2visio/struc/figure/MmdJsonObj.cs"
    with open(file_path, "r") as f:
        content = f.read()

    # The issue might be that there is `InvalidOperationException` without using `System`. Let's check using directives.
    if "using System;" not in content:
        content = "using System;\n" + content

    with open(file_path, "w") as f:
        f.write(content)

def patch_mmdjsonarray():
    file_path = "md2visio/struc/figure/MmdJsonArray.cs"
    with open(file_path, "r") as f:
        content = f.read()

    if "using System;" not in content:
        content = "using System;\n" + content

    with open(file_path, "w") as f:
        f.write(content)

patch_mmdjsonobj()
patch_mmdjsonarray()
