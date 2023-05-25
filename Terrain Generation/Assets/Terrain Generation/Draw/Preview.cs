using Mask;
using Noise;
using TerrainGenerator;
using UnityEngine;

public class Preview : MonoBehaviour
{
	private int _res = 128;

	public MeshRenderer meshRenderer;

	private NoiseGenerator _noiseGenerator;

	[Space]
	public OTerrainSettings terrainSettings;

	[Space]
	public ONoiseSettings fBmSettings;
	public OCellularSettings cellularSettigns;

	[Space]
	public OMaskSettings maskSettings1;
	public OMaskSettings maskSettings2;

	void Update()
	{
		_noiseGenerator = new NoiseGenerator(_res, fBmSettings, cellularSettigns);

		float[] mask1 = _noiseGenerator.GenerateMask(maskSettings1);
		float[] mask2 = _noiseGenerator.GenerateMask(maskSettings2);

		int arrLen = _res * _res;

		float[] mask = new float[arrLen];
		for (int i = 0; i < arrLen; i++)
			mask[i] = mask2[i] > mask1[i] ? mask2[i] : mask1[i];

		float[] noise = _noiseGenerator.GenerateFBmNoiseMap();
		float[] cellalur = _noiseGenerator.GenerateCellularNoiseMap();

		float[] result = new float[arrLen];
		for (int i = 0; i < arrLen; i++)
			result[i] = mask[i] * cellalur[i] * noise[i];

		Color[] colourMap = new Color[_res * _res];
		for (int i = 0; i < _res * _res; i++)
			colourMap[i] = Color.Lerp(Color.black, Color.white, result[i]);

		Texture2D texture = new Texture2D(_res, _res);
		texture.SetPixels(colourMap);
		texture.filterMode = FilterMode.Point;
		texture.Apply();

		meshRenderer.material.mainTexture = texture;
	}
}
