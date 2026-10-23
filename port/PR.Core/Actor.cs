using System;
using PR.Core.Graphics;

namespace PR.Core;

/// <summary>
/// Entity and sprite representation mirroring a.java.
/// Handles hitbox calculations, physics ticks, animation sequence advancement, and collisions.
/// </summary>
public sealed class Actor
{
    public static IResourceProvider? ResourceProvider;
    public static Image?[]? var_javax_microedition_lcdui_Image_arr_a;
    public static short[]? var_short_arr_c;

    // Static sprite / hitbox data
    public static byte[]? var_byte_arr_b; // Loaded from /fr (5 bytes per frame rect)
    public static Actor[]? var_a_arr_a;   // Loaded from /spr (archetype templates)

    // Sequence / animation state
    public short[][]? var_short_arr_arr_a;
    public short[]? var_short_arr_a;
    public short[]? var_short_arr_b;
    public int var_int_a; // active frame
    public int var_int_b; // facing: 1=right, 3=left, 0=up, 2=down
    public int var_int_c = -1; // forced facing
    public int var_int_d; // posX
    public int var_int_e; // posY
    public int var_int_f; // velX
    public int var_int_g; // velY
    public int var_int_h = 0; // base velX
    public int var_int_i = 0; // base velY
    public int var_int_j = -1;
    public int var_int_k = -1; // max lifetime ticks
    public int var_int_l = 0;  // age ticks
    public bool var_boolean_a = true; // active
    public int var_int_m; // half width
    public int var_int_n; // prev posY
    public int var_int_o; // prev posX
    public bool var_boolean_b;
    public int p; // spawn X
    public int q; // spawn Y
    public int r; // invincibility / hurt flash timer
    public bool var_boolean_c; // dead
    public bool var_boolean_d; // dormant
    public bool var_boolean_e; // hurt
    public bool var_boolean_f = false; // visible
    public int s; // sprite type / archetype
    public int t; // state (0=idle, 1=walk, 2=jump up, 3=fall, 4=crouch, 5=attack)
    public sbyte var_byte_a;
    public bool var_boolean_g;
    public bool var_boolean_h; // drop through platforms
    public int u; // flags bitmask
    public sbyte var_byte_b; // attack damage
    public sbyte var_byte_c;
    public bool var_boolean_i;
    public sbyte var_byte_d; // speed
    public int v;
    public sbyte[][]? var_byte_arr_arr_a;
    public sbyte[]? var_byte_arr_a;
    public int w; // palette / character index
    public bool var_boolean_o = false;
    public int x = -1; // active sequence index
    public int y = 0;  // instance counter
    public long var_long_a = -1L;
    public int z; // current HP
    public int A; // max HP
    public int B; // hitbox left
    public int C; // hitbox right
    public bool var_boolean_j;
    public bool var_boolean_k;
    public int D; // action bitmask
    public int E;
    public Actor? var_a_a;
    public Actor? var_a_b;
    public bool var_boolean_l;
    public bool var_boolean_m;
    public bool var_boolean_n;
    public int F; // projectile lifetime
    public int G; // projectile shoot interval
    public int H; // projectile shoot countdown
    public int I; // bbox left
    public int J; // bbox top
    public int K; // bbox right
    public int L; // bbox bottom

    // Sub-frame interpolation for 60fps rendering (engine tweak)
    public ActorState InterpState { get; } = new();
    public int RenderX;
    public int RenderY;

    public Actor()
    {
    }

    public Actor(Actor other)
    {
        void_a(other);
    }

    public Actor(int archetypeId)
    {
        if (var_a_arr_a != null && archetypeId >= 0 && archetypeId < var_a_arr_a.Length)
        {
            void_a(var_a_arr_a[archetypeId]);
            void_h();
            d(-1);
            void_k();
        }
    }

    public void void_a(int n)
    {
        D |= n;
    }

    public void b(int n)
    {
        D &= ~n;
    }

    public bool boolean_a(int n)
    {
        return (D & n) != 0;
    }

    public void void_a()
    {
        if (var_a_b != null)
        {
            var_int_d = var_a_b.var_int_d + (var_a_b.var_int_b == 1 ? -p : p);
            var_int_e = var_a_b.var_int_e + q;
            var_int_b = var_a_b.var_int_b;
            var_int_f = var_int_b == 1 ? var_int_h : -var_int_h;
        }
        var_boolean_l = false;
        F = 0;
        var_boolean_a = var_boolean_m;
    }

