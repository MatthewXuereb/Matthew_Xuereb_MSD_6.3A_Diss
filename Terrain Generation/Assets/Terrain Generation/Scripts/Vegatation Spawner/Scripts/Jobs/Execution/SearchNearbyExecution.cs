using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Vegatation.Jobs;

namespace Vegatation.Execution
{
	public class SearchNearbyExecution : IJobExecution
	{
		private int2 _subGridCount;

		private List<NativeList<float>> _nearbyGrowths;
		private NativeList<float>[] _chunkGrowths;

		private NativeList<SearchNearbyJob> _searchNearby;

		public SearchNearbyExecution(int res, int2 subGridRes, NativeList<float>[] chunkGrowths)
		{
			_subGridCount = new int2(
				Mathf.CeilToInt((float)res / (float)subGridRes.x),
				Mathf.CeilToInt((float)res / (float)subGridRes.y));

			_chunkGrowths = chunkGrowths;
		}

		public void Execute()
		{
			SetJob();
			RunJob();
		}

		public void SetJob()
		{
			_searchNearby = new NativeList<SearchNearbyJob>(Allocator.Temp);
			_nearbyGrowths = new List<NativeList<float>>();
			for (int x = 0, i = 0; x < _subGridCount.x; x++)
			{
				for (int z = 0; z < _subGridCount.y; z++, i++)
				{
					_nearbyGrowths.Add(new NativeList<float>(Allocator.TempJob));
					_nearbyGrowths[i] = SetChunkData(_chunkGrowths, _nearbyGrowths[i], x, z);

					_searchNearby.Add(new SearchNearbyJob
					{
						chunkGrowths = _chunkGrowths[i], // chunkGrowths
						nearbyGrowths = _nearbyGrowths[i] // nearbyGrowths
					});
				}
			}
		}

		public void RunJob()
		{
			NativeList<JobHandle> jobs = new NativeList<JobHandle>(Allocator.TempJob);
			for (int i = 0; i < _searchNearby.Length; i++)
				jobs.Add(_searchNearby[i].Schedule());
			JobHandle.CompleteAll(jobs.AsArray());
			jobs.Clear();

			_searchNearby.Dispose();
			for (int i = 0; i < _nearbyGrowths.Count; i++)
				_nearbyGrowths[i].Dispose();
		}

		private NativeList<float> SetChunkData(NativeList<float>[] chunkGrowths, NativeList<float> nearbyGrowths, int x, int z)
		{
			for (int xC = x - 1, cI = 0; xC <= x + 1; xC++)
			{
				for (int zC = z - 1; zC <= z + 1; zC++, cI++)
				{
					if (xC >= 0 && zC >= 0 && xC < _subGridCount.x && zC < _subGridCount.y)
					{
						int nI = xC * _subGridCount.x + zC;
						for (int i = 0; i < chunkGrowths[nI].Length; i++)
							nearbyGrowths.Add(chunkGrowths[nI][i]);
					}
				}
			}

			return nearbyGrowths;
		}
	}
}
