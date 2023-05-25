using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Vegatation.Jobs;

namespace Vegatation.Execution
{
	public class SetHeightsExecution : IJobExecution
	{
		private int _subGridXRes;
		private int2 _subGridCount;

		private System.Random _random;

		private NativeList<float>[] _vegetation;
		private NativeList<float>[] _growths;
		private NativeArray<float>[] _heights;

		private NativeList<SetHeightsJob> _setHeights;

		public SetHeightsExecution(int res, int2 subGridRes, System.Random random, NativeList<float>[] vegetation, NativeList<float>[] growths, NativeArray<float>[] heights)
		{
			_subGridXRes = subGridRes.x;

			_subGridCount = new int2(
				Mathf.CeilToInt((float)res / (float)subGridRes.x),
				Mathf.CeilToInt((float)res / (float)subGridRes.y));

			_random = random;

			_vegetation = vegetation;
			_growths = growths;
			_heights = heights;
		}

		public void UpdateState(NativeList<float>[] vegetation, NativeList<float>[] growths, NativeArray<float>[] heights)
		{
			_vegetation = vegetation;
			_growths = growths;
			_heights = heights;
		}

		public void Execute()
		{
			SetJob();
			RunJob();
		}

		public void SetJob()
		{
			_setHeights = new NativeList<SetHeightsJob>(Allocator.Temp);
			for (int i = 0; i < _subGridCount.x * _subGridCount.y; i++)
				_setHeights.Add(new SetHeightsJob
				{
					res = _subGridXRes + 1,

					id = (uint)_random.Next(0, 100000),

					vegetation = _vegetation[i],
					growths = _growths[i],

					heights = _heights[i]
				});
		}

		public void RunJob()
		{
			NativeList<JobHandle> jobs = new NativeList<JobHandle>(Allocator.TempJob);
			for (int i = 0; i < _setHeights.Length; i++)
				jobs.Add(_setHeights[i].Schedule());
			JobHandle.CompleteAll(jobs.AsArray());
			jobs.Clear();

			_setHeights.Dispose();
		}
	}
}
