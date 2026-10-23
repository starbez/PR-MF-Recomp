using System;
using System.IO;
using PR.Core.Graphics;

namespace PR.Core;

/// <summary>
/// Application coordinator, resource manager, and game state manager mirroring g.java.
/// </summary>
public sealed class App
{
    public const int DefaultCanvasWidth = 240;
    public const int DefaultCanvasHeight = 320;

    public static int var_int_a = DefaultCanvasWidth;
    public static int var_int_b = DefaultCanvasHeight;
    public static int var_int_g = 0; // Cumulative score

    public static readonly string[] var_java_lang_String_arr_b = { "de", "en", "fr", "it" };
    public static string[]? var_java_lang_String_arr_a;
    public static string[][]? var_java_lang_String_arr_arr_a;

    public static int[] var_int_arr_a = new int[5]; // Story high scores
    public static int[] var_int_arr_b = new int[5]; // Survival high scores

    // Character template tables loaded from /l
    public static int[]? var_int_arr_c;
    public static int[]? var_int_arr_d;
    public static int[]? var_int_arr_e;
    public static int[]? var_int_arr_f;
    public static int[]? var_int_arr_g;
    public static byte[]? var_byte_arr_b;
    public static byte[]? var_byte_arr_c;
    public static byte[]? var_byte_arr_d;
    public static byte[]? var_byte_arr_e;
    public static byte[]? var_byte_arr_f;
    public static int[]? var_int_arr_h;

    public static int var_int_l; // Level count (12)
    public static int var_int_n;
    public static int var_int_o;
    public static int var_int_p;
    public static int q;
    public static int r;
    public static int s;
    public static int t;
    public static int u;
    public static int v;
    public static int w;
    public static sbyte var_byte_e;
    public static byte var_byte_b; // Options (sound/vib)
    public static byte var_byte_d = 0; // Language (0=de default in original Java ROM)

    public static byte[]? var_byte_arr_a; // In-memory save blob

    public Game? var_f_a; // Active game instance
    public readonly IResourceProvider ResourceProvider;

    public App(IResourceProvider resourceProvider)
    {
        ResourceProvider = resourceProvider;
    }

    public static Actor a_a(int n)
    {
        if (var_int_arr_c == null || var_int_arr_d == null || var_int_arr_e == null ||
            var_int_arr_f == null || var_int_arr_g == null || var_byte_arr_b == null ||
            var_byte_arr_c == null || Actor.var_a_arr_a == null)
        {
            throw new InvalidOperationException("Character tables not initialized.");
        }

        Actor actor = new Actor(Actor.var_a_arr_a[var_int_arr_c[n]]);
        actor.z = actor.A = var_int_arr_d[n];
        actor.B = var_int_arr_e[n];
        actor.C = var_int_arr_f[n];
        actor.u = var_int_arr_g[n];
        actor.var_byte_b = (sbyte)var_byte_arr_b[n];
        actor.var_byte_d = (sbyte)var_byte_arr_c[n];
        actor.void_h();
        actor.void_g();
        return actor;
    }

