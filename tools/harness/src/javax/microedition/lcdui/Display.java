package javax.microedition.lcdui;

import javax.microedition.midlet.MIDlet;

public class Display {
    private static Display instance = new Display();
    private Displayable current;

    private Display() {}

    public static Display getDisplay(MIDlet m) {
        return instance;
    }

    public void setCurrent(Displayable nextDisplayable) {
        if (this.current != null) {
            this.current.setShown(false);
        }
        this.current = nextDisplayable;
        if (this.current != null) {
            this.current.setShown(true);
        }
    }

    public Displayable getCurrent() {
        return this.current;
    }

    public void callSerially(Runnable r) {
        if (r != null) {
            r.run();
        }
    }
}
