using Unity.Mathematics;
using UnityEngine;

namespace Vegatation
{
	[CreateAssetMenu(fileName = "VegatationSpawnerSettings", menuName = "Scriptable Objects/Vegatation Spawner", order = 1)]
	public class OVegatationSpawnerSettings : ScriptableObject
	{
		public int seed = 0;

		public int numOfTrees = 100;
		public int numOfJobs = 10;

		public int duration = 500;

		public int2 subGridRes = new int2(128, 128);

		public float timeStep = 1.0f;

		public OVegatation[] vegatations;
	}
}
