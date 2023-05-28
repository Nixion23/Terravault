using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public int width = 100;
    public int height = 100;
    public float scale = 20f;
    public float offsetX = 100f;
    public float offsetY = 100f;
    public Texture2D terrainTexture;

    public bool useRandomSeed = true;
    public int seed = 0;

    // Other Stuff
    [Range(1, 1000)]
    public int octaves = 3;
    [Range(1f, 100f)]
    public float lacunarity = 2f;
    [Range(0f, 1f)]
    public float persistence = 0.5f;

    public bool randomizeOffset = false;
    public float maxOffset = 100f;

    public float textureTiling = 1f;

    public bool applySmoothing = false;
    public int smoothIterations = 5;

    private Terrain terrain;
    private TerrainData terrainData;

    private void Start()
    {
        InitializeTerrain();
        GenerateTerrain();
        ApplyTerrainTexture();
    }

    private void InitializeTerrain()
    {
        terrainData = new TerrainData();
        terrain = Terrain.CreateTerrainGameObject(terrainData).GetComponent<Terrain>();
    }

    private void GenerateTerrain()
    {
        if (useRandomSeed)
        {
            seed = Random.Range(0, int.MaxValue);
        }

        Random.InitState(seed);

        int terrainWidth = terrainData.heightmapResolution;
        int terrainHeight = terrainData.heightmapResolution;

        GenerateTerrainData(terrainWidth, terrainHeight);
        ApplyTerrainData();
        AddTerrainCollider();
    }

    private void GenerateTerrainData(int width, int height)
    {
        terrainData.heightmapResolution = width;
        terrainData.size = new Vector3(width, scale, height);

        float[,] heights = GenerateHeights(width, height);

        terrainData.SetHeights(0, 0, heights);

        if (applySmoothing)
        {
            SmoothTerrainHeights();
        }
    }

    private float[,] GenerateHeights(int width, int height)
    {
        float[,] heights = new float[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float xCoord = ((float)x / (width - 1) * scale) + offsetX;
                float yCoord = ((float)y / (height - 1) * scale) + offsetY;
                float heightValue = PerlinNoise(xCoord, yCoord);
                heights[x, y] = heightValue;
            }
        }
        return heights;
    }

    private float PerlinNoise(float x, float y)
    {
        float noiseValue = 0f;
        float frequency = 1f;
        float amplitude = 1f;

        for (int i = 0; i < octaves; i++)
        {
            float sampleX = x * frequency;
            float sampleY = y * frequency;
            float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
            noiseValue += perlinValue * amplitude;

            frequency *= lacunarity;
            amplitude *= persistence;
        }

        return noiseValue;
    }

    private void ApplyTerrainData()
    {
        terrain.terrainData = terrainData;
    }

    private void AddTerrainCollider()
    {
        TerrainCollider terrainCollider = terrain.GetComponent<TerrainCollider>();
        if (terrainCollider == null)
        {
            terrainCollider = terrain.gameObject.AddComponent<TerrainCollider>();
        }

        terrainCollider.terrainData = terrainData;

        // Position the collider at the center of the terrain
        terrainCollider.transform.position = terrainData.bounds.center;

        // Scale the collider to match the size of the terrain
        terrainCollider.transform.localScale = new Vector3(
            terrainData.size.x / (terrainData.heightmapResolution - 1),
            terrainData.size.y,
            terrainData.size.z / (terrainData.heightmapResolution - 1)
        );
    }

    private void ApplyTerrainTexture()
    {
        if (terrainTexture != null)
        {
            TerrainLayer terrainLayer = new TerrainLayer();
            terrainLayer.diffuseTexture = terrainTexture;
            terrainLayer.tileSize = new Vector2(textureTiling, textureTiling);

            TerrainLayer[] terrainLayers = new TerrainLayer[1];
            terrainLayers[0] = terrainLayer;

            terrainData.terrainLayers = terrainLayers;
        }
    }

    private void SmoothTerrainHeights()
    {
        for (int i = 0; i < smoothIterations; i++)
        {
            float[,] smoothedHeights = new float[width, height];

            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height; y++)
                {
                    float averageHeight = (terrainData.GetHeight(x - 1, y) +
                                           terrainData.GetHeight(x + 1, y) +
                                           terrainData.GetHeight(x, y - 1) +
                                           terrainData.GetHeight(x, y + 1)) * 0.25f;
                    smoothedHeights[x, y] = averageHeight;
                }
            }

            terrainData.SetHeights(0, 0, smoothedHeights);
        }
    }
}
