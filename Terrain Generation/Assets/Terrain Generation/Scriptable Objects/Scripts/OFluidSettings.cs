using UnityEngine;

namespace Erosion
{
	[CreateAssetMenu(fileName = "Fluid Settings", menuName = "Scriptable Objects/Fluid Settings", order = 2)]
	public class OFluidSettings : ScriptableObject
	{
		[Header("Init")]
		public int duration = 1000;

		public float timeStep = 1.0f;

		[Header("Fluid")]
		public float initialFluid = 1.0f;
		public float uniformFluidAddition = 1.0f;

		public float evaporationRate = 1.0f;

		[Header("Flow")]
		public float cSA = 20.0f;
		public float gravity = 9.81f;
	}
}
