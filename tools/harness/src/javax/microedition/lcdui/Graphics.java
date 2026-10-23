package javax.microedition.lcdui;

import java.awt.Color;
import java.awt.Graphics2D;
import java.awt.Rectangle;
import java.awt.geom.AffineTransform;
import java.awt.image.BufferedImage;

public class Graphics {
    public static final int HCENTER = 1;
    public static final int VCENTER = 2;
    public static final int LEFT = 4;
    public static final int RIGHT = 8;
    public static final int TOP = 16;
    public static final int BOTTOM = 32;
    public static final int BASELINE = 64;

    private Graphics2D g2;
    private int transX = 0;
    private int transY = 0;
    private int currentColor = 0;
    private Font currentFont = Font.getDefaultFont();
    private Rectangle clipRect;

    public Graphics(Graphics2D g2, int width, int height) {
        this.g2 = g2;
        this.clipRect = new Rectangle(0, 0, width, height);
        this.g2.setClip(this.clipRect);
    }

    public Graphics2D getG2D() {
        return g2;
    }

    public void translate(int x, int y) {
        this.transX += x;
        this.transY += y;
        this.g2.translate(x, y);
    }

    public int getTranslateX() {
        return this.transX;
    }

    public int getTranslateY() {
        return this.transY;
    }

    public void setColor(int RGB) {
        this.currentColor = RGB & 0xFFFFFF;
        this.g2.setColor(new Color(this.currentColor));
    }

    public void setColor(int r, int g, int b) {
        setColor((r << 16) | (g << 8) | b);
    }

    public int getColor() {
        return this.currentColor;
    }

    public void setClip(int x, int y, int width, int height) {
        this.clipRect = new Rectangle(x, y, width, height);
        this.g2.setClip(x, y, width, height);
    }

    public void clipRect(int x, int y, int width, int height) {
        this.g2.clipRect(x, y, width, height);
        java.awt.Shape clip = this.g2.getClip();
        if (clip != null) {
            this.clipRect = clip.getBounds();
        }
    }

    public int getClipX() {
        return this.clipRect != null ? (int)this.clipRect.getX() : 0;
    }

    public int getClipY() {
        return this.clipRect != null ? (int)this.clipRect.getY() : 0;
    }

    public int getClipWidth() {
        return this.clipRect != null ? (int)this.clipRect.getWidth() : 0;
    }

    public int getClipHeight() {
        return this.clipRect != null ? (int)this.clipRect.getHeight() : 0;
    }

    public void fillRect(int x, int y, int width, int height) {
        this.g2.fillRect(x, y, width, height);
    }

    public void drawLine(int x1, int y1, int x2, int y2) {
        this.g2.drawLine(x1, y1, x2, y2);
    }

    public void drawRect(int x, int y, int width, int height) {
        this.g2.drawRect(x, y, width, height);
    }

    public void drawImage(Image img, int x, int y, int anchor) {
        if (img == null || img.getBufferedImage() == null) return;
        BufferedImage bi = img.getBufferedImage();
        int w = bi.getWidth();
        int h = bi.getHeight();
        int dx = x;
        int dy = y;

        if ((anchor & HCENTER) != 0) dx -= w / 2;
        else if ((anchor & RIGHT) != 0) dx -= w;

        if ((anchor & VCENTER) != 0) dy -= h / 2;
        else if ((anchor & BOTTOM) != 0) dy -= h;

        this.g2.drawImage(bi, dx, dy, null);
    }

    public void drawRegion(Image src, int x_src, int y_src, int width, int height, 
                           int transform, int x_dest, int y_dest, int anchor) {
        if (src == null || src.getBufferedImage() == null) return;
        BufferedImage sub = src.getBufferedImage().getSubimage(x_src, y_src, width, height);

        int destW = width;
        int destH = height;
        if (transform >= 4) { // Rotations involving 90 or 270 swap w and h
            destW = height;
            destH = width;
        }

        int dx = x_dest;
        int dy = y_dest;
        if ((anchor & HCENTER) != 0) dx -= destW / 2;
        else if ((anchor & RIGHT) != 0) dx -= destW;

        if ((anchor & VCENTER) != 0) dy -= destH / 2;
        else if ((anchor & BOTTOM) != 0) dy -= destH;

        AffineTransform at = new AffineTransform();
        at.translate(dx, dy);

        // 0=NONE, 1=MIRROR_ROT180, 2=MIRROR, 3=ROT180, 4=MIRROR_ROT270, 5=ROT90, 6=ROT270, 7=MIRROR_ROT90
        switch (transform) {
            case 1: // MIRROR_ROT180 (Vertical flip)
                at.translate(0, height);
                at.scale(1, -1);
                break;
            case 2: // MIRROR (Horizontal flip)
                at.translate(width, 0);
                at.scale(-1, 1);
                break;
            case 3: // ROT180
                at.translate(width, height);
                at.scale(-1, -1);
                break;
            case 5: // ROT90
                at.translate(height, 0);
                at.rotate(Math.PI / 2);
                break;
            case 6: // ROT270
                at.translate(0, width);
                at.rotate(-Math.PI / 2);
                break;
        }

        this.g2.drawImage(sub, at, null);
    }

    public void drawString(String str, int x, int y, int anchor) {
        if (str == null) return;
        this.g2.drawString(str, x, y);
    }

    public void setFont(Font font) {
        this.currentFont = font;
    }

    public Font getFont() {
        return this.currentFont;
    }
}
