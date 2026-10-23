using System;
using System.IO;
using PR.Core.Graphics;

namespace PR.Core;

/// <summary>
/// Core gameplay engine mirroring f.java.
/// Handles level initialization, 70ms tick loop, player controls, enemy AI, combat, camera, and scripts.
/// </summary>
public sealed class Game
{
    public static int var_int_a; // tick counter
    public static int var_int_b;
    public static int var_int_c = App.DefaultCanvasWidth;
    public static int var_int_d = App.DefaultCanvasHeight;
    public static int var_int_e = var_int_c >> 1;
    public static int var_int_f = var_int_d >> 1;
    public static Game? var_f_a;

    public static int G = 1; // Current level (1..12 or 13 for survival)
    public static int F = 0; // Current Ranger (0=Red, 1=Yellow, 2=Blue, 3=Pink, 4=Green)
    public static int H = 1; // Highest unlocked level
    public static int D = 13; // Survival level index
    public static int I;
    public static int J;
    public static int K = 8; // Max HP
    public static int L = 7; // Max magic blocks
    public static int N;
    public static int O;
    public static int P;

    public static JavaRandom var_java_util_Random_a = new JavaRandom();
    public static short[][] var_short_arr_arr_a = Array.Empty<short[]>();
    public static readonly bool[] var_boolean_arr_a = new bool[] {
        false, false, false, false, true, true, false, false, true, true,
        false, false, true, false, false, true, true, false, false, false, true
    };
    public static readonly int[] var_int_arr_b = new int[] {
        -12, -12, -10, -10, -9, -7, -6, -4, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10
    };
    public static readonly int[] var_int_arr_g = new int[] { 4, 15, 5, 16 };
    public static readonly int[] var_int_arr_h = new int[] { 4, 5, 9 };

    public static Actor? var_a_a;
    public static Actor? var_a_d;
    public static Actor? var_a_f; // Active player actor

    public static byte[]? var_byte_arr_a;
    public static byte[]? var_byte_arr_b;
    public static byte[]? var_byte_arr_c;
    public static byte[]? var_byte_arr_d;
    public static byte[]? var_byte_arr_e;
    public static byte[]? var_byte_arr_f;
    public static byte[]? var_byte_arr_g;
    public static int[]? var_int_arr_a;
    public static int[]? var_int_arr_c;

    public static sbyte var_byte_a;
    public static bool var_boolean_e; // Survival mode active
    public static bool var_boolean_f;
    public static bool var_boolean_g;
    public static bool var_boolean_h;
    public static bool var_boolean_i;
    public static bool var_boolean_j; // God mode / invincibility
    public static bool var_boolean_k;

    public static int S = -1;
    public static int T = -1;
    public static int U = -1;
    public static int X = -1;
    public static int Y = -1;
    public static int Z = 10;
    public static int aK = TileMap.var_int_b * 8;

    public static int w;
    public static int x;
    public static int y;
    public static int z;
    public static int A;
    public static int B;
    public static int C;
    public static int E;

    public static int var_int_i;
    public static int var_int_j;
    public static int var_int_k;
    public static int var_int_l;
    public static int var_int_m;
    public static int var_int_n;
    public static int var_int_o;
    public static int var_int_p;
    public static int var_int_q;
    public static int var_int_r;
    public static int var_int_s;
    public static int var_int_t;
    public static int var_int_u;
    public static int var_int_v;

    // Instance fields
    public readonly App var_g_a;
    public bool var_boolean_l;
    public bool var_boolean_m;
    public bool var_boolean_a;
    public int var_int_g; // Held input bitmask
    public int Q;         // Pending input command
    public int R = -1;
    public bool var_boolean_n;
    public bool var_boolean_o;
    public bool var_boolean_p = false; // Level completed
    public bool[] var_boolean_arr_b = new bool[9];
    public bool var_boolean_q = false;
    public bool var_boolean_b;
    public int var_int_h = -1;
    public int V = var_int_c / 4;
    public int W = -var_int_d / 4;

    public Actor? var_a_g; // Spark actor
    public Actor? var_a_h;
    public Actor[] var_a_arr_c = new Actor[4];
    public Actor? var_a_b;
    public Actor? var_a_c;
    public Actor? var_a_e; // Magic aura actor
    public Actor? var_a_i; // Boss actor
    public Actor? var_a_j;

    public bool var_boolean_r = true; // Camera tracking
    public bool var_boolean_s;
    public bool var_boolean_t;
    public bool var_boolean_u;
    public bool var_boolean_v;
    public bool var_boolean_c;
    public bool var_boolean_d;

    public int aa;
    public int ab;
    public int ac; // Smooth camX
    public int ad; // Smooth camY
    public int ae;
    public int af;
    public int ag;
    public int ah;
    public int ai; // Layer count
    public int aj; // Collision layer index
    public int ak;
    public int al;
    public int am; // Max magic
    public int an; // Current magic
    public int ao = -1;
    public int ap = -1;
    public int aq = -1;
    public int ar = -1;
    public int @as = -1;
    public int at = -1;
    public int au;
    public int av;
    public int aw;
    public int ax;
    public int ay;
    public int az;
    public int aA = -1;
    public int aB = -1;
    public int aC;
    public int aD; // Flash timer
    public int aE = -1;
    public int aF;
    public int aG; // Shake timer
    public int aH;
    public int aI;
    public int aJ;
    public int aM;
    public int aN;
    public int aO; // Survival wave counter
    public int M;  // Projectiles count

    public TileMap[]? var_c_arr_a;
    public Actor[]? var_a_arr_d;
    public Actor[]? var_a_arr_a;
    public Actor[]? var_a_arr_e;
    public Actor[]? var_a_arr_f;
    public Actor[]? var_a_arr_g; // Enemies array
    public Actor[]? var_a_arr_h;
    public Actor[]? var_a_arr_i;
    public Actor[] var_a_arr_b = new Actor[25];
    public int[]? var_int_arr_d;
    public int[]? var_int_arr_e;
    public int[]? var_int_arr_f;

    public string[]? var_java_lang_String_arr_a;

    public Game(App app)
    {
        var_g_a = app;
        var_f_a = this;
        for (int i = 0; i < var_a_arr_b.Length; ++i)
        {
            var_a_arr_b[i] = new Actor();
        }
    }

    public Actor a(Actor parent, int ox, int oy, int vx, bool bl, int animIndex)
    {
        if (var_a_a == null) throw new InvalidOperationException("var_a_a not initialized.");
        Actor proj = new Actor(var_a_a);
        proj.Interpolates = true;
        proj.E = animIndex;
        parent.var_a_a = proj;
        proj.var_a_b = parent;
        proj.void_h();
        proj.a(animIndex, false);
        proj.void_j();
        proj.void_g();
        proj.p = (short)ox;
        proj.q = (short)oy;
        proj.var_int_h = vx;
        proj.var_boolean_m = bl;
        proj.void_a();
        if (bl)
        {
            proj.var_boolean_a = true;
        }
        var_a_arr_b[M++] = proj;
        return proj;
    }

    public void void_a(Actor item)
    {
        for (int i = 0; i < var_a_arr_c.Length; ++i)
        {
            Actor a3 = var_a_arr_c[i];
            if (a3.x != -1) continue;
            a3.a(item.E, false);
            a3.var_boolean_a = true;
            a3.var_int_d = item.var_int_d;
            a3.var_int_e = item.var_int_e;
            return;
        }
    }

    public void void_a()
    {
        for (int i = 0; i < var_short_arr_arr_a.Length; ++i)
        {
            short[] seq = var_short_arr_arr_a[i];
            seq[0] = 0;
            seq[1] = 0;
            seq[2] = 0;
        }
        var_boolean_l = true;
        if (var_a_f != null)
        {
            ac = var_a_f.var_int_d + V - 20;
            ad = var_a_f.var_int_e + W - 20;
            CamInterpState.Snap(ac, ad);
            RenderCamX = ac;
            RenderCamY = ad;
            var_a_f.InterpState.Snap(var_a_f.var_int_d, var_a_f.var_int_e);
            var_a_f.RenderX = var_a_f.var_int_d;
            var_a_f.RenderY = var_a_f.var_int_e;
        }
        O = 0;
        P = 0;
        N = 0;
        E = 0;
    }

    public void void_b()
    {
        var_int_g = 0;
        var_boolean_l = false;
    }

    public void void_c()
    {
        var_int_g = 0;
        var_boolean_l = true;
    }

    private string? var_java_lang_String_b_hud;
    private int aL = -1;

    // Camera sub-frame interpolation for 60fps (engine tweak)
    public ActorState CamInterpState { get; } = new();
    public int RenderCamX;
    public int RenderCamY;

    public void UpdateInterpolation(float alpha)
    {
        if (!EngineTweaks.Smooth60Fps) return;

        var (cx, cy) = CamInterpState.GetInterpolated(alpha);
        RenderCamX = cx;
        RenderCamY = cy;

        if (var_a_f != null)
        {
            var (px, py) = var_a_f.InterpState.GetInterpolated(alpha);
            var_a_f.RenderX = px;
            var_a_f.RenderY = py;
        }

        if (var_a_arr_g != null)
        {
            foreach (var enemy in var_a_arr_g)
            {
                if (enemy != null && enemy.var_boolean_a)
                {
                    var (ex, ey) = enemy.InterpState.GetInterpolated(alpha);
                    enemy.RenderX = ex;
                    enemy.RenderY = ey;
                }
            }
        }

        if (var_a_arr_b != null)
        {
            for (int i = 0; i < M; i++)
            {
                var proj = var_a_arr_b[i];
                if (proj != null && proj.var_boolean_a)
                {
                    var (bx, by) = proj.InterpState.GetInterpolated(alpha);
                    proj.RenderX = bx;
                    proj.RenderY = by;
                }
            }
        }

        if (var_a_arr_h != null)
        {
            foreach (var plat in var_a_arr_h)
            {
                if (plat != null && plat.var_boolean_a)
                {
                    var (lx, ly) = plat.InterpState.GetInterpolated(alpha);
                    plat.RenderX = lx;
                    plat.RenderY = ly;
                }
            }
        }
    }

