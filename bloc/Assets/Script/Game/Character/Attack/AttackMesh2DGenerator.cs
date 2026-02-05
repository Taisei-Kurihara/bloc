using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 2Dメッシュ生成クラス（攻撃描画用）.
/// ShapeGenerator_Polygonと同様のロジックで多角形メッシュを生成.
/// </summary>
public static class AttackMesh2DGenerator
{
    /// <summary>
    /// 角数ごとのメッシュキャッシュ.
    /// </summary>
    private static readonly Dictionary<int, Mesh> _meshCache = new Dictionary<int, Mesh>();

    /// <summary>
    /// 角数ごとの頂点データキャッシュ（ワールド座標計算用）.
    /// </summary>
    private static readonly Dictionary<int, Vector2[]> _verticesCache = new Dictionary<int, Vector2[]>();

    /// <summary>
    /// 指定角数の多角形メッシュを取得（キャッシュあり）.
    /// </summary>
    /// <param name="angular">角数（3以上）.</param>
    /// <returns>生成されたMesh.</returns>
    public static Mesh GetMesh(int angular)
    {
        angular = Mathf.Max(3, angular);

        if (_meshCache.TryGetValue(angular, out Mesh cachedMesh))
        {
            return cachedMesh;
        }

        Mesh mesh = GenerateMesh(angular);
        _meshCache[angular] = mesh;
        return mesh;
    }

    /// <summary>
    /// 指定角数の頂点配列を取得（キャッシュあり）.
    /// 単位円上の頂点（半径1）.
    /// </summary>
    /// <param name="angular">角数（3以上）.</param>
    /// <returns>頂点配列.</returns>
    public static Vector2[] GetVertices(int angular)
    {
        angular = Mathf.Max(3, angular);

        if (_verticesCache.TryGetValue(angular, out Vector2[] cachedVertices))
        {
            return cachedVertices;
        }

        Vector2[] vertices = GenerateVertices(angular);
        _verticesCache[angular] = vertices;
        return vertices;
    }

    /// <summary>
    /// 多角形メッシュを生成.
    /// </summary>
    private static Mesh GenerateMesh(int angular)
    {
        Mesh mesh = new Mesh();
        mesh.name = $"AttackMesh_{angular}";

        // 頂点生成（中心点 + 外周頂点）.
        Vector3[] vertices = new Vector3[angular + 1];
        Vector2[] uvs = new Vector2[angular + 1];
        Color[] colors = new Color[angular + 1];

        // 中心点.
        vertices[0] = Vector3.zero;
        uvs[0] = new Vector2(0.5f, 0.5f);
        colors[0] = Color.white;

        // 外周頂点（ShapeGenerator_Polygonと同様のロジック）.
        float flatDownOffset = CalculateFlatDownOffset(angular);

        for (int i = 0; i < angular; i++)
        {
            float angle = ((i * Mathf.PI * 2f) / angular) + flatDownOffset;
            float x = Mathf.Cos(angle);
            float y = Mathf.Sin(angle);
            vertices[i + 1] = new Vector3(x, y, 0f);
            uvs[i + 1] = new Vector2((x + 1f) * 0.5f, (y + 1f) * 0.5f);
            colors[i + 1] = Color.white;
        }

        // 三角形インデックス生成（ファン形式）.
        int[] triangles = new int[angular * 3];
        for (int i = 0; i < angular; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = (i + 1) % angular + 1;
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.colors = colors;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    /// <summary>
    /// 頂点配列を生成（単位円上）.
    /// </summary>
    private static Vector2[] GenerateVertices(int angular)
    {
        Vector2[] vertices = new Vector2[angular];
        float flatDownOffset = CalculateFlatDownOffset(angular);

        for (int i = 0; i < angular; i++)
        {
            float angle = ((i * Mathf.PI * 2f) / angular) + flatDownOffset;
            vertices[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }

        return vertices;
    }

    /// <summary>
    /// フラット底オフセット計算（ShapeGenerator_Polygonと同じロジック）.
    /// </summary>
    private static float CalculateFlatDownOffset(int angular)
    {
        if (angular % 2 != 0)
        {
            return Mathf.PI / 2f; // 奇数：頂点が上向き.
        }
        else
        {
            return (angular - 2 > 3 && (angular - 2) % 4 == 0) ? 0f : Mathf.PI / angular;
        }
    }

    /// <summary>
    /// ワールド座標系での頂点位置を計算.
    /// </summary>
    /// <param name="angular">角数.</param>
    /// <param name="position">中心位置.</param>
    /// <param name="size">サイズ.</param>
    /// <param name="rotation">回転（ラジアン）.</param>
    /// <returns>ワールド座標頂点配列.</returns>
    public static Vector2[] GetWorldVertices(int angular, Vector2 position, float size, float rotation)
    {
        Vector2[] localVertices = GetVertices(angular);
        Vector2[] worldVertices = new Vector2[localVertices.Length];

        float cos = Mathf.Cos(rotation);
        float sin = Mathf.Sin(rotation);

        for (int i = 0; i < localVertices.Length; i++)
        {
            Vector2 scaled = localVertices[i] * size;
            // 回転適用.
            float rotatedX = scaled.x * cos - scaled.y * sin;
            float rotatedY = scaled.x * sin + scaled.y * cos;
            worldVertices[i] = position + new Vector2(rotatedX, rotatedY);
        }

        return worldVertices;
    }

    /// <summary>
    /// キャッシュをクリア.
    /// </summary>
    public static void ClearCache()
    {
        foreach (var mesh in _meshCache.Values)
        {
            if (mesh != null)
            {
                Object.Destroy(mesh);
            }
        }
        _meshCache.Clear();
        _verticesCache.Clear();
    }
}
