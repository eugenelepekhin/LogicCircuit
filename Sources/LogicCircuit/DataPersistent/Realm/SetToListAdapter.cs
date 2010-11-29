using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace LogicCircuit.DataPersistent {
	public class SetToListAdapter<T> : IList<T>, IList, INotifyCollectionChanged where T:class {
		
		private event NotifyCollectionChangedEventHandler collectionChanged;
		public event NotifyCollectionChangedEventHandler CollectionChanged {
			add {
				bool first = (this.collectionChanged == null);
				this.collectionChanged += value;
				if(first) {
					INotifyCollectionChanged ncc = (INotifyCollectionChanged)this.collection;
					ncc.CollectionChanged += new NotifyCollectionChangedEventHandler(this.actuallCollectionChanged);
				}
			}
			remove {
				this.collectionChanged -= value;
				if(this.collectionChanged == null) {
					INotifyCollectionChanged ncc = (INotifyCollectionChanged)this.collection;
					ncc.CollectionChanged -= new NotifyCollectionChangedEventHandler(this.actuallCollectionChanged);
				}
			}
		}

		private IEnumerable<T> collection;
		private ObservableCollection<T> list;

		public SetToListAdapter(IEnumerable<T> collection) {
			if(collection is IList<T>) {
				throw new ArgumentOutOfRangeException("collection");
			}
			this.collection = collection;
			this.CreateList();
		}

		private void CreateList() {
			this.list = new ObservableCollection<T>(this.collection);
			this.list.CollectionChanged += new NotifyCollectionChangedEventHandler(this.listCollectionChanged);
		}

		private void Reset() {
			this.CreateList();
			this.listCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		private void actuallCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
			switch(e.Action) {
			case NotifyCollectionChangedAction.Add:
				foreach(T item in e.NewItems) {
					this.list.Add(item);
				}
				break;
			case NotifyCollectionChangedAction.Remove:
				foreach(T item in e.OldItems) {
					this.list.Remove(item);
				}
				break;
			case NotifyCollectionChangedAction.Reset:
				this.Reset();
				break;
			}
		}

		private void listCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
			NotifyCollectionChangedEventHandler handler = this.collectionChanged;
			if(handler != null) {
				handler(this, e);
			}
		}

		public int IndexOf(T item) {
			return this.list.IndexOf(item);
		}

		public int IndexOf(object value) {
			T item = value as T;
			if(item != null) {
				return this.IndexOf(item);
			}
			return -1;
		}

		public void Insert(int index, T item) {
			throw new InvalidOperationException();
		}

		public void Insert(int index, object value) {
			throw new InvalidOperationException();
		}

		public void RemoveAt(int index) {
			throw new InvalidOperationException();
		}

		public T this[int index] {
			get { return this.list[index]; }
			set { throw new InvalidOperationException(); }
		}

		object IList.this[int index] {
			get { return this.list[index]; }
			set { throw new InvalidOperationException(); }
		}

		public void Add(T item) {
			throw new InvalidOperationException();
		}

		public int Add(object value) {
			throw new InvalidOperationException();
		}

		public void Clear() {
			throw new InvalidOperationException();
		}

		public bool Contains(T item) {
			return this.list.Contains(item);
		}

		public bool Contains(object value) {
			T item = value as T;
			if(item != null) {
				return this.Contains(item);
			}
			return false;
		}

		public void CopyTo(T[] array, int arrayIndex) {
			this.list.CopyTo(array, arrayIndex);
		}

		public void CopyTo(Array array, int index) {
			if(array == null) {
				throw new ArgumentNullException("array");
			}
			int count = Math.Min(array.Length - index, this.list.Count);
			for(int i = 0; i < count; i++) {
				array.SetValue(this.list[i], i + index);
			}
		}

		public int Count {
			get { return this.list.Count; }
		}

		public bool IsReadOnly {
			get { return true; }
		}

		public bool Remove(T item) {
			throw new InvalidOperationException();
		}

		public void Remove(object value) {
			throw new InvalidOperationException();
		}

		public IEnumerator<T> GetEnumerator() {
			return this.list.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return this.GetEnumerator();
		}

		public bool IsFixedSize {
			get { return true; }
		}

		public bool IsSynchronized {
			get { return false; }
		}

		public object SyncRoot {
			get { return null; }
		}
	}
}
