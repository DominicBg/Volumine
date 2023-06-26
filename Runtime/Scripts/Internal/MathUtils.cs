using Unity.Mathematics;

namespace Volumine
{
    public static class MathUtils
    {
        public static int2 IndexToPos(int i, int2 sizes)
        {
            return new int2(i % sizes.x, i / sizes.y);
        }

        public static int To1D(int3 pos, int3 sizes)
        {
            return (pos.z * sizes.x * sizes.y) + (pos.y * sizes.x) + pos.x;
        }

        public static int3 To3D(int index, int3 sizes)
        {
            int x = index % sizes.x;
            int y = (index / sizes.x) % sizes.y;
            int z = index / (sizes.x * sizes.y);
            return new int3(x, y, z);
        }

        public static unsafe uint FloatToUInt32Bits(float value)
        {
            return *(uint*)(&value);
        }
        public static unsafe float UInt32BitsToFloat(uint value)
        {
            return *(float*)(&value);
        }
        public static uint HashUint(uint x)
        {
            x += (x << 10);
            x ^= (x >> 6);
            x += (x << 3);
            x ^= (x >> 11);
            x += (x << 15);
            return x;
        }
        public static float HashToFloat(uint hashed_value)
        {
            const uint mantissaMask = 0x007FFFFFu;
            const uint one = 0x3F800000u;

            hashed_value &= mantissaMask;
            hashed_value |= one;

            float r2 = UInt32BitsToFloat(hashed_value);
            return r2 - 1;
        }
        public static float RNG(float3 v)
        {
            uint3 hashInts = new uint3()
            {
                x = HashUint(FloatToUInt32Bits(v.x)),
                y = HashUint(FloatToUInt32Bits(v.y)),
                z = HashUint(FloatToUInt32Bits(v.z)),
            };
            uint xorHash = hashInts.x ^ hashInts.y ^ hashInts.z;
            return HashToFloat(xorHash);
        }
    }
}