    public bool boolean_a()
    {
        if (H > 0 && --H == 0)
        {
            void_b();
        }
        if (var_boolean_l)
        {
            a(true);
        }
        return F <= 0 || --F != 0;
    }

    public void void_b()
    {
        void_a();
        var_boolean_l = true;
        H = G;
        var_boolean_a = true;
    }

    public static void a(DataReader dis)
    {
        int totalRects = dis.ReadShort();
        var_byte_arr_b = new byte[totalRects * 5];
        int offset = 0;
        int groups = dis.ReadShort();
        for (int i = groups; i > 0; --i)
        {
            int rectCount = dis.ReadShort();
            sbyte flag = dis.ReadByte();
            for (int j = rectCount; j > 0; --j)
            {
                dis.ReadFully(var_byte_arr_b, offset, 4);
                offset += 4;
                var_byte_arr_b[offset++] = (byte)flag;
            }
        }
    }

    public static void c(int n)
    {
        var_javax_microedition_lcdui_Image_arr_a = new Image[n];
        var_short_arr_c = new short[n];
    }

    public void d(int n)
    {
        int n2 = 1 << (8 + n);
        if (y > 0 && var_byte_arr_arr_a != null && ResourceProvider != null && var_short_arr_c != null)
        {
            for (int i = 0; i < var_byte_arr_arr_a.Length; ++i)
            {
                sbyte[] byArray = var_byte_arr_arr_a[i];
                int n3 = 5;
                while (n3 < byArray.Length)
                {
                    int n4;
                    int n5;
                    int n6;
                    n3 += 2;
                    byte by = (byte)byArray[n3++];
                    byte by2 = (byte)byArray[n3++];
                    int n7 = (short)((short)(by << 8) | by2);
                    int n8 = var_byte_arr_b![(n7 * 5) + 4];
                    int n9 = var_short_arr_c[n8] & 0x7F;
                    if (n < 0)
                    {
                        n6 = n;
                        n5 = 0;
                        for (n4 = 0; n4 < n9; ++n4)
                        {
                            n5 |= (short)(1 << (8 + n4));
                        }
                    }
                    else if (n9 - 1 < n)
                    {
                        n6 = n9 > 0 ? n9 - 1 : n;
                        n5 = (short)(1 << (8 + n6));
                    }
                    else
                    {
                        n6 = n;
                        n5 = n2;
                    }
                    if ((var_short_arr_c[n8] & n5) != 0) continue;
                    n4 = n8;
                    try
                    {
                        using var stream = ResourceProvider.OpenResource("s" + n4);
                        if (stream != null)
                        {
                            using var dis = new DataReader(stream);
                            GMIDlet.a(dis, var_javax_microedition_lcdui_Image_arr_a!, var_short_arr_c, n8, n6, true);
                        }
                    }
                    catch { }
                }
            }
        }
    }

