using System.Collections.Concurrent;
using UnityEngine;

namespace cynofield.mods.ui.things
{
    public class HistoricalData<T>
    {
        private readonly int depthSeconds;
        public HistoricalData(int depthSeconds)
        {
            this.depthSeconds = depthSeconds;
        }

        private ConcurrentDictionary<string, HistoricalRecords<T>> db = new ConcurrentDictionary<string, HistoricalRecords<T>>();

        public DeviceHistory<T> Get(string id)
        {
            var result = db.GetOrAdd(id, delegate (string _) { return new HistoricalRecords<T>(id); });
            return new DeviceHistory<T>(result, depthSeconds);
        }

        public class DeviceHistory<U>
        {
            private readonly HistoricalRecords<U> records;
            private readonly int depthSeconds;
            internal DeviceHistory(HistoricalRecords<U> records, int depthSeconds)
            {
                this.records = records;
                this.depthSeconds = depthSeconds;
            }

            public void Add(U entry)
            {
                var now = Time.time;
                var last = records.last;
                if (last == null || !last.entry.Equals(entry))
                {
                    var record = new HistoricalRecord<U>(now, entry);
                    records.data.Enqueue(record);
                    records.last = record;

                    var from = now - depthSeconds;
                    while (true)
                    {
                        records.data.TryPeek(out HistoricalRecord<U> oldest);
                        if (oldest == null || oldest.timestamp >= from)
                            return;

                        // screw the real concurrency
                        if (!records.data.TryDequeue(out HistoricalRecord<U> _))
                            return;
                    }
                }
            }

            private HistoricalRecord<U> LastRecord()
            {
                return records.last;
            }

            public U Last()
            {
                var last = LastRecord();
                return last == null ? default : last.entry;
            }

            // public int Last(int seconds, U[] array)
            // {
            //     var now = Time.time;
            //     var from = now - seconds;
            //     int count = 0;
            //     foreach (var entry in records.data)
            //     {
            //         if (entry.timestamp >= from)
            //         {
            //             if (count >= array.Length)
            //                 return count;

            //             if (array != null)
            //                 array[count] = entry.entry;
            //             count++;
            //         }
            //     }
            //     return count;
            // }

            public int ChangesInLast(int seconds)
            {
                var now = Time.time;
                var from = now - seconds;
                int count = 0;
                foreach (var entry in records.data)
                {
                    if (entry.timestamp >= from)
                        count++;
                }
                return count;
            }

            public int Size()
            {
                return records.data.Count;
            }
        }

        public class HistoricalRecords<U>
        {
            public string id;
            public HistoricalRecord<U> last;
            public readonly ConcurrentQueue<HistoricalRecord<U>> data = new ConcurrentQueue<HistoricalRecord<U>>();

            public HistoricalRecords(string id)
            {
                this.id = id;
                this.last = null;
            }
        }

        public class HistoricalRecord<U>
        {
            public float timestamp;
            public U entry;

            public HistoricalRecord(float timestamp, U entry)
            {
                this.timestamp = timestamp;
                this.entry = entry;
            }
        }
    }
}
