using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Vegatation.Jobs
{
	[BurstCompile(CompileSynchronously = true)]
	public struct GrowJob : IJob
	{
		public int subGridCount;
		public int2 subGridRes;

		public float timeStep;

		public bool canSpawn;

		public uint id;
		private Unity.Mathematics.Random _random;

		[ReadOnly] public NativeList<float> vegetation;

		public NativeList<float> growths;
		public NativeList<float> newGrowths;

		[ReadOnly] public NativeArray<float> water;

		public void Execute()
		{
			_random = new Unity.Mathematics.Random();
			_random.InitState(id + 1);

			for (int i = 0; i < growths.Length / 19; i++)
			{
				int j = i * 19;

				if (canSpawn)
					if (growths[j + 13] >= vegetation[(int)growths[j] + 2])
						if (_random.NextFloat(0.0f, 100.0f) < vegetation[(int)growths[j] + 4] * timeStep)
							Spawn(j);

				GrowTree(j);

				growths[j + 13] += timeStep;
				if (growths[j + 13] >= growths[j + 11])
					if (canSpawn)
						growths[j + 13] = -1.0f;
					else
						growths[j + 13] = growths[j + 11] + 0.1f;
			}
		}

		private void GrowTree(int i)
		{
			int vI = (int)growths[i];
			float t = Mathf.Clamp01(Mathf.InverseLerp(0.0f, vegetation[vI * 12 + 1], growths[i + 13]));
			float currentScale = growths[i + 15] * t;
			growths[i + 16] = currentScale;
			growths[i + 18] = growths[i + 17] * t;
		}

		private void Spawn(int i)
		{
			float specieId = growths[i];
			int j = (int)specieId * 12;

			float2 coord = new float2(growths[i + 2], growths[i + 4]);
			coord.x = _random.NextFloat(coord.x - vegetation[j + 5], coord.x + vegetation[j + 5]);
			coord.y = _random.NextFloat(coord.y - vegetation[j + 5], coord.y + vegetation[j + 5]);

			int2 flooredCoord = new int2(
				Mathf.FloorToInt(coord.x / subGridRes.x),
				Mathf.FloorToInt(coord.y / subGridRes.y));

			if (growths.Length / 19 < 50 && flooredCoord.x >= 0 && flooredCoord.y >= 0 && flooredCoord.x < subGridCount - 1 && flooredCoord.y < subGridCount - 1)
			{
				float waterLevel = water[flooredCoord.y * subGridCount + flooredCoord.x];
				if (waterLevel < 0.001f) 
				{
					newGrowths.Add(specieId); // 0 - Specie ID
					newGrowths.Add(0);        // 1 - Spawn ID

					newGrowths.Add(coord.x);    // 2 - x
					newGrowths.Add(0.0f);       // 3 - y
					newGrowths.Add(coord.y);    // 4 - z

					newGrowths.Add(coord.x - (subGridRes.x * flooredCoord.x)); // 5 - Grid x
					newGrowths.Add(coord.y - (subGridRes.y * flooredCoord.y)); // 6 - Grid z

					newGrowths.Add(flooredCoord.y * subGridCount + flooredCoord.x); // 7 - Coord Index

					newGrowths.Add(0.0f); //  8 - Rotation X
					newGrowths.Add(0.0f); //  9 - Rotation Y
					newGrowths.Add(0.0f); // 10 - Rotation Z

					float maxScale = _random.NextFloat(vegetation[j + 8], vegetation[j + 9]);
					float lifespan = vegetation[j + 1] * maxScale;
					newGrowths.Add(lifespan); // 11 - Lifespan
					newGrowths.Add(lifespan); // 12 - Spawn Age
					newGrowths.Add(lifespan); // 13 - Current Age
					newGrowths.Add(1.0f);     // 14 - Linear Age

					newGrowths.Add(maxScale); // 15 - Max Scale
					newGrowths.Add(maxScale); // 16 - Current Scale

					float maxDiameter = vegetation[j + 3] * maxScale;
					newGrowths.Add(maxDiameter); // 17 - Max Diameter
					newGrowths.Add(maxDiameter); // 18 - Current Diameter
				} 
			}
		}
	}
}
