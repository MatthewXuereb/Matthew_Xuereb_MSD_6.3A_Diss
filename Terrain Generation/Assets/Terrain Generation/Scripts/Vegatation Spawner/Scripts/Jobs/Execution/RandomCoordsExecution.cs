using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Vegatation.Jobs;

namespace Vegatation.Execution
{
	public class RandomCoordsExecution : IJobExecution
	{
		private int _numOfTrees, _numOfJobs;
		private int _res, _subGridCountX;

		private int _numOfSpecies;

		private int2 _subGridRes;

		private System.Random _random;

		private NativeList<float>[] _vegetation;
		private NativeList<float>[] _growths;
		private NativeList<float>[] _subGrowths;

		private NativeList<RandomCoordsJob> _randomCoords;

		public RandomCoordsExecution(int res, System.Random random, OVegatationSpawnerSettings vegatationSpawnerSettings, NativeList<float>[] vegetation, NativeList<float>[] growths, NativeList<float>[] subGrowths)
		{
			_res = res;
			_subGridRes = vegatationSpawnerSettings.subGridRes;

			_subGridCountX = Mathf.CeilToInt((float)_res / (float)_subGridRes.x);

			_numOfTrees = vegatationSpawnerSettings.numOfTrees;
			_numOfJobs = vegatationSpawnerSettings.numOfJobs;
			_numOfSpecies = vegatationSpawnerSettings.vegatations.Length;

			_random = random;

			_vegetation = vegetation;
			_growths = growths;
			_subGrowths = subGrowths;
		}

		public void Execute()
		{
			SetJob();
			RunJob();

			for (int i = 0; i < _randomCoords.Length; i++)
			{
				SetToSubGrid(_growths[i], _randomCoords[i].arrLen);

				_vegetation[i].Dispose();
				_growths[i].Dispose();
			}
			_randomCoords.Dispose();
		}

		public void SetJob()
		{
			_randomCoords = new NativeList<RandomCoordsJob>(Allocator.Temp);
			for (int i = 0; i < _numOfJobs; i++)
			{
				_randomCoords.Add(new RandomCoordsJob
				{
					arrLen = _numOfTrees / _numOfJobs,
					res = _res,
					subGridCountX = _subGridCountX,
					subGridRes = _subGridRes,

					numOfSpecies = _numOfSpecies,

					id = (uint)_random.Next(0, 100000),

					vegetation = _vegetation[i],
					growths = _growths[i]
				});
			}
		}

		public void RunJob()
		{
			NativeList<JobHandle> jobs = new NativeList<JobHandle>(Allocator.TempJob);
			for (int i = 0; i < _randomCoords.Length; i++)
				jobs.Add(_randomCoords[i].Schedule());
			JobHandle.CompleteAll(jobs.AsArray());
			jobs.Clear();
		}

		private void SetToSubGrid(NativeList<float> growths, int length)
		{
			for (int j = 0; j < length; j++)
			{
				int jI = j * 19;
				int i = (int)growths[jI + 7];

				for (int l = 0; l < 19; l++)
					_subGrowths[i].Add(growths[jI + l]);
			}
		}
	}
}