    public bool a(int n, int n2)
    {
        if (!var_boolean_f || !var_boolean_a || (var_boolean_c && r < 25 && (r & 1) != 0))
        {
            return false;
        }

        int posX = EngineTweaks.Smooth60Fps ? RenderX : var_int_d;
        int posY = EngineTweaks.Smooth60Fps ? RenderY : var_int_e;
        int n3 = n + posX;
        int n4 = n2 + posY;
        var_int_b = int_a();
        int n5 = 0;
        if (var_byte_arr_arr_a == null || var_int_a >= var_byte_arr_arr_a.Length) return false;
        sbyte[] byArray = var_byte_arr_arr_a[var_int_a];
        sbyte by = byArray[0];

        switch (var_int_b)
        {
            case 3:
                if (by == 2) n5 = 2;
                else if (by == 4) n5 = 90;
                break;
            case 1:
                if (by == 4) n5 = 270;
                break;
            case 2:
                if (by == 3) n5 = 1;
                else if (by == 4) n5 = 180;
                break;
        }

        long l = 1L;
        int n6 = 5;
        while (n6 < byArray.Length)
        {
            if ((var_long_a & l) == 0L)
            {
                n6 += 4;
                l <<= 1;
                continue;
            }
            l <<= 1;
            int n7 = byArray[n6++];
            int n8 = byArray[n6++];
            byte by2 = (byte)byArray[n6++];
            byte by3 = (byte)byArray[n6++];
            int n9 = (short)((short)(by2 << 8) | by3);
            n9 *= 5;

            int n10 = var_byte_arr_b![n9++] & 0xFF;
            int n11 = var_byte_arr_b[n9++] & 0xFF;
            byte by4 = var_byte_arr_b[n9++];
            byte by5 = var_byte_arr_b[n9++];
            int n12 = var_byte_arr_b[n9];

            short s = var_short_arr_c != null && n12 < var_short_arr_c.Length ? var_short_arr_c[n12] : (short)0;
            int n13 = w < (s & 0x7F) ? w : 0;
            Image? image = (var_javax_microedition_lcdui_Image_arr_a != null && (n12 + n13) < var_javax_microedition_lcdui_Image_arr_a.Length)
                ? var_javax_microedition_lcdui_Image_arr_a[n12 + n13] : null;

            if (by == 2 || by == 4 || by == 8)
            {
                switch (var_int_b)
                {
                    case 3:
                        n7 = -n7 - by4;
                        break;
                    case 2:
                        n8 = -n8 - by5;
                        break;
                }
            }

            if (image != null && GMIDlet.var_javax_microedition_lcdui_Graphics_a != null)
            {
                GMIDlet.var_javax_microedition_lcdui_Graphics_a.DrawRegion(image, n10, n11, by4, by5, n5, n7 + n3, n8 + n4, 20);
            }
        }

        var_boolean_o = false;
        if (boolean_a(1) && Game.var_a_d != null)
        {
            Actor a2 = Game.var_a_d;
            Game.var_a_d.var_int_d = var_int_d;
            a2.var_int_e = var_int_e;
            a2.a(n, n2);
        }
        return false;
    }

    public static void a(Actor[]? aArray, int n, int n2, int n3)
    {
        if (aArray == null) return;
        int n4 = -n - 10;
        int n5 = 240 - n + 10;
        int n6 = -n2 - 10;
        int n7 = 320 - n2 + 10;
        for (int i = aArray.Length - 1; i >= 0; --i)
        {
            Actor a2 = aArray[i];
            if (a2 == null) continue;
            if (n3 == 1 ? (a2.int_e() < n4 || a2.int_d() > n5 || a2.var_int_e < n6 || a2.int_f() > n7)
                        : (n3 == 2 && (a2.int_h() < n4 || a2.int_g() > n5 || a2.int_j() < n6 || a2.int_i() > n7)))
            {
                continue;
            }
            aArray[i].a(n, n2);
        }
    }

    public void a(DataReader dis, bool isTrigger)
    {
        s = dis.ReadByte();
        if (var_a_arr_a != null && s >= 0 && s < var_a_arr_a.Length)
        {
            void_a(var_a_arr_a[s]);
        }
        w = 0;
        short count = dis.ReadShort();
        while (--count >= 0)
        {
            sbyte tag = dis.ReadByte();
            switch (tag)
            {
                case 0:
                    var_int_k = dis.ReadShort();
                    break;
                case 1:
                    p = dis.ReadShort();
                    break;
                case 2:
                    q = dis.ReadShort();
                    break;
                case 3:
                    var_int_h = dis.ReadByte();
                    break;
                case 4:
                    var_int_i = dis.ReadByte();
                    break;
                case 5:
                    var_boolean_a = dis.ReadBoolean();
                    break;
                case 11:
                    w = dis.ReadByte();
                    break;
                case 13:
                    var_int_j = dis.ReadByte();
                    break;
                case 14:
                    G = dis.ReadByte();
                    break;
            }
        }
        if (isTrigger)
        {
            p <<= TileMap.var_int_a;
            q <<= TileMap.var_int_a;
        }
    }

    public void void_d()
    {
        I = var_int_d + int_b();
        K = var_int_d + int_c();
        int top = int_f();
        int bottom = var_int_e;
        J = top + ((bottom - top) >> 2) + 2;
        L = J + ((bottom - top) >> 1) - 7;
    }

    public void void_e()
    {
        I = int_g();
        K = int_h();
        J = int_i();
        L = int_j();
    }

