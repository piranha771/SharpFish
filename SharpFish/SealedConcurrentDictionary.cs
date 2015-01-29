using System.Collections.Generic;

namespace SharpFish
{
    /// <summary>
    /// SealedConcurrentDictionary dosen't allow removing/adding elements once created. Threadsafe.
    /// </summary>
    internal class SealedConcurrentDictionary<K,V>
    {
        private int count;
        private object sync = new object();
        private Dictionary<K,V> dict;

        public V this[K key]
        {
            get
            {
                lock (sync)
                {
                    return dict[key];
                }
            }
            set
            {
                lock (sync)
                {
                    dict[key] = value;
                }
            }
        }

        public int Count { get { return count; } }

        public SealedConcurrentDictionary(K[] keys)
        {
            count = keys.Length;
            this.dict = new Dictionary<K, V>(count);
            foreach (var key in keys)
            {
                dict[key] = default(V);
            }
        }

        public bool ContainsKey(K item)
        {
            lock (sync)
            {
                return dict.ContainsKey(item);
            }
        }

        public K[] GetKeysArray()
        {
            lock (this)
            {
                K[] result = new K[count];
                dict.Keys.CopyTo(result, 0);
                return result;
            }
        }
    }
}
