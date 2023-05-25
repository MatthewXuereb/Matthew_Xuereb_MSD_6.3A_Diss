using UnityEngine;

namespace Noise
{
    [CreateAssetMenu(fileName = "Cellular Settings", menuName = "Scriptable Objects/Cellular Settings", order = 1)]
    public class OCellularSettings : ONoiseSettings
    {
        public enum EDistance
        {
            Euclidean,
            EuclideanSquare,
            Manhattan,
            Hybrid
        }

        public enum EReturnType
        {
            Cell,
            Distance,
            Distance2,
            Distance2Add,
            Distance2Sub,
            Distance2Multiply,
            Distance2Divide
        }

        [Header("Cellular")]
        public EDistance distance = EDistance.Euclidean;
        public EReturnType returnType = EReturnType.Distance;

        [Range(0.0f, 2.0f)] public float jitter = 1.0f;
    }
}