    public void void_f()
    {
        I = var_int_d + int_b();
        K = var_int_d + int_c();
        L = int_j();
        J = (int_i() + L) >> 1;
    }

    public void void_g()
    {
        if (var_byte_arr_arr_a != null && var_byte_arr_arr_a.Length > 0)
        {
            var_int_m = (var_byte_arr_arr_a[0][3] - var_byte_arr_arr_a[0][1]) >> 1;
            var_byte_arr_a = new sbyte[var_byte_arr_arr_a.Length];
            for (int i = 0; i < var_byte_arr_arr_a.Length; ++i)
            {
                sbyte[] frame = var_byte_arr_arr_a[i];
                sbyte minY = sbyte.MaxValue;
                int n = 5;
                while (n < frame.Length)
                {
                    sbyte y = frame[n + 1];
                    n += 4;
                    if (y < minY)
                    {
                        minY = y;
                    }
                }
                var_byte_arr_a[i] = minY;
            }
        }
    }

    public void b(DataReader dis)
    {
        dis.ReadShort();
        dis.ReadShort();
        dis.ReadLong();
        int frameCount = dis.ReadShort();
        var_byte_arr_arr_a = new sbyte[frameCount][];
        for (int i = 0; i < frameCount; ++i)
        {
            short subCount = dis.ReadShort();
            byte[] buf = new byte[5 + subCount * 4];
            dis.ReadFully(buf);
            sbyte[] sbuf = new sbyte[buf.Length];
            for (int k = 0; k < buf.Length; ++k) sbuf[k] = (sbyte)buf[k];
            var_byte_arr_arr_a[i] = sbuf;
        }

        int seqCount = dis.ReadShort();
        var_short_arr_arr_a = new short[seqCount][];
        for (int i = 0; i < seqCount; ++i)
        {
            short seqLen = dis.ReadShort();
            sbyte by = dis.ReadByte();
            short[] seq = new short[9 + (by == 4 ? 2 : 4) * seqLen];
            seq[4] = by;
            seq[3] = dis.ReadByte();
            seq[5] = dis.ReadByte();
            seq[0] = -1;
            seq[1] = -1;
            for (int j = 9; j < seq.Length; ++j)
            {
                seq[j] = dis.ReadShort();
            }
            seq[6] = short.MaxValue;
            var_short_arr_arr_a[i] = seq;
        }
    }

    public void void_h()
    {
        var_boolean_f = true;
        var_int_l = 0;
        InterpState.Snap(var_int_d, var_int_e);
        RenderX = var_int_d;
        RenderY = var_int_e;
    }

    public void void_i()
    {
        var_boolean_f = false;
    }

    public int int_a()
    {
        if (var_int_c >= 0)
        {
            var_int_b = var_int_c;
            return var_int_b;
        }
        int dir = var_int_b;
        if (var_byte_arr_arr_a == null || var_int_a >= var_byte_arr_arr_a.Length)
            return dir;

        sbyte mode = var_byte_arr_arr_a[var_int_a][0];
        switch (mode)
        {
            case 3:
                if (var_int_g != 0)
                {
                    dir = var_int_g >= 0 ? 2 : 0;
                }
                else if (dir != 2 && dir != 0)
                {
                    dir = 0;
                }
                break;
            case 2:
                if (var_int_f != 0)
                {
                    dir = var_int_f >= 0 ? 1 : 3;
                }
                else if (dir != 1 && dir != 3)
                {
                    dir = 3;
                }
                break;
            case 4:
                if (Math.Abs(var_int_g) >= Math.Abs(var_int_f))
                {
                    if (var_int_g != 0)
                    {
                        dir = var_int_g >= 0 ? 2 : 0;
                    }
                }
                else if (var_int_f != 0)
                {
                    dir = var_int_f >= 0 ? 1 : 3;
                }
                break;
        }
        return dir;
    }

    public void void_j()
    {
        a(true);
    }

    public void a(bool updatePos)
    {
        if (!var_boolean_f || !var_boolean_a)
        {
            return;
        }
        var_boolean_b = false;
        var_int_o = var_int_d;
        var_int_n = var_int_e;
        a(var_short_arr_a);
        a(var_short_arr_b);
        if (updatePos)
        {
            var_int_d += var_int_f;
            var_int_e += var_int_g;
            InterpState.Update(var_int_d, var_int_e);
        }
        ++var_int_l;
        if (var_int_k > 0 && var_int_l > var_int_k)
        {
            var_boolean_a = false;
            void_i();
        }
        var_boolean_g = var_int_a != 0 && int_k() != 0 && l() != 0;
    }

