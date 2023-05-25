using System;
using UnityEngine;

namespace Erosion
{
	[CreateAssetMenu(fileName = "Erosion Settings", menuName = "Scriptable Objects/Erosion Settings", order = 1)]
	public class OErosionSettings : ScriptableObject
	{
		[Header("Init")]
		public int thermalDuration = 1000;
		public int hydraulicDuration = 1000;
		public int sedimentTransportationDuration = 1000;

		[Header("Erosion")]
		public float minSlope = 0.0f;
		public float maxDepth = 0.05f;

		[Range(0.0f, 90.0f)] public float minAngle = 0.0f;
		[NonSerialized] public float minAngleInRadians;

		[Range(0.0f, 1.0f)] public float sedimentCarryCapicity = 1.0f;
		[Range(0.0f, 1.0f)] public float erosionRate = 1.0f;
		[Range(0.0f, 1.0f)] public float depositionRate = 1.0f;
		[Range(0.0f, 0.1f)] public float slippageRate = 1.0f;

		public void OnValidate()
		{
			minAngleInRadians = minAngle * Mathf.Deg2Rad;
		}
	}
}
