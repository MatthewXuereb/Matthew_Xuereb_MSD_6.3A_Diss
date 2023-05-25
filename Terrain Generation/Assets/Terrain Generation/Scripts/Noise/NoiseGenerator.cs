using UnityEngine;
using Mask;
using Unity.Mathematics;

namespace Noise
{
	public class NoiseGenerator
	{
		private int _res;

		private float _height;

		public bool executed = false;

		private ONoiseSettings _fBmSettings;
		private OCellularSettings _cellularSettigns;

		public NoiseGenerator(int res, ONoiseSettings fBmSettings, OCellularSettings cellularSettigns)
		{
			_res = res;

			_fBmSettings = fBmSettings;
			_cellularSettigns = cellularSettigns;
		}

		public void Reset()
		{
			executed = false;
		}

		public float[] GenerateMask(OMaskSettings settings)
		{
			CSMask mask = new CSMask(_res, settings);

			mask.InstantiateArrays();
			mask.CreateBuffers();
			mask.SetData();
			mask.SetBuffers();
			mask.Execute();
			mask.GetData();

			return mask.Mask;
		}

		public float[] GenerateFBmNoiseMap()
		{
			float[] noise = new float[_res * _res];

			ComputeShader computeShader = Resources.Load<ComputeShader>("Compute Shaders/Noise/Noise");
			int mainIndex = computeShader.FindKernel("CSMain");

			float adjustedFractalFrequency = (_fBmSettings.frequency * 1000.0f) / _res / 100000.0f;
			float adjustedWarpFrequency = (_fBmSettings.warpFrequency * 1000.0f) / _res / 100000.0f;

			computeShader.SetInt("resolution", _res);
			computeShader.SetFloats("offset", _fBmSettings.offset.x, _fBmSettings.offset.y);
			computeShader.SetFloat("invert", _fBmSettings.invert ? 1 : 0);

			computeShader.SetInt("fractalSeed", _fBmSettings.seed);
			computeShader.SetInt("fractalOctaves", _fBmSettings.octaves);
			computeShader.SetFloat("fractalFrequency", adjustedFractalFrequency);
			computeShader.SetFloat("fractalLacunarity", _fBmSettings.lacunarity);
			computeShader.SetFloat("fractalGain", _fBmSettings.gain);
			computeShader.SetFloat("weightedStrength", _fBmSettings.weight);

			computeShader.SetFloat("fractalType", (int)_fBmSettings.fBMFractalType);

			computeShader.SetInt("warpSeed", _fBmSettings.seed + _fBmSettings.warpSeed);

			computeShader.SetInt("warpOctaves", _fBmSettings.warpOctaves);
			computeShader.SetFloat("warpFrequency", adjustedWarpFrequency);
			computeShader.SetFloat("warpLacunarity", _fBmSettings.warpLacunarity);
			computeShader.SetFloat("warpGain", _fBmSettings.warpGain);

			computeShader.SetFloat("domainWarpAmp", _fBmSettings.warpAmp);

			computeShader.SetFloat("warpFractalType", (int)_fBmSettings.warpFractalType);

			ComputeBuffer mainBuffer = new ComputeBuffer(noise.Length, sizeof(int));
			mainBuffer.SetData(noise);
			computeShader.SetBuffer(mainIndex, "noiseMap", mainBuffer);

			computeShader.Dispatch(mainIndex, (_res / 32) + 1, (_res / 32) + 1, 1);
			mainBuffer.GetData(noise);
			mainBuffer.Dispose();

			return noise;
		}

		public float[] GenerateCellularNoiseMap()
		{
			float[] cellular = new float[_res * _res];

			ComputeShader computeShader = Resources.Load<ComputeShader>("Compute Shaders/Noise/Cellular");
			int mainIndex = computeShader.FindKernel("CSMain");

			float adjustedFractalFrequency = (_cellularSettigns.frequency * 1000.0f) / _res / 100000.0f;
			float adjustedWarpFrequency = (_cellularSettigns.warpFrequency * 1000.0f) / _res / 100000.0f;

			computeShader.SetInt("resolution", _res);
			computeShader.SetFloats("offset", _cellularSettigns.offset.x, _cellularSettigns.offset.y);
			computeShader.SetFloat("invert", _cellularSettigns.invert ? 1 : 0);

			computeShader.SetInt("fractalSeed", _cellularSettigns.seed);
			computeShader.SetInt("fractalOctaves", _cellularSettigns.octaves);
			computeShader.SetFloat("fractalFrequency", adjustedFractalFrequency);
			computeShader.SetFloat("fractalLacunarity", _cellularSettigns.lacunarity);
			computeShader.SetFloat("fractalGain", _cellularSettigns.gain);
			computeShader.SetFloat("weightedStrength", _cellularSettigns.weight);

			computeShader.SetFloat("fractalType", (int)_cellularSettigns.fBMFractalType);

			computeShader.SetInt("warpSeed", _cellularSettigns.seed + _cellularSettigns.warpSeed);

			computeShader.SetInt("warpOctaves", _cellularSettigns.warpOctaves);
			computeShader.SetFloat("warpFrequency", adjustedWarpFrequency);
			computeShader.SetFloat("warpLacunarity", _cellularSettigns.warpLacunarity);
			computeShader.SetFloat("warpGain", _cellularSettigns.warpGain);

			computeShader.SetFloat("domainWarpAmp", _cellularSettigns.warpAmp);

			computeShader.SetFloat("warpFractalType", (int)_cellularSettigns.warpFractalType);

			computeShader.SetInt("distanceTpye", (int)_cellularSettigns.distance);
			computeShader.SetInt("returnType", (int)_cellularSettigns.returnType);
			computeShader.SetFloat("jitter", _cellularSettigns.jitter);

			ComputeBuffer mainBuffer = new ComputeBuffer(cellular.Length, sizeof(int));
			mainBuffer.SetData(cellular);
			computeShader.SetBuffer(mainIndex, "noiseMap", mainBuffer);

			computeShader.Dispatch(mainIndex, (_res / 32) + 1, (_res / 32) + 1, 1);
			mainBuffer.GetData(cellular);
			mainBuffer.Dispose();

			return cellular;
		}
	}
}
