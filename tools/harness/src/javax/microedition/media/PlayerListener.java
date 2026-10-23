package javax.microedition.media;

public interface PlayerListener {
    public static final String STARTED = "started";
    public static final String STOPPED = "stopped";
    public static final String END_OF_MEDIA = "endOfMedia";

    public void playerUpdate(Player player, String event, Object eventData);
}
