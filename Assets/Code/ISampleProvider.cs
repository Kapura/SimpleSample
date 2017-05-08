using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleSample
{
    public interface ISampleProvider
    {
        int NumSampleStreams { get; }
        Vector2[] GetSampleStream(int index);
        Color GetSampleColor(int index);
        string GetSampleName(int index);
    }
}