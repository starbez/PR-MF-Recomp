import struct
import os
from parse_pngs import parse_gmidlet_image_stream

def parse_sprites(src_dir='extracted', out_dir='extracted_png/sprites'):
    os.makedirs(out_dir, exist_ok=True)
    s_files = [f for f in sorted(os.listdir(src_dir)) if f.startswith('s') and f[1:].isdigit()]
    total_variants = 0
    
    for sf in s_files:
        path = os.path.join(src_dir, sf)
        with open(path, 'rb') as f:
            data = f.read()
        variants, var_count, offset = parse_gmidlet_image_stream(data, 0)
        assert offset == len(data), f"Sprite {sf} not completely consumed: {offset}/{len(data)}"
        for vi, v_png in enumerate(variants):
            suffix = f'_var{vi}' if len(variants) > 1 else ''
            out_name = f"{sf}{suffix}.png"
            with open(os.path.join(out_dir, out_name), 'wb') as out_f:
                out_f.write(v_png)
            total_variants += 1

    print(f"Parsed {len(s_files)} sprite files, exported {total_variants} variant PNGs to {out_dir}/.")
    return len(s_files), total_variants

if __name__ == '__main__':
    parse_sprites()
