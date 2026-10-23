using System;
using PR.Core.Graphics;

namespace PR.Core;

/// <summary>
/// Parallax tilemap layer, viewport scrolling, and 16x16 collision engine mirroring c.java.
/// </summary>
public sealed class TileMap
{
    public const int var_int_a = 4;
    public const int var_int_b = 16;

    public static Image? var_javax_microedition_lcdui_Image_a;
    public Image? var_javax_microedition_lcdui_Image_b;
    public Graphics.Graphics? var_javax_microedition_lcdui_Graphics_a;
    public static int h;

    public bool var_boolean_a = true;
    public int var_int_c; // total width in pixels
    public int d;         // total height in pixels
    public byte[]? var_byte_arr_b;
    public int m; // width in tiles
    public int n; // height in tiles
    public sbyte var_byte_a; // background index
    public short[]? var_short_arr_a;
    public byte[]? var_byte_arr_c; // collision attributes
    public int[]? var_int_arr_a;   // trigger descriptors
    public byte[]? var_byte_arr_a;  // trigger dimensions
    public int e;
    public int f;
    public int g;
    public int o;
    public int p;
    public int q;
    public int r;
    public int[]? var_int_arr_b;
    public int s; // viewport width
    public int t; // viewport height
    public int u; // camera tile X
    public int v; // camera tile Y
    public int w;
    public int x;
    public int y; // viewport tile columns
    public int z; // viewport tile rows
    public int i; // parallax scale X
    public int j; // parallax scale Y
    public int k; // parallax offset X
    public int l; // parallax offset Y
    private int A;
    private int B;
    private int C;
    private int D;
    private int E = -1000;
    private int F;
    private int G;
    private int H;
    private int I;
    private int J;

    public TileMap(int viewW, int viewH)
    {
        s = viewW;
        t = viewH;
    }

    public static int int_a(int n, int n2)
    {
        while (n < 0) n += n2;
        if (n >= n2) n %= n2;
        return n;
    }


    public bool boolean_a(int viewX, int viewY)
    {
        if (var_boolean_a)
        {
            int prevU = u;
            int prevV = v;
            u = (viewX + g) >> var_int_a;
            v = viewY >> var_int_a;
            if (u < o) u = o;
            if (v < p) v = p;
            if (u > q) u = q;
            if (v > r) v = r;
            int du = u - prevU;
            int dv = v - prevV;
            if (du == 0 && dv == 0) return false;

            int endU = u + y;
            int endV = v + z;
            if (Math.Abs(du) >= 2 || Math.Abs(dv) >= 2)
            {
                w = 0;
                x = 0;
                for (int i = u; i < endU; ++i)
                {
                    for (int j = v; j < endV; ++j)
                    {
                        c(i, j);
                    }
                }
                return true;
            }
            w = int_a(w + du, y);
            x = int_a(x + dv, z);
            if (dv > 0)
            {
                for (int i = u; i < endU; ++i)
                {
                    c(i, endV - 1);
                }
            }
            if (dv < 0)
            {
                for (int i = u; i < endU; ++i)
                {
                    c(i, v);
                }
            }
            if (du > 0)
            {
                for (int i = v; i < endV; ++i)
                {
                    c(endU - 1, i);
                }
            }
            if (du < 0)
            {
                for (int i = v; i < endV; ++i)
                {
                    c(u, i);
                }
            }
            return true;
        }
        return false;
    }

