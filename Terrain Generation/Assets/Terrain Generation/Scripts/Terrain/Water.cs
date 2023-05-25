using UnityEngine;

namespace TerrainGenerator
{
	public class Water : TerrainGenerator
	{
		private float[] _heights;

		public Water(OTerrainSettings terrainSettings, Landmass landmass, Material material)
		{
			settings = terrainSettings;

			_heights = landmass.values;

			this.material = material;

			values = new float[settings.res.x * settings.res.y];
			linearStackedValues = new float[settings.res.x, settings.res.y];
		}

		public override void SetValues(float[] values)
		{
			base.SetValues(values);

			for (int i = 0; i < values.Length; i++)
			{
				values[i] = _heights[i] < settings.seaLevel ? settings.seaLevel - _heights[i] : 0.0f;
				//values[i] += _fluidSettings.initialFluid;
			}
		}

		public override void Draw(Vector3 position)
		{
			gameObject = new GameObject();
			gameObject.transform.name = "Water";

			for (int y = 0, i = 0; y < settings.res.y; y++)
				for (int x = 0; x < settings.res.x; x++, i++)
					linearStackedValues[y, x] = Mathf.InverseLerp(0.0f, settings.height, _heights[i] + values[i]);

			data = new TerrainData();
			data.heightmapResolution = settings.res.x;
			data.enableHolesTextureCompression = true;
			data.SetHeights(0, 0, linearStackedValues);
			data.size = new Vector3(
				settings.res.x * settings.cellLength.x,
				settings.height,
				settings.res.y * settings.cellLength.y);

			TerrainCollider terrainCollider = gameObject.AddComponent<TerrainCollider>();
			terrainCollider.terrainData = data;

			terrain = gameObject.AddComponent<Terrain>();
			terrain.terrainData = data;
			terrain.allowAutoConnect = true;
			terrain.heightmapPixelError = 1;

			terrain.materialTemplate = settings.water;
		}
	}
}
