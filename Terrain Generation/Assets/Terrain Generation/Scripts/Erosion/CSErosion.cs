using UnityEngine;
using TerrainGenerator;
using System.Collections.Generic;
//using System.Diagnostics;

namespace Erosion
{
	public class CSErosion : IComputeShader
	{
		private int _arrLen;
		private int _threadGroup;

		private int _iteration = 0;

		private float _startTime;
		public bool runSim = true;

		private int[,] _indices;
		private int[,] _indices2;
		private int[,] _coords;

		private float[,] _heights;
		private float[,] _water;

		private float[,] _susSediment;
		private float[,] _waterFlux;
		private float[,] _sedimentFlux;
		private float[,] _velocity;

		private Landmass _landmassTerrain;
		private Water _waterTerrain;

		private OTerrainSettings _terrainSettings;
		private OErosionSettings _erosionSettings;
		private OFluidSettings _fluidSettings;

		private ComputeBuffer _indicesBuffer;
		private ComputeBuffer _indices2Buffer;
		private ComputeBuffer _coordsBuffer;
		private ComputeBuffer _heightsBuffer;
		private ComputeBuffer _waterBuffer;
		private ComputeBuffer _susSedimentBuffer;
		private ComputeBuffer _waterFluxBuffer;
		private ComputeBuffer _sedimentFluxBuffer;
		private ComputeBuffer _velocityBuffer;

		private ComputeShader _rainShader;
		private ComputeShader _flow2DShader;
		private ComputeShader _velocityShader;
		private ComputeShader _thermalShader;
		private ComputeShader _hydraulicShader;
		private ComputeShader _evaporationShader;
		private ComputeShader _shiftShader;

		public CSErosion(OTerrainSettings terrainSettings, OErosionSettings erosionSettings, OFluidSettings fluidSettings, Landmass landmass, Water water)
		{
			_terrainSettings = terrainSettings;
			_erosionSettings = erosionSettings;
			_fluidSettings = fluidSettings;

			_arrLen = _terrainSettings.res.x * _terrainSettings.res.y;
			_iteration = 0;

			_landmassTerrain = landmass;
			_waterTerrain = water;

			_threadGroup = Mathf.CeilToInt(_arrLen / 1024.0f);

			string path = "Compute Shaders/";
			_rainShader = Resources.Load(path + "Rain") as ComputeShader;
			_flow2DShader = Resources.Load(path + "Flow2D") as ComputeShader;
			_velocityShader = Resources.Load(path + "Velocity") as ComputeShader;
			_thermalShader = Resources.Load(path + "Thermal") as ComputeShader;
			_hydraulicShader = Resources.Load(path + "Hydraulic") as ComputeShader;
			_evaporationShader = Resources.Load(path + "Evaporation") as ComputeShader;
			_shiftShader = Resources.Load(path + "Shift") as ComputeShader;

            Debug.Log(Time.time);
        }

		public float[,] SuspendedSediment
		{
			get
			{
				return _susSediment;
			}
		}

		public float[,] Velocity
		{
			get
			{
				return _velocity;
			}
		}

