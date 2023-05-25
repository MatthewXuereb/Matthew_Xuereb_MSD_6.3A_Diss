using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Vegatation.Jobs
{
	[BurstCompile(CompileSynchronously = true)]
	public struct SetHeightsJob : IJob
	{
		public int res;

		public uint id;
		private Unity.Mathematics.Random _random;

		[ReadOnly] public NativeList<float> vegetation;
		public NativeList<float> growths;

		[ReadOnly] public NativeArray<float> heights;

		public void Execute()
		{
			_random = new Unity.Mathematics.Random();
			_random.InitState(id + 1);

			for (int i = (growths.Length / 19) - 1; i >= 0; i--)
			{
				int j = i * 19;

				int4 c = new int4(
					Mathf.FloorToInt(growths[j + 5]),
					Mathf.CeilToInt(growths[j + 5]),
					Mathf.FloorToInt(growths[j + 6]),
					Mathf.CeilToInt(growths[j + 6]));

				float4 h = new float4(
					heights[IndexOf(c.x, c.z)],
					heights[IndexOf(c.y, c.z)],
					heights[IndexOf(c.x, c.w)],
					heights[IndexOf(c.y, c.w)]);
				float b = BilinearLerp(c.y, c.w, h, j);

				float4 d = new float4(
					Mathf.Abs(b - h.x),
					Mathf.Abs(b - h.y),
					Mathf.Abs(b - h.z),
					Mathf.Abs(b - h.w));

				float a = Mathf.Atan((d.x + d.y + d.z + d.w) / 4.0f) * Mathf.Rad2Deg;
				Validate(j, b, a);
			}
		}

		private float BilinearLerp(float x, float z, float4 h, int i)
		{
			float pX = x - growths[i + 5];
			float pZ = z - growths[i + 6];

			return Mathf.Lerp(
				Mathf.Lerp(h.x, h.y, pX),
				Mathf.Lerp(h.z, h.w, pX),
				pZ);
		}

		private void Validate(int i, float b, float a)
		{
			int j = (int)growths[i] * 12;
			if (a < vegetation[j + 10] && b >= vegetation[j + 6] && b < vegetation[j + 7])
			{
				growths[i + 1] = _random.NextFloat(0.0f, vegetation[j]);

				growths[i + 3] = b;

				growths[i + 8] = _random.NextFloat(-vegetation[j + 11], vegetation[j + 11]);
				growths[i + 9] = _random.NextFloat(0.0f, 360.0f);
				growths[i + 10] = _random.NextFloat(-vegetation[j + 11], vegetation[j + 11]);

				float spawnAge = _random.NextFloat(0.0f, vegetation[j + 1]);
				//float linearAge = Mathf.InverseLerp(0.0f, vegetation[j + 1], spawnAge);
				growths[i + 12] = spawnAge;
				growths[i + 13] = 0.0f;// spawnAge;
				growths[i + 14] = 0.0f;// linearAge;

				float maxScale = _random.NextFloat(vegetation[j + 8], vegetation[j + 9]);
				growths[i + 15] = maxScale;
				growths[i + 16] = 0.0f;// maxScale * linearAge;

				float maxDiameter = vegetation[j + 3] * maxScale;
				growths[i + 17] = maxDiameter;
				growths[i + 18] = 0.0f;// Mathf.Lerp(0.0f, maxDiameter, linearAge);
			}
			else
			{
				RemoveElement(i);
			}
		}

		private void RemoveElement(int i)
		{
			for (int j = 18; j >= 0; j--)
				growths.RemoveAt(i + j);
		}

		private int IndexOf(int x, int z)
		{
			return z * res + x;
		}
	}
}
