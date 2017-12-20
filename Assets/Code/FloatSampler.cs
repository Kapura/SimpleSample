using UnityEngine;
using System;

namespace SimpleSample
{
    [Serializable]
    public class FloatSampler : ISampler<float>
    {
        public readonly int NumSamples;
        
        [SerializeField] [HideInInspector]
        private int _head;
        [SerializeField] [HideInInspector]
        private int _numSamplesCollected;
        [SerializeField] [HideInInspector]
        private Vector2[] _samples;

        public FloatSampler(int numSamples)
        {
            NumSamples = numSamples;
            _head = 0;
            _numSamplesCollected = 0;
            _samples = new Vector2[NumSamples];
        }

        public void AddSample(Sample<float> sample)
        {
            AddSample(sample.value, sample.time);
        }

        public void AddSample(float value, float time)
        {
            if (_numSamplesCollected < NumSamples)
            {
                _samples[_numSamplesCollected] = new Vector2(time, value);
                _numSamplesCollected++;
                return;
            }
            
            _samples[_head].Set(time, value);
            _head++;
            if (_head >= NumSamples)  _head = 0;
        }

        public Sample<float>[] GetSortedSamples()
        {
            int numSamplestoCopy = (_numSamplesCollected < NumSamples) ? _numSamplesCollected : NumSamples;
            
            Sample<float>[] sampleArray = new Sample<float>[numSamplestoCopy];

            if (_numSamplesCollected < NumSamples || _head == 0)
            {
                Array.Copy(_samples, sampleArray, numSamplestoCopy);
            }
            else
            {
                Array.Copy(_samples, _head, sampleArray, 0, numSamplestoCopy - _head);
                Array.Copy(_samples, 0, sampleArray, numSamplestoCopy - _head, _head);
            }

            for (int i = 0; i < numSamplestoCopy; i++)
            {
                sampleArray[i] = new Sample<float>(_samples[i].x, _samples[i].y);
            }
            
            return sampleArray;
        }

        public Vector2[] GetSortedVectorSamples()
        {
            int numSamplestoCopy = (_numSamplesCollected < NumSamples) ? _numSamplesCollected : NumSamples;

            Vector2[] sampleArray = new Vector2[numSamplestoCopy];
            for (int i = 0; i < numSamplestoCopy; i++)
            {
                sampleArray[i] = new Vector2(_samples[i].x, _samples[i].y);
            }

            Array.Sort<Vector2>(sampleArray,
                (Vector2 a, Vector2 b) =>
                {
                    if (a.x < b.x) return -1;
                    else if (a.x == b.x) return 0;
                    else return 1;
                });
            return sampleArray;
        }
    }
}