    public void d()
    {
        if (var_boolean_l || var_a_f == null) return;
        var gr = GMIDlet.var_javax_microedition_lcdui_Graphics_a;
        if (gr == null) return;

        int renderW = EngineTweaks.RenderWidth;
        int camX = EngineTweaks.Smooth60Fps ? RenderCamX : ac;
        int camY = EngineTweaks.Smooth60Fps ? RenderCamY : ad;
        if (EngineTweaks.Widescreen)
        {
            int minCamX = renderW >> 1;
            int maxCamX = var_c_arr_a != null && aj < var_c_arr_a.Length
                ? Math.Max(minCamX, var_c_arr_a[aj].var_int_c - minCamX)
                : minCamX;
            camX = Math.Clamp(camX, minCamX, maxCamX);
        }
        int n4 = camX - (renderW >> 1);
        int n5 = camY - (var_int_d >> 1);

        gr.Translate(-gr.GetTranslateX(), -gr.GetTranslateY() + var_int_b);

        if (aG > 0 && var_byte_arr_g != null)
        {
            int idx = aG * 2;
            if (idx >= 0 && idx + 1 < var_byte_arr_g.Length)
            {
                n4 += (sbyte)var_byte_arr_g[idx];
                n5 += (sbyte)var_byte_arr_g[idx + 1];
            }
        }

        if (EngineTweaks.Widescreen)
        {
            gr.SetColor(0x0C0E14);
            gr.FillRect(0, 0, renderW, var_int_d);
        }

        if (G == D)
        {
            gr.SetColor(0);
            gr.FillRect(0, 21, renderW, 35);
        }

        if (aD > 0)
        {
            gr.SetColor((aD & 1) == 0 ? 0xFF0000 : 0xFFFFFF);
            gr.FillRect(0, 0, renderW, var_int_d);
        }
        else if (var_c_arr_a != null)
        {
            for (int n3 = 0; n3 <= aj && n3 < var_c_arr_a.Length; ++n3)
            {
                int n2 = n4 * var_c_arr_a[n3].i / 10 - var_c_arr_a[n3].k;
                int n = n5 * var_c_arr_a[n3].j / 10 - var_c_arr_a[n3].l;
                var_c_arr_a[n3].void_b(n2, n);
            }
        }

        gr.SetClip(0, 0, renderW, var_int_d);
        if (!var_boolean_h)
        {
            Actor.a(var_a_arr_a, -n4, -n5, 2);
        }
        Actor.a(var_a_arr_i, -n4, -n5, 0);
        if (!var_boolean_i)
        {
            Actor.a(var_a_arr_g, -n4, -n5, 1);
        }
        Actor.a(var_a_arr_h, -n4, -n5, 0);

        if (var_a_arr_b != null)
        {
            for (int n3 = 0; n3 < M; ++n3)
            {
                var_a_arr_b[n3]?.a(-n4, -n5);
            }
        }

        if (var_a_e != null && var_a_e.var_boolean_a && ((E > 8 && E < 92) || (E & 1) == 0))
        {
            var_a_e.a(-n4, -n5);
        }

        if (var_a_f != null)
        {
            var_a_f.a(-n4, -n5);
        }

        if (var_a_g != null && var_a_g.x != -1)
        {
            if (var_a_h != null)
            {
                var_a_g.var_int_d = var_a_h.var_int_d;
                var_a_g.var_int_e = (var_a_h.var_int_e + var_a_h.int_f()) >> 1;
                var_a_g.var_int_d += (var_java_util_Random_a.NextInt() & 0xF) - 7;
                var_a_g.var_int_e += (var_java_util_Random_a.NextInt() & 0xF) - 7;
            }
            var_a_g.a(-n4, -n5);
        }

        Actor.a(var_a_arr_c, -n4, -n5, 0);
        Actor.a(var_a_arr_e, -n4, -n5, 0);

        if (aD == 0 && var_c_arr_a != null)
        {
            for (int n3 = aj + 1; n3 < ai && n3 < var_c_arr_a.Length; ++n3)
            {
                int n2 = n4 * var_c_arr_a[n3].i / 10 - var_c_arr_a[n3].k;
                int n = n5 * var_c_arr_a[n3].j / 10 - var_c_arr_a[n3].l;
                var_c_arr_a[n3].void_b(n2, n);
            }
        }

        gr.SetClip(0, 0, renderW, var_int_d);
        if (!var_boolean_h)
        {
            Actor.a(var_a_arr_d, -n4, -n5, 2);
        }
        Actor.a(var_a_arr_f, -n4, -n5, 0);

        m();

        gr.Translate(-gr.GetTranslateX(), -gr.GetTranslateY() + var_int_b);
        gr.SetClip(0, 0, renderW, var_int_d);

        if (au != 0)
        {
            gr.SetColor(0);
            gr.FillRect(0, 0, renderW, au);
            gr.FillRect(0, var_int_d - au, renderW, au);
            gr.SetColor(0xFFFFFF);
            gr.DrawLine(0, au, renderW, au);
            gr.DrawLine(0, var_int_d - au, renderW, var_int_d - au);
        }

        if (ax > 0 && var_java_lang_String_arr_a != null)
        {
            --ax;
            int n3 = GMIDlet.c(0);
            int n2 = var_java_lang_String_arr_a.Length * n3 + 6;
            int n = var_a_j != null ? var_a_j.var_int_d - n4 : (renderW >> 1);
            int n6 = var_a_j != null ? var_a_j.var_int_e - n5 - 55 - n2 : var_int_f - (n2 >> 1);
            if ((n -= aw >> 1) > renderW - aw) n = renderW - aw;
            if (n < 0) n = 0;
            if (n6 < 20) n6 = 20;

            gr.SetColor(var_a_j != null && var_a_j.s == var_a_f?.s && var_int_arr_a != null ? var_int_arr_a[var_a_j.w] : 0);
            gr.FillRect(n, n6, aw, n2);
            gr.SetColor(3551021);
            gr.DrawRect(n, n6, aw - 1, n2 - 1);
            gr.DrawRect(n + 1, n6 + 1, aw - 2, n2 - 2);
            gr.SetColor(13087385);
            gr.DrawRect(n, n6, aw - 2, n2 - 2);
            n += 3;
            n6 += 5;
            for (int i = 0; i < var_java_lang_String_arr_a.Length; ++i)
            {
                GMIDlet.a(0, var_java_lang_String_arr_a[i], n, n6, 20);
                n6 += n3;
            }
            if (var_boolean_e && ax == 0 && var_boolean_o)
            {
                var_boolean_t = false;
                var_boolean_o = false;
                this.n();
            }
        }

        if (R != -1 || var_boolean_o)
        {
            if (App.var_java_lang_String_arr_a != null && App.var_java_lang_String_arr_a.Length > 41)
            {
                GMIDlet.a(1, App.var_java_lang_String_arr_a[41], renderW - 1, var_int_d, 40);
            }
        }
        GMIDlet.a(26, 2, var_int_d - 2, 36);
    }

    private void m()
    {
        var gr = GMIDlet.var_javax_microedition_lcdui_Graphics_a;
        if (gr == null) return;

        int renderW = EngineTweaks.RenderWidth;
        gr.Translate(-gr.GetTranslateX(), -gr.GetTranslateY());
        gr.SetClip(0, 0, renderW, var_int_d + var_int_b);

        if (var_a_i != null && var_a_i.var_boolean_j && GMIDlet.var_javax_microedition_lcdui_Image_arr_a != null)
        {
            var bossFrame = GMIDlet.var_javax_microedition_lcdui_Image_arr_a[14];
            int bossX = EngineTweaks.Widescreen ? (renderW - 240) / 2 : 0;
            if (bossFrame != null) gr.DrawImage(bossFrame, bossX, 24, 20);
            a(bossX + 10, 26, 84, var_a_i.z, var_a_i.A, 16774248, 15885602);
        }

        if (GMIDlet.var_javax_microedition_lcdui_Image_arr_a != null)
        {
            Image? image = GMIDlet.var_javax_microedition_lcdui_Image_arr_a[13];
            if (image != null) gr.DrawImage(image, 0, 0, 20);
        }

        int col1 = 15539236;
        int col2 = 15565446;
        if (var_boolean_v)
        {
            col2 = 0xFFFFFF;
            col1 = 0xFFFFFF;
            var_boolean_v = false;
        }

        if (var_a_f != null)
        {
            a(3, 2, 57, var_a_f.z, var_a_f.A, col1, col2);
        }
        a(3, 10, 57, an, am, an < 320 ? 0xEE66EE : 44527, 8311023);

        if (var_a_f != null)
        {
            a(72, 2, var_a_f.z, var_a_f.A);
        }
        a(72, 10, an / 64, am / 64);

        if (var_java_lang_String_b_hud == null || aL != App.var_int_g)
        {
            aL = App.var_int_g;
            string prefix = (App.var_java_lang_String_arr_a != null && App.var_java_lang_String_arr_a.Length > 32)
                ? App.var_java_lang_String_arr_a[32] : "SCORE";
            var_java_lang_String_b_hud = prefix + ":" + App.var_int_g;
        }

        if (EngineTweaks.Widescreen)
        {
            GMIDlet.a(1, var_java_lang_String_b_hud, renderW >> 1, 5, 17);
        }
        else
        {
            GMIDlet.a(1, var_java_lang_String_b_hud, 120, 5, 20);
        }
    }

    private void a(int n, int n2, int n3, int n4)
    {
        GMIDlet.a(12, n3, n3 > 9 ? 2 : 1, n, n2, 2, 11, false);
        GMIDlet.a(12, n + 3, n2, 11, 1, 10, 0);
        GMIDlet.a(12, n4, n4 > 9 ? 2 : 1, n + (n4 > 9 ? 15 : 11), n2, 2, 11, false);
    }

    private void a(int n, int n2, int n3, int n4, int n5, int n6, int n7)
    {
        var gr = GMIDlet.var_javax_microedition_lcdui_Graphics_a;
        if (gr == null || n5 <= 0) return;
        int n8 = n3;
        int n9 = n8 * n4 / n5;
        if (n9 < 1) n9 = 1;
        else if (n9 > n8) n9 = n8;
        if (n4 <= 0) n9 = 0;

        n9 += n;
        gr.SetColor(n6);
        gr.DrawLine(n, n2 + 1, n9, n2 + 1);
        gr.DrawLine(n, n2 + 2, n9, n2 + 2);
        gr.DrawLine(n, n2 + 3, n9, n2 + 3);
        gr.DrawLine(n, n2 + 4, n9, n2 + 4);
        gr.DrawLine(n, n2 + 5, n9, n2 + 5);
        gr.SetColor(n7);
        gr.DrawLine(n, n2, n9, n2);
        gr.DrawLine(n, n2 + 6, n9, n2 + 6);
    }

    public void void_a(int key)
    {
        if (key == -7)
        {
            if (R != -1) var_boolean_n = true;
            if (var_boolean_e && var_boolean_o)
            {
                var_boolean_o = false;
                var_boolean_t = false;
                n();
            }
        }
        if (var_boolean_l || var_boolean_m) return;

        Q = int_a(key);
        if (aq == Q)
        {
            var_int_h = -1;
            aq = -1;
        }
        var_int_g |= Q;
    }

    public void b(int key)
    {
        if (var_boolean_l || var_boolean_m) return;
        int action = int_a(key);
        var_int_g &= ~action;
    }

    public int int_a(int key)
    {
        switch (key)
        {
            case 49:
                return var_boolean_arr_b[1] ? 0 : 16;
            case 51:
                return var_boolean_arr_b[3] ? 0 : 32;
            case -1:
            case 50:
                return var_boolean_arr_b[2] ? 0 : 4;
            case -2:
            case 56:
                return var_boolean_arr_b[8] ? 0 : 8;
            case -4:
            case 54:
                return var_boolean_arr_b[6] ? 0 : 1;
            case -3:
            case 52:
                return var_boolean_arr_b[4] ? 0 : 2;
            case -5:
            case 53:
                return var_boolean_arr_b[5] ? 0 : 64;
            case 48:
                return var_boolean_arr_b[0] ? 0 : 128;
        }
        return 0;
    }

