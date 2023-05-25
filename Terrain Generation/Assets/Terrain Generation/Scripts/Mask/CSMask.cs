using UnityEngine;

namespace Mask
{
	public class CSMask : IComputeShader
	{
		private int _res, _halfRes;
		private int _arrLen;
		private int _threadGroup;

		private int[,] _coords;
		private float[] _mask;

		private OMaskSettings _maskSettings;

		private ComputeBuffer _coordsBuffer;
		private ComputeBuffer _maskBuffer;

		private ComputeShader _maskShader;

		public CSMask(int res, OMaskSettings maskSettings)
		{
			_res = res;
			_halfRes = _res / 2;
			_arrLen = _res * _res;

			_threadGroup = Mathf.CeilToInt(_arrLen / 1024.0f);

			_mask = new float[_res * _res];

			_maskSettings = maskSettings;

			string path = "Compute Shaders/";
			_maskShader = Resources.Load(path + "Mask") as ComputeShader;
		}

		public float[] Mask
		{
			get
			{
				return _mask;
			}
		}

		public void InstantiateArrays()
		{
			_coords = new int[_arrLen, 2];
			_mask = new float[_arrLen];

			for (int y = 0, i = 0; y < _res; y++)
			{
				for (int x = 0; x < _res; x++, i++)
				{
					_coords[i, 0] = x;
					_coords[i, 1] = y;
				}
			}
		}

		public void CreateBuffers()
		{
			_coordsBuffer = new ComputeBuffer(_arrLen, 2 * sizeof(int));
			_maskBuffer = new ComputeBuffer(_arrLen, sizeof(float));
		}

		public void SetBuffers()
		{
			float angle = _maskSettings.rotate * Mathf.PI / 180;
			float cos = Mathf.Cos(angle);
			float sin = Mathf.Sin(angle);

			_maskShader.SetBuffer(0, "coords", _coordsBuffer);
			_maskShader.SetBuffer(0, "mask", _maskBuffer);
			_maskShader.SetFloat("canvasSize", 100.0f);
			_maskShader.SetFloat("res", _res);
			_maskShader.SetFloat("halfRes", _halfRes);
			_maskShader.SetFloat("radius", _maskSettings.radius);
			_maskShader.SetFloat("stregth", _maskSettings.stregth);
			_maskShader.SetFloats("offset", _maskSettings.offset.x, _maskSettings.offset.y);
			_maskShader.SetFloats("scale", _maskSettings.scale.x, _maskSettings.scale.y);
			_maskShader.SetFloat("cos", cos);
			_maskShader.SetFloat("sin", sin);
		}

		public void DisposeBuffers()
		{
			_coordsBuffer.Dispose();
			_maskBuffer.Dispose();
		}

		public void SetData()
		{
			_coordsBuffer.SetData(_coords);
			_maskBuffer.SetData(_mask);
		}

		public void GetData()
		{
			_maskBuffer.GetData(_mask);
		}

		public void Execute()
		{
			_maskShader.Dispatch(0, _threadGroup, 1, 1);
		}
	}
}
