using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Vegatation.Jobs
{
	[BurstCompile(CompileSynchronously = true)]
	public struct RandomCoordsJob : IJob
	{
		public int arrLen;
		public int res, subGridCountX;
		public int2 subGridRes;

		public int numOfSpecies;

		public uint id;
		private Unity.Mathematics.Random _random;

		[ReadOnly] public NativeList<float> vegetation;
		public NativeList<float> growths;

		public void Execute()
		{
			_random = new Unity.Mathematics.Random();
			_random.InitState(id + 1);

			for (int i = 0; i < arrLen; i++)
			{
				float specieId = Mathf.Floor(_random.NextFloat(0.0f, numOfSpecies));
				int j = (int)specieId * 12;
				growths.Add(specieId); // 0 - Specie ID
				growths.Add(0);        // 1 - Spawn ID

				float x = _random.NextFloat(0.0f, res - 1);
				int xI = Mathf.FloorToInt(x / subGridRes.x);

				float z = _random.NextFloat(0.0f, res - 1);
				int zI = Mathf.FloorToInt(z / subGridRes.y);

				growths.Add(x);    // 2 - x
				growths.Add(0.0f); // 3 - y
				growths.Add(z);    // 4 - z

				growths.Add(x - (subGridRes.x * xI)); // 5 - Grid x
				growths.Add(z - (subGridRes.y * zI)); // 6 - Grid z

				growths.Add(zI * subGridCountX + xI); // 7 - Coord Index

				growths.Add(0.0f); //  8 - Rotation X
				growths.Add(0.0f); //  9 - Rotation Y
				growths.Add(0.0f); // 10 - Rotation Z

				float maxScale = _random.NextFloat(vegetation[j + 8], vegetation[j + 9]);
				float lifespan = vegetation[j + 1] * maxScale;
				growths.Add(lifespan); // 11 - Lifespan
				growths.Add(lifespan); // 12 - Spawn Age
				growths.Add(lifespan); // 13 - Current Age
				growths.Add(1.0f);     // 14 - Linear Age

				growths.Add(maxScale); // 15 - Max Scale
				growths.Add(maxScale); // 16 - Current Scale

				float maxDiameter = vegetation[j + 3] * maxScale;
				growths.Add(maxDiameter); // 17 - Max Diameter
				growths.Add(maxDiameter); // 18 - Current Diameter
			}
		}
	}
}
