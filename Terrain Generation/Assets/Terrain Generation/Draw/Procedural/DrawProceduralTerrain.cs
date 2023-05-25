using Erosion;
using Mask;
using Noise;
using System;
using System.IO;
using TerrainGenerator;
using Unity.Mathematics;
using UnityEngine;
using Vegatation;

public class DrawProceduralTerrain : MonoBehaviour
{
	[Header("Settings")]
	public OTerrainSettings terrainSettings;

	[Space]
	public OErosionSettings erosionSettings;
	public OFluidSettings fluidSettings;

	[Space]
	public OVegatationSpawnerSettings vegatationSpawnerSettings;

	[Space]
	public ONoiseSettings fBmSettings;
	public OCellularSettings cellularSettigns;

	[Space]
	public OMaskSettings[] maskSettings;

	[Header("Init")]
	public MeshRenderer meshRenderer;
	public Transform parent;

	protected int arrLen;

	protected bool terrainDrawn = false;

	protected NoiseGenerator noiseGenerator;
	protected Landmass landmass;
	protected Water water;
	protected CSErosion erosion;
	protected VegatationSpawner vegatationSpawner;

	void Start()
	{
		arrLen = terrainSettings.res.x * terrainSettings.res.y;

		noiseGenerator = new NoiseGenerator(terrainSettings.res.x, fBmSettings, cellularSettigns);

		landmass = new Landmass(terrainSettings, terrainSettings.landmass);
		water = new Water(terrainSettings, landmass, terrainSettings.water);

		vegatationSpawner = new VegatationSpawner(vegatationSpawnerSettings, terrainSettings, landmass, water, parent);
	}

	private void OnDestroy()
	{
		//erosion.DisposeBuffers();
	}

	protected virtual void GenerateNoise()
	{
		throw new NotImplementedException();
	}

	protected virtual void Erode()
	{
		throw new NotImplementedException();
	}

	protected virtual void Draw()
	{
		throw new NotImplementedException();
	}

	protected virtual void Grow()
	{
		throw new NotImplementedException();
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
		Color[] colourMap = new Color[arrLen];
		for (int y = 0, i = 0; y < terrainSettings.res.y; y++)
		{
			for (int x = 0; x < terrainSettings.res.x; x++, i++)
			{
				float angle = CalcualteAngle(x, y, i);

				// R - Dry terrain with low slope
				// G - Under water
				// B - Dry terrain high slopes

				float3 rgb = new float3(
					water.values[i] < fluidSettings.uniformFluidAddition && angle < (30.0f * Mathf.Deg2Rad) ?
						1.0f :
						0.0f,
					water.values[i] >= fluidSettings.uniformFluidAddition ?
						1.0f :
						Mathf.InverseLerp(
							0.0f,
							fluidSettings.uniformFluidAddition,
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
		float[,] suspendedSediment = erosion.SuspendedSediment;

		float current = heights[i] + suspendedSediment[i, 0];
		float4 h = new float4(
			heights[indices.x] + suspendedSediment[indices.x, 0],
			heights[indices.y] + suspendedSediment[indices.y, 0],
			heights[indices.z] + suspendedSediment[indices.z, 0],
			heights[indices.w] + suspendedSediment[indices.w, 0]);

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
