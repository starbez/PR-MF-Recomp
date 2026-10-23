using System;
using System.IO;

namespace PR.Core.Graphics;

public static class GMIDlet
{
    public static Image?[]? var_javax_microedition_lcdui_Image_arr_a;
    public static short[]? var_short_arr_a;
    public static Graphics? var_javax_microedition_lcdui_Graphics_a;

    // Font data
    public static int var_int_a;
    public static int var_int_b = 65;
    public static int var_int_c = 90;
    public static int var_int_d = 97;
    public static int var_int_e = 122;
    public static int var_int_f = 25;
    public static int var_int_g = 25;
    public static int var_int_h = 50;

    public static Image?[]? var_javax_microedition_lcdui_Image_arr_b;
    public static short[]? var_short_arr_b;
    public static string var_java_lang_String_a = "";
    public static string var_java_lang_String_b = "";
    public static byte[]? var_byte_arr_a;
    public static string[]? var_java_lang_String_arr_a;
    public static sbyte[]? var_byte_arr_b;
    public static sbyte[]? var_byte_arr_c;
    public static sbyte[]? var_byte_arr_d;
    public static sbyte[]? var_byte_arr_e;
    public static sbyte[]? var_byte_arr_f;
    public static sbyte[]? var_byte_arr_g;
    public static sbyte[]? var_byte_arr_h;
    public static bool[]? var_boolean_arr_a;
    public static bool[]? var_boolean_arr_b;
    public static byte[][]? var_byte_arr_arr_a;
    public static sbyte[]? i_arr;

    public static void void_a(int n)
    {
        var_javax_microedition_lcdui_Image_arr_a = new Image[n];
        var_short_arr_a = new short[n];
    }

    public static int int_a(int n)
    {
        if (var_javax_microedition_lcdui_Image_arr_a == null || n < 0 || n >= var_javax_microedition_lcdui_Image_arr_a.Length) return 0;
        return var_javax_microedition_lcdui_Image_arr_a[n]?.Width ?? 0;
    }

    public static int b(int n)
    {
        if (var_javax_microedition_lcdui_Image_arr_a == null || n < 0 || n >= var_javax_microedition_lcdui_Image_arr_a.Length) return 0;
        return var_javax_microedition_lcdui_Image_arr_a[n]?.Height ?? 0;
    }

    public static int c(int n)
    {
        if (var_byte_arr_c == null || n < 0 || n >= var_byte_arr_c.Length) return -1;
        return var_byte_arr_c[n];
    }

    public static void a(DataReader dataInputStream, Image?[] imageArray, short[] sArray, int n, int n2, bool bl)
    {
        short s = 0;
        Image? image = null;
        sbyte by = dataInputStream.ReadByte();
        short s2 = dataInputStream.ReadShort();
        bool bl2 = false;
        if ((by & 2) == 0)
        {
            bl2 = true;
            n2 = 0;
        }
        if (s2 == 0)
        {
            return;
        }

        byte[] byArray = new byte[s2];
        dataInputStream.ReadFully(byArray);
        int n3 = 1;
        if (!bl2)
        {
            n3 = dataInputStream.ReadShort();
        }

        if (n2 < 0)
        {
            int n4 = n;
            byte[] byArray2 = new byte[s2];
            for (int i = 0; i < n3; ++i)
            {
                s = (short)((ushort)s | (ushort)(1 << (8 + i)));
                Array.Copy(byArray, 0, byArray2, 0, byArray.Length);
                if (i > 0)
                {
                    int n5 = dataInputStream.ReadByte() & 0xFF;
                    if (n5 == 0)
                    {
                        n5 = dataInputStream.ReadShort();
                    }
                    for (int j = 0; j < n5; ++j)
                    {
                        short patchOff = dataInputStream.ReadShort();
                        byArray2[patchOff] = (byte)dataInputStream.ReadByte();
                    }
                }
                if (imageArray[n4] == null)
                {
                    imageArray[n4] = image = Image.CreateImage(byArray2, 0, byArray2.Length);
                }
                ++n4;
            }
        }
        else
        {
            int n6;
            int n7 = 0;
            if (n2 > 0)
            {
                while (++n7 < n2)
                {
                    n6 = dataInputStream.ReadByte() & 0xFF;
                    if (n6 == 0)
                    {
                        n6 = dataInputStream.ReadShort();
                    }
                    dataInputStream.SkipBytes(n6 * 3);
                }
                n6 = dataInputStream.ReadByte() & 0xFF;
                if (n6 == 0)
                {
                    n6 = dataInputStream.ReadShort();
                }
                for (int i = 0; i < n6; ++i)
                {
                    short patchOff = dataInputStream.ReadShort();
                    byArray[patchOff] = (byte)dataInputStream.ReadByte();
                }
            }
            s = (short)(1 << (8 + n2));
            if ((bl && imageArray[n + n2] == null) || (!bl && imageArray[n] == null))
            {
                image = Image.CreateImage(byArray, 0, byArray.Length);
                if (bl)
                {
                    imageArray[n + n2] = image;
                }
                else
                {
                    imageArray[n] = image;
                }
            }
            while (++n7 < n3)
            {
                n6 = dataInputStream.ReadByte() & 0xFF;
                if (n6 == 0)
                {
                    n6 = dataInputStream.ReadShort();
                }
                dataInputStream.SkipBytes(n6 * 3);
            }
        }

        sArray[n] = (short)n3;
        sArray[n] = (short)(sArray[n] | s);
        if ((by & 4) != 0)
        {
            sArray[n] = (short)(sArray[n] | 0x80);
        }
    }

