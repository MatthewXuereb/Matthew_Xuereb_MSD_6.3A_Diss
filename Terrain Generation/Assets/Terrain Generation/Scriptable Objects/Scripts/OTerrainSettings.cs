using UnityEngine;
using Unity.Mathematics;

namespace TerrainGenerator
{
	[CreateAssetMenu(fileName = "Terrain Settings", menuName = "Scriptable Objects/Terrain Settings", order = 3)]
	public class OTerrainSettings : ScriptableObject
	{
		public int2 res;

		public float height = 256.0f;
		public float seaLevel = 0.0f;
		public float2 cellLength = float2.zero;

		[Header("Material")]
		public Material landmass;
		public Material water;
	}
}
