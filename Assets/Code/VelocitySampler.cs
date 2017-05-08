using UnityEngine;

namespace SimpleSample
{
    public class VelocitySampler : MonoBehaviour, ISampleProvider
    {
        public new Transform transform;

        public Color color = Color.white;

        [Tooltip("If set to 0, sample every tick of FixedUpdate")]
        public float samplePeriod = 0f;
        public int numSamples = 100;
        public int NumSampleStreams { get { return 1; } }
        
        [SerializeField] [HideInInspector]
        private FloatSampler _sampler;
        
        private float _nextSampleTime;
        private Vector3 _lastPosition;

        private void Awake()
        {
            _sampler = new FloatSampler(numSamples);
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
                float velocity = (position - _lastPosition).magnitude / Time.deltaTime;
                _sampler.AddSample(velocity, Time.time);
                _nextSampleTime = Time.time + samplePeriod;
            }
            _lastPosition = position;
        }

        public Vector2[] GetSampleStream(int index)
        {
            return _sampler.GetSortedVectorSamples();
        }

        public Color GetSampleColor(int index)
        {
            return color;
        }

        public string GetSampleName(int index)
        {
            return transform.name;
        }
    }
}