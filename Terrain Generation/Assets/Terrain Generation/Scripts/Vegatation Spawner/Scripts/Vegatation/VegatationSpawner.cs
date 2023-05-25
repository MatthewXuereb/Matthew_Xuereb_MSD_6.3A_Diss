using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Vegatation.Execution;
using TerrainGenerator;

namespace Vegatation
{
	public class VegatationSpawner : MonoBehaviour
	{
		private float _startTime, _currentTime;
		public bool runSim = true;

		private int2 _subGridCount;

		private System.Random _random;

		private Transform _parent;

		private Landmass _landmass;
		private Water _water;

		private RandomCoordsExecution _randomCoords;
		private SetHeightsExecution _setHeights;
		private GrowExecution _grow;
		private SearchNearbyExecution _searchNearby;

		private OVegatationSpawnerSettings _vegatationSpawnerSettings;
		private OTerrainSettings _terrainSettings;

		// Arrays
		private NativeList<float>[] _growths;
		private NativeList<float>[] _subGrowths;
		private NativeList<float>[] _newGrowths;

		private NativeList<float>[] _vegetation;
		private NativeList<float>[] _subVegetation;

		private NativeArray<float>[] _subHeights;
		private NativeArray<float>[] _subWater;

		public VegatationSpawner(OVegatationSpawnerSettings vegatationSpawnerSettings, OTerrainSettings terrainSettings, Landmass landmass, Water water, Transform parent)
		{
			_terrainSettings = terrainSettings;
			_vegatationSpawnerSettings = vegatationSpawnerSettings;
			_parent = parent;

			_landmass = landmass;
			_water = water;

			_random = new System.Random(_vegatationSpawnerSettings.seed);

			_subGridCount = new int2(
				Mathf.CeilToInt((float)_terrainSettings.res.x / (float)_vegatationSpawnerSettings.subGridRes.x),
				Mathf.CeilToInt((float)_terrainSettings.res.y / (float)_vegatationSpawnerSettings.subGridRes.y));
		}

		public void Init()
        {
            _startTime = Time.time;

            InstantiateArrays();

            _randomCoords = new RandomCoordsExecution(_terrainSettings.res.x, _random, _vegatationSpawnerSettings, _vegetation, _growths, _subGrowths);
			_randomCoords.Execute();

			_setHeights = new SetHeightsExecution(_terrainSettings.res.x, _vegatationSpawnerSettings.subGridRes, _random, _subVegetation, _subGrowths, _subHeights);
			_setHeights.Execute();

			_grow = new GrowExecution(_terrainSettings.res.x, _vegatationSpawnerSettings.subGridRes, _vegatationSpawnerSettings.timeStep, _random, _setHeights, _subVegetation, _subGrowths, _newGrowths, _subHeights, _subWater);
		}

		public void Simulate()
		{
			if (_currentTime < _vegatationSpawnerSettings.duration)
			{
				//Debug.Log(_currentTime + "/" + _vegatationSpawnerSettings.duration + " - " + ((_currentTime / _vegatationSpawnerSettings.duration) * 100) + "%");

				_grow.canSpawn = _currentTime < _vegatationSpawnerSettings.duration - 100 ? true : false;
				_grow.Execute();

				//_searchNearby = new SearchNearbyExecution(_terrainSettings.res.x, _vegatationSpawnerSettings.subGridRes, _subGrowths);
				//_searchNearby.Execute();

				Pop();

				_currentTime += _vegatationSpawnerSettings.timeStep;
			}
			else
			{
				if (runSim)
				{
					Debug.Log("Duration of sim = " + (Time.time - _startTime) + " seconds");
					runSim = false;

					for (int x = 0, i = 0; x < _subGridCount.x; x++)
					{
						for (int z = 0; z < _subGridCount.y; z++, i++)
						{
							Spawn(_subGrowths[i]);
							_newGrowths[i].Dispose();
						}
					}
				}
			}
		}

		private void OnDisable()
		{
			for (int i = 0; i < _subGridCount.x * _subGridCount.y; i++)
			{
				_subHeights[i].Dispose();
				_subWater[i].Dispose();
				_subVegetation[i].Dispose();
				_subGrowths[i].Dispose();
			}
		}

