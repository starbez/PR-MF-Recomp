package javax.microedition.midlet;

import java.util.HashMap;
import java.util.Map;

public abstract class MIDlet {
    private static Map<String, String> properties = new HashMap<>();

    protected MIDlet() {}

    public static void setAppProperty(String key, String value) {
        properties.put(key, value);
    }

    public String getAppProperty(String key) {
        return properties.get(key);
    }

    public void notifyDestroyed() {}

    public void notifyPaused() {}

    public void resumeRequest() {}

    public boolean platformRequest(String URL) {
        return false;
    }

    public abstract void startApp() throws MIDletStateChangeException;
    public abstract void pauseApp();
    public abstract void destroyApp(boolean unconditional) throws MIDletStateChangeException;
}