    public void e()
    {
        PR.Core.Audio.SoundEngine.Stop();
        var_f_a = this;
        var_g_a.serviceRepaints();

        if (var_a_a == null)
        {
            var_a_a = new Actor(App.q);
            var_a_a.void_g();
            var_a_a.a(0, false);
        }
        for (int i = 0; i < var_a_arr_c.Length; ++i)
        {
            var_a_arr_c[i] = new Actor(App.r);
        }
        if (var_a_d == null)
        {
            var_a_d = new Actor(App.t);
            var_a_d.a(0, false);
        }

        var_boolean_f = (I & (1 << G)) != 0;

        using (var stream = var_g_a.ResourceProvider.OpenResource("l" + G))
        {
            if (stream == null) throw new FileNotFoundException("Level file l" + G + " not found.");
            using var dis = new DataReader(stream);

            var_g_a.a(0, 6);
            sbyte blockId = dis.ReadByte();
            sbyte palId = dis.ReadByte();
            aj = dis.ReadByte();

            int eventCount = dis.ReadShort();
            while (--eventCount >= 0)
            {
                switch (dis.ReadByte())
                {
                    case 0:
                        S = dis.ReadByte();
                        break;
                }
            }

            var_g_a.a(1, 6);
            int actorCount = dis.ReadShort();
            var_a_arr_d = new Actor[actorCount];
            for (int i = 0; i < actorCount; ++i)
            {
                Actor a2 = new Actor();
                a2.a(dis, false);
                var_a_arr_d[i] = a2;
            }

            var_g_a.a(2, 6);
            an = am = L * 64;
            dis.ReadShort();

            var_a_f = a_a(dis);
            var_a_f.Interpolates = true;
            var_a_f.w = F;
            var_a_f.d(F);
            var_a_f.t = 0;
            var_a_f.a(1, false);
            var_a_f.var_int_c = 1;
            var_a_f.InterpState.Snap(var_a_f.var_int_d, var_a_f.var_int_e);
            var_a_f.RenderX = var_a_f.var_int_d;
            var_a_f.RenderY = var_a_f.var_int_e;

            if (var_a_f.s == var_int_n)
            {
                a(var_a_f, -20, -29, 8, false, 5);
                var_a_g = new Actor(App.v);
            }
            else
            {
                var_a_f.z = var_a_f.A = K;
                b(var_a_f);
                var_a_g = new Actor(App.s);
            }

            var_g_a.a(3, 6);
            if (var_boolean_e)
            {
                a(3, 23, true);
                S = -1;
                var_a_arr_i = Array.Empty<Actor>();
                var_a_arr_a = var_a_arr_i;
                var_a_arr_f = var_a_arr_i;
                var_a_arr_e = var_a_arr_i;
                var_a_arr_h = var_a_arr_i;
                var_short_arr_arr_a = Array.Empty<short[]>();

                if (var_byte_arr_b != null)
                {
                    var_a_arr_g = new Actor[var_byte_arr_b.Length];
                    for (int i = 0; i < var_a_arr_g.Length; ++i)
                    {
                        Actor a2 = App.a_a(var_byte_arr_b[i]);
                        a2.Interpolates = true;
                        c(a2);
                        a2.var_boolean_k = false;
                        a2.d(-1);
                        var_a_arr_g[i] = a2;
                    }
                }
                var_boolean_o = true;
                var_boolean_t = true;
            }
            else
            {
                b(dis);
            }

            var_g_a.a(4, 6);
            if (var_a_arr_g != null)
            {
                var_int_arr_e = new int[var_a_arr_g.Length];
            }

            var_int_c = EngineTweaks.Widescreen ? EngineTweaks.RenderWidth : App.var_int_a;
            var_int_d = App.var_int_b;
            var_int_e = var_int_c >> 1;
            var_int_f = var_int_d >> 1;

            // Load block file /b<blockId>
            using (var bStream = var_g_a.ResourceProvider.OpenResource("b" + blockId))
            {
                if (bStream == null) throw new FileNotFoundException("Block file b" + blockId + " not found.");
                using var bDis = new DataReader(bStream);
                ai = bDis.ReadByte();
                var_c_arr_a = new TileMap[ai];
                for (int i = 0; i < ai; ++i)
                {
                    var_c_arr_a[i] = new TileMap(var_int_c, var_int_d);
                    var_c_arr_a[i].a(bDis);
                    var_c_arr_a[i].void_a(var_int_c, var_int_d);
                    var_c_arr_a[i].a(0, 0, 1, 1);
                }
            }

            var_g_a.a(5, 6);
            X = var_c_arr_a[0].var_byte_a;
            Y = palId;
            if (GMIDlet.var_javax_microedition_lcdui_Image_arr_a != null)
            {
                GMIDlet.var_javax_microedition_lcdui_Image_arr_a[4] = null;
                using (var bgStream = var_g_a.ResourceProvider.OpenResource("bg" + X))
                {
                    if (bgStream != null)
                    {
                        using var bgDis = new DataReader(bgStream);
                        GMIDlet.a(bgDis, GMIDlet.var_javax_microedition_lcdui_Image_arr_a, GMIDlet.var_short_arr_a!, 4, Y, false);
                    }
                }
                TileMap.var_javax_microedition_lcdui_Image_a = GMIDlet.var_javax_microedition_lcdui_Image_arr_a[4];
                if (TileMap.var_javax_microedition_lcdui_Image_a != null)
                {
                    TileMap.h = TileMap.var_javax_microedition_lcdui_Image_a.Width / TileMap.var_int_b;
                }
            }
            var_g_a.a(6, 6);

            for (int i = 0; i < var_a_arr_d.Length; ++i)
            {
                Actor a2 = var_a_arr_d[i];
                a2.void_h();
                a2.d(-1);
                a2.void_k();
            }

            void_a();
            void_b();
            var_g_a.repaint();

            var_int_a = 0;
            var_boolean_a = false;
            var_boolean_m = true;
            ae = var_int_c >> 1;
            af = var_int_d >> 1;
            ag = var_c_arr_a[aj].var_int_c - (var_int_c >> 1);
            ah = var_c_arr_a[aj].d - (var_int_d >> 1);
            if (ag < ae) ag = ae;
        }
    }

    private void b(Actor actor)
    {
        if (actor.s != var_int_v) return;
        switch (actor.w)
        {
            case 0:
                a(actor, -25, -32, 8, false, 2);
                break;
            case 2:
                if (var_a_e == null)
                {
                    Actor aura = new Actor(actor);
                    aura.void_h();
                    aura.w = 2;
                    aura.var_boolean_a = false;
                    aura.void_g();
                    aura.t = 0;
                    var_a_e = aura;
                }
                break;
            case 3:
                Actor w1 = a(actor, -20, -29, 8, false, 3);
                Actor w2 = a(actor, 20, -30, -8, false, 3);
                if (var_a_b == null)
                {
                    var_a_b = w1;
                    var_a_c = w2;
                }
                break;
        }
    }

    private void c(Actor actor)
    {
        if ((actor.u & 0x80) != 0 && var_a_i == null)
        {
            var_a_i = actor;
        }
        if ((actor.u & 0x40) != 0)
        {
            a(actor, -18, -41, 8, false, 1);
        }
    }

    private void b(DataReader dis)
    {
        int enemyCount = dis.ReadShort();
        var_a_arr_g = new Actor[enemyCount];
        var_a_i = null;
        for (int i = 0; i < enemyCount; ++i)
        {
            Actor a2 = a_a(dis);
            a2.Interpolates = true;
            var_a_arr_g[i] = a2;
            a2.z += var_byte_a;
            a2.A += var_byte_a;
            a2.d(a2.w);
            c(a2);
            b(a2);
            if (a2.s == var_int_l)
            {
                a(a2, -14, -34, w, false, 4);
            }
            else if (a2.s == var_int_k)
            {
                a(a2, -14, -44, w, false, 5);
            }
        }

        int trigCount = dis.ReadShort();
        var_a_arr_e = new Actor[trigCount];
        var_int_arr_d = new int[trigCount];
        for (int i = 0; i < trigCount; ++i)
        {
            Actor a2 = new Actor();
            a2.a(dis, true);
            a2.void_h();
            a2.d(-1);
            a2.void_k();
            a2.a(0, false);
            var_a_arr_e[i] = a2;
            var_int_arr_d[i] = -1;
            if (i < 3 && a2.s == var_int_o && var_byte_arr_e != null)
            {
                a2.var_boolean_a = (var_byte_arr_e[G] & (1 << i)) == 0;
            }
        }

        int platCount = dis.ReadShort();
        var_a_arr_h = new Actor[platCount];
        var_int_arr_f = new int[platCount];
        for (int i = 0; i < platCount; ++i)
        {
            var_int_arr_f[i] = (short)(dis.ReadShort() * TileMap.var_int_b);
            bool bl = dis.ReadBoolean();
            sbyte spd = dis.ReadByte();
            dis.ReadShort();
            Actor a3 = new Actor();
            a3.Interpolates = true;
            a3.a(dis, true);
            a3.void_g();
            a3.void_h();
            a3.var_boolean_a = true;
            a3.d(-1);
            a3.void_k();
            a3.var_int_n = a3.var_int_e;
            a3.a(0, false);
            a3.var_byte_d = spd;
            if (a3.q < var_int_arr_f[i])
            {
                int tmp = a3.q;
                a3.q = var_int_arr_f[i];
                var_int_arr_f[i] = tmp;
            }
            a3.var_boolean_j = bl;
            var_a_arr_h[i] = a3;
        }

        int shootCount = dis.ReadShort();
        var_a_arr_i = new Actor[shootCount];
        for (int i = 0; i < shootCount; ++i)
        {
            Actor a4 = new Actor();
            sbyte shootSpeed = dis.ReadByte();
            sbyte shootInterval = dis.ReadByte();
            dis.ReadShort();
            a4.a(dis, true);
            a4.void_h();
            a4.d(-1);
            a4.void_k();
            var_a_arr_i[i] = a4;
            Actor proj = a(a4, -10, -15, shootSpeed, false, 0);
            proj.H = shootInterval;
            proj.G = shootInterval;
        }

        int decoCount = dis.ReadShort();
        var_a_arr_a = new Actor[decoCount];
        for (int i = 0; i < decoCount; ++i)
        {
            Actor a5 = new Actor();
            a5.a(dis, false);
            var_a_arr_a[i] = a5;
            a5.void_h();
            a5.d(-1);
            a5.void_k();
            if (a5.s == var_int_p && i == decoCount - 1 && var_byte_arr_e != null)
            {
                a5.a((var_byte_arr_e[G] & 8) == 0 ? 0 : 2, false);
            }
        }

        int accCount = dis.ReadShort();
        var_a_arr_f = new Actor[accCount];
        for (int i = 0; i < accCount; ++i)
        {
            Actor a6 = new Actor();
            a6.a(dis, false);
            a6.void_h();
            a6.d(-1);
            a6.void_k();
            var_a_arr_f[i] = a6;
        }

        int scriptCount = dis.ReadShort();
        var_short_arr_arr_a = new short[scriptCount][];
        for (int i = 0; i < scriptCount; ++i)
        {
            short[] seq = new short[dis.ReadShort() / 2 + 5];
            for (int j = 5; j < seq.Length; ++j)
            {
                seq[j] = dis.ReadShort();
            }
            var_short_arr_arr_a[i] = seq;
        }
    }

