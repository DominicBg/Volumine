using System.Collections;
using System.Collections.Generic;
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

        public static uint3 HashUInt3(float3 v)
        {
            return new uint3()
            {
                x = HashUint(FloatToUInt32Bits(v.x)),
                y = HashUint(FloatToUInt32Bits(v.y)),
                z = HashUint(FloatToUInt32Bits(v.z)),
            };
        }

        public static uint XORHash(uint3 hashInts)
        {
            return hashInts.x ^ hashInts.y ^ hashInts.z;
        }

        public static float RNG(float3 v)
        {
            uint3 hashInts = HashUInt3(v);
            uint xorHash = XORHash(hashInts);
            return HashToFloat(xorHash);
        }

        public static float3 RandomPosFromPos(float3 p)
        {
            uint3 hashInts = HashUInt3(p);
            uint xorHash = XORHash(hashInts);
            var rng = Random.CreateFromIndex(xorHash);
            return rng.NextFloat3();
        }

        public static float PeriodicalCellularNoise(float3 p, float scale)
        {
            p *= scale;
            int3 mainCell = (int3)math.floor(p);
            float minDist = float.MaxValue;

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        int3 cellIndex = mainCell + new int3(x, y, z);
                        float3 cellPos = cellIndex + RandomPosFromPos(Repeat(cellIndex, scale));
                        float dist = math.distance(p, cellPos);
                        minDist = math.min(minDist, dist);
                    }
                }
            }

            return minDist;
        }

        public static float Repeat(float t, float length)
        {
            return math.clamp(t - math.floor(t / length) * length, 0f, length);
        }
        public static float3 Repeat(float3 t, float3 length)
        {
            return math.clamp(t - math.floor(t / length) * length, 0f, length);
        }

        public static IEnumerable<int3> Int3Iterator(int3 from, int3 to, math.RotationOrder order = math.RotationOrder.XYZ)
        {
            for (int x = from.x; x <= to.x; x++)
            {
                for (int y = from.y; y <= to.y; y++)
                {
                    for (int z = from.z; z <= to.z; z++)
                    {
                        int3 i3 = new int3(x, y, z);
                        switch (order)
                        {
                            case math.RotationOrder.XZY:
                                i3 = i3.xzy;
                                break;
                            case math.RotationOrder.YXZ:
                                i3 = i3.yxz;
                                break;
                            case math.RotationOrder.YZX:
                                i3 = i3.yzx;
                                break;
                            case math.RotationOrder.ZXY:
                                i3 = i3.zxy;
                                break;
                            case math.RotationOrder.ZYX:
                                i3 = i3.zyx;
                                break;
                        }
                        yield return i3;
                    }
                }
            }
        }
    }
}