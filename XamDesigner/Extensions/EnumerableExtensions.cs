using System;
using System.Collections;
using System.Collections.Generic;

namespace XamDesigner
{
	public static class EnumerableExtensions
	{
		public delegate void GenericDelegate();
		public delegate void GenericDelegate<T>(T item);
		public delegate void GenericDelegateWithIndex<T>(T item, int index);
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

		public static void DoForEach(this IEnumerable enumerable, GenericDelegateWithIndex<object> action, Type FilterByType = null){

			int index = 0;
			var enumerator = enumerable.GetEnumerator ();

				while (enumerator.MoveNext ()) {
					var item = enumerator.Current;
					if (FilterByType != null) {
						if (item.GetType () == FilterByType) {
							action (item, index);
						}
					}else{
						action (item, index);
					}
					index++;
				}
			
		}

	}
}

