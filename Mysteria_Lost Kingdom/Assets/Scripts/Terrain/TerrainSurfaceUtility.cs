using UnityEngine;

public static class TerrainSurfaceUtility
{
    public static Surface GetSurface(Vector3 worldPos)
    {
        Terrain terrain = Terrain.activeTerrain;
        if (!terrain) return Surface.Default;

        TerrainData data = terrain.terrainData;
        Vector3 terrainPos = terrain.transform.position;

        int mapX = Mathf.FloorToInt(
            (worldPos.x - terrainPos.x) / data.size.x * data.alphamapWidth
        );

        int mapZ = Mathf.FloorToInt(
            (worldPos.z - terrainPos.z) / data.size.z * data.alphamapHeight
        );

        float[,,] splatmap = data.GetAlphamaps(mapX, mapZ, 1, 1);

        int dominantIndex = 0;
        float max = 0f;

        for (int i = 0; i < splatmap.GetLength(2); i++)
        {
            if (splatmap[0, 0, i] > max)
            {
                max = splatmap[0, 0, i];
                dominantIndex = i;
            }
        }

        switch (dominantIndex)
        {
            case 0: return Surface.Grass;
            case 1: return Surface.Gravel;
            case 2: return Surface.Stone;
            default: return Surface.Default;
        }
    }
}
