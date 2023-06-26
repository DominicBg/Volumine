using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Volumine;

public static class VolumineAPI
{
    public static Texture2D GenerateTexture2D(int2 sizes, Noise[] noises)
    {
        Texture2D texture = new Texture2D(sizes.x, sizes.y, TextureFormat.RGBAFloat, false);
        NativeArray<float> outputs = new NativeArray<float>(sizes.x * sizes.y, Allocator.TempJob);
        NativeArray<Noise> nativeNoises = new NativeArray<Noise>(noises, Allocator.TempJob);

        new RenderNoiseTextureJob()
        {
            noises = nativeNoises,
            outputs = outputs,
            sizes = new int3(sizes.x, sizes.y, 1)
        }.Schedule(outputs.Length, 16).Complete();

        for (int x = 0; x < sizes.x; x++)
        {
            for (int y = 0; y < sizes.y; y++)
            {
                texture.SetPixel(x, y, Color.white * outputs[x + y * sizes.x]);
            }
        }

        outputs.Dispose();
        nativeNoises.Dispose();

        texture.Apply();

        return texture;
    }

    public static Texture3D GenerateTexture3D(int3 sizes, Noise[] noises)
    {
        Texture3D texture = new Texture3D(sizes.x, sizes.y, sizes.z, TextureFormat.RGBAFloat, false);

        NativeArray<float> outputs = new NativeArray<float>(sizes.x * sizes.y * sizes.z, Allocator.TempJob);
        NativeArray<Noise> nativeNoises = new NativeArray<Noise>(noises, Allocator.TempJob);

        new RenderNoiseTextureJob()
        {
            noises = nativeNoises,
            outputs = outputs,
            sizes = sizes,
            is3D = true
        }.Schedule(outputs.Length, 16).Complete();

        for (int z = 0; z < sizes.z; z++)
        {
            for (int y = 0; y < sizes.y; y++)
            {
                for (int x = 0; x < sizes.x; x++)
                {
                    int index = MathUtils.To1D(new int3(x, y, z), sizes);
                    texture.SetPixel(x, y, z, Color.white * outputs[index]);
                }
            }
        }
        outputs.Dispose();
        nativeNoises.Dispose();

        texture.Apply();

        return texture;
    }

    public static Texture2D GenerateTexture32x32x32(Noise[] noises)
    {
        int3 sizes = 32;
        Texture2D texture = new Texture2D(sizes.x * sizes.z, sizes.y, TextureFormat.RGBAFloat, false);
        NativeArray<float> outputs = new NativeArray<float>(sizes.x * sizes.y * sizes.z, Allocator.TempJob);
        NativeArray<Noise> nativeNoises = new NativeArray<Noise>(noises, Allocator.TempJob);

        new RenderNoiseTextureJob()
        {
            noises = nativeNoises,
            outputs = outputs,
            sizes = sizes,
            is3D = true
        }.Schedule(outputs.Length, 16).Complete();

        for (int x = 0; x < sizes.x; x++)
        {
            for (int z = 0; z < sizes.z; z++)
            {
                for (int y = 0; y < sizes.y; y++)
                {
                    int index = MathUtils.To1D(new int3(x, y, z), sizes);
                    texture.SetPixel(x + (sizes.x * z), y, Color.white * outputs[index]);
                }
            }
        }
        outputs.Dispose();
        nativeNoises.Dispose();

        texture.Apply();

        return texture;
    }
}