    public void a(DataReader dis)
    {
        var_byte_a = dis.ReadByte();
        dis.ReadByte(); // tile size (16)
        dis.ReadByte(); // dummy
        m = dis.ReadShort();
        n = dis.ReadShort();
        var_boolean_a = dis.ReadByte() != 0;
        i = dis.ReadByte();
        j = dis.ReadByte();
        k = dis.ReadShort();
        l = dis.ReadShort();

        int n4 = (n + 1) * 2;
        var_short_arr_a = new short[n];
        for (int row = 0; row < n; ++row)
        {
            var_short_arr_a[row] = (short)(dis.ReadShort() - n4);
        }

        int byteCount = dis.ReadShort() - n4;
        var_byte_arr_b = new byte[byteCount];
        dis.ReadFully(var_byte_arr_b);

        int loopCount = dis.ReadUnsignedByte();
        int paletteSize = dis.ReadUnsignedByte() + 1;
        var_int_arr_b = new int[paletteSize];
        for (int pCount = loopCount; pCount > 0; --pCount)
        {
            int val = dis.ReadShort() - 3;
            int idx = dis.ReadUnsignedByte() - 3;
            var_int_arr_b[idx] = val;
        }

        sbyte defaultAttr = dis.ReadByte();
        var_byte_arr_c = new byte[paletteSize + 3];
        for (int idx = var_byte_arr_c.Length - 1; idx >= 0; --idx)
        {
            var_byte_arr_c[idx] = (byte)defaultAttr;
        }
        var_byte_arr_c[0] = 0;
        var_byte_arr_c[1] = 0;
        var_byte_arr_c[2] = 0;

        int customAttrs = dis.ReadShort();
        while (customAttrs-- > 0)
        {
            var_byte_arr_c[dis.ReadByte() & 0xFF] = (byte)dis.ReadByte();
        }

        dis.ReadShort();
        dis.ReadByte();

        int triggerCount = dis.ReadShort();
        var_int_arr_a = new int[triggerCount];
        var_byte_arr_a = new byte[triggerCount];
        for (int idx = 0; idx < triggerCount; ++idx)
        {
            int desc = dis.ReadInt();
            sbyte dim = dis.ReadByte();
            int filter = (desc >> 29) & 3;
            if ((filter == 1 && Game.var_boolean_f) || (filter == 2 && !Game.var_boolean_f))
            {
                desc = -1;
            }
            var_int_arr_a[idx] = desc;
            var_byte_arr_a[idx] = (byte)dim;
        }
        E = -1000;
    }

    public void void_a(int n, int n2)
    {
        if (var_boolean_a)
        {
            int n3 = ((n + (var_int_b >> 1) + 1) >> var_int_a) + 1;
            int n4 = ((n2 + (var_int_b >> 1) + 1) >> var_int_a) + 1;
            if (var_javax_microedition_lcdui_Image_b == null)
            {
                var_javax_microedition_lcdui_Image_b = Image.CreateImage(n3 * var_int_b, n4 * var_int_b);
                var_javax_microedition_lcdui_Graphics_a = var_javax_microedition_lcdui_Image_b.GetGraphics();
            }
        }
    }

    public void void_b(int n, int n2)
    {
        n += g;
        if (var_boolean_a)
        {
            int n3 = n - (u << var_int_a);
            int n4 = n2 - (v << var_int_a);
            int n5 = Math.Min(s, ((y - w) << var_int_a) - n3);
            int n6 = Math.Min(t, ((z - x) << var_int_a) - n4);
            int n7 = s - n5;
            int n8 = t - n6;

            var gr = GMIDlet.var_javax_microedition_lcdui_Graphics_a;
            if (gr == null || var_javax_microedition_lcdui_Image_b == null) return;

            if (n5 > 0 && n6 > 0)
            {
                gr.SetClip(0, 0, n5, n6);
                gr.DrawImage(var_javax_microedition_lcdui_Image_b, -(n3 + (w << var_int_a)), -(n4 + (x << var_int_a)), 20);
            }
            if (n7 > 0 && n6 > 0)
            {
                gr.SetClip(n5, 0, n7, n6);
                gr.DrawImage(var_javax_microedition_lcdui_Image_b, n5, -(n4 + (x << var_int_a)), 20);
            }
            if (n5 > 0 && n8 > 0)
            {
                gr.SetClip(0, n6, n5, n8);
                gr.DrawImage(var_javax_microedition_lcdui_Image_b, -(n3 + (w << var_int_a)), n6, 20);
            }
            if (n7 > 0 && n8 > 0)
            {
                gr.SetClip(n5, n6, n7, n8);
                gr.DrawImage(var_javax_microedition_lcdui_Image_b, n5, n6, 20);
            }
            return;
        }

        u = n >> var_int_a;
        v = n2 >> var_int_a;
        if (u < o) u = o;
        if (v < p) v = p;
        if (u > q) u = q;
        if (v > r) v = r;

        int drawTilesX = EngineTweaks.Widescreen ? ((EngineTweaks.RenderWidth + (var_int_b >> 1) + 1) >> var_int_a) + 2 : y;
        int n9 = u + drawTilesX;
        int n10 = v + z;
        var_javax_microedition_lcdui_Graphics_a = GMIDlet.var_javax_microedition_lcdui_Graphics_a;
        w = 0;
        x = 0;
        int n11 = n - (u << var_int_a);
        int n12 = n2 - (v << var_int_a);
        if (GMIDlet.var_javax_microedition_lcdui_Graphics_a == null) return;

        GMIDlet.var_javax_microedition_lcdui_Graphics_a.Translate(-n11, -n12);
        for (int i = u; i < n9; ++i)
        {
            for (int j = v; j < n10; ++j)
            {
                c(i, j);
            }
        }
        GMIDlet.var_javax_microedition_lcdui_Graphics_a.Translate(n11, n12);
    }