    public static Image? a(DataReader dataInputStream, int n)
    {
        Image? image = null;
        if (n >= 0 && var_javax_microedition_lcdui_Image_arr_a != null && n < var_javax_microedition_lcdui_Image_arr_a.Length)
        {
            var_javax_microedition_lcdui_Image_arr_a[n] = null;
        }
        dataInputStream.ReadByte();
        short s = dataInputStream.ReadShort();
        if (s > 0)
        {
            byte[] byArray = new byte[s];
            dataInputStream.ReadFully(byArray, 0, s);
            image = Image.CreateImage(byArray, 0, s);
            if (n >= 0 && var_javax_microedition_lcdui_Image_arr_a != null && n < var_javax_microedition_lcdui_Image_arr_a.Length)
            {
                var_javax_microedition_lcdui_Image_arr_a[n] = image;
            }
        }
        return image;
    }

    public static void a(Image? image, int n, int n2, int n3)
    {
        if (image != null && var_javax_microedition_lcdui_Graphics_a != null)
        {
            var_javax_microedition_lcdui_Graphics_a.DrawImage(image, n, n2, n3);
        }
    }

    public static void a(int n, int n2, int n3, int n4)
    {
        if (var_javax_microedition_lcdui_Image_arr_a != null && n >= 0 && n < var_javax_microedition_lcdui_Image_arr_a.Length)
        {
            var img = var_javax_microedition_lcdui_Image_arr_a[n];
            if (img != null && var_javax_microedition_lcdui_Graphics_a != null)
            {
                var_javax_microedition_lcdui_Graphics_a.DrawImage(img, n2, n3, n4);
            }
        }
    }

    public static void a(int n, int n2, int n3, int n4, int n5, int n6, int n7)
    {
        if (var_javax_microedition_lcdui_Image_arr_a != null && n >= 0 && n < var_javax_microedition_lcdui_Image_arr_a.Length)
        {
            Image? image = var_javax_microedition_lcdui_Image_arr_a[n];
            if (image != null) a(image, n2, n3, n4, n5, n6, n7);
        }
    }

    public static void a(Image image, int n, int n2, int n3, int n4, int n5, int n6)
    {
        if (var_javax_microedition_lcdui_Graphics_a == null || image == null || n3 <= 0 || n4 <= 0) return;
        int cellW = image.Width / n3;
        int cellH = image.Height / n4;
        var_javax_microedition_lcdui_Graphics_a.DrawRegion(image, n5 * cellW, n6 * cellH, cellW, cellH, 0, n, n2, 20);
    }

    public static int a(int n, int n2, int n3, int n4, int n5, int n6, int n7, bool bl)
    {
        int cellW = int_a(n) / n7;
        n4 -= cellW;
        for (int i = 0; i < n3; ++i)
        {
            int digit = 10;
            if (n2 > 0)
            {
                digit = n2 % 10;
            }
            else if (n2 == 0 && (bl || i == 0))
            {
                digit = 0;
            }
            a(n, n4, n5, n7, 1, digit, 0);
            n4 -= cellW + 1;
            n2 /= 10;
        }
        return n4 + cellW;
    }

