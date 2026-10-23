package tools;

import java.awt.image.BufferedImage;
import java.io.*;
import java.lang.reflect.Field;
import java.lang.reflect.Method;
import java.util.*;
import javax.imageio.ImageIO;
import javax.microedition.lcdui.Canvas;
import javax.microedition.lcdui.Graphics;
import javax.microedition.media.Manager;

public class Harness {

    public static Field findField(Class<?> cls, String name, Class<?> type) {
        for (Field f : cls.getDeclaredFields()) {
            if (f.getName().equals(name) && f.getType() == type) {
                f.setAccessible(true);
                return f;
            }
        }
        for (Field f : cls.getFields()) {
            if (f.getName().equals(name) && f.getType() == type) {
                f.setAccessible(true);
                return f;
            }
        }
        throw new RuntimeException("Field not found: " + cls.getName() + "." + name + " of type " + type.getName());
    }

    public static void main(String[] args) throws Exception {
        int level = 1;
        int character = 0; // 0=Red
        int totalTicks = 100;
        long seed = 12345L;
        String scriptPath = null;
        String tracePath = null;
        String framesDir = null;

        for (int i = 0; i < args.length; i++) {
            if ("--level".equals(args[i]) && i + 1 < args.length) level = Integer.parseInt(args[++i]);
            else if ("--character".equals(args[i]) && i + 1 < args.length) character = Integer.parseInt(args[++i]);
            else if ("--ticks".equals(args[i]) && i + 1 < args.length) totalTicks = Integer.parseInt(args[++i]);
            else if ("--seed".equals(args[i]) && i + 1 < args.length) seed = Long.parseLong(args[++i]);
            else if ("--script".equals(args[i]) && i + 1 < args.length) scriptPath = args[++i];
            else if ("--trace".equals(args[i]) && i + 1 < args.length) tracePath = args[++i];
            else if ("--frames".equals(args[i]) && i + 1 < args.length) framesDir = args[++i];
        }

        File extractedDir = new File("extracted");
        HarnessClassLoader loader = new HarnessClassLoader(extractedDir, Harness.class.getClassLoader());

        // Setup Audio Logger
        final List<String> audioLogs = new ArrayList<>();
        Manager.logger = new Manager.AudioEventLogger() {
            public void onSoundEvent(String type, int loopCount) {
                audioLogs.add(HarnessClk.currentTickTime + ":" + type + ":" + loopCount);
            }
        };

        // Create Graphics Context (240x320)
        BufferedImage screen = new BufferedImage(240, 320, BufferedImage.TYPE_INT_ARGB);
        Graphics g = new Graphics(screen.createGraphics(), 240, 320);
        Canvas.activeGraphics = g;

        // Load classes via custom loader
        Class<?> midletCls = loader.loadClass("GMIDlet");
        Class<?> gCls = loader.loadClass("g");
        Class<?> fCls = loader.loadClass("f");
        Class<?> aCls = loader.loadClass("a");

        Object midlet = midletCls.getDeclaredConstructor().newInstance();
        Object gCanvas = gCls.getDeclaredConstructor(midletCls).newInstance(midlet);

        // Run g.m() to load all resource packs
        Method mMethod = gCls.getDeclaredMethod("m");
        mMethod.setAccessible(true);
        mMethod.invoke(gCanvas);

        // Configure Game Engine f
        Field gLevelField = findField(fCls, "G", int.class);
        gLevelField.setInt(null, level);

        Field fChar = findField(fCls, "F", int.class);
        fChar.setInt(null, character);

        // Fixed Seed PRNG
        Field rngField = findField(fCls, "a", Random.class);
        rngField.set(null, new Random(seed));

        // Instantiate f
        Object game = fCls.getDeclaredConstructor(gCls).newInstance(gCanvas);
        Field activeGameField = findField(fCls, "a", fCls);
        activeGameField.set(null, game);
        Field gActiveGameField = findField(gCls, "a", fCls);
        gActiveGameField.set(null, game);

        // Initialize Level (f.e())
        Method eMethod = fCls.getDeclaredMethod("e");
        eMethod.setAccessible(true);
        eMethod.invoke(game);

        // Re-seed Random after level loading so gameplay RNG starts identically
        rngField.set(null, new Random(seed));

        // Methods for tick, draw, input
        Method tickMethod = fCls.getDeclaredMethod("b");
        tickMethod.setAccessible(true);
        Method drawMethod = fCls.getDeclaredMethod("d");
        drawMethod.setAccessible(true);
        Method pressMethod = fCls.getDeclaredMethod("a", int.class);
        pressMethod.setAccessible(true);
        Method releaseMethod = fCls.getDeclaredMethod("b", int.class);
        releaseMethod.setAccessible(true);

        // Parse input script if supplied: map of tick -> list of commands
        Map<Integer, List<int[]>> inputSchedule = new HashMap<>();
        if (scriptPath != null && new File(scriptPath).exists()) {
            try (BufferedReader br = new BufferedReader(new FileReader(scriptPath))) {
                String line;
                while ((line = br.readLine()) != null) {
                    line = line.trim();
                    if (line.isEmpty() || line.startsWith("#")) continue;
                    String[] parts = line.split("\\s+");
                    int t = Integer.parseInt(parts[0]);
                    String cmd = parts[1];
                    int key = Integer.parseInt(parts[2]);
                    inputSchedule.computeIfAbsent(t, k -> new ArrayList<>()).add(new int[]{"press".equalsIgnoreCase(cmd) ? 1 : 0, key});
                }
            }
        }

        PrintWriter traceOut = null;
        if (tracePath != null) {
            File tf = new File(tracePath);
            if (tf.getParentFile() != null) tf.getParentFile().mkdirs();
            traceOut = new PrintWriter(new FileWriter(tf));
            traceOut.println("# Power Rangers Trace: Level=" + level + ", Character=" + character + ", Seed=" + seed);
            traceOut.println("# tick,px,py,vx,vy,state,frame,hp,max_hp,magic,score,cam_x,cam_y,enemy_count,e0_x,e0_y");
        }

        if (framesDir != null) {
            new File(framesDir).mkdirs();
        }

        // Fields to inspect for trace
        Field playerField = findField(fCls, "f", aCls);
        Field camXField = findField(fCls, "ac", int.class);
        Field camYField = findField(fCls, "ad", int.class);
        Field enemiesField = findField(fCls, "g", java.lang.reflect.Array.newInstance(aCls, 0).getClass());
        Field magicField = findField(fCls, "an", int.class);
        Field scoreField = findField(gCls, "g", int.class);

        Field posX = findField(aCls, "d", int.class);
        Field posY = findField(aCls, "e", int.class);
        Field velX = findField(aCls, "f", int.class);
        Field velY = findField(aCls, "g", int.class);
        Field stateT = findField(aCls, "t", int.class);
        Field frameX = findField(aCls, "x", int.class);
        Field curHp = findField(aCls, "z", int.class);
        Field maxHp = findField(aCls, "A", int.class);

        HarnessClk.reset(1000L);

        // Main Tick Loop
        for (int tick = 0; tick < totalTicks; tick++) {
            // Dispatch input events
            List<int[]> events = inputSchedule.get(tick);
            if (events != null) {
                for (int[] ev : events) {
                    if (ev[0] == 1) {
                        pressMethod.invoke(game, ev[1]);
                    } else {
                        releaseMethod.invoke(game, ev[1]);
                    }
                }
            }

            // Execute Tick
            boolean done = (Boolean) tickMethod.invoke(game);

            // Record Trace
            if (traceOut != null) {
                Object player = playerField.get(null);
                int px = player != null ? posX.getInt(player) : 0;
                int py = player != null ? posY.getInt(player) : 0;
                int vx = player != null ? velX.getInt(player) : 0;
                int vy = player != null ? velY.getInt(player) : 0;
                int st = player != null ? stateT.getInt(player) : 0;
                int fr = player != null ? frameX.getInt(player) : 0;
                int hp = player != null ? curHp.getInt(player) : 0;
                int mhp = player != null ? maxHp.getInt(player) : 0;
                int mg = magicField.getInt(game);
                int sc = scoreField.getInt(null);
                int cx = camXField.getInt(game);
                int cy = camYField.getInt(game);

                Object[] enemies = (Object[]) enemiesField.get(game);
                int eCount = enemies != null ? enemies.length : 0;
                int e0x = (eCount > 0 && enemies[0] != null) ? posX.getInt(enemies[0]) : 0;
                int e0y = (eCount > 0 && enemies[0] != null) ? posY.getInt(enemies[0]) : 0;

                traceOut.printf("%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d%n",
                        tick, px, py, vx, vy, st, fr, hp, mhp, mg, sc, cx, cy, eCount, e0x, e0y);
            }

            // Record Frame if requested
            if (framesDir != null && (tick % 25 == 0 || tick == totalTicks - 1)) {
                drawMethod.invoke(game);
                File frameFile = new File(framesDir, String.format("frame_%04d.png", tick));
                ImageIO.write(screen, "png", frameFile);
            }

            if (done) break;
        }

        if (traceOut != null) {
            traceOut.close();
        }

        System.out.println("Harness execution finished: " + totalTicks + " ticks executed deterministically.");
    }
}