    private Actor a_a(DataReader dis)
    {
        Actor actor = App.a_a(dis.ReadByte());
        actor.p = (short)(dis.ReadShort() * TileMap.var_int_b);
        actor.q = (short)(dis.ReadShort() * TileMap.var_int_b);
        int propCount = dis.ReadShort();
        while (--propCount >= 0)
        {
            sbyte tag = dis.ReadByte();
            switch (tag)
            {
                case 0:
                    actor.w = dis.ReadByte();
                    break;
                case 1:
                    actor.var_boolean_k = dis.ReadBoolean();
                    break;
                case 2:
                    actor.u = dis.ReadShort();
                    break;
                case 3:
                    actor.var_boolean_a = dis.ReadBoolean();
                    break;
                case 5:
                    w = dis.ReadByte();
                    break;
                case 6:
                    x = dis.ReadByte();
                    break;
                case 7:
                    y = dis.ReadByte();
                    break;
                case 8:
                    z = dis.ReadByte();
                    break;
                case 9:
                    A = dis.ReadByte();
                    break;
                case 10:
                    B = dis.ReadByte();
                    break;
                case 11:
                    C = dis.ReadByte();
                    break;
            }
        }
        actor.void_h();
        actor.void_k();
        return actor;
    }

    public bool boolean_b()
    {
        if (!var_boolean_a && var_int_a > 1)
        {
            var_boolean_a = true;
            var_boolean_m = false;
            var_g_a.a(null);
            var_g_a.serviceRepaints();
            PR.Core.Audio.SoundEngine.Play(1, var_int_a);
            if (S >= 0)
            {
                a(S, 0, 0);
            }
        }

        ++var_int_a;
        if (App.var_int_g > 9999999)
        {
            App.var_int_g = 9999999;
        }

        if (!var_boolean_l && var_a_f != null && var_c_arr_a != null)
        {
            if (av != au)
            {
                au += av > au ? 1 : -1;
            }
            if (aD > 0)
            {
                --aD;
            }
            if (aG > 0)
            {
                --aG;
            }
            if (EngineTweaks.GodMode && var_a_f != null)
            {
                var_a_f.z = var_a_f.A;
            }
            if (EngineTweaks.InfiniteMagic)
            {
                an = am;
            }
            if (at == 0 && var_int_h != -1)
            {
                at = -1;
                var_int_h = -1;
            }

            for (int i = 0; i < var_short_arr_arr_a.Length; ++i)
            {
                c(i);
            }

            if (R != -1)
            {
                if (a(var_short_arr_arr_a[R]))
                {
                    if (var_boolean_n)
                    {
                        ax = 0;
                        var_short_arr_arr_a[R][0] = 0;
                        R = -1;
                        var_boolean_n = false;
                    }
                }
                else
                {
                    R = -1;
                    var_boolean_n = false;
                }
            }

            if (var_a_arr_h != null && var_int_arr_f != null)
            {
                for (int i = 0; i < var_a_arr_h.Length; ++i)
                {
                    if (!var_a_arr_h[i].var_boolean_j) continue;
                    var_a_arr_h[i].void_j();
                    if (var_a_arr_h[i].var_int_e <= var_int_arr_f[i])
                    {
                        var_a_arr_h[i].var_int_g = var_a_arr_h[i].var_byte_d;
                        continue;
                    }
                    if (var_a_arr_h[i].var_int_e < var_a_arr_h[i].q) continue;
                    var_a_arr_h[i].var_int_g = -var_a_arr_h[i].var_byte_d;
                }
            }

            if (!var_boolean_t)
            {
                if (var_boolean_e && var_a_arr_g != null)
                {
                    int waveCleared = 1;
                    for (int i = 0; i < var_a_arr_g.Length; ++i)
                    {
                        if (!var_a_arr_g[i].var_boolean_a) continue;
                        waveCleared = 0;
                        break;
                    }
                    if (waveCleared != 0)
                    {
                        n();
                    }
                }

                if (an < am)
                {
                    ++an;
                }

                if (@as != -1)
                {
                    int tileX = var_a_f.var_int_d / TileMap.var_int_b;
                    if (tileX == @as)
                    {
                        Q = 0;
                        var_int_g = 0;
                        @as = -1;
                        var_int_h = -1;
                    }
                    else
                    {
                        Q = @as > tileX ? 1 : 2;
                        var_int_g = Q;
                    }
                }

                var_boolean_k = false;
                if (!(var_a_f.var_boolean_d || var_a_f.var_boolean_c || var_a_f.var_boolean_e ||
                      var_int_g == 0 || var_a_f.boolean_a(1) || var_a_f.x == 20))
                {
                    if (Q == 128)
                    {
                        var_boolean_k = true;
                    }
                    j();
                }

                if ((var_int_g & 2) == 0 && (var_int_g & 1) == 0)
                {
                    if (var_a_f.t == 2 || var_a_f.t == 3)
                    {
                        var_a_f.var_int_f = al;
                    }
                    else if (!var_a_f.var_boolean_d)
                    {
                        var_a_f.var_int_f = 0;
                    }
                    if (var_a_f.t == 1)
                    {
                        var_a_f.t = 0;
                        var_a_f.a(1, false);
                    }
                }

                d(var_a_f);

                if (var_a_e != null)
                {
                    var_a_e.a(false);
                }

                if (var_a_arr_g != null)
                {
                    for (int i = 0; i < var_a_arr_g.Length; ++i)
                    {
                        Actor enemy = var_a_arr_g[i];
                        d(enemy);
                        if (var_a_f.var_boolean_e || var_a_f.var_boolean_c) continue;
                        if (enemy.s == var_int_l || enemy.s == var_int_k)
                        {
                            e(enemy);
                        }
                        d(i);
                    }
                }

                if (ak > 0)
                {
                    --ak;
                }

                if (var_a_arr_e != null)
                {
                    for (int i = 0; i < var_a_arr_e.Length; ++i)
                    {
                        Actor trig = var_a_arr_e[i];
                        if (trig.s == var_int_j && trig.G != 0 && --trig.H <= 0)
                        {
                            trig.H = trig.G;
                            trig.var_boolean_a = !trig.var_boolean_a;
                        }
                        trig.void_j();
                    }
                }

                if (var_a_arr_f != null)
                {
                    for (int i = 0; i < var_a_arr_f.Length; ++i)
                    {
                        Actor acc = var_a_arr_f[i];
                        acc.void_j();
                        acc.var_int_d = var_a_f.var_int_d + acc.p;
                        acc.var_int_e = var_a_f.var_int_e + acc.q;
                    }
                }

                for (int i = 0; i < M; ++i)
                {
                    Actor proj = var_a_arr_b[i];
                    if (proj.boolean_a()) continue;
                    void_a(proj);
                    proj.void_a();
                }

                for (int i = 0; i < var_a_arr_c.Length; ++i)
                {
                    var_a_arr_c[i].a(false);
                }

                if (E > 0)
                {
                    if (var_a_d != null)
                    {
                        var_a_d.a(false);
                    }
                    if (--E == 0)
                    {
                        if (var_a_e != null)
                        {
                            var_a_e.a(22, false);
                        }
                        if (var_a_arr_g != null)
                        {
                            for (int i = 0; i < var_a_arr_g.Length; ++i)
                            {
                                var_a_arr_g[i].b(3);
                            }
                        }
                        if (F == 4)
                        {
                            var_a_f.t = 0;
                            var_a_f.a(1, false);
                        }
                    }
                }

                h();
                i();
                if (var_a_g != null) var_a_g.void_j();
                var_c_arr_a[aj].a(var_a_f, var_a_arr_g, this);

                if (var_boolean_k && E == 0)
                {
                    k();
                }

                for (int i = 0; i < M; ++i)
                {
                    Actor proj = var_a_arr_b[i];
                    if (!proj.var_boolean_l || !var_c_arr_a[aj].a(proj.int_g(), proj.int_i(), proj.int_h(), proj.int_j(), 1)) continue;
                    void_a(proj);
                    proj.void_a();
                }

                if (ao != -1 && var_a_f.int_d() < ao)
                {
                    var_a_f.var_int_d += (ao - var_a_f.int_d());
                }
                if (ap != -1 && var_a_f.int_e() > ap)
                {
                    var_a_f.var_int_d -= (var_a_f.int_e() - ap);
                }

                if (aB != -1 && var_a_i != null && var_a_i.z < aC)
                {
                    a(aB, 0, 0);
                    aB = -1;
                }
                if (aE != -1 && var_a_f.z < aF)
                {
                    a(aE, 0, 0);
                    aE = -1;
                }
            }

            if (var_a_arr_d != null)
            {
                for (int i = 0; i < var_a_arr_d.Length; ++i)
                {
                    var_a_arr_d[i].void_j();
                }
            }
            if (var_a_arr_a != null)
            {
                for (int i = 0; i < var_a_arr_a.Length; ++i)
                {
                    var_a_arr_a[i].void_j();
                }
            }

            if (var_boolean_r)
            {
                int lookahead = EngineTweaks.Widescreen ? (App.var_int_a >> 2) : (var_int_c >> 2);
                aa = var_a_f.s == var_int_n ? var_a_f.var_int_d : var_a_f.var_int_d + (var_a_f.var_int_b == 1 ? lookahead : -lookahead);
            }
            if (var_boolean_s)
            {
                if (ao != -1 && aa < ao + var_int_e)
                {
                    aa = ao + var_int_e;
                }
                if (ap != -1 && aa > ap - var_int_e)
                {
                    aa = ap - var_int_e;
                }
            }

            int dx = aa - ac;
            if (dx != 0)
            {
                int absX = Math.Abs(dx);
                if (absX < Z)
                {
                    ac = aa;
                }
                else
                {
                    int step = absX > (var_int_c / 2) ? 2 * Z : Z;
                    ac = dx > 0 ? (ac + step) : (ac - step);
                }
                if (ac < ae) ac = ae;
                else if (ac > ag) ac = ag;
            }

            if (var_boolean_r && var_a_f.var_byte_arr_a != null)
            {
                ab = var_a_f.var_int_e + (var_a_f.var_byte_arr_a[0] >> 1);
            }

            int dy = ab - ad;
            if (dy != 0)
            {
                int absY = Math.Abs(dy);
                if (absY < Z)
                {
                    ad = ab;
                }
                else
                {
                    int step = absY > (var_int_d / 2) ? 2 * Z : Z;
                    ad = dy > 0 ? (ad + step) : (ad - step);
                }
                if (ad < af) ad = af;
                else if (ad > ah) ad = ah;
            }

            CamInterpState.Update(ac, ad);

            if (var_a_f != null)
            {
                var_a_f.InterpState.Update(var_a_f.var_int_d, var_a_f.var_int_e);
            }

            if (var_a_arr_g != null)
            {
                for (int i = 0; i < var_a_arr_g.Length; i++)
                {
                    var enemy = var_a_arr_g[i];
                    if (enemy != null && enemy.var_boolean_a)
                    {
                        enemy.InterpState.Update(enemy.var_int_d, enemy.var_int_e);
                    }
                }
            }

            if (var_a_arr_b != null)
            {
                for (int i = 0; i < M; i++)
                {
                    var proj = var_a_arr_b[i];
                    if (proj != null && proj.var_boolean_a)
                    {
                        proj.InterpState.Update(proj.var_int_d, proj.var_int_e);
                    }
                }
            }

            if (var_a_arr_h != null)
            {
                for (int i = 0; i < var_a_arr_h.Length; i++)
                {
                    var plat = var_a_arr_h[i];
                    if (plat != null && plat.var_boolean_a)
                    {
                        plat.InterpState.Update(plat.var_int_d, plat.var_int_e);
                    }
                }
            }

            for (int i = 0; i < ai; ++i)
            {
                int lx = var_c_arr_a[i].i * (ac - (var_int_c / 2)) / 10;
                int ly = var_c_arr_a[i].j * (ad - (var_int_d / 2)) / 10;
                var_c_arr_a[i].boolean_a(lx, ly);
            }

            if (var_a_f != null && var_a_f.var_boolean_e)
            {
                a(false);
            }

            var_g_a.repaint();
            var_g_a.serviceRepaints();
        }

        return var_boolean_p;
    }

