package javax.microedition.media;

public interface Player extends Controllable {
    public static final int UNREALIZED = 100;
    public static final int REALIZED = 200;
    public static final int PREFETCHED = 300;
    public static final int STARTED = 400;
    public static final int CLOSED = 0;

    public void realize() throws Exception;
    public void prefetch() throws Exception;
    public void start() throws Exception;
    public void stop() throws Exception;
    public void deallocate();
    public void close();
    public void setLoopCount(int count);
    public Control getControl(String controlType);
    public void addPlayerListener(PlayerListener playerListener);
    public void removePlayerListener(PlayerListener playerListener);
}
