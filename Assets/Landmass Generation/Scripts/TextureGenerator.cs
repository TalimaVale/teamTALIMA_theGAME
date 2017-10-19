using System.Collections;
using UnityEngine;

public static class TextureGenerator {

    public static Texture2D TextureFromHeightMap(HeightMap heightMap) {
        int width = heightMap.values.GetLength(0);
        int height = heightMap.values.GetLength(1);

        Color[] colorMap = new Color[width * height];
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, Mathf.InverseLerp(heightMap.minValue, heightMap.maxValue, heightMap.values[x, y]));
                ;
            }
        }

        return TextureFromColorMap(colorMap, width, height);
    }

    public static Texture2D TextureFromColorMap(Color[] colorMap, int width, int height) {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colorMap);
        texture.Apply();
        return texture;
    }
}
