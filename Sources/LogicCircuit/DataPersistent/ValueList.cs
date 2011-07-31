using System;
using System.Diagnostics;

namespace LogicCircuit.DataPersistent {
	internal class ValueList<TRow> where TRow:struct {
		/// <summary>
		/// Logarithm of page size on base of 2
		/// </summary>
		private const int LogPageSize = (
			#if DEBUG
				6
			#else
				10
			#endif
		);
		/// <summary>
		/// Number of items on the page
		/// </summary>
		private const int PageSize = 1 << LogPageSize;
		/// <summary>
		/// Used to retrieve index of item on the page from overall index
		/// </summary>
		private const int IndexOnPageMask = PageSize - 1;

		/// <summary>
		/// Array of pages each page contains PageSize items
		/// </summary>
		private TRow[][] page = new TRow[1][];
		/// <summary>
		/// Number of items in the list
		/// </summary>
		public int Count { get; private set; }

		/// <summary>
		/// Ensures memory is allocated to accommodate for one more element
		/// </summary>
		public void PrepareAdd() {
			int index = this.Count;
			if(int.MaxValue - 1 <= index) {
				throw new InvalidOperationException(Properties.Resources.ErrorValueListTooBig);
			}
			int pageIndex = index >> LogPageSize;
			if(pageIndex == this.page.Length) {
				TRow[][] p = this.page;
				Array.Resize<TRow[]>(ref p, p.Length * 2);
				LockFreeSync.WriteBarrier();
				this.page = p;
			}
			int itemIndex = index & IndexOnPageMask;
			if(itemIndex == 0 && this.page[pageIndex] == null) {
				this.page[pageIndex] = new TRow[PageSize];
			}
		}

		/// <summary>
		/// Adds new element assuming enough memory for it already exist. This method should be prepared by PrepareAdd call.
		/// </summary>
		/// <param name="row"></param>
		/// <returns></returns>
		public int FixedAdd(ref TRow row) {
			int index = this.Count;
			//if(index == int.MaxValue) {
			//    throw new OverflowException();
			//}
			int pageIndex = index >> LogPageSize;
			int itemIndex = index & IndexOnPageMask;
			this.page[pageIndex][itemIndex] = row;
			LockFreeSync.WriteBarrier();
			this.Count++;
			return index;
		}

		/// <summary>
		/// Allocates new item in the prepared list and returns it's index
		/// </summary>
		/// <returns></returns>
		public int FixedAllocate() {
			return this.Count++;
		}

		/// <summary>
		/// Adds item in the list
		/// </summary>
		/// <param name="row"></param>
		/// <returns></returns>
		public int Add(ref TRow row) {
			this.PrepareAdd();
			return this.FixedAdd(ref row);
		}

		/// <summary>
		/// Shrinks list to new size.
		/// It is guarantee that space allocated by PrepareAdd will not be freed here.
		/// </summary>
		/// <param name="newSize"></param>
		public void Shrink(int newSize) {
			int oldSize = this.Count;
			if(newSize < oldSize) {
				this.Count = newSize;
				LockFreeSync.WriteBarrier();
				for(int i = newSize >> LogPageSize; i < this.page.Length; i++) {
					TRow[] p = this.page[i];
					if(p == null) {
						return;
					}
					for(int j = newSize & IndexOnPageMask; j < p.Length; j++) {
						if(oldSize <= newSize++) {
							return;
						}
						p[j] = default(TRow);
					}
				}
			} else if(oldSize < newSize) {
				throw new ArgumentOutOfRangeException("newSize");
			}
		}

		/// <summary>
		/// Gets "address" of the item
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public Address ItemAddress(int index) {
			if(0 <= index && index < this.Count) {
				return new Address(this.page[index >> LogPageSize], index & IndexOnPageMask);
			} else {
				throw new ArgumentOutOfRangeException("index");
			}
		}

		public struct Address {
			private TRow[] page;
			public TRow[] Page { get { return this.page; } }

			private int index;
			public int Index { get { return this.index; } }

			public Address(TRow[] page, int index) {
				Debug.Assert(page != null && 0 <= index && index < page.Length);
				this.page = page;
				this.index = index;
			}
		}
	}
}
