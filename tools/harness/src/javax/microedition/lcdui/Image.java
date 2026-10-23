package javax.microedition.lcdui;

import java.awt.Graphics2D;
import java.awt.geom.AffineTransform;
import java.awt.image.BufferedImage;
import java.io.ByteArrayInputStream;
import java.io.IOException;
import java.io.InputStream;
import javax.imageio.ImageIO;

public class Image {
    private BufferedImage bufferedImage;
    private boolean mutable;

    public Image(BufferedImage bi, boolean mutable) {
        this.bufferedImage = bi;
        this.mutable = mutable;
    }

    public BufferedImage getBufferedImage() {
        return bufferedImage;
    }

    public static Image createImage(int width, int height) {
        BufferedImage bi = new BufferedImage(Math.max(1, width), Math.max(1, height), BufferedImage.TYPE_INT_ARGB);
        return new Image(bi, true);
    }

    public static Image createImage(byte[] imageData, int imageOffset, int imageLength) {
        try {
            ByteArrayInputStream bais = new ByteArrayInputStream(imageData, imageOffset, imageLength);
            BufferedImage bi = ImageIO.read(bais);
            if (bi == null) {
                bi = new BufferedImage(1, 1, BufferedImage.TYPE_INT_ARGB);
            }
            return new Image(bi, false);
        } catch (Exception e) {
            return new Image(new BufferedImage(1, 1, BufferedImage.TYPE_INT_ARGB), false);
        }
    }

    public static Image createImage(InputStream stream) throws IOException {
        BufferedImage bi = ImageIO.read(stream);
        if (bi == null) {
            bi = new BufferedImage(1, 1, BufferedImage.TYPE_INT_ARGB);
        }
        return new Image(bi, false);
    }

    public static Image createImage(Image image, int x, int y, int width, int height, int transform) {
        BufferedImage sub = image.bufferedImage.getSubimage(x, y, width, height);
        BufferedImage dest = new BufferedImage(width, height, BufferedImage.TYPE_INT_ARGB);
        Graphics2D g2 = dest.createGraphics();
        
        // 0=TRANS_NONE, 1=TRANS_MIRROR_ROT180, 2=TRANS_MIRROR, 3=TRANS_ROT180, 4=TRANS_MIRROR_ROT270, 5=TRANS_ROT90, 6=TRANS_ROT270, 7=TRANS_MIRROR_ROT90
        AffineTransform at = new AffineTransform();
        if (transform == 2) { // MIRROR
            at.translate(width, 0);
            at.scale(-1, 1);
        } else if (transform == 1) { // MIRROR_ROT180
            at.translate(0, height);
            at.scale(1, -1);
        } else if (transform == 3) { // ROT180
            at.translate(width, height);
            at.scale(-1, -1);
        }
        g2.drawImage(sub, at, null);
        g2.dispose();
        return new Image(dest, false);
    }

    public Graphics getGraphics() {
        return new Graphics(bufferedImage.createGraphics(), bufferedImage.getWidth(), bufferedImage.getHeight());
    }

    public int getWidth() {
        return bufferedImage != null ? bufferedImage.getWidth() : 0;
    }

    public int getHeight() {
        return bufferedImage != null ? bufferedImage.getHeight() : 0;
    }

    public boolean isMutable() {
        return mutable;
    }
}
