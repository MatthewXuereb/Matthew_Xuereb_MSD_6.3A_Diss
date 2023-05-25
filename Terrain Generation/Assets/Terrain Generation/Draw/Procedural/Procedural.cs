using Erosion;
using UnityEngine;

public class Procedural : DrawProceduralTerrain
{
    private void Update()
	{
		GenerateNoise();

        Erode();

        if (!erosion.runSim)
			if (!terrainDrawn) Draw();
			else vegatationSpawner.Simulate();
	}

	protected override void GenerateNoise()
    {
		if (!noiseGenerator.executed)
		{
			float[] mask1 = noiseGenerator.GenerateMask(maskSettings[0]);
			float[] mask2 = noiseGenerator.GenerateMask(maskSettings[1]);

			float[] mask = new float[arrLen];
			for (int i = 0; i < arrLen; i++)
				mask[i] = mask2[i] > mask1[i] ? mask2[i] : mask1[i];

			float[] noise = noiseGenerator.GenerateFBmNoiseMap();
			float[] cellalur = noiseGenerator.GenerateCellularNoiseMap();

			float[] result = new float[arrLen];
			for (int i = 0; i < arrLen; i++)
				result[i] = mask[i] * cellalur[i] * noise[i];
			noiseGenerator.executed = true;
			landmass.SetValues(result);
			erosion = new CSErosion(terrainSettings, erosionSettings, fluidSettings, landmass, water);
			erosion.InstantiateArrays();

			erosion.CreateBuffers();
			erosion.SetData();
			erosion.SetBuffers();
		}
	}

	protected override void Erode()
	{
		erosion.Execute();
	}

	protected override void Draw()
	{
		erosion.GetData();
		erosion.DisposeBuffers();

		landmass.Draw(Vector3.zero);
		water.Draw(Vector3.zero);

		Texture2D output = CreateTexture();
		meshRenderer.material.mainTexture = output;

		landmass.material.SetTexture("Splatmap", output);
		WriteFile(output);

		terrainDrawn = true;
		vegatationSpawner.Init();
	}

	protected override void Grow()
	{
		if (vegatationSpawner.runSim)
			vegatationSpawner.Simulate();
	}
}
