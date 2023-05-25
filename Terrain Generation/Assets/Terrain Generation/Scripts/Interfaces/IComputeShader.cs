public interface IComputeShader
{
	public void InstantiateArrays();

	public void CreateBuffers();
	public void SetBuffers();
	public void DisposeBuffers();

	public void SetData();
	public void GetData();

	public void Execute();
}