    public static void void_a(DataReader dataInputStream)
    {
        var_int_b = 65;
        var_int_c = 90;
        var_int_d = 97;
        var_int_e = 122;
        var_int_f = var_int_c - var_int_b;
        var_int_g = var_int_e - var_int_d;
        var_int_h = var_int_f + var_int_g;

        var_int_a = dataInputStream.ReadShort();
        var_javax_microedition_lcdui_Image_arr_b = new Image[var_int_a];
        var_short_arr_b = new short[var_int_a];

        for (int n = 0; n < var_int_a; n += var_short_arr_b[n] & 0x7F)
        {
            a(dataInputStream, var_javax_microedition_lcdui_Image_arr_b, var_short_arr_b, n, -1, false);
        }

        // Read strings & font tables
        // In J2ME readUTF() is 2 bytes length + utf8
        var_java_lang_String_a = ReadUTF(dataInputStream);
        var_java_lang_String_b = ReadUTF(dataInputStream);

        short bLen = dataInputStream.ReadShort();
        var_byte_arr_a = new byte[bLen];
        dataInputStream.ReadFully(var_byte_arr_a);
        dataInputStream.ReadShort();

        var_java_lang_String_arr_a = new string[var_int_a];
        var_byte_arr_c = new sbyte[var_int_a];
        var_byte_arr_b = new sbyte[var_int_a];
        var_byte_arr_d = new sbyte[var_int_a];
        var_byte_arr_e = new sbyte[var_int_a];
        var_byte_arr_f = new sbyte[var_int_a];
        var_byte_arr_g = new sbyte[var_int_a];
        var_boolean_arr_a = new bool[var_int_a];
        var_boolean_arr_b = new bool[var_int_a];
        var_byte_arr_arr_a = new byte[var_int_a][];
        i_arr = new sbyte[var_int_a];
        var_byte_arr_h = new sbyte[var_int_a];

        dataInputStream.ReadShort();

        for (int n = 0; n < var_int_a; ++n)
        {
            int n2 = n;
            int n3 = var_short_arr_b[n2] & 0x7F;
            var_java_lang_String_arr_a[n] = ReadUTF(dataInputStream);
            int n4 = var_java_lang_String_arr_a[n].Length;

            var_byte_arr_d[n] = var_byte_arr_c[n] = (sbyte)(var_javax_microedition_lcdui_Image_arr_b[n2]?.Height ?? 0);
            var_byte_arr_c[n] = (sbyte)(var_byte_arr_c[n] + (sbyte)(var_byte_arr_c[n] >> 2));
            var_boolean_arr_a[n] = false;
            var_byte_arr_arr_a[n] = null!;
            i_arr[n] = 0;

            int n6 = dataInputStream.ReadShort();
            while (--n6 >= 0)
            {
                switch (dataInputStream.ReadByte())
                {
                    case 0:
                        var_boolean_arr_a[n] = dataInputStream.ReadBoolean();
                        break;
                    case 1:
                        var_byte_arr_arr_a[n] = new byte[dataInputStream.ReadShort()];
                        dataInputStream.ReadFully(var_byte_arr_arr_a[n]);
                        break;
                    case 2:
                        i_arr[n] = dataInputStream.ReadByte();
                        break;
                    case 3:
                        var_byte_arr_g[n] = dataInputStream.ReadByte();
                        break;
                    case 4:
                        var_byte_arr_c[n] = dataInputStream.ReadByte();
                        break;
                    case 5:
                        var_byte_arr_e[n] = dataInputStream.ReadByte();
                        break;
                    case 6:
                        var_byte_arr_f[n] = dataInputStream.ReadByte();
                        break;
                    case 7:
                        var_boolean_arr_b[n] = dataInputStream.ReadByte() != 0;
                        break;
                    case 8:
                        var_byte_arr_h[n] = dataInputStream.ReadByte();
                        break;
                }
            }

            if (var_byte_arr_arr_a[n] != null)
            {
                var_byte_arr_b[n] = -1;
                if (var_byte_arr_h[n] <= 0)
                {
                    var_byte_arr_h[n] = (sbyte)var_byte_arr_arr_a[n][0];
                }
            }
            else
            {
                int imgW = var_javax_microedition_lcdui_Image_arr_b[n2]?.Width ?? 0;
                var_byte_arr_b[n] = var_boolean_arr_a[n]
                    ? (sbyte)(imgW / (var_int_h - var_int_g + n4))
                    : (sbyte)(imgW / (var_int_h + n4));
            }

            while (n3 > 1)
            {
                ++n;
                var_java_lang_String_arr_a[n] = var_java_lang_String_arr_a[n - 1];
                var_byte_arr_c[n] = var_byte_arr_c[n - 1];
                var_byte_arr_b[n] = var_byte_arr_b[n - 1];
                var_byte_arr_d[n] = var_byte_arr_d[n - 1];
                var_boolean_arr_a[n] = var_boolean_arr_a[n - 1];
                var_byte_arr_arr_a[n] = var_byte_arr_arr_a[n - 1];
                i_arr[n] = i_arr[n - 1];
                var_byte_arr_h[n] = var_byte_arr_h[n - 1];
                var_byte_arr_e[n] = var_byte_arr_e[n - 1];
                var_byte_arr_f[n] = var_byte_arr_f[n - 1];
                var_byte_arr_g[n] = var_byte_arr_g[n - 1];
                var_boolean_arr_b[n] = var_boolean_arr_b[n - 1];
                --n3;
            }
        }
    }