		public void InstantiateArrays()
		{
			_indices = new int[_arrLen, 4];
			_indices2 = new int[_arrLen, 4];
			_coords = new int[_arrLen, 2];

			_heights = new float[_arrLen, 2];
			_water = new float[_arrLen, 2];

			_susSediment = new float[_arrLen, 2];
			_waterFlux = new float[_arrLen, 4];
			_sedimentFlux = new float[_arrLen, 4];
			_velocity = new float[_arrLen, 4];

			for (int y = 0, i = 0; y < _terrainSettings.res.y; y++)
			{
				for (int x = 0; x < _terrainSettings.res.x; x++, i++)
				{
					_heights[i, 0] = _landmassTerrain.values[i];
					_water[i, 0] = _waterTerrain.values[i] + _fluidSettings.initialFluid;

					_coords[i, 0] = x;
					_coords[i, 1] = y;

					_indices[i, 0] = x - 1 < 0 ? i : i - 1;
					_indices[i, 1] = x + 1 > _terrainSettings.res.x - 1 ? i : i + 1;
					_indices[i, 2] = y - 1 < 0 ? i : i - _terrainSettings.res.y;
					_indices[i, 3] = y + 1 > _terrainSettings.res.y - 1 ? i : i + _terrainSettings.res.y;
					_indices2[i, 0] = x - 1 < 0 || y - 1 < 0 ? i : i - _terrainSettings.res.x - 1;
					_indices2[i, 1] = x + 1 > _terrainSettings.res.x - 1 || y - 1 < 0 ? i : i - _terrainSettings.res.x + 1;
					_indices2[i, 2] = x - 1 < 0 || y + 1 > _terrainSettings.res.y - 1 ? i : i + _terrainSettings.res.x - 1;
					_indices2[i, 3] = x + 1 > _terrainSettings.res.y - 1 || y + 1 > _terrainSettings.res.y - 1 ? i : i + _terrainSettings.res.x + 1;
				}
			}
		}

		public void CreateBuffers()
		{
			_indicesBuffer = new ComputeBuffer(_arrLen, 4 * sizeof(int));
			_indices2Buffer = new ComputeBuffer(_arrLen, 4 * sizeof(int));
			_coordsBuffer = new ComputeBuffer(_arrLen, 2 * sizeof(int));
			_heightsBuffer = new ComputeBuffer(_arrLen, 2 * sizeof(float));
			_waterBuffer = new ComputeBuffer(_arrLen, 2 * sizeof(float));
			_susSedimentBuffer = new ComputeBuffer(_arrLen, 2 * sizeof(float));
			_waterFluxBuffer = new ComputeBuffer(_arrLen, 4 * sizeof(float));
			_sedimentFluxBuffer = new ComputeBuffer(_arrLen, 4 * sizeof(float));
			_velocityBuffer = new ComputeBuffer(_arrLen, 4 * sizeof(float));
		}

