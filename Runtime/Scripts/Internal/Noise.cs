using UnityEngine;
using Unity.Mathematics;

namespace Volumine
{
    public enum NoiseType { Worley, WorleyPeriodic, ClassicNoise, SimplexNoise, PeriodicNoise, Random }

    [System.Serializable]
    public struct Noise
    {
        public float amplitude;
        public float scale;
        public float3 offset;
        public NoiseType type;
        public bool isInverted;

        [Range(0, 5)]
        public int fbmCount;

        public float CalculateValue(float3 pos)
        {
            float sum = 0;
            float freq = 1;
            float amp = 1;

            //Account for when the value is 0
            for (int i = 0; i <= fbmCount; i++)
            {
                float noiseValue = CalculateNoise(pos + offset, scale * freq);
                //normalize [-1, 1] to [0, 1]
                noiseValue = noiseValue * 0.5f + 0.5f;
                if (isInverted)
                    noiseValue = 1 - noiseValue;

                sum += amp * amplitude * math.saturate(noiseValue);
                freq *= 2;
                amp *= 0.5f;
            }
            return sum;
        }

        float CalculateNoise(float3 pos, float scale)
        {
            switch (type)
            {
                case NoiseType.Worley:
                    return noise.cellular(pos * scale).x;
                case NoiseType.ClassicNoise:
                    return noise.cnoise(pos * scale);
                case NoiseType.SimplexNoise:
                    return noise.snoise(pos * scale);
                case NoiseType.Random:
                    return MathUtils.RNG(pos * scale);
                case NoiseType.WorleyPeriodic:
                    return MathUtils.PeriodicalCellularNoise(pos, scale);
                case NoiseType.PeriodicNoise:
                    return noise.pnoise(pos * scale, scale);
            }

            return 0;
        }
    }
}