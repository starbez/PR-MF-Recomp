package javax.microedition.media;

import java.io.InputStream;
import javax.microedition.media.control.VolumeControl;

public class Manager {
    public static AudioEventLogger logger;

    public interface AudioEventLogger {
        void onSoundEvent(String type, int loopCount);
    }

    public static Player createPlayer(final InputStream stream, final String type) {
        return new Player() {
            private int loopCount = 1;
            private VolumeControl vc = new VolumeControl() {
                private int level = 100;
                public int setLevel(int l) { this.level = l; return level; }
                public int getLevel() { return level; }
                public void setMute(boolean m) {}
                public boolean isMuted() { return false; }
            };

            public void realize() {}
            public void prefetch() {}
            public void start() {
                if (logger != null) {
                    logger.onSoundEvent(type, loopCount);
                }
            }
            public void stop() {}
            public void deallocate() {}
            public void close() {}
            public void setLoopCount(int count) { this.loopCount = count; }
            public Control getControl(String controlType) {
                if ("VolumeControl".equals(controlType)) return vc;
                return null;
            }
            public Control[] getControls() {
                return new Control[]{vc};
            }
            public void addPlayerListener(PlayerListener pl) {}
            public void removePlayerListener(PlayerListener pl) {}
        };
    }
}
