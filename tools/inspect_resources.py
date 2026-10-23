import os
import re

classes = ['GMIDlet', 'a', 'b', 'c', 'd', 'e', 'f', 'g']

for cls in classes:
    java_file = os.path.join('decompiled', f'{cls}.java')
    bytecode_file = os.path.join('decompiled', 'bytecode', f'{cls}.txt')
    
    with open(java_file, 'r', encoding='utf-8') as f:
        code = f.read()
    with open(bytecode_file, 'r', encoding='utf-8') as f:
        bc = f.read()

    print(f"==================== {cls} ====================")
    
    # Look for getResourceAsStream in bytecode and java
    res_calls = re.findall(r'(\w+)\.class\.getResourceAsStream\(([^)]+)\)', code)
    print("getResourceAsStream calls in Java:", res_calls)
    
    # Look for string constants in bytecode
    ldc_strings = re.findall(r'ldc\s+#\d+\s+//\s+String\s+(.*)', bc)
    res_candidates = [s for s in ldc_strings if any(s.startswith(x) for x in ['/', 'b', 'l', 's', 'f', 'i', 't', 'm', 'p', 'r'])]
    print("String constants in bytecode:", sorted(set(ldc_strings)))