    private static string ReadUTF(DataReader dis)
    {
        int len = dis.ReadUnsignedShort();
        byte[] buf = new byte[len];
        dis.ReadFully(buf);
        return System.Text.Encoding.UTF8.GetString(buf);
    }

    public static void a(int n, string str, int n2, int n3, int n4)
    {
        if (string.IsNullOrEmpty(str)) return;
        char[] chars = str.ToCharArray();
        a(n, chars, 0, chars.Length, n2, n3, n4);
    }

    public static void a(int n, char[] cArray, int n2, int n3, int n4, int n5, int n6)
    {
        if (var_javax_microedition_lcdui_Graphics_a == null || var_javax_microedition_lcdui_Image_arr_b == null ||
            n < 0 || n >= var_javax_microedition_lcdui_Image_arr_b.Length) return;

        Image? image = var_javax_microedition_lcdui_Image_arr_b[n];
        if (image == null) return;

        sbyte by = var_byte_arr_e != null ? var_byte_arr_e[n] : (sbyte)0;
        int n7 = var_byte_arr_f != null ? var_byte_arr_f[n] : 0;
        int n8 = var_byte_arr_g != null ? var_byte_arr_g[n] : 0;
        bool bl = var_boolean_arr_a != null && var_boolean_arr_a[n];

        if ((n6 & 8) != 0) n4 -= a(n, cArray, n3);
        else if ((n6 & 1) != 0) n4 -= a(n, cArray, n3) >> 1;

        if ((n6 & 0x20) != 0) n5 -= var_byte_arr_c?[n] ?? 0;
        else if ((n6 & 2) != 0) n5 -= (var_byte_arr_c?[n] ?? 0) >> 1;

        int n9 = 0;
        int n10 = (var_byte_arr_b != null && var_byte_arr_b[n] != -1) ? var_byte_arr_b[n] : (var_byte_arr_h != null ? var_byte_arr_h[n] : 0);
        int n11 = n10;
        sbyte by2 = var_byte_arr_d != null ? var_byte_arr_d[n] : (sbyte)0;
        int n12 = 0;
        sbyte by3 = i_arr != null ? i_arr[n] : (sbyte)0;
        int n13 = (var_byte_arr_b != null && var_byte_arr_b[n] != -1) ? 0 : (var_byte_arr_arr_a != null && var_byte_arr_arr_a[n] != null ? var_byte_arr_arr_a[n].Length : 0);
        int n14 = var_int_h + 2;
        if (bl) n14 = var_int_g + 1;

        int n15 = n2 + n3;
        for (int i = n2; i < n15; ++i)
        {
            int n16;
            int n17;
            int n18;
            int n19;
            int n20;
            byte by4;
            bool bl3;

            int n21 = cArray[i];
            if (bl && ((n21 >= var_int_b && n21 <= var_int_c) || (n21 >= var_int_d && n21 <= var_int_e) || (var_int_d != 97 && n21 >= 97 && n21 <= 122)))
            {
                n21 = a_char((char)n21);
            }
            else if (var_int_d != 97 && n21 >= 97 && n21 <= 122)
            {
                n21 = a_char((char)n21);
            }

            bl3 = false;
            by4 = 0;
            bool bl5 = n21 >= var_int_b && n21 <= var_int_c;
            bool bl4 = false;
            if (!bl5 && !bl)
            {
                bl4 = n21 >= var_int_d && n21 <= var_int_e;
            }
            if (!bl5 && !bl4)
            {
                n20 = var_java_lang_String_a.Length;
                for (n19 = 0; n19 < n20; ++n19)
                {
                    if (var_java_lang_String_a[n19] != n21) continue;
                    n21 = var_java_lang_String_b[n19];
                    by4 = var_byte_arr_a != null ? var_byte_arr_a[n19] : (byte)0;
                    if (!bl) break;
                    n21 = a_char((char)n21);
                    break;
                }
                bl5 = n21 >= var_int_b && n21 <= var_int_c;
                bl4 = false;
                if (!bl5 && !bl)
                {
                    bl4 = n21 >= var_int_d && n21 <= var_int_e;
                }
            }

            if (bl5)
            {
                n12 = n21 - var_int_b;
                if (bl) bl3 = true;
            }
            else if (bl4)
            {
                bl3 = true;
                n12 = n21 - var_int_d + var_int_f + 1;
            }
            else
            {
                if (n21 == 32)
                {
                    n4 += n10 + by3;
                    continue;
                }
                n20 = var_java_lang_String_arr_a != null ? var_java_lang_String_arr_a[n].Length : 0;
                n19 = n21;
                if (bl && n21 == 231) n19 = 199;
                for (n18 = 0; n18 < n20; ++n18)
                {
                    n17 = var_java_lang_String_arr_a![n][n18];
                    if (n17 != n21 && (!bl || n17 != n19)) continue;
                    n12 = n18 + n14;
                    break;
                }
            }

            if (var_byte_arr_b != null && var_byte_arr_b[n] == -1)
            {
                n16 = 0;
                for (n20 = 0; n12 > n20 && n12 < n13; ++n20)
                {
                    n16 += var_byte_arr_arr_a![n][n20];
                }
                n11 = var_byte_arr_arr_a![n][n20];
            }
            else
            {
                n16 = n12 * n11;
            }

            var_javax_microedition_lcdui_Graphics_a.DrawRegion(image, n16, 0, n11, by2, 0, n4, n5, 20);

            if (by4 > 0)
            {
                n20 = n11 >> 1;
                n19 = 4;
                if (by4 == 4 || by4 == 5)
                {
                    n19 = 3;
                }
                else if (by4 == 6 || by4 == 7)
                {
                    n19 = 2;
                }
                else if (by4 > 7)
                {
                    n19 = 1;
                }

                if (var_byte_arr_b != null && var_byte_arr_b[n] == -1)
                {
                    n18 = var_byte_arr_arr_a![n][n13 - n19];
                    n16 = image.Width;
                    while (n19 > 0)
                    {
                        n16 -= var_byte_arr_arr_a[n][n13 - n19];
                        --n19;
                    }
                }
                else
                {
                    n18 = n11;
                    n16 = image.Width - n18 * n19;
                }

                if (by4 == 3 || by4 == 5 || by4 == 7 || by4 == 9)
                {
                    n9 = by2 >> 1;
                }
                if (by4 == 2)
                {
                    n20 += n18 >> 1;
                }
                n20 += by;
                n17 = n8;
                if (bl3 || bl)
                {
                    n17 = n7;
                }
                var_javax_microedition_lcdui_Graphics_a.DrawRegion(image, n16, n9, n18, by2 >> 1, 0, n4 + n20, n5 + --n17, 33);
                n9 = 0;
            }

            n4 += n11 + by3;
        }
    }

