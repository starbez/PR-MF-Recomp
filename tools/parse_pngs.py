import struct
import os

def parse_gmidlet_image_stream(data, offset=0):
    flag = data[offset]
    offset += 1
    png_len = struct.unpack('>H', data[offset:offset+2])[0]
    offset += 2
    if png_len == 0:
        return [], 1, offset
    base_png = bytearray(data[offset:offset+png_len])
    offset += png_len
    assert base_png.startswith(b'\x89PNG\r\n\x1a\n'), f"Not PNG at {offset-png_len}"
    variants = [bytes(base_png)]
    var_count = 1
    if (flag & 2) != 0:
        var_count = struct.unpack('>H', data[offset:offset+2])[0]
        offset += 2
        for v in range(1, var_count):
            cur_png = bytearray(base_png)
            patch_count = data[offset]
            offset += 1
            if patch_count == 0:
                patch_count = struct.unpack('>H', data[offset:offset+2])[0]
                offset += 2
            for _ in range(patch_count):
                patch_off = struct.unpack('>H', data[offset:offset+2])[0]
                offset += 2
                val = data[offset]
                offset += 1
                cur_png[patch_off] = val
            variants.append(bytes(cur_png))
    return variants, var_count, offset

def extract_all_pngs(src_dir='extracted', out_dir='extracted_png'):
    os.makedirs(out_dir, exist_ok=True)
    exported = 0

    # 1. i.png
    ipath = os.path.join(src_dir, 'i.png')
    if os.path.exists(ipath):
        with open(ipath, 'rb') as f:
            data = f.read()
        assert data.startswith(b'\x89PNG')
        with open(os.path.join(out_dir, 'icon.png'), 'wb') as f:
            f.write(data)
        exported += 1

    # 2. lp__* and lpok
    for name in ['lp__de', 'lp__en', 'lp__fr', 'lp__it', 'lpok']:
        path = os.path.join(src_dir, name)
        if os.path.exists(path):
            with open(path, 'rb') as f:
                data = f.read()
            length = struct.unpack('>H', data[:2])[0]
            png = data[2:2+length]
            assert 2 + length == len(data)
            assert png.startswith(b'\x89PNG')
            with open(os.path.join(out_dir, f'{name}.png'), 'wb') as f:
                f.write(png)
            exported += 1

    # 3. subtitle__*
    for name in ['subtitle__de', 'subtitle__en', 'subtitle__fr', 'subtitle__it']:
        path = os.path.join(src_dir, name)
        if os.path.exists(path):
            with open(path, 'rb') as f:
                data = f.read()
            flag = data[0]
            length = struct.unpack('>H', data[1:3])[0]
            png = data[3:3+length]
            assert 3 + length == len(data)
            assert png.startswith(b'\x89PNG')
            with open(os.path.join(out_dir, f'{name}.png'), 'wb') as f:
                f.write(png)
            exported += 1

    # 4. lm
    lm_path = os.path.join(src_dir, 'lm')
    if os.path.exists(lm_path):
        with open(lm_path, 'rb') as f:
            data = f.read()
        offset = 0
        idx = 0
        while offset < len(data):
            flag = data[offset]; offset += 1
            length = struct.unpack('>H', data[offset:offset+2])[0]; offset += 2
            png = data[offset:offset+length]; offset += length
            assert png.startswith(b'\x89PNG')
            with open(os.path.join(out_dir, f'lm_{idx}.png'), 'wb') as f:
                f.write(png)
            idx += 1
            exported += 1
        assert offset == len(data)

    # 5. bg0..bg4
    for bgi in range(5):
        path = os.path.join(src_dir, f'bg{bgi}')
        if os.path.exists(path):
            with open(path, 'rb') as f:
                data = f.read()
            variants, var_count, offset = parse_gmidlet_image_stream(data, 0)
            assert offset == len(data)
            for vi, var_png in enumerate(variants):
                suffix = f'_v{vi}' if len(variants) > 1 else ''
                with open(os.path.join(out_dir, f'bg{bgi}{suffix}.png'), 'wb') as f:
                    f.write(var_png)
                exported += 1

    print(f"Exported {exported} PNG images to {out_dir}/ successfully.")
    return exported

if __name__ == '__main__':
    extract_all_pngs()
