using UnityEngine;

namespace SimpleSample.Behaviours
{
    public class VelocitySampler : SampleBehaviour
    {
        public Color color = Color.white;

        [Tooltip("If set to 0, sample every tick of FixedUpdate")]
        public float samplePeriod = 0f;
        public int numSamples = 100;
        public override int NumSampleStreams { get { return 1; } }
        
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

        public override Vector2[] GetSampleStream(int index)
        {
            return _sampler.GetSortedVectorSamples();
        }

        public override Color GetSampleColor(int index)
        {
            return color;
        }

        public override string GetSampleName(int index)
        {
            return transform.name;
        }
    }
}