    private void void_a(Actor actor, int vx, int vy)
    {
        if (var_c_arr_a == null) return;
        if (vx != 0)
        {
            actor.var_int_d += vx;
            var_c_arr_a[aj].void_a(actor);
        }
        actor.var_int_e += vy;
        bool landed = var_c_arr_a[aj].a(actor, vy < 0, vy > 0);
        if (actor.t == 3 && landed)
        {
            actor.t = 0;
            actor.a(1, false);
            actor.var_int_f = 0;
        }
        if (actor.t == 2 && landed)
        {
            actor.t = 3;
            actor.var_byte_a = 8;
        }
    }

    private void d(Actor actor)
    {
        if (actor.var_boolean_e && actor.r > 0 && --actor.r == 0)
        {
            actor.var_boolean_e = false;
            actor.void_k();
            actor.t = 0;
            actor.D = 0;
            actor.a(1, false);
            actor.var_boolean_a = true;
        }
        if (!actor.var_boolean_a) return;

        if (actor.t == 5)
        {
            actor.var_int_f = 0;
        }

        if (actor.t == 2 || actor.t == 3)
        {
            int[] curve = (actor.s == var_int_n && var_int_arr_c != null) ? var_int_arr_c : var_int_arr_b;
            if (actor == var_a_f && actor.var_byte_a == 8 && var_boolean_c)
            {
                var_boolean_c = false;
                actor.var_byte_a = 0;
                actor.a(11, false);
            }
            int vy = curve[actor.var_byte_a];
            if (actor.var_byte_a < curve.Length - 1)
            {
                actor.var_byte_a = (sbyte)(actor.var_byte_a + 1);
            }
            actor.var_int_g = vy;
            if (actor.x == 2 && vy > 0)
            {
                actor.x = -1;
            }
            if (actor.x == -1)
            {
                int anim = 3;
                switch (actor.t)
                {
                    case 0:
                        anim = 1;
                        break;
                    case 1:
                        anim = 0;
                        break;
                }
                actor.a(anim, false);
            }
        }

        actor.a(false);
        int vx = actor.var_int_f;
        if (actor.t == 2 || actor.t == 3)
        {
            if (vx > 0) vx -= 2;
            else if (vx < 0) vx += 2;
        }

        void_a(actor, vx, actor.var_int_g);

        if (actor.var_boolean_c && actor.x == -1)
        {
            actor.var_int_f = 0;
            if (actor.r == 0)
            {
                actor.r = 40;
            }
            else if (--actor.r == 0)
            {
                actor.var_boolean_d = false;
                actor.var_boolean_c = false;
                actor.var_boolean_e = true;
                actor.a(1, false);
                actor.var_boolean_a = (actor.u & 0x80) != 0;
                if (actor.var_boolean_k)
                {
                    actor.r = 50;
                }
            }
        }

        if (var_a_arr_h != null)
        {
            for (int i = 0; i < var_a_arr_h.Length; ++i)
            {
                a(actor, i);
            }
        }

        if (var_c_arr_a != null && actor.t != 2 && actor.t != 3 && !actor.var_boolean_b && !actor.var_boolean_n)
        {
            int savedY = actor.var_int_e++;
            if (!var_c_arr_a[aj].a(actor, false, true))
            {
                actor.t = 3;
                actor.var_byte_a = 8;
                actor.a(3, false);
            }
            actor.var_int_e = savedY;
        }

        actor.var_boolean_h = false;

        if (actor.x == -1)
        {
            if (actor.var_boolean_n)
            {
                for (int i = 0; i < M; ++i)
                {
                    if (var_a_arr_b[i].var_a_b != actor) continue;
                    var_a_arr_b[i].void_b();
                    if (actor.s == var_int_n)
                    {
                        var_a_arr_b[i].F = 50;
                        continue;
                    }
                    if (actor == var_a_f)
                    {
                        var_a_arr_b[i].F = 15;
                        continue;
                    }
                    if (actor.s != var_int_l && actor.s != var_int_k) continue;
                    var_a_arr_b[i].F = x;
                }
                actor.var_boolean_n = false;
                if (actor.t == 5) actor.t = 0;
            }

            actor.var_boolean_d = false;
            if (actor.s != var_int_l && actor.s != var_int_k)
            {
                if (actor.t == 4) actor.a(7, false);
                else if (actor.t == 1) actor.a(0, false);
                else if (actor.t != 3 && actor.t != 2)
                {
                    actor.t = 0;
                    actor.a(1, false);
                    actor.var_int_f = 0;
                }
            }
        }

        if (actor.x == 20)
        {
            actor.void_e();
            return;
        }
        if (actor.s == var_int_k)
        {
            actor.void_f();
            return;
        }
        actor.void_d();
    }

    private void a(Actor actor, int platIdx)
    {
        if (var_a_arr_h == null) return;
        Actor plat = var_a_arr_h[platIdx];
        if (!plat.var_boolean_j || !plat.var_boolean_a) return;

        if (actor.var_int_e <= plat.var_int_e && actor.var_int_e >= plat.var_int_e - 6 &&
            actor.int_e() >= plat.int_d() && actor.int_d() <= plat.int_e() &&
            actor.var_int_n <= plat.var_int_n)
        {
            actor.var_int_e = plat.var_int_e;
            actor.var_int_g = 0;
            actor.var_boolean_b = true;
            if (actor.t == 3)
            {
                actor.t = 0;
                actor.a(1, false);
                actor.var_int_f = 0;
            }
        }
    }

    private void d(int enemyIdx)
    {
        if (var_a_arr_g == null || var_int_arr_e == null || var_c_arr_a == null) return;
        Actor enemy = var_a_arr_g[enemyIdx];
        Actor target = (var_a_e != null && var_a_e.var_boolean_a) ? var_a_e : var_a_f!;

        if (!enemy.var_boolean_a || enemy.var_boolean_e || enemy.var_boolean_c || enemy.var_boolean_d) return;
        if (enemy.boolean_a(1))
        {
            enemy.var_int_f = 0;
            return;
        }

        int dx = target.var_int_d - enemy.var_int_d;
        int dy = target.var_int_e - enemy.var_int_e;

        if (!enemy.var_boolean_j)
        {
            bool trigger = false;
            if ((enemy.u & 8) != 0)
            {
                trigger = true;
                if (!boolean_a(enemy)) return;
            }
            if ((enemy.u & 0x10) != 0)
            {
                trigger = true;
                if (dy > TileMap.var_int_b || dy < -TileMap.var_int_b) return;
            }
            if (trigger) enemy.var_boolean_j = true;
            else return;
        }

        bool isBoss = enemy.s == var_int_v && (enemy.u & 0x80) != 0;
        if (isBoss)
        {
            if (enemy.w == 4 && enemy.t == 5 && var_int_a - aH > 20)
            {
                enemy.a(1, false);
                enemy.var_int_f = 0;
                enemy.t = 0;
                var_a_f!.b(2);
            }
            else if (enemy.w == 1 && var_int_a - aH > 50)
            {
                var_a_f!.b(1);
            }
        }

        if (enemy.t != 0 && enemy.t != 1) return;
        if (enemy.boolean_b()) return;

        if ((enemy.u & 0x80) == 0)
        {
            --enemy.v;
            if (enemy.v < -10)
            {
                enemy.v = 22 + (var_java_util_Random_a.NextInt() & 7);
            }
            if (enemy.v < 0)
            {
                enemy.t = 0;
                if (enemy.x != 1) enemy.a(1, false);
                enemy.var_int_f = 0;
                return;
            }
        }

        if (enemy.var_byte_c == 0)
        {
            if (isBoss)
            {
                int dt = var_int_a - aH;
                if (enemy.w == 0)
                {
                    if (enemy.var_a_a != null && !enemy.var_a_a.var_boolean_l && dt > 70)
                    {
                        aH = var_int_a;
                        enemy.a(18, false);
                        enemy.var_boolean_n = true;
                        enemy.t = 5;
                        enemy.var_int_f = 0;
                        return;
                    }
                }
                else
                {
                    if (enemy.w == 1 && dt > 130)
                    {
                        aH = var_int_a;
                        enemy.t = 5;
                        enemy.a(17, false);
                        var_a_f!.void_a(1);
                        enemy.var_int_f = 0;
                        var_a_f!.var_int_f = 0;
                        return;
                    }
                    if (enemy.w == 3 && dt > 80)
                    {
                        aH = var_int_a;
                        enemy.a(19, false);
                        enemy.var_boolean_n = true;
                        enemy.t = 5;
                        enemy.var_int_f = 0;
                        return;
                    }
                    if (enemy.w == 4 && dt > 80)
                    {
                        aH = var_int_a;
                        enemy.var_int_c = dx > 0 ? 1 : 3;
                        enemy.a(20, false);
                        enemy.t = 5;
                        E = 20;
                        return;
                    }
                }
            }

            if ((enemy.u & 0x40) != 0 && enemy.var_a_a != null && !enemy.var_a_a.var_boolean_l)
            {
                if (dy < TileMap.var_int_b && dy > -TileMap.var_int_b)
                {
                    f(enemy);
                    enemy.var_boolean_n = true;
                    return;
                }
            }
            else if (enemy.boolean_a(target))
            {
                f(enemy);
                var_int_arr_e[enemyIdx] = 0;
            }
        }

        if (enemy.var_byte_c != 0)
        {
            enemy.var_byte_c = (sbyte)(enemy.var_byte_c - 1);
            if (enemy.var_byte_c == 0)
            {
                enemy.var_int_f = 0;
                if (enemy.var_boolean_n)
                {
                    enemy.var_int_c = dx > 0 ? 1 : 3;
                    enemy.a(15, false);
                    enemy.var_boolean_n = true;
                    return;
                }
                enemy.a(4 + Math.Abs(var_java_util_Random_a.NextInt()) % 2, false);
            }
            return;
        }

        if (aI > 0)
        {
            if (--aI == 0) enemy.var_boolean_j = true;
            return;
        }

        int timer = var_int_arr_e[enemyIdx];
        if (timer != 0)
        {
            if (dy < TileMap.var_int_b && dy > -TileMap.var_int_b && dx < var_int_e && dx > -var_int_e)
            {
                var_int_arr_e[enemyIdx] = 0;
                return;
            }
            if (enemy.t != 1)
            {
                enemy.t = 1;
                enemy.a(0, false);
            }
            else
            {
                var_c_arr_a[aj].b(enemy);
                if (enemy.var_int_f == 0 || enemy.var_int_o == enemy.var_int_d)
                {
                    var_int_arr_e[enemyIdx] = timer > 0 ? -40 : 40;
                    timer = var_int_arr_e[enemyIdx];
                }
            }
            enemy.var_int_f = timer > 0 ? enemy.var_byte_d : -enemy.var_byte_d;
            enemy.var_int_c = enemy.var_int_f > 0 ? 1 : 3;
            int prevTimer = timer;
            timer = timer > 0 ? --timer : ++timer;
            if (timer == 0) timer = prevTimer > 0 ? -40 : 40;
            var_int_arr_e[enemyIdx] = timer;
            return;
        }

        if (isBoss && aJ-- <= 0)
        {
            aJ = 5;
            int roll = Math.Abs(var_java_util_Random_a.NextInt()) % 100;
            if (roll < 30)
            {
                enemy.var_int_f = 0;
                enemy.t = 0;
                enemy.a(1, false);
                aI = 25;
                return;
            }
        }

        enemy.var_int_f = 0;
        enemy.var_int_c = dx > 0 ? 1 : 3;
        if (dx > 10)
        {
            enemy.var_int_f = enemy.var_byte_d;
            if ((enemy.u & 2) == 0) var_c_arr_a[aj].b(enemy);
        }
        else if (dx < -10)
        {
            enemy.var_int_f = -enemy.var_byte_d;
            if ((enemy.u & 2) == 0) var_c_arr_a[aj].b(enemy);
        }

        if (enemy.var_int_f != 0)
        {
            enemy.t = 1;
            if (enemy.x != 0) enemy.a(0, false);
        }
        else
        {
            enemy.t = 0;
            if (enemy.x != 1) enemy.a(1, false);
        }

        if ((enemy.u & 1) != 0 && target.t != 2 && target.t != 3 && dy < -20 && var_c_arr_a[aj].boolean_a(enemy, 70))
        {
            if ((var_java_util_Random_a.NextInt() & 1) == 0) enemy.var_int_f = 0;
            enemy.var_byte_a = 0;
            enemy.a(2, false);
            enemy.t = 2;
            return;
        }

        if ((enemy.u & 4) != 0 && dy > 10)
        {
            enemy.var_int_g = 1;
            enemy.var_boolean_h = true;
        }

        if (enemy.t == 0 && (enemy.u & 0x80) == 0 && timer == 0 && !enemy.var_boolean_b &&
            target.t != 2 && target.t != 3 && var_c_arr_a[aj].boolean_a(enemy))
        {
            var_int_arr_e[enemyIdx] = (var_java_util_Random_a.NextInt() & 1) == 0 ? 20 : -20;
        }
    }

