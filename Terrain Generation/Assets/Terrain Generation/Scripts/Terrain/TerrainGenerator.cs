using System;
using UnityEngine;

namespace TerrainGenerator
{
    public class TerrainGenerator
    {
        protected OTerrainSettings settings;

        public Material material;

        public float[] values;
        protected float[,] linearStackedValues;

        public GameObject gameObject;
        public TerrainData data;
        public Terrain terrain;

        public virtual void SetValues(float[] values)
        {
            for (int i = 0; i < values.Length; i++)
                this.values[i] = values[i] * settings.height;
        }

        public virtual void Draw(Vector3 position)
        {
            throw new NotImplementedException();
		}

		public virtual void Draw(Vector3 position, Texture2D heightmap, int res)
		{
			throw new NotImplementedException();
		}
	}
}
