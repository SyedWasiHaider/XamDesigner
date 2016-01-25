using System;
using System.Collections;

namespace XamDesigner
{
	public static class EnumerableExtensions
	{
		public delegate void GenericDelegate<T>(T item);
		public static void DoForEach(this IEnumerable enumerable, GenericDelegate<object> action, Type FilterByType = null){
			foreach (var item in enumerable) {
				if (FilterByType != null) {
					if (item.GetType () == FilterByType) {
						action (item);
					}
				}else{
					action (item);
				}
			}
		}
	}
}

