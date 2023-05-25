using UnityEngine;

namespace Mask
{
	[CreateAssetMenu(fileName = "Mask Settings", menuName = "Scriptable Objects/Mask Settings", order = 1)]
	public class OMaskSettings : ScriptableObject
	{
		[Header("Mask")]
		public float radius = 10.0f;
		public float stregth = 1.0f;

		public Vector2 offset = Vector2.zero;
		public Vector2 scale = Vector2.one;

		[Range(0.0f, 360.0f)] public float rotate = 0.0f;

		public float height = 100.0f;
	}
}