    private void c(int n, int n2)
    {
        int col = EngineTweaks.Widescreen ? Math.Clamp(n, 0, m - 1) : int_a(n, m);
        int n3 = int_b(col, int_a(n2, this.n));
        int n4 = (var_byte_arr_c != null && n3 < var_byte_arr_c.Length) ? (var_byte_arr_c[n3] & 0xC0) : 0;
        if (!var_boolean_a && n3 == 0) return;

        int n5 = int_a(n - u + w, y) << var_int_a;
        int n6 = int_a(n2 - v + x, z) << var_int_a;
        if (var_boolean_a && var_javax_microedition_lcdui_Graphics_a != null)
        {
            var_javax_microedition_lcdui_Graphics_a.SetClip(n5, n6, var_int_b, var_int_b);
            var_javax_microedition_lcdui_Graphics_a.ClearRect(n5, n6, var_int_b, var_int_b);
        }
        if (n3 == 0) return;
        int n7 = n3 - 1;
        if (n3 > 2 && var_int_arr_b != null && n3 - 3 < var_int_arr_b.Length)
        {
            n7 = var_int_arr_b[n3 - 3] + 2;
        }

        int n8 = 0;
        switch (n4)
        {
            case 64: n8 = 2; break;
            case 128: n8 = 1; break;
            case 192: n8 = 3; break;
        }

        if (var_javax_microedition_lcdui_Graphics_a != null && var_javax_microedition_lcdui_Image_a != null && h > 0)
        {
            var_javax_microedition_lcdui_Graphics_a.SetClip(n5, n6, var_int_b, var_int_b);
            var_javax_microedition_lcdui_Graphics_a.DrawRegion(var_javax_microedition_lcdui_Image_a,
                (n7 % h) << var_int_a, (n7 / h) << var_int_a, var_int_b, var_int_b, n8, n5, n6, 20);
        }
    }

    public void a(int x1, int y1, int x2, int y2)
    {
        int totalCols = (x2 - x1) * m;
        int totalRows = (y2 - y1) * n;
        var_int_c = totalCols << var_int_a;
        d = totalRows << var_int_a;
        y = ((s + (var_int_b >> 1) + 1) >> var_int_a) + 1;
        z = ((t + (var_int_b >> 1) + 1) >> var_int_a) + 1;
        if (!EngineTweaks.Widescreen)
        {
            if (y > totalCols) y = totalCols;
            if (z > totalRows) z = totalRows;
        }
        o = x1 * m;
        p = y1 * m;
        q = x2 * m - y;
        r = y2 * n - z;
        if (EngineTweaks.Widescreen && q < o)
        {
            q = o;
        }
        u = -1073741824;
    }

    private bool boolean_b(int tx, int ty)
    {
        return tx >= o && tx < q + y && ty >= p && ty < r + z;
    }

    public void a(int tx, int ty, byte tileId)
    {
        if (ty != E)
        {
            if (var_short_arr_a == null || var_byte_arr_b == null) return;
            F = var_short_arr_a[ty];
            A = var_byte_arr_b[F++] & 0xFF;
            B = var_byte_arr_b[F++] & 0xFF;
            C = var_byte_arr_b[F++] & 0xFF;
            D = var_byte_arr_b[F++] & 0xFF;
            E = ty;
        }
        if (tx < A || tx >= A + C || var_byte_arr_b == null) return;
        var_byte_arr_b[F + tx - A] = tileId;
        void_d(tx, ty);
    }

