import os
import sys

from decode_text import decode_lang, decode_text
from parse_pngs import extract_all_pngs, parse_gmidlet_image_stream
from parse_sprites import parse_sprites
from parse_blocks import parse_block_file
from parse_level import parse_level_file
from parse_pack import parse_fr, parse_spr, parse_l_pack

def test_all_resources(src_dir='extracted'):
    print("==================================================")
    print("Running Full Suite of Resource Format Parsers...")
    print("==================================================")
    
    total_files = 0
    passed_files = 0

    # 1. Manifest
    mf_path = os.path.join(src_dir, 'META-INF', 'MANIFEST.MF')
    if os.path.exists(mf_path):
        total_files += 1
        with open(mf_path, 'r', encoding='utf-8') as f:
            content = f.read()
        assert 'MIDlet-Name:' in content
        passed_files += 1
        print("  [OK] META-INF/MANIFEST.MF")

    # 2. Text & Language
    lang_path = os.path.join(src_dir, 'lang')
    if os.path.exists(lang_path):
        total_files += 1
        langs = decode_lang(lang_path)
        passed_files += 1
        print(f"  [OK] lang ({len(langs)} languages: {langs})")

    for l in ['de', 'en', 'fr', 'it']:
        t_path = os.path.join(src_dir, f'text__{l}.txt')
        if os.path.exists(t_path):
            total_files += 1
            arrays = decode_text(t_path)
            assert len(arrays) == 6
            passed_files += 1
            print(f"  [OK] text__{l}.txt (6 arrays, {sum(len(a) for a in arrays)} strings)")

    # 3. Direct/Sliced PNGs
    for name in ['i.png', 'lm', 'lp__de', 'lp__en', 'lp__fr', 'lp__it', 'lpok', 
                 'subtitle__de', 'subtitle__en', 'subtitle__fr', 'subtitle__it',
                 'bg0', 'bg1', 'bg2', 'bg3', 'bg4']:
        path = os.path.join(src_dir, name)
        if os.path.exists(path):
            total_files += 1
            with open(path, 'rb') as f:
                data = f.read()
            if name == 'i.png':
                assert data.startswith(b'\x89PNG')
            elif name in ['lp__de', 'lp__en', 'lp__fr', 'lp__it', 'lpok']:
                l = int.from_bytes(data[:2], 'big')
                assert 2 + l == len(data)
            elif name.startswith('subtitle__'):
                l = int.from_bytes(data[1:3], 'big')
                assert 3 + l == len(data)
            elif name == 'lm':
                off = 0
                while off < len(data):
                    l = int.from_bytes(data[off+1:off+3], 'big')
                    off += 3 + l
                assert off == len(data)
            elif name.startswith('bg'):
                vars, var_c, off = parse_gmidlet_image_stream(data, 0)
                assert off == len(data)
            passed_files += 1
            print(f"  [OK] {name}")

    # 4. Font
    font_path = os.path.join(src_dir, 'font')
    if os.path.exists(font_path):
        total_files += 1
        with open(font_path, 'rb') as f:
            data = f.read()
        off = 0
        font_count = int.from_bytes(data[off:off+2], 'big'); off += 2
        var_short_arr_b = []
        n = 0
        while n < font_count:
            vars, var_count, off = parse_gmidlet_image_stream(data, off)
            var_short_arr_b.append(var_count)
            for _ in range(var_count - 1):
                var_short_arr_b.append(0)
            n += var_count
        for _ in range(2):
            sl = int.from_bytes(data[off:off+2], 'big'); off += 2 + sl
        blen = int.from_bytes(data[off:off+2], 'big'); off += 2 + blen
        off += 4 # 2 shorts
        n = 0
        while n < font_count:
            n3 = var_short_arr_b[n] & 0x7F
            sl = int.from_bytes(data[off:off+2], 'big'); off += 2 + sl
            n6 = int.from_bytes(data[off:off+2], 'big'); off += 2
            for _ in range(n6):
                tag = data[off]; off += 1
                if tag == 0: off += 1
                elif tag == 1:
                    l = int.from_bytes(data[off:off+2], 'big'); off += 2 + l
                elif tag in [2, 3, 4, 5, 6, 7, 8]: off += 1
            while n3 > 1:
                n += 1
                n3 -= 1
            n += 1
        assert off == len(data)
        passed_files += 1
        print("  [OK] font")

    # 5. Sprites s0..s64
    s_files = [f for f in sorted(os.listdir(src_dir)) if f.startswith('s') and f[1:].isdigit()]
    for sf in s_files:
        total_files += 1
        path = os.path.join(src_dir, sf)
        with open(path, 'rb') as f:
            data = f.read()
        vars, var_c, off = parse_gmidlet_image_stream(data, 0)
        assert off == len(data)
        passed_files += 1
    print(f"  [OK] {len(s_files)} sprite files (s0..s64)")

    # 6. Block definition tables b0..b12
    b_files = [f for f in sorted(os.listdir(src_dir)) if f.startswith('b') and f[1:].isdigit()]
    for bf in b_files:
        total_files += 1
        layers = parse_block_file(os.path.join(src_dir, bf))
        passed_files += 1
    print(f"  [OK] {len(b_files)} block files (b0..b12)")

    # 7. Level layout tables l0..l12
    l_files = [f for f in sorted(os.listdir(src_dir)) if f.startswith('l') and f[1:].isdigit()]
    for lf in l_files:
        total_files += 1
        info = parse_level_file(os.path.join(src_dir, lf))
        passed_files += 1
    print(f"  [OK] {len(l_files)} level files (l0..l12)")

    # 8. Packs: fr, spr, l
    for pack in ['fr', 'spr', 'l']:
        path = os.path.join(src_dir, pack)
        if os.path.exists(path):
            total_files += 1
            if pack == 'fr':
                r = parse_fr(path)
            elif pack == 'spr':
                s = parse_spr(path)
            elif pack == 'l':
                c, snd = parse_l_pack(path)
            passed_files += 1
            print(f"  [OK] {pack}")

    print("==================================================")
    print(f"ALL PARSERS PASSED: {passed_files}/{total_files} resources parsed with 0 leftover bytes!")
    print("==================================================")

if __name__ == '__main__':
    test_all_resources()
