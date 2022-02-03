using System;
using System.Collections.Generic;

namespace NeoModTest
{
	// Token: 0x02000015 RID: 21
	public static class EnumeratorExtensions
	{
		// Token: 0x0600007F RID: 127 RVA: 0x000024E7 File Offset: 0x000006E7
		public static IEnumerable<T> ToEnumerable<T>(this IEnumerator<T> enumerator)
		{
			while (enumerator.MoveNext())
			{
				!0 ! = enumerator.Current;
				yield return !;
			}
			yield break;
		}
	}
}
