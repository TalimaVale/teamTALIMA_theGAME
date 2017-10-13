using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HeightMapGenerator {

    public static HeightMap GenerateHeightMap(int width, int height, HeightMapSettings settings, Vector2 sampleCenter) {
        float[,] values = Noise.GenerateNoiseMap(width, height, settings.noiseSettings, sampleCenter);

        //CUSTOM: for FalloffMap
        if (settings.useFalloff) {
            float[,] falloffMap = FalloffGenerator.GenerateFalloffMap(width);

            for (int y = 0; y < values.GetLength(0); y++) {
                for (int x = 0; x < values.GetLength(1); x++) {
                    //Do the thing
                    values[x, y] = Mathf.Clamp01(values[x, y] - falloffMap[x, y]);
                }
            }
        }

        AnimationCurve heightCurve_threadsafe = new AnimationCurve(settings.heightCurve.keys);

        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        for(int i = 0; i < width; i++) {
            for(int j = 0; j < height; j++) {
                values[i, j] *= heightCurve_threadsafe.Evaluate(values[i, j]) * settings.heightMultiplier;

                if (values[i,j] > maxValue) {
                    maxValue = values[i, j];
                }
                if (values[i,j] < minValue) {
                    minValue = values[i, j];
                }
            }
        }

        return new HeightMap(values, minValue, maxValue);
    }
}

public struct HeightMap {
    public readonly float[,] values;
    public readonly float minValue;
    public readonly float maxValue;

    public HeightMap(float[,] values, float minValue, float maxValue) {
        this.values = values;
        this.minValue = minValue;
        this.maxValue = maxValue;
    }
}