    private void e(Actor enemy)
    {
        if (var_a_f == null || var_c_arr_a == null) return;
        int dx = enemy.var_int_d - var_a_f.var_int_d;
        if (enemy.x == -1)
        {
            if (enemy.var_boolean_j)
            {
                bool atEdge = enemy.var_int_d < 3 * TileMap.var_int_b || enemy.var_int_d > var_c_arr_a[aj].var_int_c - 3 * TileMap.var_int_b;
                int roll = Math.Abs(var_java_util_Random_a.NextInt()) % 100;
                enemy.var_int_c = dx > 0 ? 3 : 1;
                if (atEdge && roll < 50) atEdge = false;
                if (atEdge) roll /= 2;

                if (Math.Abs(dx) < y && !atEdge)
                {
                    if (roll < z)
                    {
                        enemy.var_int_f = 0;
                        enemy.a(4, false);
                        return;
                    }
                    if (roll < A)
                    {
                        enemy.var_int_f = 0;
                        enemy.a(5, false);
                        return;
                    }
                }
                else
                {
                    if (enemy.var_a_a != null && !enemy.var_a_a.var_boolean_l && roll < B)
                    {
                        enemy.var_int_f = 0;
                        enemy.a(15, false);
                        enemy.var_boolean_n = true;
                        return;
                    }
                    if (roll < C)
                    {
                        enemy.t = 6;
                        enemy.a(0, false);
                        enemy.var_int_f = dx > 0 ? -enemy.var_byte_d : enemy.var_byte_d;
                        return;
                    }
                }
            }
            enemy.a(1, false);
            enemy.var_int_f = 0;
        }
    }

    private void f(Actor enemy)
    {
        enemy.t = 0;
        enemy.a(1, false);
        enemy.var_byte_c = enemy.var_byte_b;
        enemy.var_byte_c = (sbyte)(enemy.var_byte_c + 1);
        enemy.var_int_f = 0;
    }

    private bool boolean_a(Actor actor, int damage, int dir)
    {
        if (EngineTweaks.GodMode && actor == var_a_f) return false;
        if (var_boolean_j && actor == var_a_f) return false;
        if ((actor.s == var_int_l || actor.s == var_int_k || actor.s == var_int_n) && actor.x == 15) return false;
        if (var_a_f != null && var_a_f.s == var_int_n && damage > 1 && var_byte_arr_g != null)
        {
            aG = var_byte_arr_g.Length >> 1;
        }
        if (actor.var_boolean_d) return false;
        if (actor == var_a_f) var_boolean_v = true;

        actor.b(1);
        actor.var_byte_c = 0;
        actor.var_boolean_n = false;
        actor.var_boolean_j = true;
        actor.var_int_c = dir;
        actor.z -= damage;

        if (actor.z <= 0)
        {
            if (G == 3 && actor != var_a_f && actor.s == var_int_v && actor.w == 2 && (actor.u & 0x80) == 0 && var_a_arr_g != null)
            {
                actor.var_boolean_a = false;
                Actor lastEnemy = var_a_arr_g[var_a_arr_g.Length - 1];
                lastEnemy.var_int_d = actor.var_int_d;
                lastEnemy.var_int_e = actor.var_int_e;
                lastEnemy.var_boolean_a = true;
                lastEnemy.var_int_c = actor.var_int_c;
                actor = lastEnemy;
                lastEnemy.var_boolean_e = false;
                actor.r = 0;
            }
            actor.a(14, false);
            actor.var_boolean_c = true;
            actor.var_int_f = 0;
            return true;
        }

        int hurtAnim = 13;
        if ((actor.s == var_int_l || actor.s == var_int_k) && damage > 1)
        {
            hurtAnim = 17;
        }
        actor.var_int_f = actor.var_int_c == 1 ? -6 : 6;
        actor.a(hurtAnim, false);
        actor.var_boolean_d = true;
        return false;
    }

    private bool a(Actor attacker, Actor target, bool showSpark)
    {
        if (!attacker.var_boolean_g || attacker.var_boolean_i || target.var_boolean_c || target.var_boolean_e || target.var_boolean_d)
            return false;

        if (attacker.boolean_a(target))
        {
            int damage = 1;
            if (attacker.x == 20)
            {
                if (target.boolean_a(2)) return false;
                target.void_a(2);
                damage = 2;
            }
            else
            {
                attacker.var_boolean_i = true;
            }

            if (showSpark && var_a_g != null)
            {
                int sparkAnim = 0;
                if (var_a_f != null && var_a_f.s == var_int_n)
                {
                    var_a_h = target;
                    sparkAnim = var_java_util_Random_a.NextInt() & 1;
                }
                var_a_g.a(sparkAnim, false);
                var_a_g.var_int_d = attacker.var_int_d + ((attacker.int_k() + attacker.m()) >> 1);
                var_a_g.var_int_e = attacker.var_int_e + ((attacker.l() + attacker.n()) >> 1);
            }
            return boolean_a(target, damage, attacker.var_int_b == 1 ? 3 : 1);
        }
        return false;
    }

    private void e(int enemyIdx)
    {
        if (var_a_arr_g == null) return;
        Actor enemy = var_a_arr_g[enemyIdx];
        int points = int_a(enemy);
        App.var_int_g += points;
        N += points;
        if (ar == enemyIdx)
        {
            var_int_h = -1;
            ar = -1;
        }
        if (at > 0)
        {
            --at;
        }
    }

    private void h()
    {
        if (var_a_f == null || var_a_arr_g == null) return;
        for (int i = 0; i < var_a_arr_g.Length; ++i)
        {
            Actor enemy = var_a_arr_g[i];
            if (!enemy.var_boolean_a) continue;
            if (a(var_a_f, enemy, true))
            {
                e(i);
            }
            if (var_a_f.t == 5 || !a(enemy, var_a_f, var_a_f.s == var_int_n)) continue;
            g(enemy);
        }

        for (int i = 0; i < M; ++i)
        {
            Actor proj = var_a_arr_b[i];
            if (!proj.var_boolean_l) continue;
            Actor parent = proj.var_a_b!;
            if (parent == var_a_f)
            {
                bool hit = false;
                for (int j = 0; j < var_a_arr_g.Length; ++j)
                {
                    Actor enemy = var_a_arr_g[j];
                    if (!enemy.var_boolean_a || enemy.var_boolean_e || enemy.var_boolean_c || !enemy.b(proj)) continue;
                    if (boolean_a(enemy, var_a_f.w == 0 ? 3 : 2, 4 - proj.var_int_b))
                    {
                        e(j);
                    }
                    void_a(proj);
                    proj.void_a();
                    hit = true;
                    break;
                }
                if (hit) continue;
            }
            else
            {
                if (!var_a_f.b(proj)) continue;
                int damage = 1;
                if (parent.s == var_int_k) damage = 2;
                else if (parent.s == var_int_l) damage = 3;
                if (boolean_a(var_a_f, damage, 4 - proj.var_int_b))
                {
                    g(proj.var_a_b!);
                }
                void_a(proj);
                proj.void_a();
            }
        }
    }

    private void g(Actor enemy)
    {
        if (enemy != null && (enemy.u & 0x80) == 0)
        {
            a(enemy, 2, 12 + Math.Abs(var_java_util_Random_a.NextInt()) % 3, 130, 70);
        }
    }