    public void m()
    {
        // 1. Parse /l pack
        using (var stream = ResourceProvider.OpenResource("l"))
        {
            if (stream == null) throw new FileNotFoundException("Resource /l not found.");
            using var dis = new DataReader(stream);

            var_int_l = dis.ReadByte();
            int scoreCount = dis.ReadShort();
            int[] scoreTable = new int[scoreCount];
            for (int i = 0; i < scoreCount; ++i)
            {
                scoreTable[i] = dis.ReadInt();
            }
            Game.var_int_arr_a = scoreTable;

            var_byte_arr_d = new byte[dis.ReadShort()];
            dis.ReadFully(var_byte_arr_d);
            Game.var_byte_arr_f = new byte[var_byte_arr_d.Length];

            Game.var_byte_arr_a = new byte[dis.ReadShort()];
            dis.ReadFully(Game.var_byte_arr_a);

            var_int_n = dis.ReadByte();
            var_int_o = dis.ReadByte();
            var_int_p = dis.ReadByte();
            dis.ReadByte();
            q = dis.ReadByte();
            r = dis.ReadByte();
            s = dis.ReadByte();
            Game.var_int_i = dis.ReadByte();
            Game.var_int_j = dis.ReadByte();
            Game.var_int_m = dis.ReadByte();
            t = dis.ReadByte();
            Game.var_int_l = dis.ReadByte();
            Game.var_int_k = dis.ReadByte();
            Game.var_int_n = dis.ReadByte();
            Game.var_int_o = dis.ReadByte();
            Game.var_int_p = dis.ReadByte();
            u = dis.ReadByte();
            var_byte_e = dis.ReadByte();

            int hCount = dis.ReadShort();
            var_int_arr_h = new int[hCount];
            for (int i = 0; i < hCount; ++i)
            {
                var_int_arr_h[i] = dis.ReadByte();
            }

            Game.D = dis.ReadByte(); // Survival level index (13)

            var_byte_arr_e = new byte[dis.ReadShort()];
            dis.ReadFully(var_byte_arr_e);

            int cCount = dis.ReadShort();
            int[] cArray = new int[cCount];
            for (int i = 0; i < cCount; ++i)
            {
                cArray[i] = dis.ReadByte();
            }
            Game.var_int_arr_c = cArray;

            w = dis.ReadByte();
            Game.var_byte_arr_b = new byte[dis.ReadShort()];
            dis.ReadFully(Game.var_byte_arr_b);
            Game.var_byte_arr_d = new byte[dis.ReadShort()];
            dis.ReadFully(Game.var_byte_arr_d);
            Game.var_byte_arr_c = new byte[dis.ReadShort()];
            dis.ReadFully(Game.var_byte_arr_c);

            Game.var_int_q = dis.ReadByte();
            Game.var_int_r = dis.ReadByte();
            Game.var_int_s = dis.ReadByte();
            Game.var_int_t = dis.ReadByte();
            Game.var_int_u = dis.ReadByte();
            Game.var_int_v = dis.ReadByte();

            Game.var_byte_arr_g = new byte[dis.ReadShort()];
            dis.ReadFully(Game.var_byte_arr_g);

            var_byte_arr_f = new byte[dis.ReadShort()];
            dis.ReadFully(var_byte_arr_f);
            v = dis.ReadByte();
            dis.ReadShort();

            short s2 = dis.ReadShort();
            var_int_arr_c = new int[s2];
            var_int_arr_d = new int[s2];
            var_int_arr_e = new int[s2];
            var_int_arr_f = new int[s2];
            var_int_arr_g = new int[s2];
            var_byte_arr_b = new byte[s2];
            var_byte_arr_c = new byte[s2];

            for (int i = 0; i < s2; ++i)
            {
                var_int_arr_c[i] = dis.ReadByte();
                var_int_arr_d[i] = dis.ReadByte();
                var_int_arr_e[i] = dis.ReadByte();
                var_int_arr_f[i] = dis.ReadByte();
                var_int_arr_g[i] = dis.ReadShort();
                var_byte_arr_c[i] = (byte)dis.ReadByte();
                short sTagCount = dis.ReadShort();
                for (int j = 0; j < sTagCount; ++j)
                {
                    switch (dis.ReadByte())
                    {
                        case 1:
                            var_byte_arr_b[i] = (byte)dis.ReadByte();
                            break;
                    }
                }
            }

            sbyte n2 = dis.ReadByte();
            sbyte actorImgCount = dis.ReadByte();
            GMIDlet.void_a(n2);
            int n = 0;
            ++n;
            GMIDlet.a(dis, 0);
            ++n;
            GMIDlet.a(dis, 1);
            while (n < n2)
            {
                GMIDlet.a(dis, GMIDlet.var_javax_microedition_lcdui_Image_arr_a!, GMIDlet.var_short_arr_a!, n++, -1, false);
            }
            dis.ReadByte();

            // Load Living Mobile sound tracks
            PR.Core.Audio.SoundEngine.Load(dis);

            Actor.ResourceProvider = ResourceProvider;
            Actor.c(actorImgCount);
        }

        // 1.5 Parse /font
        using (var fontStream = ResourceProvider.OpenResource("font"))
        {
            if (fontStream != null)
            {
                using var fontDis = new DataReader(fontStream);
                GMIDlet.void_a(fontDis);
            }
        }

        // 1.6 Parse /text__en.txt
        using (var textStream = ResourceProvider.OpenResource("text__" + var_java_lang_String_arr_b[var_byte_d] + ".txt"))
        {
            if (textStream != null)
            {
                using var textDis = new DataReader(textStream);
                var_java_lang_String_arr_a = ReadStringArray(textDis);
                var_java_lang_String_arr_arr_a = new string[3][];
                var_java_lang_String_arr_arr_a[0] = ReadStringArray(textDis);
                var_java_lang_String_arr_arr_a[1] = ReadStringArray(textDis);
                var_java_lang_String_arr_arr_a[2] = ReadStringArray(textDis);
            }
        }

        // 2. Parse /fr
        using (var stream = ResourceProvider.OpenResource("fr"))
        {
            if (stream == null) throw new FileNotFoundException("Resource /fr not found.");
            using var dis = new DataReader(stream);
            Actor.a(dis);
        }

        // 3. Parse /spr
        using (var stream = ResourceProvider.OpenResource("spr"))
        {
            if (stream == null) throw new FileNotFoundException("Resource /spr not found.");
            using var dis = new DataReader(stream);
            short sprCount = dis.ReadShort();
            Actor.e(sprCount);
            for (int i = 0; i < sprCount; ++i)
            {
                Actor actor = new Actor();
                actor.b(dis);
                actor.s = i;
                Actor.var_a_arr_a![i] = actor;
            }
        }
    }

