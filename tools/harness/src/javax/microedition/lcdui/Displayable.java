package javax.microedition.lcdui;

public abstract class Displayable {
    protected int width = 240;
    protected int height = 320;
    private boolean isShown = false;

    public int getWidth() {
        return width;
    }

    public int getHeight() {
        return height;
    }

    public boolean isShown() {
        return isShown;
    }

    void setShown(boolean shown) {
        this.isShown = shown;
    }
}
