import struct
import os

def parse_block_layer(data, offset):
    var_byte_a = data[offset]  # bg ID / X
    tile_size = data[offset+1] # tile size, 16
    assert tile_size == 16, f"Expected 16, got {tile_size}"
    dummy = data[offset+2]
    offset += 3
    m, n = struct.unpack('>HH', data[offset:offset+4]) # width, height in tiles
    offset += 4
    has_flags = data[offset] != 0
    offset += 1
    parallax_x = data[offset]
    parallax_y = data[offset+1]
    offset += 2
    offset_x, offset_y = struct.unpack('>hh', data[offset:offset+4])
    offset += 4
    
    n4 = (n + 1) * 2
    row_offsets = []
    for _ in range(n):
        s_val = struct.unpack('>h', data[offset:offset+2])[0] - n4
        offset += 2
        row_offsets.append(s_val)
    
    n3 = struct.unpack('>h', data[offset:offset+2])[0] - n4
    offset += 2
    tile_data = data[offset:offset+n3]
    offset += n3
    
    n5 = data[offset] + 1
    offset += 1
    i_count = data[offset]
    offset += 1
    for _ in range(i_count):
        idx = data[offset] - 3
        offset += 1
        n2 = struct.unpack('>h', data[offset:offset+2])[0] - 3
        offset += 2
    
    n2 = data[offset]
    offset += 1
    
    tag_count = struct.unpack('>h', data[offset:offset+2])[0]
    offset += 2
    for _ in range(tag_count):
        t_idx = data[offset]
        t_val = data[offset+1]
        offset += 2
    
    offset += 2 # readShort
    offset += 1 # readByte
    
    n_count = struct.unpack('>h', data[offset:offset+2])[0]
    offset += 2
    triggers = []
    for _ in range(n_count):
        val_i = struct.unpack('>i', data[offset:offset+4])[0]
        offset += 4
        by_val = data[offset]
        offset += 1
        triggers.append((val_i, by_val))
    
    return {
        'bg_id': var_byte_a,
        'tile_size': tile_size,
        'width': m,
        'height': n,
        'parallax': (parallax_x, parallax_y),
        'offset': (offset_x, offset_y),
        'trigger_count': len(triggers)
    }, offset

def parse_block_file(path):
    with open(path, 'rb') as f:
        data = f.read()
    layer_count = data[0]
    offset = 1
    layers = []
    for _ in range(layer_count):
        layer_info, offset = parse_block_layer(data, offset)
        layers.append(layer_info)
    assert offset == len(data), f"Mismatch in {path}: {offset}/{len(data)}"
    return layers

def parse_all_blocks(src_dir='extracted'):
    b_files = [f for f in sorted(os.listdir(src_dir)) if f.startswith('b') and f[1:].isdigit()]
    for bf in b_files:
        layers = parse_block_file(os.path.join(src_dir, bf))
        print(f"  {bf:4}: {len(layers)} layers, primary grid={layers[0]['width']}x{layers[0]['height']} tiles, 16x16 px")
    print(f"Parsed {len(b_files)} block definition files successfully.")

if __name__ == '__main__':
    parse_all_blocks()
