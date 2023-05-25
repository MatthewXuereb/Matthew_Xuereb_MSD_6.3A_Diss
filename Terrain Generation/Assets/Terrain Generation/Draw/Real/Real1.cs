using UnityEngine;

public class Real1 : DrawRealTerrain
{
	private void Update()
	{
		Flow();

		if (!fluid.runSim)
			if (!terrainDrawn) Draw();
			else Grow();
	}
}