		private void InstantiateArrays()
		{
			_vegetation = new NativeList<float>[_vegatationSpawnerSettings.numOfJobs];
			_growths = new NativeList<float>[_vegatationSpawnerSettings.numOfJobs];
			for (int i = 0; i < _vegetation.Length; i++)
			{
				_vegetation[i] = new NativeList<float>(Allocator.TempJob);
				for (int j = 0; j < _vegatationSpawnerSettings.vegatations.Length; j++)
					_vegetation[i] = _vegatationSpawnerSettings.vegatations[j].SetVegatationData(_vegetation[i]);

				_growths[i] = new NativeList<float>(_vegatationSpawnerSettings.numOfTrees / _vegatationSpawnerSettings.numOfJobs, Allocator.TempJob);
			}

			_subHeights = new NativeArray<float>[_subGridCount.x * _subGridCount.y];
			_subWater = new NativeArray<float>[_subGridCount.x * _subGridCount.y];
			_subVegetation = new NativeList<float>[_subGridCount.x * _subGridCount.y];
			_subGrowths = new NativeList<float>[_subGridCount.x * _subGridCount.y];
			_newGrowths = new NativeList<float>[_subGridCount.x * _subGridCount.y];
			for (int subX = 0, subIndex = 0; subX < _subGridCount.x; subX++)
			{
				for (int subZ = 0; subZ < _subGridCount.y; subZ++, subIndex++)
				{
					_subHeights[subIndex] = new NativeArray<float>((_vegatationSpawnerSettings.subGridRes.x + 1) * (_vegatationSpawnerSettings.subGridRes.y + 1), Allocator.Persistent);
					_subWater[subIndex] = new NativeArray<float>((_vegatationSpawnerSettings.subGridRes.x + 1) * (_vegatationSpawnerSettings.subGridRes.y + 1), Allocator.Persistent);
					for (int x = subX * _vegatationSpawnerSettings.subGridRes.x, i = 0; x < (subX * _vegatationSpawnerSettings.subGridRes.x) + _vegatationSpawnerSettings.subGridRes.x + 1; x++)
					{
						for (int z = subZ * _vegatationSpawnerSettings.subGridRes.y; z < (subZ * _vegatationSpawnerSettings.subGridRes.y) + _vegatationSpawnerSettings.subGridRes.y + 1; z++, i++)
						{
							_subHeights[subIndex][i] = x >= _terrainSettings.res.x || z >= _terrainSettings.res.y ? 0.0f : _landmass.values[IndexOf(x, z)];
							_subWater[subIndex][i] = x >= _terrainSettings.res.x || z >= _terrainSettings.res.y ? 0.0f : _water.values[IndexOf(x, z)];
						}
					}

					_subVegetation[subIndex] = new NativeList<float>(Allocator.Persistent);
					for (int i = 0; i < _vegatationSpawnerSettings.vegatations.Length; i++)
						_subVegetation[subIndex] = _vegatationSpawnerSettings.vegatations[i].SetVegatationData(_subVegetation[subIndex]);

					_subGrowths[subIndex] = new NativeList<float>(Allocator.Persistent);
					_newGrowths[subIndex] = new NativeList<float>(Allocator.Persistent);
				}
			}
		}

		private void Spawn(NativeList<float> _growths)
		{
			for (int j = 0; j < _growths.Length / 19; j++)
			{
				int k = j * 19;

				int specieId = (int)_growths[k];
				int spawnId = (int)_growths[k + 1];

				Vector3 pos = new Vector3(
					_growths[k + 2],
					_growths[k + 3],
					_growths[k + 4]);

				Vector3 rot = new Vector3(
					_growths[k + 8],
					_growths[k + 9],
					_growths[k + 10]);

				float scale = _growths[k + 16];

				Vector3 spawnPos = new Vector3(pos.z, pos.y, pos.x);
				GameObject tree = Instantiate(_vegatationSpawnerSettings.vegatations[specieId].vegatationObjects[spawnId], spawnPos, Quaternion.Euler(rot), _parent);
				tree.transform.localScale = Vector3.one * scale;
			}
		}

		private void Pop()
		{
			for (int x = 0; x < _subGridCount.x; x++)
			{
				for (int z = 0; z < _subGridCount.y; z++)
				{
					int i = z * _subGridCount.x + x;
					for (int j = (_subGrowths[i].Length / 19) - 1; j >= 0; j--)
						if (_subGrowths[i][j * 19 + 13] == -1)
							for (int k = 18; k >= 0; k--)
								_subGrowths[i].RemoveAt(j * 19 + k);
				}
			}
		}

		private int IndexOf(int x, int z)
		{
			return z * _terrainSettings.res.x + x;
		}
	}
}