		public void SetBuffers()
		{
			_rainShader.SetBuffer(0, "water", _waterBuffer);
			_rainShader.SetFloat("uniformRainAmount", _fluidSettings.uniformFluidAddition * _fluidSettings.timeStep);

			_flow2DShader.SetBuffer(0, "indices", _indicesBuffer);
			_flow2DShader.SetBuffer(0, "coords", _coordsBuffer);
			_flow2DShader.SetBuffer(0, "heights", _heightsBuffer);
			_flow2DShader.SetBuffer(0, "water", _waterBuffer);
			_flow2DShader.SetBuffer(0, "waterFlux", _waterFluxBuffer);
			_flow2DShader.SetBuffer(1, "indices", _indicesBuffer);
			_flow2DShader.SetBuffer(1, "coords", _coordsBuffer);
			_flow2DShader.SetBuffer(1, "water", _waterBuffer);
			_flow2DShader.SetBuffer(1, "waterFlux", _waterFluxBuffer);

			_flow2DShader.SetBuffer(2, "indices", _indicesBuffer);
			_flow2DShader.SetBuffer(2, "coords", _coordsBuffer);
			_flow2DShader.SetBuffer(2, "heights", _heightsBuffer);
			_flow2DShader.SetBuffer(2, "sediment", _susSedimentBuffer);
			_flow2DShader.SetBuffer(2, "sedimentFlux", _sedimentFluxBuffer);
			_flow2DShader.SetBuffer(3, "indices", _indicesBuffer);
			_flow2DShader.SetBuffer(3, "coords", _coordsBuffer);
			_flow2DShader.SetBuffer(3, "sediment", _susSedimentBuffer);
			_flow2DShader.SetBuffer(3, "sedimentFlux", _sedimentFluxBuffer);

			_flow2DShader.SetInt("arrLen", _arrLen);
			_flow2DShader.SetFloat("timeStep", _fluidSettings.timeStep);
			_flow2DShader.SetFloat("cSA", _fluidSettings.cSA);
			_flow2DShader.SetFloat("gravity", _fluidSettings.gravity);
			_flow2DShader.SetFloats("res", _terrainSettings.res.x, _terrainSettings.res.y);
			_flow2DShader.SetFloats("cellLength", _terrainSettings.cellLength.x, _terrainSettings.cellLength.y);

			_velocityShader.SetBuffer(0, "ids", _indicesBuffer);
			_velocityShader.SetBuffer(0, "coords", _coordsBuffer);
			_velocityShader.SetBuffer(0, "flux", _waterFluxBuffer);
			_velocityShader.SetBuffer(0, "velocity", _velocityBuffer);
			_velocityShader.SetFloats("res", _terrainSettings.res.x, _terrainSettings.res.y);

			_hydraulicShader.SetBuffer(0, "ids", _indicesBuffer);
			_hydraulicShader.SetBuffer(0, "coords", _coordsBuffer);
			_hydraulicShader.SetBuffer(0, "heights", _heightsBuffer);
			_hydraulicShader.SetBuffer(0, "water", _waterBuffer);
			_hydraulicShader.SetBuffer(0, "susSediment", _susSedimentBuffer);
			_hydraulicShader.SetBuffer(0, "velocity", _velocityBuffer);
			_hydraulicShader.SetFloat("timeStep", _fluidSettings.timeStep);
			_hydraulicShader.SetFloat("sedimentCarryCapicity", _erosionSettings.sedimentCarryCapicity);
			_hydraulicShader.SetFloat("erosionRate", _erosionSettings.erosionRate);
			_hydraulicShader.SetFloat("depositionRate", _erosionSettings.depositionRate);
			_hydraulicShader.SetFloat("minSlope", _erosionSettings.minSlope);
			_hydraulicShader.SetFloat("maxDepth", _erosionSettings.maxDepth);
			_hydraulicShader.SetFloats("cellLength", _terrainSettings.cellLength.x, _terrainSettings.cellLength.y);

			_evaporationShader.SetBuffer(0, "water", _waterBuffer);
			_evaporationShader.SetFloat("evaporationRate", _fluidSettings.evaporationRate * _fluidSettings.timeStep);

			_thermalShader.SetBuffer(0, "ids", _indicesBuffer);
			_thermalShader.SetBuffer(0, "ids2", _indices2Buffer);
			_thermalShader.SetBuffer(0, "heights", _heightsBuffer);
			_thermalShader.SetBuffer(0, "water", _waterBuffer);
			_thermalShader.SetFloat("timeStep", _fluidSettings.timeStep);
			_thermalShader.SetFloat("slippageRate", _erosionSettings.slippageRate);
			_thermalShader.SetFloat("diagonalLengh", Mathf.Sqrt(Mathf.Pow(_terrainSettings.cellLength.x, 2) + Mathf.Pow(_terrainSettings.cellLength.y, 2)));
			_thermalShader.SetFloat("minAngleInRadians", _erosionSettings.minAngleInRadians);
			_thermalShader.SetFloats("cellLength", _terrainSettings.cellLength.x, _terrainSettings.cellLength.y);

			_shiftShader.SetBuffer(0, "heights", _heightsBuffer);
			_shiftShader.SetBuffer(1, "water", _waterBuffer);
			_shiftShader.SetBuffer(2, "susSediment", _susSedimentBuffer);
		}

		public void DisposeBuffers()
		{
			_indicesBuffer.Dispose();
			_indices2Buffer.Dispose();
			_coordsBuffer.Dispose();
			_heightsBuffer.Dispose();
			_waterBuffer.Dispose();
			_susSedimentBuffer.Dispose();
			_waterFluxBuffer.Dispose();
			_sedimentFluxBuffer.Dispose();
			_velocityBuffer.Dispose();
		}

		public void SetData()
		{
			_indicesBuffer.SetData(_indices);
			_indices2Buffer.SetData(_indices2);
			_coordsBuffer.SetData(_coords);
			_heightsBuffer.SetData(_heights);
			_waterBuffer.SetData(_water);
			_susSedimentBuffer.SetData(_susSediment);
			_waterFluxBuffer.SetData(_waterFlux);
			_sedimentFluxBuffer.SetData(_sedimentFlux);
			_velocityBuffer.SetData(_velocity);
		}

