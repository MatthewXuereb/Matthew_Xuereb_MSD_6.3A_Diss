using Erosion;
using TerrainGenerator;
using UnityEngine;

public class CSFlud : IComputeShader
{
	private int _arrLen;
	private int _threadGroup;

	private int _iteration = 0;

	private float _startTime;
	public bool runSim = true;

	private int[,] _indices;
	private int[,] _coords;

	private float[,] _heights;
	private float[,] _water;
	private float[,] _waterFlux;

	private Landmass _landmassTerrain;
	private Water _waterTerrain;

	private OTerrainSettings _terrainSettings;
	private OFluidSettings _fluidSettings;

	private ComputeBuffer _indicesBuffer;
	private ComputeBuffer _coordsBuffer;
	private ComputeBuffer _heightsBuffer;
	private ComputeBuffer _waterBuffer;
	private ComputeBuffer _waterFluxBuffer;

	private ComputeShader _flow2DShader;
	private ComputeShader _shiftShader;

	public CSFlud(OTerrainSettings terrainSettings, OFluidSettings fluidSettings, Landmass landmass, Water water)
	{
		_terrainSettings = terrainSettings;
		_fluidSettings = fluidSettings;

		_arrLen = _terrainSettings.res.x * _terrainSettings.res.y;
		_iteration = 0;

		_landmassTerrain = landmass;
		_waterTerrain = water;

		_threadGroup = Mathf.CeilToInt(_arrLen / 1024.0f);

		string path = "Compute Shaders/";
		_flow2DShader = Resources.Load(path + "Flow2D") as ComputeShader;
		_shiftShader = Resources.Load(path + "Shift") as ComputeShader;
		
		Debug.Log(Time.time);

    }

	public void InstantiateArrays()
	{
		_indices = new int[_arrLen, 4];
		_coords = new int[_arrLen, 2];

		_heights = new float[_arrLen, 2];
		_water = new float[_arrLen, 2];

		_waterFlux = new float[_arrLen, 4];

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
			}
		}
	}

	public void CreateBuffers()
	{
		_indicesBuffer = new ComputeBuffer(_arrLen, 4 * sizeof(int));
		_coordsBuffer = new ComputeBuffer(_arrLen, 2 * sizeof(int));
		_heightsBuffer = new ComputeBuffer(_arrLen, 2 * sizeof(float));
		_waterBuffer = new ComputeBuffer(_arrLen, 2 * sizeof(float));
		_waterFluxBuffer = new ComputeBuffer(_arrLen, 4 * sizeof(float));
	}

	public void SetBuffers()
	{
		_flow2DShader.SetBuffer(0, "indices", _indicesBuffer);
		_flow2DShader.SetBuffer(0, "coords", _coordsBuffer);
		_flow2DShader.SetBuffer(0, "heights", _heightsBuffer);
		_flow2DShader.SetBuffer(0, "water", _waterBuffer);
		_flow2DShader.SetBuffer(0, "waterFlux", _waterFluxBuffer);
		_flow2DShader.SetBuffer(1, "indices", _indicesBuffer);
		_flow2DShader.SetBuffer(1, "coords", _coordsBuffer);
		_flow2DShader.SetBuffer(1, "water", _waterBuffer);
		_flow2DShader.SetBuffer(1, "waterFlux", _waterFluxBuffer);

		/*_flow2DShader.SetBuffer(2, "indices", _indicesBuffer);
		_flow2DShader.SetBuffer(2, "coords", _coordsBuffer);
		_flow2DShader.SetBuffer(2, "heights", _heightsBuffer);
		_flow2DShader.SetBuffer(2, "sediment", _susSedimentBuffer);
		_flow2DShader.SetBuffer(2, "sedimentFlux", _sedimentFluxBuffer);
		_flow2DShader.SetBuffer(3, "indices", _indicesBuffer);
		_flow2DShader.SetBuffer(3, "coords", _coordsBuffer);
		_flow2DShader.SetBuffer(3, "sediment", _susSedimentBuffer);
		_flow2DShader.SetBuffer(3, "sedimentFlux", _sedimentFluxBuffer);*/

		_flow2DShader.SetInt("arrLen", _arrLen);
		_flow2DShader.SetFloat("timeStep", _fluidSettings.timeStep);
		_flow2DShader.SetFloat("cSA", _fluidSettings.cSA);
		_flow2DShader.SetFloat("gravity", _fluidSettings.gravity);
		_flow2DShader.SetFloats("res", _terrainSettings.res.x, _terrainSettings.res.y);
		_flow2DShader.SetFloats("cellLength", _terrainSettings.cellLength.x, _terrainSettings.cellLength.y);

		_shiftShader.SetBuffer(1, "water", _waterBuffer);
	}

	public void DisposeBuffers()
	{
		_indicesBuffer.Dispose();
		_coordsBuffer.Dispose();
		_heightsBuffer.Dispose();
		_waterBuffer.Dispose();
		_waterFluxBuffer.Dispose();
	}

	public void SetData()
	{
		_indicesBuffer.SetData(_indices);
		_coordsBuffer.SetData(_coords);
		_heightsBuffer.SetData(_heights);
		_waterBuffer.SetData(_water);
		_waterFluxBuffer.SetData(_waterFlux);
	}

	public void GetData()
	{
		_waterBuffer.GetData(_water);

		for (int y = 0, i = 0; y < _terrainSettings.res.y; y++)
			for (int x = 0; x < _terrainSettings.res.x; x++, i++)
				_waterTerrain.values[i] = _water[i, 1];
	}

	public void Execute()
	{
		if (_iteration == 0)
			_startTime = Time.time;

		if (runSim)
		{
			float duration = _fluidSettings.duration;
			//Debug.Log(_iteration + "/" + duration + " - " + ((_iteration / duration) * 100) + "%");

			if (_iteration <= duration)
			{
				FluidSimulation();
			}
			else
			{
				runSim = false;

				Debug.Log("Duration of sim = " + (Time.time - _startTime) + " seconds");
			}

			_iteration++;
		}
	}

	private void FluidSimulation()
	{
		_flow2DShader.Dispatch(0, _threadGroup, 1, 1);
		_flow2DShader.Dispatch(1, _threadGroup, 1, 1);
		_shiftShader.Dispatch(1, _threadGroup, 1, 1);
	}
}
