using UnityEngine;
using Unity.Mathematics;
using System;

namespace Noise
{
	[CreateAssetMenu(fileName = "Noise Settings", menuName = "Scriptable Objects/Noise Settings", order = 1)]
	public class ONoiseSettings : ScriptableObject
	{
		[Header("Init")]
		public float2 offset;

		public bool invert;

		public enum EFractalType
		{
			None,
			FBm,
			Ridged,
			PingPong
		}

		[Header("fBm")]
		public int seed = 0;
		public int octaves = 5;

		[Range(1.0f, 1000.0f)] public float frequency = 10.0f;
		[Range(1.0f, 5.0f)] public float lacunarity = 3.0f;
		[Range(0.0f, 1.0f)] public float gain = 0.5f;

		[Range(0.0f, 1.0f)] public float weight = 0.5f;

		public EFractalType fBMFractalType;

		[Header("Warp")]
		public int warpSeed = 0;
		public int warpOctaves = 5;

		[Range(1.0f, 1000.0f)] public float warpFrequency = 10.0f;
		[Range(1.0f, 5.0f)] public float warpLacunarity = 3.0f;
		[Range(0.0f, 1.0f)] public float warpGain = 0.5f;

		public float warpAmp = 1.0f;

		public EFractalType warpFractalType;
	}
}
