using Erosion;
using Noise;
using System;
using System.IO;
using TerrainGenerator;
using Unity.Mathematics;
using UnityEngine;
using Vegatation;

public class DrawRealTerrain : MonoBehaviour
{
	[Header("Settings")]
	public OTerrainSettings terrainSettings;

	[Space]
	public OFluidSettings fluidSettings;

	[Space]
	public OVegatationSpawnerSettings vegatationSpawnerSettings;

	[Header("Init")]
	public Texture2D heightmap;
	public MeshRenderer meshRenderer;
	public Transform parent;

	protected int arrLen;

	protected bool terrainDrawn = false;

	protected NoiseGenerator noiseGenerator;
	protected Landmass landmass;
	protected Water water;
	protected CSFlud fluid;
	protected VegatationSpawner vegatationSpawner;

	private void Start()
	{
		arrLen = heightmap.width * heightmap.height;

		landmass = new Landmass(terrainSettings, terrainSettings.landmass);
		water = new Water(terrainSettings, landmass, terrainSettings.water);
		float[] heightmapArray = new float[arrLen];
		for (int y = 0, i = 0; y < heightmap.height; y++)
			for (int x = 0; x < heightmap.width; x++, i++)
				heightmapArray[i] = heightmap.GetPixel(y, x).r;
		landmass.SetValues(heightmapArray);

		fluid = new CSFlud(terrainSettings, fluidSettings, landmass, water);
		fluid.InstantiateArrays();

		fluid.CreateBuffers();
		fluid.SetData();
		fluid.SetBuffers();
		
		vegatationSpawner = new VegatationSpawner(vegatationSpawnerSettings, terrainSettings, landmass, water, parent);
	}

	private void Update()
	{
		Flow();

		if (!fluid.runSim)
			if (!terrainDrawn) Draw();
			else Grow();
	}

	private void OnDestroy()
	{
		//fluid.DisposeBuffers();
	}

	protected void Flow()
	{
		fluid.Execute();
	}

	protected void Draw()
	{
		fluid.GetData();
		fluid.DisposeBuffers();

		landmass.Draw(Vector3.zero);
		water.Draw(Vector3.zero);

		Texture2D output = CreateTexture();
		meshRenderer.material.mainTexture = output;

		landmass.material.SetTexture("Splatmap", output);
		WriteFile(output);

		terrainDrawn = true;
		vegatationSpawner.Init();
	}

	protected void Grow()
	{
		if (vegatationSpawner.runSim)
			vegatationSpawner.Simulate();
	}

	protected Texture2D CreateTexture()
	{
		Texture2D texture = new Texture2D(terrainSettings.res.x, terrainSettings.res.y);
		texture.SetPixels(GenerateColourMap());
		texture.Apply();

		return texture;
	}

	private Color[] GenerateColourMap()
	{
		float minWater = 0.1f;
		Color[] colourMap = new Color[arrLen];
		for (int y = 0, i = 0; y < terrainSettings.res.y; y++)
		{
			for (int x = 0; x < terrainSettings.res.x; x++, i++)
			{
				float angle = CalcualteAngle(x, y, i);

				float3 rgb = new float3(
					water.values[i] < minWater && angle < (30.0f * Mathf.Deg2Rad) ?
						1.0f :
						0.0f,
					water.values[i] >= minWater ?
						1.0f :
						Mathf.InverseLerp(
							0.0f,
							minWater,
							water.values[i]),
					angle >= (30.0f * Mathf.Deg2Rad) ?
						1.0f :
						Mathf.InverseLerp(
							Mathf.Max(0.0f, 27.50f) * Mathf.Deg2Rad,
							30.0f * Mathf.Deg2Rad,
							angle));

				colourMap[i] = new Color(rgb.x, rgb.z, rgb.y);
			}
		}

		return colourMap;
	}

	private float CalcualteAngle(int x, int y, int i)
	{
		int4 indices = new int4(
			x - 1 < 0 ? i : i - 1,
			x + 1 > terrainSettings.res.x - 1 ? i : i + 1,
			y - 1 < 0 ? i : i - terrainSettings.res.y,
			y + 1 > terrainSettings.res.y - 1 ? i : i + terrainSettings.res.y);

		float[] heights = landmass.values;

		float current = heights[i];
		float4 h = new float4(
			heights[indices.x],
			heights[indices.y],
			heights[indices.z],
			heights[indices.w]);

		float4 heightDiffrences = new float4(
			 Mathf.Abs(current - h.x),
			 Mathf.Abs(current - h.y),
			 Mathf.Abs(current - h.z),
			 Mathf.Abs(current - h.w));

		return (heightDiffrences.x + heightDiffrences.y + heightDiffrences.z + heightDiffrences.w) / 4.0f;
	}

	protected void WriteFile(Texture2D output)
	{
		File.WriteAllBytes("Assets/splatmap.png", output.EncodeToPNG());
	}
}
