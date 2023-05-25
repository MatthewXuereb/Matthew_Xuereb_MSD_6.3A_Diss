using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Vegatation.Jobs;

namespace Vegatation.Execution
{
	public class GrowExecution : IJobExecution
	{
		private int2 _subGridCount;
		private int2 _subGridRes;

		private float _timeStep;

		public bool canSpawn;

		private System.Random _random;
		private SetHeightsExecution _setHeights;

		private NativeList<float>[] _vegetation;
		private NativeList<float>[] _growths;
		private NativeList<float>[] _newGrowths;
		private NativeArray<float>[] _heights;
		private NativeArray<float>[] _water;

		private NativeList<GrowJob> _grow;

		public GrowExecution(int res, int2 subGridRes, float timeStep, System.Random random, SetHeightsExecution setHeights, NativeList<float>[] vegetation, NativeList<float>[] growths, NativeList<float>[] newGrowths, NativeArray<float>[] heights, NativeArray<float>[] water)
		{
			_subGridRes = subGridRes;

			_subGridCount = new int2(
				Mathf.CeilToInt((float)res / (float)_subGridRes.x),
				Mathf.CeilToInt((float)res / (float)_subGridRes.y));

			_timeStep = timeStep;

			_random = random;

			_setHeights = setHeights;

			_vegetation = vegetation;
			_growths = growths;
			_newGrowths = newGrowths;
			_heights = heights;
			_water = water;
		}

		public void Execute()
		{
			SetJob();
			RunJob();

			NativeList<float> n = new NativeList<float>(Allocator.Temp);
			for (int i = 0; i < _subGridCount.x * _subGridCount.y; i++)
			{
				for (int j = 0; j < _newGrowths[i].Length; j++)
					n.Add(_newGrowths[i][j]);
				_newGrowths[i].Clear();
			}

			SetToSubGrid(n, _newGrowths, n.Length / 19);
			_setHeights.UpdateState(_vegetation, _newGrowths, _heights);
			_setHeights.Execute();

			for (int i = 0; i < _subGridCount.x * _subGridCount.y; i++)
				if (_newGrowths[i].Length > 0)
					for (int k = 0; k < _newGrowths[i].Length / 19; k++)
						for (int j = 0; j < 19; j++)
							_growths[i].Add(_newGrowths[i][k * 19 + j]);

			n.Dispose();
		}

		public void SetJob()
		{
			_grow = new NativeList<GrowJob>(Allocator.Temp);
			for (int i = 0; i < _subGridCount.x * _subGridCount.y; i++)
			{
				_newGrowths[i].Clear();
				_grow.Add(new GrowJob
				{
					subGridCount = _subGridCount.x,
					subGridRes = _subGridRes,

					timeStep = _timeStep,

					canSpawn = canSpawn,

					id = (uint)_random.Next(0, 100000),

					vegetation = _vegetation[i],
					growths = _growths[i],

					newGrowths = _newGrowths[i],

					water = _water[i]
				});
			}
		}

		public void RunJob()
		{
			NativeList<JobHandle> jobs = new NativeList<JobHandle>(Allocator.TempJob);
			for (int i = 0; i < _grow.Length; i++)
				jobs.Add(_grow[i].Schedule());
			JobHandle.CompleteAll(jobs.AsArray());

			jobs.Clear();
			_grow.Dispose();
		}

		private void SetToSubGrid(NativeList<float> growths, NativeList<float>[] subG, int arrLen)
		{
			for (int j = 0; j < arrLen; j++)
			{
				int jI = j * 19;
				int i = (int)growths[jI + 7];

				for (int l = 0; l < 19; l++)
					subG[i].Add(growths[jI + l]);
			}
		}
	}
}
