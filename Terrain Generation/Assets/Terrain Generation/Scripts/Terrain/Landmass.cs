using Unity.Mathematics;
using UnityEngine;

namespace TerrainGenerator
{
	public class Landmass : TerrainGenerator
	{
		public Landmass(OTerrainSettings terrainSettings, Material material)
		{
			settings = terrainSettings;

			this.material = material;

			values = new float[settings.res.x * settings.res.y];
			linearStackedValues = new float[settings.res.x, settings.res.y];
		}

		public override void SetValues(float[] values)
		{
			base.SetValues(values);
		}

		public override void Draw(Vector3 position)
		{
			gameObject = new GameObject();
			gameObject.transform.name = "Terrain";
			gameObject.transform.position = position;

			for (int y = 0, i = 0; y < settings.res.y; y++)
				for (int x = 0; x < settings.res.x; x++, i++)
					linearStackedValues[y, x] = Mathf.InverseLerp(0.0f, settings.height, values[i]);

			data = new TerrainData();
			data.heightmapResolution = settings.res.x;
			data.SetHeights(0, 0, linearStackedValues);
			data.size = new Vector3(settings.res.x * settings.cellLength.x, settings.height, settings.res.y * settings.cellLength.y);

			TerrainCollider terrainCollider = gameObject.AddComponent<TerrainCollider>();
			terrainCollider.terrainData = data;

			terrain = gameObject.AddComponent<Terrain>();
			terrain.terrainData = data;
			terrain.allowAutoConnect = true;
			terrain.heightmapPixelError = 1;
			terrain.materialTemplate = settings.landmass;
		}

		public override void Draw(Vector3 position, Texture2D heightmap, int res)
		{
			gameObject = new GameObject();
			gameObject.transform.name = "Terrain";
			gameObject.transform.position = position;

			for (int y = 0, i = 0; y < res; y++)
				for (int x = 0; x < res; x++, i++)
					linearStackedValues[y, x] = heightmap.GetPixel(y, x).r;

			data = new TerrainData();
			data.heightmapResolution = res;
			data.SetHeights(0, 0, linearStackedValues);
			data.size = new Vector3(res, settings.height, res);

			TerrainCollider terrainCollider = gameObject.AddComponent<TerrainCollider>();
			terrainCollider.terrainData = data;

			terrain = gameObject.AddComponent<Terrain>();
			terrain.terrainData = data;
			terrain.allowAutoConnect = true;
			terrain.heightmapPixelError = 1;
			terrain.materialTemplate = settings.landmass;
		}

		public void SetTexture(int2 res, int arrLen)
		{
			Color[] colourMap = new Color[arrLen];
			for (int i = 0; i < arrLen; i++)
				colourMap[i] = Color.white;

			Texture2D texture = new Texture2D(res.x, res.y);
			texture.SetPixels(colourMap);
			texture.Apply();

			terrain.materialTemplate = settings.landmass;
		}

		public void SetSedimentTexture(float[,] susSediment, int2 res, int arrLen, Vector2 cellLength)
		{
			int p = 1;
			Color[] colourMap = new Color[arrLen];
			for (int i = 0; i < arrLen; i++)
			{
				float v = Mathf.Pow(Mathf.InverseLerp(0.0f, 1.0f * cellLength.x, susSediment[i, 0]), p);

				colourMap[i] = new Color(0.0f, v, 0.0f);
			}

			Texture2D texture = new Texture2D(res.x, res.y);
			texture.SetPixels(colourMap);
			texture.Apply();

			material.mainTexture = texture;
			terrain.materialTemplate = material;
		}

		public void SetUVTexture(float[,] velocity, int2 res, int arrLen, Vector2 cellLength)
		{
			int p = 1;
			Color[] colourMap = new Color[arrLen];
			for (int i = 0; i < arrLen; i++)
			{
				float uValue = Mathf.Pow(Mathf.InverseLerp(0.0f, 1.0f * cellLength.x, velocity[i, 0] + velocity[i, 1]), p);
				float vValue = Mathf.Pow(Mathf.InverseLerp(0.0f, 1.0f * cellLength.y, velocity[i, 2] + velocity[i, 3]), p);

				colourMap[i] = new Color(uValue, vValue, 0.0f);
			}

			Texture2D texture = new Texture2D(res.x, res.y);
			texture.SetPixels(colourMap);
			texture.Apply();

			material.mainTexture = texture;
			terrain.materialTemplate = material;
		}
	}
}
