package tools;

import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;

public class HarnessClassLoader extends ClassLoader {
    private File classDir;

    public HarnessClassLoader(File classDir, ClassLoader parent) {
        super(parent);
        this.classDir = classDir;
    }

    @Override
    protected Class<?> findClass(String name) throws ClassNotFoundNotFoundException {
        File f = new File(classDir, name.replace('.', '/') + ".class");
        if (!f.exists()) {
            throw new ClassNotFoundNotFoundException(name);
        }
        try {
            byte[] data = new byte[(int) f.length()];
            try (FileInputStream fis = new FileInputStream(f)) {
                int read = 0;
                while (read < data.length) {
                    read += fis.read(data, read, data.length - read);
                }
            }

            if ("f".equals(name)) {
                byte[] target = "java/lang/System".getBytes("UTF-8");
                byte[] repl = "tools/HarnessClk".getBytes("UTF-8");
                for (int i = 0; i <= data.length - target.length; i++) {
                    boolean match = true;
                    for (int j = 0; j < target.length; j++) {
                        if (data[i + j] != target[j]) {
                            match = false;
                            break;
                        }
                    }
                    if (match) {
                        System.arraycopy(repl, 0, data, i, repl.length);
                    }
                }
            }

            return defineClass(name, data, 0, data.length);
        } catch (IOException e) {
            throw new ClassNotFoundNotFoundException(name, e);
        }
    }
}
class ClassNotFoundNotFoundException extends ClassNotFoundException {
    public ClassNotFoundNotFoundException(String s) { super(s); }
    public ClassNotFoundNotFoundException(String s, Throwable t) { super(s, t); }
}
