using UnityEngine;

namespace SimpleSample
{
    public class Vector3Sampler : MonoBehaviour, ISampleProvider
    {
        public new Transform transform;

        public Color colorX = Color.red;
        public Color colorY = Color.green;
        public Color colorZ = Color.blue;
        public Color colorMagnitude = Color.white;

        [Tooltip("If set to 0, sample every tick of FixedUpdate")]
        public float samplePeriod = 0f;
        public int numSamples = 100;
        public int NumSampleStreams { get { return 4; } }
        
        [SerializeField] [HideInInspector]
        private FloatSampler _samplerX;
        [SerializeField] [HideInInspector]
        private FloatSampler _samplerY;
        [SerializeField] [HideInInspector]
        private FloatSampler _samplerZ;
        [SerializeField] [HideInInspector]
        private FloatSampler _samplerMagnitude;
        
        private float _nextSampleTime;
        private Vector3 _lastPosition;

        private void Awake()
        {
            _samplerX = new FloatSampler(numSamples);
            _samplerY = new FloatSampler(numSamples);
            _samplerZ = new FloatSampler(numSamples);
            _samplerMagnitude = new FloatSampler(numSamples);
            _nextSampleTime = Time.time + samplePeriod;
            _lastPosition = transform.localPosition;
        }

        private void Reset()
        {
            transform = GetComponent<Transform>();
        }
        
        private void FixedUpdate()
        {
            Vector3 position = transform.localPosition;
            if (Time.time > _nextSampleTime)
            {
                Vector3 delta = (position - _lastPosition) / Time.deltaTime;

                _samplerX.AddSample(delta.x, Time.time);
                _samplerY.AddSample(delta.y, Time.time);
                _samplerZ.AddSample(delta.z, Time.time);
                _samplerMagnitude.AddSample(delta.magnitude, Time.time);
                _nextSampleTime = Time.time + samplePeriod;
            }
            _lastPosition = position;
        }

        public Vector2[] GetSampleStream(int index)
        {
            switch (index)
            {
                case 0:
                    return _samplerX.GetSortedVectorSamples();
                case 1:
                    return _samplerY.GetSortedVectorSamples();
                case 2:
                    return _samplerZ.GetSortedVectorSamples();
                case 3:
                    return _samplerMagnitude.GetSortedVectorSamples();
                default:
                    throw new System.IndexOutOfRangeException();
            }
        }

        public Color GetSampleColor(int index)
        {
            switch (index)
            {
                case 0:
                    return colorX;
                case 1:
                    return colorY;
                case 2:
                    return colorZ;
                case 3:
                    return colorMagnitude;
                default:
                    throw new System.IndexOutOfRangeException();
            }
        }

        public string GetSampleName(int index)
        {
            switch (index)
            {
                case 0:
                    return string.Format("{0}.x", transform.name);
                case 1:
                    return string.Format("{0}.y", transform.name);
                case 2:
                    return string.Format("{0}.z", transform.name);
                case 3:
                    return string.Format("{0}.magnitude", transform.name);
                default:
                    throw new System.IndexOutOfRangeException();
            }
        }
    }
}