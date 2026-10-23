import struct
import os
from parse_pngs import parse_gmidlet_image_stream

MIME_TABLE = ['midi', 'x-wav', 'mpeg', 'basic', 'x-tone-seq', 'x-mid', 'x-smaf', 'amr', 'amr-wb']

def parse_fr(path):
    with open(path, 'rb') as f:
        data = f.read()
    offset = 0
    total_rects = struct.unpack('>h', data[offset:offset+2])[0]; offset += 2
    groups = struct.unpack('>h', data[offset:offset+2])[0]; offset += 2
    rect_list = []
    for _ in range(groups):
        rect_count = struct.unpack('>h', data[offset:offset+2])[0]; offset += 2
        flag = data[offset]; offset += 1
        for _ in range(rect_count):
            x, y, w, h = struct.unpack('>4b', data[offset:offset+4]); offset += 4
            rect_list.append((x, y, w, h, flag))
    assert offset == len(data), f"fr leftover: {len(data)-offset}"
    return rect_list

def parse_spr(path):
    with open(path, 'rb') as f:
        data = f.read()
    offset = 0
    spr_count = struct.unpack('>h', data[offset:offset+2])[0]; offset += 2
    sprites = []
    for _ in range(spr_count):
        offset += 12 # 2 shorts + 1 long header
        frame_count = struct.unpack('>h', data[offset:offset+2])[0]; offset += 2
        frames = []
        for _ in range(frame_count):
            sub_count = struct.unpack('>h', data[offset:offset+2])[0]; offset += 2
            sub_data = data[offset:offset + 5 + sub_count * 4]; offset += 5 + sub_count * 4
            frames.append(sub_data)
        seq_count = struct.unpack('>h', data[offset:offset+2])[0]; offset += 2
        sequences = []
        for _ in range(seq_count):
            seq_len = struct.unpack('>h', data[offset:offset+2])[0]; offset += 2
            by = data[offset]
            b3 = data[offset+1]
            b5 = data[offset+2]
            offset += 3
            words = (2 if by == 4 else 4) * seq_len
            seq_words = [struct.unpack('>h', data[offset+w*2:offset+w*2+2])[0] for w in range(words)]
            offset += words * 2
            sequences.append({'type': by, 'len': seq_len, 'words': seq_words})
        sprites.append({'frames': frames, 'sequences': sequences})
    assert offset == len(data), f"spr leftover: {len(data)-offset}"
    return sprites

def parse_l_pack(path, audio_out_dir='sons'):
    os.makedirs(audio_out_dir, exist_ok=True)
    with open(path, 'rb') as f:
        data = f.read()
    
    offset = 0
    var_int_l = data[offset]; offset += 1
    
    # Tables
    n = struct.unpack('>h', data[offset:offset+2])[0]; offset += 2 + n * 4
    n = struct.unpack('>h', data[offset:offset+2])[0]; offset += 2 + n
    n = struct.unpack('>h', data[offset:offset+2])[0]; offset += 2 + n
    offset += 16 + 2 # single config bytes
    n = struct.unpack('>h', data[offset:offset+2])[0]; offset += 2 + n # var_int_arr_h
    offset += 1 # f.D
    n = struct.unpack('>h', data[offset:offset+2])[0]; offset += 2 + n
    n = struct.unpack('>h', data[offset:offset+2])[0]; offset += 2 + n
    offset += 1 # w
    n = struct.unpack('>h', data[offset:offset+2])[0]; offset += 2 + n
    n = struct.unpack('>h', data[offset:offset+2])[0]; offset += 2 + n
    n = struct.unpack('>h', data[offset:offset+2])[0]; offset += 2 + n
    offset += 6 # f.var_int_q..v
    n = struct.unpack('>h', data[offset:offset+2])[0]; offset += 2 + n
    n = struct.unpack('>h', data[offset:offset+2])[0]; offset += 2 + n
    offset += 1 + 2 # v + readShort
    
    # Metadata records
    s2 = struct.unpack('>h', data[offset:offset+2])[0]; offset += 2
    for _ in range(s2):
        offset += 7
        s = struct.unpack('>h', data[offset:offset+2])[0]; offset += 2
        for _ in range(s):
            tag = data[offset]; offset += 1
            if tag == 1: offset += 1
            
    # Embedded PNG images
    img_count = data[offset]; offset += 2
    images = []
    for i in range(img_count):
        flag = data[offset]; offset += 1
        png_len = struct.unpack('>H', data[offset:offset+2])[0]; offset += 2
        if png_len == 0:
            images.append([])
            continue
        assert data[offset:offset+4] == b'\x89PNG'
        base_png = data[offset:offset+png_len]; offset += png_len
        variants = [base_png]
        if (flag & 2) != 0:
            var_count = struct.unpack('>H', data[offset:offset+2])[0]; offset += 2
            for _ in range(1, var_count):
                cur_png = bytearray(base_png)
                pc = data[offset]; offset += 1
                if pc == 0:
                    pc = struct.unpack('>H', data[offset:offset+2])[0]; offset += 2
                for _ in range(pc):
                    poff = struct.unpack('>H', data[offset:offset+2])[0]; offset += 2
                    val = data[offset]; offset += 1
                    cur_png[poff] = val
                variants.append(bytes(cur_png))
        images.append(variants)
        
    offset += 1 # readByte()
    
    # Audio payloads: e.a(dataInputStream)
    sound_count = struct.unpack('>h', data[offset:offset+2])[0]; offset += 2
    audio_files = []
    for snd_i in range(1, sound_count + 1):
        offset += 1 # readByte()
        snd_len = struct.unpack('>h', data[offset:offset+2])[0]; offset += 2
        blob = data[offset:offset+snd_len]
        mime_idx = data[offset+snd_len]
        offset += snd_len + 1
        mime = MIME_TABLE[mime_idx]
        ext = 'mid' if mime in ['midi', 'x-mid'] else ('wav' if mime == 'x-wav' else mime)
        out_path = os.path.join(audio_out_dir, f'sound_{snd_i}.{ext}')
        with open(out_path, 'wb') as sf:
            sf.write(blob)
        audio_files.append((out_path, len(blob), mime))
        n2 = struct.unpack('>h', data[offset:offset+2])[0]; offset += 2
        offset += n2 # readByte() loop
        offset += 2  # readShort()
        
    assert offset == len(data), f"Mismatch in {path}: {offset}/{len(data)}"
    return len(images), audio_files

def parse_all_packs(src_dir='extracted'):
    fr_rects = parse_fr(os.path.join(src_dir, 'fr'))
    print(f"fr: successfully parsed {len(fr_rects)} bounding rects.")
    
    sprites = parse_spr(os.path.join(src_dir, 'spr'))
    print(f"spr: successfully parsed {len(sprites)} sprite animation definitions.")
    
    img_count, sounds = parse_l_pack(os.path.join(src_dir, 'l'))
    print(f"l: successfully parsed {img_count} image definitions and extracted {len(sounds)} audio tracks:")
    for path, size, mime in sounds:
        print(f"  -> {path} ({size} bytes, MIME: {mime})")

if __name__ == '__main__':
    parse_all_packs()
