package tools;

public class HarnessClk {
    public static long currentTickTime = 1000L;
    public static boolean tickStart = true;

    public static void reset(long start) {
        currentTickTime = start;
        tickStart = true;
    }

    public static long currentTimeMillis() {
        if (tickStart) {
            tickStart = false;
            return currentTickTime;
        } else {
            tickStart = true;
            currentTickTime += 70L;
            return currentTickTime;
        }
    }
}
