using System.Text;
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

    public static class SampleProviderExtensions
    {
        public static string StreamToCSV(this ISampleProvider provider, int index)
        {
            StringBuilder sb = new StringBuilder();
            Vector2[] stream = provider.GetSampleStream(index);
            int numSamples = stream.Length;
            for (int i = 0; i < numSamples; i++)
            {
                sb.AppendFormat("{0},{1}\n", stream[i].x, stream[i].y);
            }
            return sb.ToString();
        }
    }
}