    public static int a(int n, char[] cArray, int n2)
    {
        int total = 0;
        for (int i = 0; i < n2; ++i)
        {
            total += a_char_width(n, cArray[i]);
        }
        return total - (i_arr != null && n < i_arr.Length ? i_arr[n] : 0);
    }

    public static int a_char_width(int n, char c2)
    {
        if (var_byte_arr_arr_a == null || n >= var_byte_arr_arr_a.Length) return -1;
        bool bl = var_boolean_arr_a != null && var_boolean_arr_a[n];
        int n2 = var_byte_arr_b != null ? var_byte_arr_b[n] : 0;
        int n3 = 0;

        if (n2 != -1)
        {
            return n2;
        }

        if (c2 > var_int_e && var_java_lang_String_a != null && var_java_lang_String_b != null)
        {
            int n5 = var_java_lang_String_a.Length;
            for (int n4 = 0; n4 < n5; ++n4)
            {
                if (var_java_lang_String_a[n4] != c2) continue;
                c2 = var_java_lang_String_b[n4];
                break;
            }
        }

        if (bl && ((c2 >= var_int_b && c2 <= var_int_c) || (c2 >= var_int_d && c2 <= var_int_e) || (var_int_d != 97 && c2 >= 'a' && c2 <= 'z')))
        {
            c2 = a_char(c2);
        }
        else if (var_int_d != 97 && c2 >= 'a' && c2 <= 'z')
        {
            c2 = a_char(c2);
        }

        if (c2 >= var_int_b && c2 <= var_int_c)
        {
            n3 = c2 - var_int_b;
        }
        else if (c2 >= var_int_d && c2 <= var_int_e)
        {
            n3 = c2 - var_int_d + var_int_f + 1;
            if (bl) n3 -= var_int_f + 1;
        }
        else if (c2 == ' ')
        {
            return var_byte_arr_h != null ? var_byte_arr_h[n] : 0;
        }
        else
        {
            int n5 = var_java_lang_String_arr_a != null ? var_java_lang_String_arr_a[n].Length : 0;
            for (int n4 = 0; n4 < n5; ++n4)
            {
                if (var_java_lang_String_arr_a![n][n4] != c2) continue;
                if (bl)
                {
                    n3 = n4 + var_int_h - var_int_g + 1;
                    break;
                }
                n3 = n4 + var_int_h + 2;
                break;
            }
        }

        if (var_byte_arr_arr_a[n] != null && n3 >= 0 && n3 < var_byte_arr_arr_a[n].Length)
        {
            n2 = var_byte_arr_arr_a[n][n3] + (i_arr != null && n < i_arr.Length ? i_arr[n] : 0);
        }
        return n2;
    }