    private void i()
    {
        if (var_a_arr_e == null || var_a_f == null) return;
        for (int i = 0; i < var_a_arr_e.Length; ++i)
        {
            Actor trig = var_a_arr_e[i];
            if (!trig.var_boolean_a) continue;
            int type = trig.s;
            if (var_a_f.b(trig))
            {
                if (type == var_int_i)
                {
                    if (var_a_f.z >= var_a_f.A) continue;
                    var_a_f.z = var_a_f.A;
                    App.var_int_g += 10;
                    O += 10;
                }
                else if (type == var_int_j)
                {
                    if (ak == 0)
                    {
                        ak = 6;
                        boolean_a(var_a_f, 1, var_a_f.var_int_b);
                    }
                    void_a(var_a_f, var_a_f.var_int_b == 1 ? -4 : 4, 0);
                    continue;
                }
                else if (type == var_int_m)
                {
                    if (var_boolean_u || var_a_f.var_boolean_e || var_a_f.var_boolean_c) continue;
                    var_boolean_u = true;
                    boolean_a(var_a_f, 1, var_a_f.var_int_b);
                    var_a_f.t = 0;
                    int_c();
                    continue;
                }
                else if (type == var_int_o)
                {
                    if (trig.w == 2)
                    {
                        App.var_int_g += 100;
                        O += 100;
                        l();
                    }
                    else
                    {
                        App.var_int_g += 500;
                        O += 500;
                        if (trig.w == 0)
                        {
                            var_a_f.A = ++K;
                            a(var_a_f, 2, 9, 110, 50);
                        }
                        else if (trig.w == 1)
                        {
                            am = ++L * 64;
                            a(var_a_f, 2, 10, 110, 50);
                        }
                        if (i == 0 && var_byte_arr_f != null)
                        {
                            var_byte_arr_f[G + G - 1] = 5;
                        }
                        if (i < 3 && var_byte_arr_e != null)
                        {
                            var_byte_arr_e[G] = (byte)(var_byte_arr_e[G] | (1 << i));
                        }
                    }
                }
                trig.var_boolean_a = false;
                continue;
            }
            if (type != var_int_m) continue;
            var_boolean_u = false;
        }
    }

    private void j()
    {
        if (var_a_f == null) return;
        int anim = -1;
        switch (Q)
        {
            case 1:
            case 2:
                if (var_a_f.t == 0 || var_a_f.t == 4)
                {
                    var_a_f.t = 1;
                    anim = 0;
                }
                al = 0;
                break;
            case 16:
                var_a_f.var_int_c = 3;
                anim = int_c();
                al = -var_a_f.var_byte_d;
                break;
            case 32:
                var_a_f.var_int_c = 1;
                anim = int_c();
                al = var_a_f.var_byte_d;
                break;
            case 4:
                anim = int_c();
                al = 0;
                break;
            case 8:
                if (var_a_f.t == 0 || var_a_f.t == 1)
                {
                    var_a_f.t = 4;
                    anim = 6;
                    var_a_f.var_int_f = 0;
                    break;
                }
                if (var_a_f.t != 4 || var_a_f.var_boolean_b) break;
                var_a_f.var_int_g = 1;
                var_a_f.var_boolean_h = true;
                break;
            case 64:
                if (var_a_f.boolean_b() || var_a_f.var_boolean_d) break;
                if (var_a_f.s == var_int_n && var_a_f.t == 4)
                {
                    var_a_f.t = 0;
                }
                anim = var_a_f.t == 4 ? 8 + Math.Abs(var_java_util_Random_a.NextInt()) % 2 :
                       (var_a_f.t == 2 || var_a_f.t == 3 ? (var_a_f.s == var_int_n ? 4 : 12) :
                       (var_a_f.s == var_int_n ? var_int_arr_h[Math.Abs(var_java_util_Random_a.NextInt()) % 3] :
                       var_int_arr_g[Math.Abs(var_java_util_Random_a.NextInt()) % 4]));
                break;
        }

        Q = 0;
        if ((var_int_g & 2) != 0)
        {
            var_a_f.var_int_f = -var_a_f.var_byte_d;
            var_a_f.var_int_c = 3;
            if (var_a_f.t == 0)
            {
                anim = 0;
                var_a_f.t = 1;
            }
        }
        if ((var_int_g & 1) != 0)
        {
            var_a_f.var_int_f = var_a_f.var_byte_d;
            var_a_f.var_int_c = 1;
            if (var_a_f.t == 0)
            {
                anim = 0;
                var_a_f.t = 1;
            }
        }
        if (anim >= 0 && anim != var_a_f.x)
        {
            var_a_f.a(anim, false);
        }
        if (var_a_f.t == 4 || var_a_f.boolean_b())
        {
            var_a_f.var_int_f = 0;
        }
    }

    private void k()
    {
        if (an < 320 || var_a_f == null) return;
        var_a_f.var_int_f = 0;

        if (var_a_f.s == var_int_n)
        {
            if (var_a_f.var_a_a != null && var_a_f.var_a_a.var_boolean_l) return;
            var_a_f.a(15, false);
            var_a_f.var_boolean_n = true;
            var_a_f.t = 5;
        }
        else
        {
            switch (F)
            {
                case 0:
                    if (var_a_f.var_a_a != null && var_a_f.var_a_a.var_boolean_l) return;
                    var_a_f.a(18, false);
                    var_a_f.var_boolean_n = true;
                    var_a_f.t = 5;
                    break;
                case 1:
                    var_a_f.a(17, false);
                    var_a_f.t = 5;
                    if (var_a_arr_g != null)
                    {
                        for (int i = 0; i < var_a_arr_g.Length; ++i)
                        {
                            Actor enemy = var_a_arr_g[i];
                            if (!enemy.var_boolean_a || enemy.var_boolean_e || enemy.var_boolean_c ||
                                enemy.var_int_d < var_a_f.var_int_d - aK || enemy.var_int_d > var_a_f.var_int_d + aK ||
                                enemy.var_int_e < var_a_f.var_int_e - aK || enemy.var_int_e > var_a_f.var_int_e + aK) continue;
                            enemy.void_a(1);
                            enemy.t = 0;
                            enemy.a(1, false);
                            E = 80;
                        }
                    }
                    break;
                case 2:
                    if (var_a_e != null)
                    {
                        var_a_e.var_int_b = var_a_f.var_int_b;
                        var_a_e.var_int_d = var_a_f.var_int_d + (var_a_f.var_int_b == 1 ? 6 : -6);
                        var_a_e.var_int_e = var_a_f.var_int_e;
                        var_a_e.var_boolean_a = true;
                        var_a_e.a(21, false);
                        E = 100;
                    }
                    break;
                case 3:
                    var_a_f.a(19, false);
                    var_a_f.var_boolean_n = true;
                    var_a_f.t = 5;
                    break;
                case 4:
                    if (var_a_f.t == 3 || var_a_f.t == 2) return;
                    var_a_f.a(20, false);
                    var_a_f.t = 5;
                    E = 20;
                    break;
            }
        }
        an -= 320;
    }

    private int int_c()
    {
        if (var_a_f == null) return -1;
        bool isJumping = var_a_f.t == 2;
        if (var_a_f.s == var_int_n) isJumping = false;

        if (var_a_f.t == 4)
        {
            var_a_f.t = 0;
            return 10;
        }

        if (var_a_f.t == 0 || var_a_f.t == 1 || (isJumping && !var_boolean_d))
        {
            if (var_a_f.t == 2 && var_a_f.var_byte_a < 8)
            {
                var_boolean_d = true;
                var_boolean_c = true;
                return -1;
            }
            var_boolean_d = var_a_f.t == 2;
            var_a_f.var_int_f = 0;
            var_a_f.var_int_g = 0;
            var_a_f.var_byte_a = 0;
            var_a_f.t = 2;
            return var_boolean_d ? 11 : 2;
        }
        return -1;
    }

    public static short a(int scriptIdx, int targetX, int targetY)
    {
        if (scriptIdx >= 0 && scriptIdx < var_short_arr_arr_a.Length)
        {
            if (scriptIdx == T)
            {
                PR.Core.Audio.SoundEngine.Play(2, var_int_a);
            }
            else if (scriptIdx == U)
            {
                PR.Core.Audio.SoundEngine.Play(3, var_int_a);
            }

            if (!a(var_short_arr_arr_a[scriptIdx]))
            {
                short[] seq = var_short_arr_arr_a[scriptIdx];
                short s = seq[6];
                seq[0] = 7;
                seq[2] = 0;
                seq[1] = 0;
                seq[3] = (short)targetX;
                seq[4] = (short)targetY;
                return s > 1 ? (short)0 : s;
            }
        }
        return -1;
    }

    public static bool a(short[]? seq)
    {
        return seq != null && seq[0] > 0;
    }

    private Actor? a(int group, int index)
    {
        switch (group)
        {
            case 0:
                return var_a_f;
            case 1:
                return (var_a_arr_g != null && index >= 0 && index < var_a_arr_g.Length) ? var_a_arr_g[index] : null;
            case 2:
                return (var_a_arr_d != null && index >= 0 && index < var_a_arr_d.Length) ? var_a_arr_d[index] : null;
            case 3:
                return (var_a_arr_a != null && index >= 0 && index < var_a_arr_a.Length) ? var_a_arr_a[index] : null;
            case 4:
                return (var_a_arr_f != null && index >= 0 && index < var_a_arr_f.Length) ? var_a_arr_f[index] : null;
        }
        return null;
    }

