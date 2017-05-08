using UnityEngine;
using System.Collections;

namespace SimpleSample
{
    public interface ISampler<T>
    {
        void AddSample(Sample<T> sample);
        void AddSample(T value, float time);
        Sample<T>[] GetSortedSamples();
    }
}