    public void a(int seqIndex, bool isSubSeq)
    {
        if (var_boolean_c || var_boolean_d)
        {
            return;
        }
        var_boolean_n = false;
        var_boolean_i = false;
        if (var_short_arr_arr_a != null && seqIndex < var_short_arr_arr_a.Length)
        {
            short[]? seq;
            if (var_short_arr_a != null)
            {
                var_short_arr_a[0] = 0;
            }
            if (seqIndex == -1)
            {
                seq = null;
            }
            else
            {
                seq = var_short_arr_arr_a[seqIndex];
                seq[0] = 9;
                seq[2] = 0;
                seq[1] = 0;
            }
            if (isSubSeq)
            {
                var_short_arr_b = seq;
                return;
            }
            var_short_arr_a = seq;
            x = seqIndex;
        }
    }

    public void a(short[]? seq)
    {
        if (seq != null && seq[0] > 0)
        {
            while ((seq[1] = (short)(seq[1] - 1)) == -1)
            {
                if (seq[0] == seq.Length)
                {
                    seq[2] = (short)(seq[2] + 1);
                    if (seq[2] < seq[3] || seq[3] <= 0)
                    {
                        seq[0] = 9;
                        if (seq[4] == 2 || seq[4] == 3) continue;
                        seq[1] = 0;
                        continue;
                    }
                    seq[0] = 0;
                    if ((seq[5] & 2) > 0)
                    {
                        var_boolean_a = false;
                    }
                    if ((seq[5] & 4) > 0)
                    {
                        void_i();
                    }
                    if ((seq[5] & 0x10) > 0)
                    {
                        b(var_short_arr_b);
                    }
                    if (seq[0] == 0)
                    {
                        x = -1;
                        return;
                    }
                    return;
                }
                short sIdx = seq[0];
                seq[0] = (short)(sIdx + 1);
                seq[1] = seq[sIdx];
                int dx = 0;
                int dy = 0;
                if (seq[4] != 4)
                {
                    short s2 = seq[0];
                    seq[0] = (short)(s2 + 1);
                    dx = seq[s2];
                    short s3 = seq[0];
                    seq[0] = (short)(s3 + 1);
                    dy = seq[s3];
                }
                if ((seq[4] & 0x10) > 0 || seq[4] == 4)
                {
                    short s4 = seq[0];
                    seq[0] = (short)(s4 + 1);
                    var_int_a = seq[s4];
                }
                if (var_byte_arr_arr_a != null && var_int_a < var_byte_arr_arr_a.Length &&
                    var_byte_arr_arr_a[var_int_a][0] == 2 && int_a() == 3)
                {
                    dx = -dx;
                }
                switch (seq[4] & 0xF)
                {
                    case 4:
                        break;
                    case 1:
                        var_int_f += dx;
                        var_int_g += dy;
                        break;
                    case 0:
                        var_int_f = dx;
                        var_int_g = dy;
                        break;
                }
            }
        }
    }

    public static void b(short[]? seq)
    {
        if (seq != null)
        {
            seq[0] = -1;
        }
    }

    public void void_k()
    {
        var_int_d = p;
        var_int_e = q;
        var_int_f = var_int_h;
        var_int_g = var_int_i;
        z = A;
        var_boolean_j = (u & 0x20) != 0;
        if (var_short_arr_arr_a != null)
        {
            for (int i = 0; i < var_short_arr_arr_a.Length; ++i)
            {
                short[] seq = var_short_arr_arr_a[i];
                seq[0] = 0;
                seq[1] = 0;
            }
            a(var_int_j, false);
        }
        var_int_c = -1;
        var_int_b = int_a();
        v = 5;
    }

    public int int_b()
    {
        if ((var_int_c != -1 ? var_int_c : var_int_b) == 3)
        {
            return -C;
        }
        return B;
    }

    public int int_c()
    {
        if ((var_int_c != -1 ? var_int_c : var_int_b) == 3)
        {
            return -B;
        }
        return C;
    }

