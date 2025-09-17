#!/usr/bin/env python3
import re

# Read the file
with open('Content/PackageLoader.cs', 'r') as f:
    content = f.read()

# Remove all ", EntityCounts counts = null" from method signatures
content = re.sub(r',\s*EntityCounts counts = null\)', ')', content)

# Remove all "if (counts != null) counts.XXX" lines
content = re.sub(r'\s*if \(counts != null\) counts\.[^;]+;.*\n', '', content)

# Write the file back
with open('Content/PackageLoader.cs', 'w') as f:
    f.write(content)

print("Removed all EntityCounts parameters and usages")