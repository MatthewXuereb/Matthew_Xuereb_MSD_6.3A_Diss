using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Vegatation.Jobs
{
	[BurstCompile(CompileSynchronously = true)]
	public struct SearchNearbyJob : IJob
	{
		public NativeList<float> chunkGrowths;
		[ReadOnly] public NativeList<float> nearbyGrowths;

		public void Execute()
		{
			for (int i = 0; i < chunkGrowths.Length / 19; i++)
			{
				int j = i * 19;
				if (chunkGrowths[j + 13] != -1)
				{
					float3 currentTreePosition = GetPosition(chunkGrowths, j);
					float currentTreeRadius = GetRadius(chunkGrowths, j);

					SearchChunk(j, nearbyGrowths, currentTreePosition, currentTreeRadius);
				}
			}
		}

		private void SearchChunk(int i, NativeList<float> nG, float3 currentTreePosition, float currentTreeRadius)
		{
			for (int nK = 0; nK < nG.Length / 19; nK++)
			{
				int nKI = nK * 19;
				if (nG[nKI + 13] != -1 && i != nKI && chunkGrowths[i + 13] != -1)
				{
					float3 indexedTreePosition = GetPosition(nG, nKI);
					float indexedTreeRadius = GetRadius(nG, nKI);

					float distance = GetDistance(currentTreePosition, indexedTreePosition, currentTreeRadius, indexedTreeRadius);
					if (distance <= 0)
					{
						chunkGrowths[i + 13] = -1;
						return;
					}
				}
			}
		}

		private float3 GetPosition(NativeList<float> chunkGrowths, int i)
		{
			return new float3(
				chunkGrowths[i + 2],
				chunkGrowths[i + 3],
				chunkGrowths[i + 4]);
		}

		private float GetRadius(NativeList<float> chunkGrowths, int i)
		{
			return chunkGrowths[i + 18] / 2.0f;
		}

		private float GetDistance(float3 cP, float3 iP, float cR, float iR)
		{
			return Vector3.Distance(cP, iP) - cR - iR;
		}
	}
}
