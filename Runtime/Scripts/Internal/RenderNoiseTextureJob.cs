using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;

namespace Volumine
{
    [BurstCompile]
    struct RenderNoiseTextureJob : IJobParallelFor
    {
        public int3 sizes;
        [ReadOnly] public NativeArray<Noise> noises;
        public NativeArray<float> outputs;
        public bool is3D;

        public void Execute(int index)
        {
            float3 uv;
            if (is3D)
            {
                uv = (float3)MathUtils.To3D(index, sizes) / sizes;
            }
            else
            {
                uv = new float3(MathUtils.IndexToPos(index, sizes.xy), 0) / sizes;
            }

            float value = 0;
            for (int i = 0; i < noises.Length; i++)
            {
                value += noises[i].CalculateValue(uv);
            }

            //add multiple channels?
            value = math.saturate(value);
            outputs[index] = value;
        }
    }
}