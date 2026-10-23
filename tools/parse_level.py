import struct
import os

def parse_a_actor(data, offset, is_bl=False):
    s = data[offset]
    offset += 1
    count = struct.unpack('>h', data[offset:offset+2])[0]
    offset += 2
    tags = {}
    for _ in range(count):
        tag = data[offset]
        offset += 1
        if tag in [0, 1, 2]:
            val = struct.unpack('>h', data[offset:offset+2])[0]
            offset += 2
        elif tag in [3, 4, 11, 13, 14]:
            val = data[offset]
            offset += 1
        elif tag == 5:
            val = data[offset] != 0
            offset += 1
        else:
            raise ValueError(f"Unknown a tag {tag} at {offset-1}")
        tags[tag] = val
    return {'s': s, 'tags': tags}, offset

def parse_a_a_actor(data, offset):
    s1 = data[offset]
    offset += 1  # Note: 1 byte in bytecode!
    p = struct.unpack('>h', data[offset:offset+2])[0]
    q = struct.unpack('>h', data[offset+2:offset+4])[0]
    offset += 4
    n = struct.unpack('>h', data[offset:offset+2])[0]
    offset += 2
    tags = {}
    for _ in range(n):
        tag = data[offset]
        offset += 1
        if tag in [0, 5, 6, 7, 8, 9, 10, 11]:
            val = data[offset]
            offset += 1
        elif tag in [1, 3]:
            val = data[offset] != 0
            offset += 1
        elif tag == 2:
            val = struct.unpack('>h', data[offset:offset+2])[0]
            offset += 2
        elif tag == 4:
            val = None
        else:
            raise ValueError(f"Unknown a_a tag {tag} at {offset-1}")
        tags[tag] = val
    return {'s1': s1, 'p': p, 'q': q, 'tags': tags}, offset

def parse_level_file(path):
    with open(path, 'rb') as f:
        data = f.read()
    offset = 0
    by = data[offset]       # corresponding b<N> index
    by2 = data[offset+1]    # bg variant index
    aj = data[offset+2]     # primary tile layer index
    offset += 3
    
    level_tags = {}
    n2 = struct.unpack('>h', data[offset:offset+2])[0]
    offset += 2
    for _ in range(n2):
        tag = data[offset]; offset += 1
        if tag == 0:
            level_tags[tag] = data[offset]; offset += 1
        else:
            raise ValueError(f"Unknown level tag {tag}")
    
    # Static decorations / actors (var_a_arr_d)
    n2 = struct.unpack('>h', data[offset:offset+2])[0]; offset += 2
    decorations = []
    for _ in range(n2):
        actor, offset = parse_a_actor(data, offset, False)
        decorations.append(actor)
    
    offset += 2  # dummy short
    player_spawn, offset = parse_a_a_actor(data, offset)
    
    # Enemies (var_a_arr_g)
    n3 = struct.unpack('>h', data[offset:offset+2])[0]; offset += 2
    enemies = []
    for _ in range(n3):
        enemy, offset = parse_a_a_actor(data, offset)
        enemies.append(enemy)
    
    # Triggers / Interactive items (var_a_arr_e)
    n3 = struct.unpack('>h', data[offset:offset+2])[0]; offset += 2
    triggers = []
    for _ in range(n3):
        trig, offset = parse_a_actor(data, offset, True)
        triggers.append(trig)
    
    # Moving hazards / platforms (var_a_arr_h)
    n3 = struct.unpack('>h', data[offset:offset+2])[0]; offset += 2
    hazards = []
    for _ in range(n3):
        h_pos = struct.unpack('>h', data[offset:offset+2])[0]; offset += 2
        bl = data[offset] != 0; offset += 1
        n_byte = data[offset]; offset += 1
        offset += 2  # short
        act, offset = parse_a_actor(data, offset, True)
        hazards.append({'pos': h_pos, 'bl': bl, 'speed': n_byte, 'actor': act})
    
    # Spawners / Pickups (var_a_arr_i)
    n3 = struct.unpack('>h', data[offset:offset+2])[0]; offset += 2
    pickups = []
    for _ in range(n3):
        n_val = data[offset]; offset += 1
        by_b = data[offset]; offset += 1
        offset += 2  # short
        act, offset = parse_a_actor(data, offset, True)
        pickups.append({'val': n_val, 'type': by_b, 'actor': act})
    
    # Checkpoints / Exit zones (var_a_arr_a)
    n3 = struct.unpack('>h', data[offset:offset+2])[0]; offset += 2
    checkpoints = []
    for _ in range(n3):
        act, offset = parse_a_actor(data, offset, False)
        checkpoints.append(act)
    
    # Foreground items (var_a_arr_f)
    n3 = struct.unpack('>h', data[offset:offset+2])[0]; offset += 2
    fg_items = []
    for _ in range(n3):
        act, offset = parse_a_actor(data, offset, False)
        fg_items.append(act)
    
    # Level spline / path curves (var_short_arr_arr_a)
    s_count = struct.unpack('>h', data[offset:offset+2])[0]; offset += 2
    splines = []
    for _ in range(s_count):
        s_len = struct.unpack('>h', data[offset:offset+2])[0]; offset += 2
        spline = []
        for _ in range(s_len // 2):
            val = struct.unpack('>h', data[offset:offset+2])[0]; offset += 2
            spline.append(val)
        splines.append(spline)
    
    assert offset == len(data), f"Mismatch in {path}: consumed {offset}/{len(data)}"
    return {
        'block_idx': by,
        'bg_variant': by2,
        'active_layer': aj,
        'decorations': len(decorations),
        'enemies': len(enemies),
        'triggers': len(triggers),
        'hazards': len(hazards),
        'pickups': len(pickups),
        'checkpoints': len(checkpoints),
        'splines': len(splines)
    }

def parse_all_levels(src_dir='extracted'):
    l_files = [f for f in sorted(os.listdir(src_dir)) if f.startswith('l') and f[1:].isdigit()]
    for lf in l_files:
        info = parse_level_file(os.path.join(src_dir, lf))
        print(f"  {lf:4}: block=b{info['block_idx']} bg_var={info['bg_variant']}, enemies={info['enemies']}, triggers={info['triggers']}, hazards={info['hazards']}, splines={info['splines']}")
    print(f"Parsed {len(l_files)} level layout files successfully.")

if __name__ == '__main__':
    parse_all_levels()
