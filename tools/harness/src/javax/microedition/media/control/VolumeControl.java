package javax.microedition.media.control;

public interface VolumeControl extends javax.microedition.media.Control {
    public int setLevel(int level);
    public int getLevel();
    public void setMute(boolean mute);
    public boolean isMuted();
}
