namespace Shared.Http;

public class PagedResult<T>
{
	public int TotalCount { get; }
	public List<T> Values { get; }
	public PagedResult(int totalCount, List<T> values)
	{
		TotalCount = totalCount;
		Values = values;
	}
}