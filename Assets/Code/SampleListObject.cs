using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleSample
{
    [CreateAssetMenu(fileName = "NewSampleListObject.asset", menuName = "New Sample List")]
    public class SampleListObject : ScriptableObject, ISampleProvider
    {
        public Color color = Color.white;

        public Vector2[] samples;

        public int NumSampleStreams { get { return 1; } }

        public Color GetSampleColor(int index)
        {
            return color;
        }

        public string GetSampleName(int index)
        {
            return this.name;
        }

        public Vector2[] GetSampleStream(int index)
        {
            return samples;
        }
    }
}