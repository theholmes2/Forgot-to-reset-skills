using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public static class TailmapfixAutoSlicer
{
    private const string TargetPath = "Assets/Art/Tilemap/tailmapfix.png";

    [MenuItem("Tools/Tilemap/Slice Tailmapfix By Object Size")]
    public static void SliceTailmapfix()
    {
        TextureImporter importer = AssetImporter.GetAtPath(TargetPath) as TextureImporter;

        if (importer == null)
        {
            Debug.LogError("tailmapfix.png를 찾을 수 없음: " + TargetPath);
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.isReadable = true;
        importer.SaveAndReimport();

        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TargetPath);

        if (texture == null)
        {
            Debug.LogError("Texture 로드 실패: " + TargetPath);
            return;
        }

        Color32[] pixels = texture.GetPixels32();
        int width = texture.width;
        int height = texture.height;

        bool[] visited = new bool[pixels.Length];
        List<SpriteRect> spriteRects = new List<SpriteRect>();

        int spriteIndex = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = GetIndex(x, y, width);

                if (visited[index])
                    continue;

                if (IsBackground(pixels[index]))
                {
                    visited[index] = true;
                    continue;
                }

                RectInt bounds = FindObjectBounds(x, y, width, height, pixels, visited);

                if (bounds.width < 3 || bounds.height < 3)
                    continue;

                // 너무 긴 가이드 라인 같은 것은 제외
                if (bounds.width > 900 && bounds.height < 16)
                    continue;

                Rect rect = ConvertToUnityRect(bounds, height);

                SpriteRect spriteRect = new SpriteRect
                {
                    name = "tailmap_piece_" + spriteIndex.ToString("000"),
                    spriteID = GUID.Generate(),
                    rect = rect,
                    alignment = SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f)
                };

                spriteRects.Add(spriteRect);
                spriteIndex++;
            }
        }

        // Sprite Editor 데이터를 수정하기 위한 Provider 생성
        SpriteDataProviderFactories factory = new SpriteDataProviderFactories();
        factory.Init();

        ISpriteEditorDataProvider dataProvider =
            factory.GetSpriteEditorDataProviderFromObject(importer);

        dataProvider.InitSpriteEditorDataProvider();

        // 실제 Sprite Rect 목록 적용
        dataProvider.SetSpriteRects(spriteRects.ToArray());

        // Sprite 이름과 ID 연결
        ISpriteNameFileIdDataProvider nameFileIdProvider =
            dataProvider.GetDataProvider<ISpriteNameFileIdDataProvider>();

        List<SpriteNameFileIdPair> nameFileIdPairs = new List<SpriteNameFileIdPair>();

        foreach (SpriteRect rect in spriteRects)
        {
            nameFileIdPairs.Add(new SpriteNameFileIdPair(rect.name, rect.spriteID));
        }

        nameFileIdProvider.SetNameFileIdPairs(nameFileIdPairs);

        // 변경사항 적용
        dataProvider.Apply();

        importer.isReadable = false;
        importer.SaveAndReimport();

        Debug.Log("tailmapfix.png 자동 슬라이스 완료. 생성된 Sprite 수: " + spriteRects.Count);
    }

    private static RectInt FindObjectBounds(
        int startX,
        int startY,
        int width,
        int height,
        Color32[] pixels,
        bool[] visited)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(startX, startY));
        visited[GetIndex(startX, startY, width)] = true;

        int minX = startX;
        int maxX = startX;
        int minY = startY;
        int maxY = startY;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            minX = Mathf.Min(minX, current.x);
            maxX = Mathf.Max(maxX, current.x);
            minY = Mathf.Min(minY, current.y);
            maxY = Mathf.Max(maxY, current.y);

            TryAdd(current.x - 1, current.y, width, height, pixels, visited, queue);
            TryAdd(current.x + 1, current.y, width, height, pixels, visited, queue);
            TryAdd(current.x, current.y - 1, width, height, pixels, visited, queue);
            TryAdd(current.x, current.y + 1, width, height, pixels, visited, queue);
        }

        int padding = 2;

        minX = Mathf.Max(0, minX - padding);
        minY = Mathf.Max(0, minY - padding);
        maxX = Mathf.Min(width - 1, maxX + padding);
        maxY = Mathf.Min(height - 1, maxY + padding);

        return new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1);
    }

    private static void TryAdd(
        int x,
        int y,
        int width,
        int height,
        Color32[] pixels,
        bool[] visited,
        Queue<Vector2Int> queue)
    {
        if (x < 0 || y < 0 || x >= width || y >= height)
            return;

        int index = GetIndex(x, y, width);

        if (visited[index])
            return;

        visited[index] = true;

        if (IsBackground(pixels[index]))
            return;

        queue.Enqueue(new Vector2Int(x, y));
    }

    private static bool IsBackground(Color32 color)
    {
        // tailmapfix.png의 검은 배경을 빈 공간으로 취급
        return color.a == 0 || (color.r < 18 && color.g < 18 && color.b < 18);
    }

    private static int GetIndex(int x, int y, int width)
    {
        return y * width + x;
    }

    private static Rect ConvertToUnityRect(RectInt bounds, int textureHeight)
    {
        // GetPixels 기준 Y와 Sprite Rect 기준 Y가 반대라 변환 필요
        return new Rect(
            bounds.x,
            textureHeight - bounds.y - bounds.height,
            bounds.width,
            bounds.height
        );
    }
}