    public static char a_char(char c2)
    {
        if (c2 >= var_int_b && c2 <= var_int_c) return c2;
        if (c2 == '\u03c2') return '\u03a3';
        return (char)(c2 + var_int_b - var_int_d);
    }

    public static int a(int n, string str)
    {
        if (string.IsNullOrEmpty(str)) return 0;
        int total = 0;
        for (int i = 0; i < str.Length; ++i)
        {
            total += a_char_width(n, str[i]);
        }
        return total;
    }

    public static string[] WordWrap(string str, int maxW, int fontId)
    {
        if (string.IsNullOrEmpty(str)) return Array.Empty<string>();
        if (a(fontId, str) > maxW)
        {
            var vector = new System.Collections.Generic.List<string>();
            int n3 = 0;
            int n4 = 1;
            int n5 = 1;
            while (n3 < n4)
            {
                if (a(fontId, str.Substring(n3, n4 - n3)) >= maxW)
                {
                    while (a(fontId, str.Substring(n3, n5 - n3)) >= maxW && n5 > n3)
                    {
                        --n5;
                    }
                    if (n5 < str.Length && str[n5] == ' ')
                    {
                        vector.Add(str.Substring(n3, n5 - n3));
                    }
                    else
                    {
                        vector.Add(str.Substring(n3, Math.Min(n5 + 1, str.Length) - n3));
                    }
                    n3 = n5 + 1;
                }
                else if (n4 == str.Length)
                {
                    vector.Add(str.Substring(n3, n4 - n3));
                    break;
                }
                n5 = n4;
                n4 = (n4 + 1 < str.Length) ? str.IndexOf(' ', n4 + 1) : -1;
                if (n4 == -1) n4 = str.Length;
            }
            return vector.ToArray();
        }
        return new string[] { str };
    }
}
