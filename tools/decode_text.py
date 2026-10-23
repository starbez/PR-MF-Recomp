import struct
import os
import sys

def decode_lang(path):
    with open(path, 'rb') as f:
        data = f.read()
    count = struct.unpack('>H', data[:2])[0]
    offset = 2
    langs = []
    for _ in range(count):
        slen = struct.unpack('>H', data[offset:offset+2])[0]
        offset += 2
        lang = data[offset:offset+slen].decode('utf-8')
        offset += slen
        langs.append(lang)
    assert offset == len(data), f"Leftover bytes in {path}: {len(data) - offset}"
    return langs

def decode_text(path):
    """
    Parses text__*.txt files.
    The file contains 6 string arrays:
      Array 0: Main UI strings
      Array 1: In-game dialogue & character names
      Array 2: Story cutscene subtitles
      Array 3: Tutorial text
      Array 4: Help / controls text
      Array 5: About / credits text
    """
    with open(path, 'rb') as f:
        data = f.read()
    offset = 0
    arrays = []
    while offset < len(data):
        count = struct.unpack('>H', data[offset:offset+2])[0]
        offset += 2
        strings = []
        for _ in range(count):
            slen = struct.unpack('>H', data[offset:offset+2])[0]
            offset += 2
            s = data[offset:offset+slen].decode('utf-8')
            offset += slen
            strings.append(s)
        arrays.append(strings)
    assert offset == len(data), f"Leftover bytes in {path}: {len(data) - offset}"
    return arrays

if __name__ == '__main__':
    base_dir = 'extracted' if os.path.exists('extracted') else '.'
    lang_file = os.path.join(base_dir, 'lang')
    if os.path.exists(lang_file):
        langs = decode_lang(lang_file)
        print(f"Languages ({len(langs)}): {langs}")
        for l in langs:
            tf = os.path.join(base_dir, f'text__{l}.txt')
            if os.path.exists(tf):
                arrays = decode_text(tf)
                total_strings = sum(len(a) for a in arrays)
                print(f"  {l}: {len(arrays)} arrays, total {total_strings} strings successfully parsed (exact byte match)")
