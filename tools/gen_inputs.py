import os

def gen_scripts(out_dir='tools/scripts'):
    os.makedirs(out_dir, exist_ok=True)

    # 1. Idle Script (100 ticks)
    with open(os.path.join(out_dir, 'idle_100.txt'), 'w') as f:
        f.write("# Deterministic Idle Script (100 ticks)\n")
        f.write("# Format: tick [press|release] key_code\n")

    # 2. Movement Script (150 ticks): Move right, jump, move left, crouch
    with open(os.path.join(out_dir, 'movement_150.txt'), 'w') as f:
        f.write("# Movement Script (150 ticks)\n")
        # Key 54 = Right (bit 1)
        # Key 50 = Up / Jump (bit 4)
        # Key 52 = Left (bit 2)
        # Key 56 = Down / Crouch (bit 8)
        f.write("5 press 54\n")
        f.write("35 press 50\n")
        f.write("55 release 50\n")
        f.write("70 release 54\n")
        f.write("80 press 52\n")
        f.write("110 release 52\n")
        f.write("120 press 56\n")
        f.write("140 release 56\n")

    # 3. Combat Script (200 ticks): Movement + Slashing combos
    with open(os.path.join(out_dir, 'combat_200.txt'), 'w') as f:
        f.write("# Combat Script (200 ticks)\n")
        # Key 53 = Fire / Slash (bit 64)
        f.write("5 press 54\n")
        f.write("25 press 53\n")
        f.write("28 release 53\n")
        f.write("35 press 53\n")
        f.write("38 release 53\n")
        f.write("45 press 53\n")
        f.write("48 release 53\n")
        f.write("70 release 54\n")
        f.write("80 press 50\n")  # Jump
        f.write("85 press 53\n")  # Jump-slash
        f.write("88 release 53\n")
        f.write("100 release 50\n")
        f.write("120 press 54\n")
        f.write("150 press 53\n")
        f.write("153 release 53\n")
        f.write("180 release 54\n")

    # 4. Survival Mode Script (150 ticks)
    with open(os.path.join(out_dir, 'survival_150.txt'), 'w') as f:
        f.write("# Survival Mode Script (150 ticks)\n")
        f.write("5 press 54\n")
        f.write("30 press 53\n")
        f.write("33 release 53\n")
        f.write("60 press 50\n")
        f.write("80 release 50\n")
        f.write("100 release 54\n")

    print(f"Generated deterministic test scripts in {out_dir}/.")

if __name__ == '__main__':
    gen_scripts()