    private void void_d(int n, int n2)
    {
        if (n >= u && n < u + y && n2 >= v && n2 < v + z)
        {
            c(n, n2);
        }
    }

    private int int_b(int tx, int ty)
    {
        if (ty != E)
        {
            if (var_short_arr_a == null || var_byte_arr_b == null) return 0;
            F = var_short_arr_a[ty];
            A = var_byte_arr_b[F++] & 0xFF;
            B = var_byte_arr_b[F++] & 0xFF;
            C = var_byte_arr_b[F++] & 0xFF;
            D = var_byte_arr_b[F++] & 0xFF;
            E = ty;
        }
        if (var_byte_arr_b == null) return 0;
        int tile = tx < A ? B : (tx >= A + C ? D : var_byte_arr_b[F + tx - A] & 0xFF);
        return tile;
    }

    public void void_a(Actor actor)
    {
        int topTile = actor.int_f() >> var_int_a;
        int bottomTile = (actor.var_int_e - 1) >> var_int_a;
        for (int ty = topTile; ty <= bottomTile; ++ty)
        {
            int left = actor.int_d();
            int leftTile = left >> var_int_a;
            if (a(leftTile, ty, 1))
            {
                int adjust = ((leftTile + 1) << var_int_a) - left;
                actor.var_int_d += adjust;
                actor.var_int_f = 0;
            }
            int right = actor.int_e();
            int rightTile = (right - 1) >> var_int_a;
            if (a(rightTile, ty, 1))
            {
                int adjust = right - (rightTile << var_int_a);
                actor.var_int_d -= adjust;
                actor.var_int_f = 0;
            }
        }
    }

    public bool a(Actor actor, bool testCeil, bool testFloor)
    {
        bool collided = false;
        int leftTile = actor.int_d() >> var_int_a;
        int rightTile = (actor.int_e() - 1) >> var_int_a;
        for (int tx = leftTile; tx <= rightTile; ++tx)
        {
            int bottom = actor.var_int_e;
            int bottomTile = (bottom - 1) >> var_int_a;
            if (testFloor && a(tx, bottomTile, actor.var_boolean_h ? 1 : 5) && actor.var_int_n <= (bottomTile << var_int_a))
            {
                int adjust = bottom - (bottomTile << var_int_a);
                actor.var_int_e -= adjust;
                actor.var_int_g = 0;
                collided = true;
            }
            int top = actor.int_f();
            int topTile = top >> var_int_a;
            if (testCeil && a(tx, topTile, 1))
            {
                int adjust = ((topTile + 1) << var_int_a) - top;
                actor.var_int_e += adjust;
                actor.var_int_g = 0;
                collided = true;
            }
        }
        return collided;
    }

    private bool a(int tx, int ty, int flag)
    {
        if (boolean_b(tx, ty))
        {
            e = tx % m;
            f = ty % n;
            if (var_byte_arr_c == null) return false;
            byte attr = var_byte_arr_c[int_b(e, f)];
            return (attr & flag) != 0;
        }
        return false;
    }

    public bool a(int x1, int y1, int x2, int y2, int flag)
    {
        int leftTile = x1 >> var_int_a;
        int rightTile = (x2 - 1) >> var_int_a;
        int topTile = y1 >> var_int_a;
        int bottomTile = (y2 - 1) >> var_int_a;
        for (int ty = topTile; ty <= bottomTile; ++ty)
        {
            for (int tx = leftTile; tx <= rightTile; ++tx)
            {
                if (!boolean_b(tx, ty) || var_byte_arr_c == null) continue;
                byte attr = var_byte_arr_c[int_b(tx, ty)];
                if ((attr & flag) != 0) return true;
            }
        }
        return false;
    }

    public void b(Actor actor)
    {
        int vx = actor.var_int_f;
        int targetX = vx;
        int tx;
        if (vx < 0)
        {
            targetX += actor.int_d();
            tx = targetX >> var_int_a;
        }
        else if (vx > 0)
        {
            targetX += actor.int_e();
            tx = (targetX - 1) >> var_int_a;
        }
        else
        {
            return;
        }
        int ty = actor.var_int_e >> var_int_a;
        if (!a(tx, ty, 5))
        {
            actor.var_int_f = 0;
        }
    }

