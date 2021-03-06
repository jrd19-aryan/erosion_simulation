using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NoiseMapGPU
{
    public static float[,] NoiseMapGenerator(int sizeX, int sizeY, float scale, float maxHeight, int octaves, float persis, float lac, int seed, Vector2 offset, ComputeShader NoiseMapShader)
    {
        float[,] map = new float[sizeX, sizeY];
        float[] mapArr = new float[sizeX * sizeY];

        Vector2[] RandOff = new Vector2[octaves];

        System.Random prng = new System.Random(seed);
        for (int i = 0; i < octaves; i++)
        {
            RandOff[i].x = prng.Next(-100000, 100000);
            RandOff[i].y = prng.Next(-100000, 100000);
        }

        int floatToIntMultiplier = 1000;
        int[] minMaxHeight = { floatToIntMultiplier * octaves, 0 };

        ComputeBuffer rndOffsetsBuffer = new ComputeBuffer(RandOff.Length, 2 * sizeof(float));
        rndOffsetsBuffer.SetData(RandOff);
        NoiseMapShader.SetBuffer(0, "RandOff", rndOffsetsBuffer);

        ComputeBuffer mapBuffer = new ComputeBuffer(mapArr.Length, sizeof(float));
        mapBuffer.SetData(mapArr);
        NoiseMapShader.SetBuffer(0, "map", mapBuffer);

        ComputeBuffer minMaxBuffer = new ComputeBuffer(minMaxHeight.Length, sizeof(int));
        minMaxBuffer.SetData(minMaxHeight);
        NoiseMapShader.SetBuffer(0, "minMax", minMaxBuffer);

        float[] offsets = { offset.x, offset.y };
        NoiseMapShader.SetFloats("offset", offsets);

        NoiseMapShader.SetFloat("lac", lac);
        NoiseMapShader.SetFloat("scale", scale);
        NoiseMapShader.SetFloat("persis", persis);
        NoiseMapShader.SetInt("floatToIntMultiplier", floatToIntMultiplier);
        NoiseMapShader.SetInt("octaves", octaves);
        NoiseMapShader.SetInt("sizeX", sizeX);
        NoiseMapShader.SetInt("sizeY", sizeY);

        NoiseMapShader.Dispatch(0, mapArr.Length / 32, 1, 1);

        mapBuffer.GetData(mapArr);
        minMaxBuffer.GetData(minMaxHeight);
        mapBuffer.Release();
        minMaxBuffer.Release();
        rndOffsetsBuffer.Release();

        float minVal = (float)minMaxHeight[0] / (float)floatToIntMultiplier;
        float maxVal = (float)minMaxHeight[1] / (float)floatToIntMultiplier;

        for (int y = 0; y < sizeY; y++)
        {
            for (int x = 0; x < sizeX; x++)
            {
                map[x, y] = maxHeight * Mathf.InverseLerp(minVal, maxVal, mapArr[sizeY * x + y]);
            }
        }

        return map;
    }
}