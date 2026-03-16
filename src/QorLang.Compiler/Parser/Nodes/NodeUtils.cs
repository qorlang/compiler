namespace QorLang.Compiler.Parser.Nodes;

public static class NodeUtils
{
	public static int GetArrayHash<T>(T[]? array)
	{
		if (array == null) return 0;
		unchecked
		{
			int hash = 17;
			foreach (var item in array)
			{
				hash = hash * 31 + (item?.GetHashCode() ?? 0);
			}
			return hash;
		}
	}
}