    public bool boolean_a(Actor actor)
    {
        int tx = actor.var_int_d >> var_int_a;
        int ty = actor.var_int_e >> var_int_a;
        int count = 0;
        for (int i = tx - 2; i <= tx + 2; ++i)
        {
            count = a(i, ty, 5) ? count + 1 : 0;
            if (count > 2) return true;
        }
        return false;
    }

    public bool boolean_a(Actor actor, int dist)
    {
        int tx = actor.var_int_d >> var_int_a;
        int bottom = actor.var_int_e;
        int endY = (bottom - 1) >> var_int_a;
        int startY = (bottom - dist) >> var_int_a;
        for (int ty = startY + 1; ty <= endY; ++ty)
        {
            if (a(tx, ty, 4)) return true;
        }
        return false;
    }

    private bool b(Actor actor, int triggerIdx)
    {
        if (!actor.var_boolean_a || var_int_arr_a == null) return false;
        int desc = var_int_arr_a[triggerIdx];
        if (desc == -1) return false;

        int leftTile = actor.int_d() >> var_int_a;
        int rightTile = (actor.int_e() - 1) >> var_int_a;
        int topTile = actor.int_f() >> var_int_a;
        int bottomTile = (actor.var_int_e - 1) >> var_int_a;

        if (leftTile < I && rightTile >= G && topTile < J && bottomTile >= H)
        {
            if (((desc >> 25) & 1) == 0)
            {
                var_int_arr_a[triggerIdx] = -1;
            }
            Game.a((desc >> 18) & 0x7F, G, H);
            return true;
        }
        return false;
    }

    private void void_a(Actor actor, int triggerIdx)
    {
        if (!actor.var_boolean_a || var_int_arr_a == null) return;
        int desc = var_int_arr_a[triggerIdx];
        if (desc == -1) return;

        int leftTile = actor.int_g() >> var_int_a;
        int rightTile = (actor.int_h() - 1) >> var_int_a;
        int topTile = actor.int_i() >> var_int_a;
        int bottomTile = (actor.int_j() - 1) >> var_int_a;

        if (leftTile < I && rightTile >= G && topTile < J && bottomTile >= H)
        {
            if (((desc >> 25) & 1) == 0)
            {
                var_int_arr_a[triggerIdx] = -1;
            }
            Game.a((desc >> 18) & 0x7F, G, H);
        }
    }

    public void a(Actor player, Actor[]? enemies, Game game)
    {
        if (var_int_arr_a == null || var_byte_arr_a == null) return;

        for (int i = 0; i < var_int_arr_a.Length; ++i)
        {
            int desc = var_int_arr_a[i];
            if (desc == -1) continue;

            G = desc & 0x1FF;
            H = (desc >> 9) & 0x1FF;
            I = G + (var_byte_arr_a[i] >> 4);
            J = H + (var_byte_arr_a[i] & 0xF);

            int type = (desc >> 26) & 7;
            if (type == 3)
            {
                if (Game.F != 0 || player.var_a_a == null || !player.var_a_a.var_boolean_l) continue;
                void_a(player.var_a_a, i);
                continue;
            }
            if (type == 6)
            {
                if (Game.F != 3) continue;
                if (game.var_a_b != null && game.var_a_b.var_boolean_l)
                {
                    void_a(game.var_a_b, i);
                }
                if (game.var_a_c != null && game.var_a_c.var_boolean_l)
                {
                    void_a(game.var_a_c, i);
                }
                continue;
            }
            if (type == 4)
            {
                if (Game.F == 1 && (game.var_int_g & 0x80) != 0 && b(player, i))
                {
                    Game.var_boolean_k = false;
                }
            }
            else if (type == 5 && Game.F == 2 && (game.var_int_g & 0x80) != 0 && b(player, i))
            {
                Game.var_boolean_k = false;
            }
            if ((type == 0 || type == 2) && player.var_boolean_a)
            {
                b(player, i);
            }
            if (type != 1 && type != 2 || enemies == null) continue;
            for (int j = 0; j < enemies.Length; ++j)
            {
                Actor enemy = enemies[j];
                if (!enemy.var_boolean_a) continue;
                b(enemy, i);
            }
        }
    }
}
