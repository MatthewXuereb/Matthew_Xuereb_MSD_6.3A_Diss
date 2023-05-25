using Unity.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Vegatation", menuName = "Scriptable Objects/Vegatation", order = 1)]
public class OVegatation : ScriptableObject
{
	public int id;

	[Header("Vegatation")]
	public GameObject[] vegatationObjects;

	[Header("Lifetime")]
	public float lifespan = 0;
	public float fertilityAge;

	public float maxDiameter = 1.0f;

	[Range(0.0f, 100.0f)] public float spawnRate = 0.0f;
	public float seedSpread = 1.0f;

	[Header("Height")]
	public float minHeight;
	public float maxHeight;

	[Header("Scale")]
	public float minScale;
	public float maxScale;

	[Range(0.0f, 90.0f)] public float maxAngle = 30.0f;
	[Range(0.0f, 10.0f)] public float maxPivot;

	public NativeList<float> SetVegatationData(NativeList<float> vegatation)
	{
		vegatation.Add(vegatationObjects.Length); // 0

		vegatation.Add(lifespan); // 1
		vegatation.Add(fertilityAge); // 2

		vegatation.Add(maxDiameter); // 3
		vegatation.Add(spawnRate); // 4
		vegatation.Add(seedSpread); // 5

		vegatation.Add(minHeight); // 6
		vegatation.Add(maxHeight); // 7

		vegatation.Add(minScale); // 8
		vegatation.Add(maxScale); // 9

		vegatation.Add(maxAngle); // 10
		vegatation.Add(maxPivot); // 11

		return vegatation;
	}
}
