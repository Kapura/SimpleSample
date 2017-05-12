using UnityEngine;

namespace SimpleSample.Behaviours
{
    public abstract class SampleBehaviour : MonoBehaviour, ISampleProvider
    {
        public abstract int NumSampleStreams { get; }
        public abstract Vector2[] GetSampleStream(int index);
        public abstract Color GetSampleColor(int index);
        public abstract string GetSampleName(int index);
    }
}