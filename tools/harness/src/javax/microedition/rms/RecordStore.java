package javax.microedition.rms;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

public class RecordStore {
    private static Map<String, RecordStore> stores = new HashMap<>();
    private String name;
    private List<byte[]> records = new ArrayList<>();

    private RecordStore(String name) {
        this.name = name;
        this.records.add(null); // 1-based indexing in MIDP RecordStore
    }

    public static synchronized RecordStore openRecordStore(String recordStoreName, boolean createIfNecessary) 
            throws RecordStoreException {
        RecordStore rs = stores.get(recordStoreName);
        if (rs == null) {
            if (!createIfNecessary) {
                throw new RecordStoreException("Record store not found: " + recordStoreName);
            }
            rs = new RecordStore(recordStoreName);
            stores.put(recordStoreName, rs);
        }
        return rs;
    }

    public void closeRecordStore() {}

    public static synchronized void deleteRecordStore(String recordStoreName) {
        stores.remove(recordStoreName);
    }

    public int getNumRecords() {
        return records.size() - 1;
    }

    public byte[] getRecord(int recordId) throws RecordStoreException {
        if (recordId <= 0 || recordId >= records.size() || records.get(recordId) == null) {
            throw new RecordStoreException("Invalid record id: " + recordId);
        }
        return records.get(recordId).clone();
    }

    public int addRecord(byte[] data, int offset, int numBytes) {
        byte[] copy = new byte[numBytes];
        System.arraycopy(data, offset, copy, 0, numBytes);
        records.add(copy);
        return records.size() - 1;
    }

    public void setRecord(int recordId, byte[] newData, int offset, int numBytes) throws RecordStoreException {
        if (recordId <= 0 || recordId >= records.size()) {
            throw new RecordStoreException("Invalid record id: " + recordId);
        }
        byte[] copy = new byte[numBytes];
        System.arraycopy(newData, offset, copy, 0, numBytes);
        records.set(recordId, copy);
    }

    public int getSize() {
        int total = 0;
        for (int i = 1; i < records.size(); i++) {
            if (records.get(i) != null) total += records.get(i).length;
        }
        return total;
    }
}
