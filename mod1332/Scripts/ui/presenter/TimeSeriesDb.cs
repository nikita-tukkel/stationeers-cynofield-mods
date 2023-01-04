using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace cynofield.mods.ui.presenter
{
    public class TimeSeriesDb
    {
        private readonly ConcurrentDictionary<string, TimeSeriesRecord> db = new ConcurrentDictionary<string, TimeSeriesRecord>();

        // TODO note tsr maximum history depth and keep track of latest update time for every tsr.
        //  Remove tsr if it wasn't updated for too long.
        public void Add(string id, TimeSeriesRecord tsr)
        {
            if (id == null)
                throw new Exception("null id of time series record is not allowed");
            db[id] = tsr ?? throw new Exception("null time series record is not allowed");
        }

        public TimeSeriesRecord Get(string id, TimeSeriesRecordCreator creator)
        {
            if (id == null)
                throw new Exception("null id of time series record is not allowed");
            return db.GetOrAdd(id, delegate (string _) { return creator(); });
        }

        public delegate TimeSeriesRecord TimeSeriesRecordCreator();
    }

    public class TimeSeriesRecord
    {
        private readonly ConcurrentDictionary<string, object> buffers = new ConcurrentDictionary<string, object>();

        public TimeSeriesBuffer<T> Add<T>(string id, TimeSeriesBuffer<T> tsb)
        {
            if (id == null)
                throw new Exception("null id of time series buffer is not allowed");
            buffers[id] = tsb ?? throw new Exception("null buffer is not allowed");
            return tsb;
        }

        public TimeSeriesBuffer<T> Get<T>(string id)
        {
            var entry = buffers[id]; // will throw System.Collections.Generic.KeyNotFoundException
            if (entry == null)
            {
                throw new Exception($"Unexpected null TimeSeriesBuffer for buffers[{id}]");
            }
            if (!(entry is TimeSeriesBuffer<T> result))
            {
                throw new Exception($"{entry} is not compatible with {typeof(T)}");
            }
            return result;
        }
    }

    public class TimeSeriesBuffer<T>
    {
        private readonly float resolution;
        private readonly T[] buffer;
        public readonly Type type;
        private readonly Meta?[] meta;
        private int position = -1;
        private readonly TimeProvider timeProvider;
        public TimeSeriesBuffer(T[] buffer, float resolution = 0.5f, TimeProvider timeProvider = null)
        {
            this.resolution = resolution;
            this.buffer = buffer;
            this.type = typeof(T);
            this.meta = new Meta?[buffer.Length];
            this.timeProvider = timeProvider ?? (() => Time.time);
        }

        public delegate float TimeProvider();
        private float GetTime() => timeProvider();

        public void Add(T v) => Add(v, GetTime());
        public void Add(T v, float now)
        {
            lock (this)
            {
                if (IsEmpty())
                { // just add the first record
                    AddInternal(v, now, now, 0);
                }
                else
                {
                    T currentValue = buffer[position];
                    if (v.Equals(currentValue))
                        return;

                    Meta currentMeta = (Meta)meta[position]; // expect not null
                    if (currentMeta.timestamp >= now)
                        return; // not adding values from the past

                    if (currentMeta.timeframe + resolution > now)
                    {
                        // update current timeframe
                        UpdateInternal(v, now, currentMeta.timeframe);
                    }
                    else
                    {
                        var newpos = NextPosInternal(position);
                        AddInternal(v, now, now, newpos);
                    }
                }
            }
        }

        private bool IsEmpty() => position < 0 || position >= buffer.Length;

        private int NextPosInternal(int i)
        {
            if (i < 0 || i >= buffer.Length)
                return -1;

            if (++i >= buffer.Length)
                i = 0;
            return i;
        }

        private int PrevPosInternal(int i)
        {
            if (i < 0 || i >= buffer.Length)
                return -1;

            if (--i < 0)
                i = buffer.Length - 1;
            return i;
        }

        private void AddInternal(T v, float now, float timeframe, int newpos)
        {
            buffer[newpos] = v;
            meta[newpos] = new Meta()
            {
                timeframe = timeframe,
                timestamp = now
            };
            position = newpos;
        }

        private void UpdateInternal(T v, float now, float timeframe)
        {
            buffer[position] = v;
            meta[position] = new Meta()
            {
                timeframe = timeframe,
                timestamp = now
            };
        }

        public T Current { get => GetCurrent().Item2; }
        public T Prev { get => GetPrev(position).Item2; }

        public (Meta?, T, int) GetCurrent()
        {
            var i = position;
            if (i < 0 || i >= buffer.Length)
                return (null, default, -1);

            return (meta[i], buffer[i], i);
        }

        public (Meta?, T, int) GetPrev() => GetPrev(position);
        public (Meta?, T, int) GetPrev(int i)
        {
            i = PrevPosInternal(i);
            if (i < 0 || i >= buffer.Length)
                return (null, default, -1);
            Meta? mprev = meta[i];
            if (mprev == null)
                return (null, default, -1);
            return (meta[i], buffer[i], i);
        }

        public float ChangeAge() => ChangeAge(GetTime());
        public float ChangeAge(float now)
        {
            var (m, _, _) = GetCurrent();
            if (m == null)
                return -1;
            return now - ((Meta)m).timestamp;
        }

        public int ChangesCount(float period) => ChangesCount(period, GetTime());
        public int ChangesCount(float period, float now)
        {
            var from = now - period;

            int changes = 0;
            var (meta, _, pos) = GetCurrent();
            for (var i = 1; i < buffer.Length && meta != null; i++, (meta, _, pos) = GetPrev(pos))
            {
                if (((Meta)meta).timestamp < from)
                    break;
                changes++;
            }
            return changes;
        }

        public override string ToString()
        {
            return $"{resolution}s {type}[{buffer.Length}]";
        }
    }

    public struct Meta
    {
        public float timeframe;
        public float timestamp;

        public override string ToString()
        {
            return $"{timestamp}s";
        }
    }
}
