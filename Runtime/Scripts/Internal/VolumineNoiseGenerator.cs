using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

namespace Volumine
{
    [CreateAssetMenu(fileName = "Volumine Noise Generator", menuName = "Volumine/Volumine Noise Generator", order = 1)]
    public class VolumineNoiseGenerator : ScriptableObject
    {
        public int3 sizes = 32;
        public Noise[] noises = DefaultNoises;

        public static Noise[] DefaultNoises =>
            new Noise[]
            {
                new Noise()
                {
                    amplitude = .5f,
                    isInverted = true,
                    scale = 1,
                    type = NoiseType.Worley,
                    fbmCount = 3
                },
                new Noise()
                {
                    amplitude = -.25f,
                    isInverted = true,
                    scale = 3,
                    type = NoiseType.ClassicNoise
                },

            };

        [HideInInspector] public UnityEvent OnValidateCallback = new UnityEvent();
        private void OnValidate()
        {
            OnValidateCallback.Invoke();
        }
    }
}