		public void GetData()
		{
			_heightsBuffer.GetData(_heights);
			_waterBuffer.GetData(_water);

			for (int y = 0, i = 0; y < _terrainSettings.res.y; y++)
			{
				for (int x = 0; x < _terrainSettings.res.x; x++, i++)
				{
					_landmassTerrain.values[i] = _heights[i, 1];
					_waterTerrain.values[i] = _water[i, 1];
				}
			}
		}

		public void Execute()
		{
			if (_iteration == 0)
				_startTime = Time.time;

			if (runSim)
			{
				float duration = _erosionSettings.thermalDuration + _erosionSettings.hydraulicDuration + _erosionSettings.sedimentTransportationDuration + _fluidSettings.duration;
				//Debug.Log(_iteration + "/" + duration + " - " + ((_iteration / duration) * 100) + "%");

				float limit = _erosionSettings.thermalDuration;
				if (_iteration <= limit)
				{
					ThermalSimulation();
				}
				else
				{
					limit += _erosionSettings.hydraulicDuration;
					if (_iteration <= limit)
					{
						HydraulicSimulation();
					}
					else
					{
						limit += _erosionSettings.sedimentTransportationDuration;
						if (_iteration <= limit)
						{
							SedimentTransportationSimulation();
						}
						else
						{
							limit += _fluidSettings.duration;
							if (_iteration <= limit)
							{
								FluidSimulation();
							}
							else
							{
								runSim = false;

								Debug.Log("Duration of sim = " + (Time.time - _startTime) + " seconds");
							}
						}
					}
				}

				_iteration++;
			}
		}

		private void ThermalSimulation()
		{
			_thermalShader.Dispatch(0, _threadGroup, 1, 1);
			_shiftShader.Dispatch(0, _threadGroup, 1, 1);
		}

		private void HydraulicSimulation()
		{
			_rainShader.Dispatch(0, _threadGroup, 1, 1);

			_flow2DShader.Dispatch(0, _threadGroup, 1, 1);
			_flow2DShader.Dispatch(1, _threadGroup, 1, 1);
			_shiftShader.Dispatch(1, _threadGroup, 1, 1);

			_velocityShader.Dispatch(0, _threadGroup, 1, 1);

			_hydraulicShader.Dispatch(0, _threadGroup, 1, 1);
			_shiftShader.Dispatch(0, _threadGroup, 1, 1);
			_shiftShader.Dispatch(1, _threadGroup, 1, 1);
			_shiftShader.Dispatch(2, _threadGroup, 1, 1);

			_flow2DShader.Dispatch(2, _threadGroup, 1, 1);
			_flow2DShader.Dispatch(3, _threadGroup, 1, 1);
			_shiftShader.Dispatch(2, _threadGroup, 1, 1);

			_evaporationShader.Dispatch(0, _threadGroup, 1, 1);

			_thermalShader.Dispatch(0, _threadGroup, 1, 1);
			_shiftShader.Dispatch(0, _threadGroup, 1, 1);
		}

		private void SedimentTransportationSimulation()
		{
			_flow2DShader.Dispatch(2, _threadGroup, 1, 1);
			_flow2DShader.Dispatch(3, _threadGroup, 1, 1);
			_shiftShader.Dispatch(2, _threadGroup, 1, 1);
		}

		private void FluidSimulation()
		{
			_flow2DShader.Dispatch(0, _threadGroup, 1, 1);
			_flow2DShader.Dispatch(1, _threadGroup, 1, 1);
			_shiftShader.Dispatch(1, _threadGroup, 1, 1);

			_flow2DShader.Dispatch(2, _threadGroup, 1, 1);
			_flow2DShader.Dispatch(3, _threadGroup, 1, 1);
			_shiftShader.Dispatch(2, _threadGroup, 1, 1);
		}
    }
}
