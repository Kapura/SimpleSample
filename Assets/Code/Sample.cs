using UnityEngine;
using System.Collections;
using System;

namespace SimpleSample
{
    public struct Sample<T> : IComparable<Sample<T>>
    {
        public T value;
        public float time;

        public Sample(T value, float time)
        {
            this.value = value;
            this.time = time;
        }
    
        public int CompareTo(Sample<T> other)
        {
            if (other.time > this.time)
                return -1;
            if (other.time == this.time)
                return 0;
            return 1;
        }
    }
}