    public int int_d()
    {
        return var_int_d - var_int_m;
    }

    public int int_e()
    {
        return var_int_d + var_int_m;
    }

    public int int_f()
    {
        if (var_byte_arr_a == null || var_int_a >= var_byte_arr_a.Length)
        {
            return var_int_e;
        }
        return var_int_e + var_byte_arr_a[var_int_a];
    }

    public int int_g()
    {
        return var_int_d + int_k();
    }

    public int int_h()
    {
        return var_int_d + m();
    }

    public int int_i()
    {
        if (var_byte_arr_arr_a == null || var_int_a >= var_byte_arr_arr_a.Length) return var_int_e;
        return var_int_e + var_byte_arr_arr_a[var_int_a][2];
    }

    public int int_j()
    {
        if (var_byte_arr_arr_a == null || var_int_a >= var_byte_arr_arr_a.Length) return var_int_e;
        return var_int_e + var_byte_arr_arr_a[var_int_a][4];
    }

    public int int_k()
    {
        if (var_byte_arr_arr_a == null || var_int_a >= var_byte_arr_arr_a.Length) return 0;
        if ((var_int_c != -1 ? var_int_c : var_int_b) == 3)
        {
            return -var_byte_arr_arr_a[var_int_a][3];
        }
        return var_byte_arr_arr_a[var_int_a][1];
    }

    public int l()
    {
        if (var_byte_arr_arr_a == null || var_int_a >= var_byte_arr_arr_a.Length) return 0;
        return var_byte_arr_arr_a[var_int_a][2];
    }

    public int m()
    {
        if (var_byte_arr_arr_a == null || var_int_a >= var_byte_arr_arr_a.Length) return 0;
        if ((var_int_c != -1 ? var_int_c : var_int_b) == 3)
        {
            return -var_byte_arr_arr_a[var_int_a][1];
        }
        return var_byte_arr_arr_a[var_int_a][3];
    }

    public int n()
    {
        if (var_byte_arr_arr_a == null || var_int_a >= var_byte_arr_arr_a.Length) return 0;
        return var_byte_arr_arr_a[var_int_a][4];
    }

    public static void e(int n)
    {
        var_a_arr_a = new Actor[n];
    }

    public void void_a(Actor other)
    {
        s = other.s;
        w = other.w;
        var_byte_arr_arr_a = other.var_byte_arr_arr_a;
        var_byte_arr_a = other.var_byte_arr_a;
        var_boolean_a = other.var_boolean_a;
        var_int_d = other.var_int_d;
        var_int_e = other.var_int_e;
        var_int_f = other.var_int_f;
        var_int_g = other.var_int_g;
        p = other.p;
        q = other.q;
        var_int_h = other.var_int_h;
        var_int_i = other.var_int_i;
        var_int_k = other.var_int_k;
        var_int_m = other.var_int_m;
        B = other.B;
        C = other.C;
        u = other.u;
        var_byte_b = other.var_byte_b;
        var_byte_d = other.var_byte_d;

        if (var_a_arr_a != null && s >= 0 && s < var_a_arr_a.Length)
        {
            y = ++var_a_arr_a[s].y;
            if (y > 1 && other.var_short_arr_arr_a != null)
            {
                var_short_arr_arr_a = new short[other.var_short_arr_arr_a.Length][];
                for (int i = 0; i < var_short_arr_arr_a.Length; ++i)
                {
                    var_short_arr_arr_a[i] = new short[other.var_short_arr_arr_a[i].Length];
                    Array.Copy(other.var_short_arr_arr_a[i], var_short_arr_arr_a[i], var_short_arr_arr_a[i].Length);
                }
                return;
            }
        }
        var_short_arr_arr_a = other.var_short_arr_arr_a;
    }

    public bool boolean_a(Actor other)
    {
        return I <= other.int_e() && K >= other.int_d() && J <= other.var_int_e && L >= other.int_f();
    }

    public bool b(Actor other)
    {
        return int_d() <= other.int_h() && int_e() >= other.int_g() && int_f() <= other.int_j() && var_int_e >= other.int_i();
    }

    public bool boolean_b()
    {
        return x != -1 && x < Game.var_boolean_arr_a.Length && Game.var_boolean_arr_a[x];
    }
}
