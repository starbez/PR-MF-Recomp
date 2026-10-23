package javax.microedition.lcdui;

public class Font {
    public static final int STYLE_PLAIN = 0;
    public static final int STYLE_BOLD = 1;
    public static final int STYLE_ITALIC = 2;
    public static final int STYLE_UNDERLINED = 4;
    public static final int SIZE_SMALL = 8;
    public static final int SIZE_MEDIUM = 0;
    public static final int SIZE_LARGE = 16;
    public static final int FACE_SYSTEM = 0;
    public static final int FACE_MONOSPACE = 32;
    public static final int FACE_PROPORTIONAL = 64;

    private static Font defaultFont = new Font();

    public static Font getDefaultFont() {
        return defaultFont;
    }

    public static Font getFont(int face, int style, int size) {
        return defaultFont;
    }

    public int getHeight() { return 12; }
    public int getBaselinePosition() { return 10; }
    public int stringWidth(String str) { return str != null ? str.length() * 6 : 0; }
    public int charWidth(char ch) { return 6; }
    public int substringWidth(String str, int offset, int len) { return len * 6; }
}