    public void c(int scriptIdx)
    {
        if (scriptIdx == var_int_h || scriptIdx >= var_short_arr_arr_a.Length) return;
        short[] seq = var_short_arr_arr_a[scriptIdx];
        if (seq != null && seq[0] > 0)
        {
            bool stepped = false;
            while ((seq[1] = (short)(seq[1] - 1)) == -1)
            {
                stepped = true;
                if (var_boolean_q) var_boolean_q = false;

                if (seq[0] == seq.Length)
                {
                    seq[2] = (short)(seq[2] + 1);
                    if (seq[2] >= seq[5] && seq[5] > 0)
                    {
                        seq[0] = 0;
                        if (scriptIdx == T || scriptIdx == U)
                        {
                            bool win = scriptIdx == T;
                            T = -1;
                            U = -1;
                            a(win);
                        }
                        return;
                    }
                    seq[0] = 7;
                    seq[1] = 0;
                }

                short op = seq[seq[0]++];
                seq[1] = seq[seq[0]++];

                switch (op)
                {
                    case 0:
                        var_boolean_r = false;
                        aa = seq[seq[0]++];
                        ab = seq[seq[0]++];
                        Z = seq[seq[0]++];
                        if (Z <= 0) Z = int.MaxValue;
                        break;
                    case 1:
                        var_boolean_r = true;
                        Z = seq[seq[0]++];
                        if (Z <= 0) Z = int.MaxValue;
                        break;
                    case 2:
                        var_boolean_m = true;
                        var_int_g = 0;
                        if (var_a_f != null)
                        {
                            var_a_f.var_int_f = 0;
                            var_a_f.t = 0;
                            var_a_f.a(1, false);
                        }
                        break;
                    case 3:
                        var_boolean_m = false;
                        break;
                    case 4:
                        int grp = seq[seq[0]++];
                        int idx = seq[seq[0]++];
                        int sX = seq[seq[0]++];
                        int sY = seq[seq[0]++];
                        int sW = seq[seq[0]++];
                        int sH = seq[seq[0]++];
                        a(a(grp, idx), sX, sY, sW, sH);
                        break;
                    case 5:
                        var_boolean_t = seq[seq[0]++] != 0;
                        break;
                    case 7:
                        if (var_a_arr_g != null)
                        {
                            var_a_arr_g[seq[seq[0]++]].var_boolean_j = true;
                        }
                        break;
                    case 16:
                        if (var_a_arr_g != null)
                        {
                            var_a_arr_g[seq[seq[0]++]].var_boolean_j = false;
                        }
                        break;
                    case 17:
                        if (var_a_arr_e != null)
                        {
                            var_a_arr_e[seq[seq[0]++]].var_boolean_a = false;
                        }
                        break;
                    case 18:
                        if (var_byte_arr_g != null) aG = var_byte_arr_g.Length >> 1;
                        break;
                    case 19:
                        if (var_byte_arr_e != null && (var_byte_arr_e[G] & 8) != 0)
                        {
                            var_short_arr_arr_a[scriptIdx][0] = 0;
                            return;
                        }
                        if (var_byte_arr_g != null) aG = var_byte_arr_g.Length >> 1;
                        App.var_int_g += 500;
                        P += 500;
                        an = am;
                        break;
                    case 20:
                        aD = seq[seq[0]++];
                        break;
                    case 8:
                        at = seq[seq[0]++];
                        break;
                    case 9:
                        ay = seq[seq[0]++];
                        az = seq[seq[0]++];
                        break;
                    case 10:
                        l();
                        break;
                    case 11:
                        if (var_a_arr_h != null)
                        {
                            var_a_arr_h[seq[seq[0]++]].var_boolean_j = true;
                        }
                        break;
                    case 12:
                        aA = seq[seq[0]++];
                        var_int_h = scriptIdx;
                        break;
                    case 13:
                        if (seq[seq[0]++] == 0) I &= ~(1 << G);
                        else I |= 1 << G;
                        break;
                    case 14:
                        R = scriptIdx;
                        var_boolean_n = false;
                        break;
                    case 15:
                        aB = seq[seq[0]++];
                        aC = seq[seq[0]++];
                        break;
                    case 21:
                        aE = seq[seq[0]++];
                        aF = seq[seq[0]++];
                        break;
                    case 40:
                        for (int k = 0; k < var_boolean_arr_b.Length; ++k)
                        {
                            var_boolean_arr_b[k] = true;
                        }
                        var_int_g = 0;
                        break;
                    case 41:
                        var_boolean_arr_b[seq[seq[0]++]] = false;
                        break;
                    case 42:
                        Actor? aOn = a(seq[seq[0]++], seq[seq[0]++]);
                        if (aOn != null) aOn.var_boolean_a = true;
                        break;
                    case 43:
                        Actor? aOff = a(seq[seq[0]++], seq[seq[0]++]);
                        if (aOff != null) aOff.var_boolean_a = false;
                        break;
                    case 44:
                        if (var_c_arr_a != null)
                        {
                            int tx = seq[seq[0]++];
                            int ty = seq[seq[0]++];
                            byte tid = (byte)(seq[seq[0]++] + 3);
                            var_c_arr_a[aj].a(tx, ty, tid);
                        }
                        break;
                    case 55:
                        if (var_c_arr_a != null)
                        {
                            int tx = seq[3] + seq[seq[0]++];
                            int ty = seq[4] + seq[seq[0]++];
                            byte tid = (byte)(seq[seq[0]++] + 3);
                            var_c_arr_a[aj].a(tx, ty, tid);
                        }
                        break;
                    case 45:
                        a(seq[seq[0]++], seq[seq[0]++], seq[seq[0]++] != 0);
                        break;
                    case 46:
                        aq = seq[seq[0]++];
                        var_int_h = scriptIdx;
                        break;
                    case 47:
                        if (var_a_arr_i != null)
                        {
                            int shootIdx = seq[seq[0]++];
                            if (var_a_arr_i[shootIdx].var_a_a != null)
                            {
                                var_a_arr_i[shootIdx].var_a_a!.void_b();
                            }
                        }
                        break;
                    case 48:
                        a(seq[seq[0]++] == 0);
                        break;
                    case 49:
                        ar = seq[seq[0]++];
                        var_int_h = scriptIdx;
                        break;
                    case 50:
                        Actor? aSpawn = a(seq[seq[0]++], seq[seq[0]++]);
                        short sAnim = seq[seq[0]++];
                        if (aSpawn != null)
                        {
                            aSpawn.void_h();
                            aSpawn.var_boolean_a = true;
                            aSpawn.a(sAnim, false);
                        }
                        break;
                    case 51:
                        @as = seq[seq[0]++];
                        var_int_h = scriptIdx;
                        break;
                    case 52:
                        var_int_h = scriptIdx;
                        break;
                    case 53:
                        av = seq[seq[0]++];
                        break;
                    case 54:
                        short s46 = seq[seq[0]++];
                        short s48 = seq[seq[0]++];
                        short s50 = seq[seq[0]++];
                        short s52 = seq[seq[0]++];
                        if (s52 == 0 || (s52 == 1 && !var_boolean_f) || (s52 == 2 && var_boolean_f))
                        {
                            a(s46, s48, s50);
                        }
                        break;
                    case 56:
                        Actor? aAnim = a(seq[seq[0]++], seq[seq[0]++]);
                        short anIndex = seq[seq[0]++];
                        if (aAnim != null)
                        {
                            aAnim.a(anIndex, false);
                            aAnim.var_boolean_a = true;
                        }
                        break;
                    case 57:
                        int gIdx = seq[seq[0]++];
                        int eIdx = seq[seq[0]++];
                        Actor? aMove = a(gIdx, eIdx);
                        short ox = seq[seq[0]++];
                        short oy = seq[seq[0]++];
                        if (aMove != null)
                        {
                            aMove.var_int_d = seq[3] * TileMap.var_int_b + ox;
                            aMove.var_int_e = seq[4] * TileMap.var_int_b + oy;
                        }
                        break;
                }

                if (var_boolean_b) seq[1] = 0;
            }

            if (var_boolean_b && !stepped)
            {
                seq[1] = 0;
            }
        }
        else if (aA == scriptIdx)
        {
            aA = -1;
            var_int_h = -1;
        }
    }

    private void a(int x1, int x2, bool lockCam)
    {
        ao = x1;
        ap = x2;
        var_boolean_s = lockCam;
        if (ao != -1) ao *= TileMap.var_int_b;
        if (ap != -1) ap *= TileMap.var_int_b;
    }

    private void a(Actor? actor, int grp, int idx, int sW, int sH)
    {
        if (App.var_java_lang_String_arr_arr_a != null &&
            grp >= 0 && grp < App.var_java_lang_String_arr_arr_a.Length &&
            idx >= 0 && idx < App.var_java_lang_String_arr_arr_a[grp].Length)
        {
            a(actor, App.var_java_lang_String_arr_arr_a[grp][idx], sW, sH);
        }
    }

    private void a(Actor? actor, string str, int sW, int sH)
    {
        if (sW > App.var_int_a) sW = App.var_int_a;
        var_a_j = actor;
        aw = sW;
        ax = sH;
        var_java_lang_String_arr_a = GMIDlet.WordWrap(str, sW - 7, 0);
        var_boolean_q = true;
    }

    private void l()
    {
        if (az > 0 && --ay == 0)
        {
            a(az, 0, 0);
        }
    }

    public void a(bool win)
    {
        if (win && G >= H && G < D)
        {
            H = G + 1;
        }
        var_boolean_p = true;
    }

    public static void void_a(DataReader dis)
    {
        if (dis == null)
        {
            App.var_int_g = 0;
            G = 0;
            return;
        }
        App.var_int_g = dis.ReadInt();
        G = dis.ReadByte();
        H = dis.ReadByte();
        I = dis.ReadShort();
        J = dis.ReadByte();
        K = dis.ReadByte();
        L = dis.ReadByte();
        if (var_byte_arr_e != null) dis.ReadFully(var_byte_arr_e);
        if (var_byte_arr_f != null) dis.ReadFully(var_byte_arr_f);
        var_boolean_g = dis.ReadBoolean();
        var_byte_a = dis.ReadByte();
    }

    public static void a(DataWriter dw)
    {
        dw.WriteInt(App.var_int_g);
        dw.WriteByte(G);
        dw.WriteByte(H);
        dw.WriteShort(I);
        dw.WriteByte(J);
        dw.WriteByte(K);
        dw.WriteByte(L);
        if (var_byte_arr_e != null) dw.Write(var_byte_arr_e, 0, var_byte_arr_e.Length);
        if (var_byte_arr_f != null) dw.Write(var_byte_arr_f, 0, var_byte_arr_f.Length);
        dw.WriteBoolean(var_boolean_g);
        dw.WriteByte(var_byte_a);
    }

    public bool boolean_a(Actor actor)
    {
        int logicHalfW = 120;
        int logicHalfH = 160;
        int logicCamX = ac;
        if (EngineTweaks.Widescreen && var_a_f != null && var_c_arr_a != null && aj < var_c_arr_a.Length)
        {
            int origMinX = logicHalfW;
            int origMaxX = Math.Max(origMinX, var_c_arr_a[aj].var_int_c - logicHalfW);
            int origLookahead = var_a_f.s == var_int_n ? 0 : (var_a_f.var_int_b == 1 ? 60 : -60);
            logicCamX = Math.Clamp(var_a_f.var_int_d + origLookahead, origMinX, origMaxX);
        }
        return actor.int_e() - logicCamX >= -logicHalfW - 20 &&
               actor.int_d() - logicCamX <= logicHalfW + 20 &&
               actor.var_int_e - ad >= -logicHalfH - 20 &&
               actor.int_f() - ad <= logicHalfH + 20;
    }

    private void n()
    {
        ++aO;
        if (var_a_arr_g == null || var_int_arr_e == null || var_byte_arr_c == null || var_byte_arr_d == null) return;
        for (int i = 0; i < var_a_arr_g.Length; ++i)
        {
            var_int_arr_e[i] = 0;
            var_a_arr_g[i].var_boolean_a = false;
            var_a_arr_g[i].var_boolean_j = false;
        }
        int startM = aM;
        int enemyId;
        while (aM < var_byte_arr_c.Length && (enemyId = (sbyte)var_byte_arr_c[aM]) != -1)
        {
            if (enemyId >= 0 && enemyId < var_a_arr_g.Length)
            {
                Actor enemy = var_a_arr_g[enemyId];
                enemy.p = (short)((sbyte)var_byte_arr_d[aN] * TileMap.var_int_b);
                enemy.q = (short)((sbyte)var_byte_arr_d[aN + 1] * TileMap.var_int_b);
                enemy.var_boolean_e = false;
                enemy.void_k();
                enemy.t = 0;
                enemy.a(1, false);
                enemy.var_boolean_a = true;
                enemy.var_boolean_j = true;
                aN = (aN + 2) % var_byte_arr_d.Length;
            }
            ++aM;
        }
        ++aM;
        if (aM >= var_byte_arr_c.Length)
        {
            aM = startM;
        }
    }

    private static int int_a(Actor actor)
    {
        int type = actor.s;
        if (type == var_int_q) return 10;
        if (type == var_int_r) return 20;
        if (type == var_int_s) return 25;
        if (type == var_int_t) return 30;
        if (type == var_int_u) return 50;
        if (type == var_int_k) return 500;
        if (type == var_int_l) return 1000;
        if (type == var_int_v && (actor.u & 0x80) != 0) return 200;
        return 0;
    }
}
