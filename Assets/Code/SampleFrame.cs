using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleSample
{
    // Frame to hold draw samples a la the Graph Window in an in-game view
        [ExecuteInEditMode]
    public class SampleFrame : MonoBehaviour
    {
        public LineRenderer lineRenderer;

        public SampleListObject slo;

        private Object[] _sampleObjects;
        private List<ISampleProvider> selectedSamples;

        private void Update()
        {
            if (slo != null)
            {
                // TODO: collect samples from multiple sources
                Vector2[] samples = slo.samples;
                int numSamples = samples.Length;
                
                Vector2 dataMin = new Vector2(Mathf.Infinity, Mathf.Infinity);
                Vector2 dataMax = new Vector2(Mathf.NegativeInfinity, Mathf.NegativeInfinity);
                for (int i = 0; i < numSamples; i++)
                {
                    if (samples[i].x < dataMin.x) dataMin.x = samples[i].x;
                    if (samples[i].y < dataMin.y) dataMin.y = samples[i].y;
                    if (samples[i].x > dataMax.x) dataMax.x = samples[i].x;
                    if (samples[i].y > dataMax.y) dataMax.y = samples[i].y;
                }

                // Normalise point values
                Vector3[] normalisedSamples = new Vector3[numSamples];
                for (int i = 0; i < numSamples; i++)
                {
                    normalisedSamples[i] = new Vector3(1 - Mathf.InverseLerp(dataMin.x, dataMax.x, samples[i].x), Mathf.InverseLerp(dataMin.y, dataMax.y, samples[i].y));
                }

                Color sampleColor = slo.GetSampleColor(0);
                lineRenderer.startColor = sampleColor;
                lineRenderer.endColor = sampleColor;

                lineRenderer.colorGradient.colorKeys = new GradientColorKey[2] {
                        new GradientColorKey(sampleColor, 0),
                        new GradientColorKey(sampleColor, 1) };

                lineRenderer.positionCount = numSamples;
                lineRenderer.SetPositions(normalisedSamples);
            }
        }
    }
}