    public void LoadLanguage(int langIndex)
    {
        var_byte_d = (byte)Math.Clamp(langIndex, 0, var_java_lang_String_arr_b.Length - 1);
        using var textStream = ResourceProvider.OpenResource("text__" + var_java_lang_String_arr_b[var_byte_d] + ".txt");
        if (textStream != null)
        {
            using var textDis = new DataReader(textStream);
            var_java_lang_String_arr_a = ReadStringArray(textDis);
            var_java_lang_String_arr_arr_a = new string[3][];
            var_java_lang_String_arr_arr_a[0] = ReadStringArray(textDis);
            var_java_lang_String_arr_arr_a[1] = ReadStringArray(textDis);
            var_java_lang_String_arr_arr_a[2] = ReadStringArray(textDis);
        }
    }

    public void a(int step, int totalSteps)
    {
    }

    public void repaint()
    {
        if (GMIDlet.var_javax_microedition_lcdui_Graphics_a != null && var_f_a != null)
        {
            var_f_a.d();
        }
    }

    public void serviceRepaints()
    {
        repaint();
    }

    public void a(object? screen)
    {
    }

    public void o()
    {
    }

    public void b(bool flag)
    {
    }

    public void b(int score)
    {
    }

    public void i()
    {
        // Serialize current progress to in-memory save blob var_byte_arr_a
        using var ms = new MemoryStream();
        using var dw = new DataWriter(ms);
        Game.a(dw);
        dw.Flush();
        var_byte_arr_a = ms.ToArray();
    }

    public void h()
    {
        // Restore progress from in-memory save blob var_byte_arr_a
        if (var_byte_arr_a != null && var_byte_arr_a.Length > 0)
        {
            using var ms = new MemoryStream(var_byte_arr_a);
            using var dr = new DataReader(ms);
            Game.void_a(dr);
        }
    }

    private static string[] ReadStringArray(DataReader dis)
    {
        int count = dis.ReadShort();
        string[] arr = new string[count];
        for (int i = 0; i < count; i++)
        {
            int len = dis.ReadUnsignedShort();
            byte[] buf = new byte[len];
            dis.ReadFully(buf);
            arr[i] = System.Text.Encoding.UTF8.GetString(buf);
        }
        return arr;
    }
}
