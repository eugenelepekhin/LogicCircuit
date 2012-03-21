#if UnitTestInternal //|| DEBUG
#define ValidateTree
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LogicCircuit.DataPersistent {
	internal partial class BTree<TField> {

		private struct Key {
			public TField field;
			public RowId  rowId;

			#if DEBUG
				public override string ToString() {
					return string.Format(System.Globalization.CultureInfo.InvariantCulture, "Key({0}, {1})", this.field.ToString(), this.rowId.ToString());
				}
			#endif
		}

		private interface IKeyField : IField<Node, Key> {
			int Compare(ref Node data, TField value);
		}

		private struct Node {

			public static IField<Node>[] CreateFields(IComparer<TField> comparer,
				out IField<Node, int> countField, out IField<Node, bool> isLeafField,
				out IKeyField[] keyFields, out IField<Node, RowId>[] childFields
			) {
				countField = new CountField();
				isLeafField = new IsLeafField();
				keyFields = new IKeyField[] {
					new K0Field(comparer),
					new K1Field(comparer),
					new K2Field(comparer),
					new K3Field(comparer),
					new K4Field(comparer)
				};
				childFields = new IField<Node, RowId>[] {
					new C0Field(),
					new C1Field(),
					new C2Field(),
					new C3Field(),
					new C4Field(),
					new C5Field()
				};
				IField<Node>[] fields = new IField<Node>[2 + keyFields.Length + childFields.Length];
				int index = 0;
				fields[index++] = countField;
				fields[index++] = isLeafField;
				foreach(IField<Node> f in keyFields) {
					fields[index++] = f;
				}
				foreach(IField<Node> f in childFields) {
					fields[index++] = f;
				}
				return fields;
			}

			public int Count;
			private bool isLeaf;
			public bool IsLeaf { get { return !this.isLeaf; } set { this.isLeaf = !value; } }
			public Key K0;
			public Key K1;
			public Key K2;
			public Key K3;
			public Key K4;
			public RowId C0;
			public RowId C1;
			public RowId C2;
			public RowId C3;
			public RowId C4;
			public RowId C5;

			#if DEBUG
				public override string ToString() {
					Node node = this;
					Func<int, Key> k = i => {
						switch(i) {
						case 0: return node.K0; case 1: return node.K1; case 2: return node.K2; case 3: return node.K3; case 4: return node.K4;
						default: throw new InvalidOperationException();
						}
					};
					Func<int, RowId> c = i => {
						switch(i) {
						case 0: return node.C0; case 1: return node.C1; case 2: return node.C2; case 3: return node.C3; case 4: return node.C4; case 5: return node.C5;
						default: throw new InvalidOperationException();
						}
					};
					System.Text.StringBuilder text = new System.Text.StringBuilder();
					text.AppendFormat("{0}<", node.IsLeaf ? "Leaf" : "Node");
					for(int i = 0; i < node.Count; i++) {
						if(0 < i) { text.Append(", "); }
						text.AppendFormat("C{0}={1}, K{0}={2}", i, c(i).Value, k(i));
					}
					text.AppendFormat(", C{0}={1}>", node.Count, c(node.Count).Value);
					return text.ToString();
				}
			#endif

			private class CountField : IField<Node, int> {
				public int DefaultValue { get { return 0; } }
				public int GetValue(ref Node record) { return record.Count; }
				public void SetValue(ref Node record, int value) {
					record.Count = value;
				}
				public string Name { get { return "count"; } }
				public int Order { get; set; }
				public int Compare(ref Node data1, ref Node data2) { return data1.Count - data2.Count; }
				public int Compare(int x, int y) { return x - y; }
			}
			private class IsLeafField : IField<Node, bool> {
				public bool DefaultValue { get { return true; } }
				public bool GetValue(ref Node record) { return record.IsLeaf; }
				public void SetValue(ref Node record, bool value) { record.IsLeaf = value; }
				public string Name { get { return "isLeaf"; } }
				public int Order { get; set; }
				public int Compare(ref Node data1, ref Node data2) { return (!data1.IsLeaf).CompareTo(!data2.IsLeaf); }
				public int Compare(bool x, bool y) { return x.CompareTo(y); }
			}
			private class K0Field : IKeyField {
				private readonly IComparer<TField> comparer;
				public K0Field(IComparer<TField> comparer) { this.comparer = comparer; }
				public Key DefaultValue { get { return default(Key); } }
				public Key GetValue(ref Node record) { return record.K0; }
				public void SetValue(ref Node record, Key value) { record.K0 = value; }
				public string Name { get { return "k0"; } }
				public int Order { get; set; }
				public int Compare(ref Node data1, ref Node data2) {
					int result = this.comparer.Compare(data1.K0.field, data2.K0.field);
					if(result == 0) {
						return data1.K0.rowId.Value - data2.K0.rowId.Value;
					}
					return result;
				}
				public int Compare(Key x, Key y) {
					int result = this.comparer.Compare(x.field, y.field);
					if(result == 0) {
						return x.rowId.Value - y.rowId.Value;
					}
					return result;
				}
				public int Compare(ref Node data, TField value) {
					return this.comparer.Compare(data.K0.field, value);
				}
			}
			private class K1Field : IKeyField {
				private readonly IComparer<TField> comparer;
				public K1Field(IComparer<TField> comparer) { this.comparer = comparer; }
				public Key DefaultValue { get { return default(Key); } }
				public Key GetValue(ref Node record) { return record.K1; }
				public void SetValue(ref Node record, Key value) { record.K1 = value; }
				public string Name { get { return "k1"; } }
				public int Order { get; set; }
				public int Compare(ref Node data1, ref Node data2) {
					int result = this.comparer.Compare(data1.K1.field, data2.K1.field);
					if(result == 0) {
						return data1.K1.rowId.Value - data2.K1.rowId.Value;
					}
					return result;
				}
				public int Compare(Key x, Key y) {
					int result = this.comparer.Compare(x.field, y.field);
					if(result == 0) {
						return x.rowId.Value - y.rowId.Value;
					}
					return result;
				}
				public int Compare(ref Node data, TField value) {
					return this.comparer.Compare(data.K1.field, value);
				}
			}
			private class K2Field : IKeyField {
				private readonly IComparer<TField> comparer;
				public K2Field(IComparer<TField> comparer) { this.comparer = comparer; }
				public Key DefaultValue { get { return default(Key); } }
				public Key GetValue(ref Node record) { return record.K2; }
				public void SetValue(ref Node record, Key value) { record.K2 = value; }
				public string Name { get { return "k2"; } }
				public int Order { get; set; }
				public int Compare(ref Node data1, ref Node data2) {
					int result = this.comparer.Compare(data1.K2.field, data2.K2.field);
					if(result == 0) {
						return data1.K2.rowId.Value - data2.K2.rowId.Value;
					}
					return result;
				}
				public int Compare(Key x, Key y) {
					int result = this.comparer.Compare(x.field, y.field);
					if(result == 0) {
						return x.rowId.Value - y.rowId.Value;
					}
					return result;
				}
				public int Compare(ref Node data, TField value) {
					return this.comparer.Compare(data.K2.field, value);
				}
			}
			private class K3Field : IKeyField {
				private readonly IComparer<TField> comparer;
				public K3Field(IComparer<TField> comparer) { this.comparer = comparer; }
				public Key DefaultValue { get { return default(Key); } }
				public Key GetValue(ref Node record) { return record.K3; }
				public void SetValue(ref Node record, Key value) { record.K3 = value; }
				public string Name { get { return "k3"; } }
				public int Order { get; set; }
				public int Compare(ref Node data1, ref Node data2) {
					int result = this.comparer.Compare(data1.K3.field, data2.K3.field);
					if(result == 0) {
						return data1.K3.rowId.Value - data2.K3.rowId.Value;
					}
					return result;
				}
				public int Compare(Key x, Key y) {
					int result = this.comparer.Compare(x.field, y.field);
					if(result == 0) {
						return x.rowId.Value - y.rowId.Value;
					}
					return result;
				}
				public int Compare(ref Node data, TField value) {
					return this.comparer.Compare(data.K3.field, value);
				}
			}
			private class K4Field : IKeyField {
				private readonly IComparer<TField> comparer;
				public K4Field(IComparer<TField> comparer) { this.comparer = comparer; }
				public Key DefaultValue { get { return default(Key); } }
				public Key GetValue(ref Node record) { return record.K4; }
				public void SetValue(ref Node record, Key value) { record.K4 = value; }
				public string Name { get { return "k4"; } }
				public int Order { get; set; }
				public int Compare(ref Node data1, ref Node data2) {
					int result = this.comparer.Compare(data1.K4.field, data2.K4.field);
					if(result == 0) {
						return data1.K4.rowId.Value - data2.K4.rowId.Value;
					}
					return result;
				}
				public int Compare(Key x, Key y) {
					int result = this.comparer.Compare(x.field, y.field);
					if(result == 0) {
						return x.rowId.Value - y.rowId.Value;
					}
					return result;
				}
				public int Compare(ref Node data, TField value) {
					return this.comparer.Compare(data.K4.field, value);
				}
			}
			private class C0Field : IField<Node, RowId> {
				public RowId DefaultValue { get { return new RowId(); } }
				public RowId GetValue(ref Node record) { return record.C0; }
				public void SetValue(ref Node record, RowId value) { record.C0 = value; }
				public string Name { get { return "c0"; } }
				public int Order { get; set; }
				public int Compare(ref Node data1, ref Node data2) { return data1.C0.CompareTo(data2.C0); }
				public int Compare(RowId x, RowId y) { return x.CompareTo(y); }
			}
			private class C1Field : IField<Node, RowId> {
				public RowId DefaultValue { get { return new RowId(); } }
				public RowId GetValue(ref Node record) { return record.C1; }
				public void SetValue(ref Node record, RowId value) { record.C1 = value; }
				public string Name { get { return "c1"; } }
				public int Order { get; set; }
				public int Compare(ref Node data1, ref Node data2) { return data1.C1.CompareTo(data2.C1); }
				public int Compare(RowId x, RowId y) { return x.CompareTo(y); }
			}
			private class C2Field : IField<Node, RowId> {
				public RowId DefaultValue { get { return new RowId(); } }
				public RowId GetValue(ref Node record) { return record.C2; }
				public void SetValue(ref Node record, RowId value) { record.C2 = value; }
				public string Name { get { return "c2"; } }
				public int Order { get; set; }
				public int Compare(ref Node data1, ref Node data2) { return data1.C2.CompareTo(data2.C2); }
				public int Compare(RowId x, RowId y) { return x.CompareTo(y); }
			}
			private class C3Field : IField<Node, RowId> {
				public RowId DefaultValue { get { return new RowId(); } }
				public RowId GetValue(ref Node record) { return record.C3; }
				public void SetValue(ref Node record, RowId value) { record.C3 = value; }
				public string Name { get { return "c3"; } }
				public int Order { get; set; }
				public int Compare(ref Node data1, ref Node data2) { return data1.C3.CompareTo(data2.C3); }
				public int Compare(RowId x, RowId y) { return x.CompareTo(y); }
			}
			private class C4Field : IField<Node, RowId> {
				public RowId DefaultValue { get { return new RowId(); } }
				public RowId GetValue(ref Node record) { return record.C4; }
				public void SetValue(ref Node record, RowId value) { record.C4 = value; }
				public string Name { get { return "c4"; } }
				public int Order { get; set; }
				public int Compare(ref Node data1, ref Node data2) { return data1.C4.CompareTo(data2.C4); }
				public int Compare(RowId x, RowId y) { return x.CompareTo(y); }
			}
			private class C5Field : IField<Node, RowId> {
				public RowId DefaultValue { get { return new RowId(); } }
				public RowId GetValue(ref Node record) { return record.C5; }
				public void SetValue(ref Node record, RowId value) { record.C5 = value; }
				public string Name { get { return "c5"; } }
				public int Order { get; set; }
				public int Compare(ref Node data1, ref Node data2) { return data1.C5.CompareTo(data2.C5); }
				public int Compare(RowId x, RowId y) { return x.CompareTo(y); }
			}
		}

		private readonly IField<Node, int> countField;
		private readonly IField<Node, bool> isLeafField;
		private readonly IKeyField[] keyFields;
		private readonly IField<Node, RowId>[] childFields;
		private readonly IField<Node>[] fields;
		private readonly int MinDegree;
		private readonly SnapTable<Node> table;
		private readonly IntArray data;

		public BTree(SnapStore store, string name, IComparer<TField> comparer) {
			this.fields = Node.CreateFields(comparer, out this.countField, out this.isLeafField, out this.keyFields, out this.childFields);
			this.MinDegree = this.keyFields.Length / 2;
			Debug.Assert(1 < this.MinDegree && this.MinDegree * 2 + 1 == this.keyFields.Length && this.keyFields.Length + 1 == this.childFields.Length,
				"Wrong field definition"
			);
			this.table = new SnapTable<Node>(store, name, 1, this.fields, false);
			this.data = new IntArray(store, name + "~Data~", 1);
			#if ValidateTree
				this.Validate();
			#endif
		}

		private RowId Root(int version) {
			return new RowId(this.data.Value(0, version));
		}

		private void SetRoot(RowId rowId) {
			this.data.SetValue(0, rowId.Value);
		}

		public bool IsEmpty(int version) {
			return this.table.GetField(this.Root(version), this.countField, version) == 0;
		}

		public void Insert(TField value, RowId rowId) {
			Key key = new Key() {
				field = value,
				rowId = rowId
			};
			RowId rootId = this.Root(this.table.SnapStore.Version);
			if(this.table.GetLatestField<int>(rootId, this.countField) < this.keyFields.Length) {
				this.InsertNoneFull(rootId, key);
			} else {
				Debug.Assert(this.table.GetLatestField<int>(rootId, this.countField) == this.keyFields.Length);
				Node node = new Node() {
					IsLeaf = false,
					C0 = rootId
				};
				rootId = this.table.Insert(ref node);
				this.SetRoot(rootId);
				this.SplitChild(rootId, 0);
				this.InsertNoneFull(rootId, key);
			}
			#if ValidateTree
				this.Validate();
			#endif
		}

		private void SplitChild(RowId rowId, int child) {
			Node node;
			this.table.GetLatestData(rowId, out node);
			Debug.Assert(node.Count < this.keyFields.Length, "The node should be none full");
			// make a room for a new key that will bubble up from child node
			for(int i = node.Count - 1; child <= i; i--) {
				this.keyFields[i + 1].SetValue(ref node, this.keyFields[i].GetValue(ref node));
			}
			// make a room for a new child
			for(int i = node.Count; child < i; i--) {
				this.childFields[i + 1].SetValue(ref node, this.childFields[i].GetValue(ref node));
			}

			RowId childId = this.childFields[child].GetValue(ref node);
			Node oldChild;
			this.table.GetLatestData(childId, out oldChild);
			Debug.Assert(oldChild.Count == this.keyFields.Length, "The node should be full in order to be split");

			Node newChild = new Node() {
				IsLeaf = oldChild.IsLeaf
			};
			this.Move(ref oldChild, this.MinDegree + 1, ref newChild);
			oldChild.Count--;
			Debug.Assert(oldChild.Count == newChild.Count && newChild.Count == this.MinDegree);
			node.Count++;
			this.keyFields[child].SetValue(ref node, this.keyFields[this.MinDegree].GetValue(ref oldChild));
			this.keyFields[this.MinDegree].SetValue(ref oldChild, default(Key));
			this.childFields[child + 1].SetValue(ref node, this.table.Insert(ref newChild));
			this.table.SetData(childId, ref oldChild);
			this.table.SetData(rowId, ref node);
		}

		private void InsertNoneFull(RowId rowId, Key key) {
			Node node;
			this.table.GetLatestData(rowId, out node);
			Debug.Assert(node.Count < this.keyFields.Length, "Node should be none full to perform the InserNoneFull");
			int i = node.Count - 1;
			if(node.IsLeaf) {
				while(0 <= i && this.keyFields[i].Compare(key, this.keyFields[i].GetValue(ref node)) < 0) {
					this.keyFields[i + 1].SetValue(ref node, this.keyFields[i].GetValue(ref node));
					i--;
				}
				this.keyFields[i + 1].SetValue(ref node, key);
				node.Count++;
				this.table.SetData(rowId, ref node);
			} else {
				while(0 <= i && this.keyFields[i].Compare(key, this.keyFields[i].GetValue(ref node)) < 0) {
					i--;
				}
				i++;
				RowId childId = this.childFields[i].GetValue(ref node);
				if(this.table.GetLatestField<int>(childId, this.countField) == this.keyFields.Length) {
					this.SplitChild(rowId, i);
					if(this.keyFields[i].Compare(key, this.table.GetLatestField<Key>(rowId, this.keyFields[i])) > 0) {
						i++;
					}
				}
				this.InsertNoneFull(this.table.GetLatestField<RowId>(rowId, this.childFields[i]), key);
			}
		}

		public bool Remove(TField value, RowId rowId) {
			Key key = new Key() {
				field = value,
				rowId = rowId
			};
			RowId rootId = this.Root(this.table.SnapStore.Version);
			bool deleted = this.Delete(rootId, key);

			#if ValidateTree
				this.Validate();
			#endif

			return deleted;
		}

		private Key Maximum(RowId rowId, int version) {
			Node node;
			for(;;) {
				this.table.GetData(rowId, version, out node);
				if(node.IsLeaf) {
					return this.keyFields[node.Count - 1].GetValue(ref node);
				}
				rowId = this.childFields[node.Count].GetValue(ref node);
			}
		}

		private Key Minimum(RowId rowId, int version) {
			Node node;
			for(;;) {
				this.table.GetData(rowId, version, out node);
				if(node.IsLeaf) {
					return node.K0;
				}
				rowId = node.C0;
			}
		}

		/// <summary>
		/// Gets minimum value of the field. Will throw if the tree is empty.
		/// </summary>
		/// <param name="version"></param>
		/// <returns></returns>
		public TField MinimumValue(int version) {
			return this.Minimum(this.Root(version), version).field;
		}

		/// <summary>
		/// Get maximum value of the field. Throws if the tree is empty
		/// </summary>
		/// <param name="version"></param>
		/// <returns></returns>
		public TField MaximumValue(int version) {
			return this.Maximum(this.Root(version), version).field;
		}

		private int Lookup(ref Node node, Key key) {
			int i;
			for(i = 0; i < node.Count && this.keyFields[i].Compare(this.keyFields[i].GetValue(ref node), key) < 0; i++);
			return i;
		}

		private void RemoveAt(ref Node node, int index) {
			for(int i = index; i < node.Count - 1; i++) {
				this.keyFields[i].SetValue(ref node, this.keyFields[i + 1].GetValue(ref node));
				this.childFields[i].SetValue(ref node, this.childFields[i + 1].GetValue(ref node));
			}
			node.Count--;
			this.childFields[node.Count].SetValue(ref node, this.childFields[node.Count + 1].GetValue(ref node));
		}

		private void Move(ref Node from, int start, ref Node to) {
			Debug.Assert(0 <= start && start <= from.Count);
			int count = from.Count - start;
			Debug.Assert(0 <= count && count <= this.keyFields.Length - to.Count);
			for(int i = 0; i < count; i++) {
				this.keyFields[to.Count + i].SetValue(ref to, this.keyFields[start + i].GetValue(ref from));
				this.keyFields[start + i].SetValue(ref from, default(Key));
			}
			for(int i = 0; i <= count; i++) {
				this.childFields[to.Count + i].SetValue(ref to, this.childFields[start + i].GetValue(ref from));
				#if DEBUG
					this.childFields[start + i].SetValue(ref from, new RowId());
				#endif
			}
			from.Count -= count;
			to.Count += count;
			Debug.Assert(0 <= from.Count && from.Count <= this.keyFields.Length);
			Debug.Assert(0 <= to.Count && to.Count <= this.keyFields.Length);
		}

		private bool Delete(RowId rowId, Key key) {
			int version = this.table.SnapStore.Version;
			Node node;
			this.table.GetLatestData(rowId, out node);
			int i = this.Lookup(ref node, key);
			Debug.Assert(i == node.Count || 0 <= i && i < node.Count && this.keyFields[i].Compare(this.keyFields[i].GetValue(ref node), key) >= 0);
			if(i < node.Count && this.keyFields[i].Compare(this.keyFields[i].GetValue(ref node), key) == 0) { // key was found in the node
				if(node.IsLeaf) {
					this.RemoveAt(ref node, i);
					this.table.SetData(rowId, ref node);
				} else {
					RowId leftId = this.childFields[i].GetValue(ref node);
					Node left;
					this.table.GetLatestData(leftId, out left);
					if(this.MinDegree < left.Count) {
						Key max = this.Maximum(leftId, version);
						this.keyFields[i].SetValue(ref node, max);
						this.Delete(leftId, max);
						this.table.SetData(rowId, ref node);
					} else {
						Debug.Assert(left.Count == this.MinDegree);
						RowId rightId = this.childFields[i + 1].GetValue(ref node);
						Node right;
						this.table.GetLatestData(rightId, out right);
						if(this.MinDegree < right.Count) {
							Key min = this.Minimum(rightId, version);
							this.keyFields[i].SetValue(ref node, min);
							this.Delete(rightId, min);
							this.table.SetData(rowId, ref node);
						} else {
							Debug.Assert(right.Count == this.MinDegree);
							// Merge left and all of right into left. Left now contains 2 * MinDegree + 1 keys
							this.keyFields[left.Count].SetValue(ref left, key);
							left.Count++;
							this.Move(ref right, 0, ref left);
							Debug.Assert(left.Count == 2 * this.MinDegree + 1 && right.Count == 0);
							this.RemoveAt(ref node, i);
							Debug.Assert(this.MinDegree <= node.Count || this.Root(version) == rowId);
							if(node.Count == 0) {
								this.SetRoot(leftId);
								this.table.Delete(rowId); // TODO: recycle it
							} else {
								this.childFields[i].SetValue(ref node, leftId);
								this.table.SetData(rowId, ref node);
							}
							this.table.SetData(leftId, ref left);
							// TODO: recycle it instead
							this.table.Delete(rightId);
							this.Delete(leftId, key);
						}
					}
				}
				return true;
			} else if(!node.IsLeaf) { // key was not found in the node
				Debug.Assert(0 < node.Count);
				RowId childId = this.childFields[i].GetValue(ref node);
				Node child;
				this.table.GetLatestData(childId, out child);
				Debug.Assert(this.MinDegree <= child.Count);
				if(this.MinDegree == child.Count) {
					RowId siblingId;
					int siblingIndex = i - 1;
					if(0 <= siblingIndex) {
						siblingId = this.childFields[siblingIndex].GetValue(ref node);
					} else {
						siblingIndex = i + 1;
						siblingId = this.childFields[siblingIndex].GetValue(ref node);
					}
					Node sibling;
					this.table.GetLatestData(siblingId, out sibling);
					Debug.Assert(this.MinDegree <= sibling.Count);
					if(this.MinDegree == sibling.Count && siblingIndex < i && i < node.Count) {
						siblingIndex = i + 1;
						siblingId = this.childFields[siblingIndex].GetValue(ref node);
						this.table.GetLatestData(siblingId, out sibling);
						Debug.Assert(this.MinDegree <= sibling.Count);
					}
					if(this.MinDegree < sibling.Count) {
						if(i < siblingIndex) { // right sibling
							this.keyFields[child.Count].SetValue(ref child, this.keyFields[i].GetValue(ref node));
							child.Count++;
							this.childFields[child.Count].SetValue(ref child, this.childFields[0].GetValue(ref sibling));
							this.keyFields[i].SetValue(ref node, this.keyFields[0].GetValue(ref sibling));
							this.RemoveAt(ref sibling, 0);
							this.table.SetData(rowId, ref node);
							this.table.SetData(childId, ref child);
							this.table.SetData(siblingId, ref sibling);
						} else { // left sibling
							for(int j = child.Count - 1; 0 <= j; j--) {
								this.keyFields[j + 1].SetValue(ref child, this.keyFields[j].GetValue(ref child));
							}
							for(int j = child.Count; 0 <= j; j--) {
								this.childFields[j + 1].SetValue(ref child, this.childFields[j].GetValue(ref child));
							}
							this.keyFields[0].SetValue(ref child, this.keyFields[siblingIndex].GetValue(ref node));
							this.keyFields[siblingIndex].SetValue(ref node, this.keyFields[sibling.Count - 1].GetValue(ref sibling));
							this.childFields[0].SetValue(ref child, this.childFields[sibling.Count].GetValue(ref sibling));
							sibling.Count--;
							child.Count++;
							this.table.SetData(rowId, ref node);
							this.table.SetData(childId, ref child);
							this.table.SetData(siblingId, ref sibling);
						}
					} else { // child and both siblings has MinDegree keys
						// we cannot descend to a child node with only MinDegree keys so
						// merge child with sibling and
						// make the appropriate key of node the middle key of the new node, child.
						// Note: This may cause the root to collapse, thus making child the new root.
						if(siblingIndex < i) {
							int j = i;
							i = siblingIndex;
							siblingIndex = j;
							RowId id = childId;
							childId = siblingId;
							siblingId = id;
							Node n = child;
							child = sibling;
							sibling = n;
						}
						this.keyFields[child.Count].SetValue(ref child, this.keyFields[i].GetValue(ref node));
						child.Count++;
						this.Move(ref sibling, 0, ref child);
						Debug.Assert(child.Count == this.keyFields.Length);
						Debug.Assert(sibling.Count == 0);
						this.RemoveAt(ref node, i);
						this.childFields[i].SetValue(ref node, childId);
						this.table.Delete(siblingId); // TODO: reuse it
						this.table.SetData(childId, ref child);
						if(node.Count == 0) {
							Debug.Assert(this.Root(version) == rowId);
							this.table.Delete(rowId);
							this.SetRoot(childId);
						} else {
							this.table.SetData(rowId, ref node);
						}
					}
				}
				return this.Delete(childId, key);
			}
			return false;
		}

		public bool Exists(TField value, int version) {
			RowId rowId = this.Root(version);
			for(;;) {
				Node node;
				this.table.GetData(rowId, version, out node);
				int i;
				for(i = 0; i < node.Count && this.keyFields[i].Compare(ref node, value) < 0; i++);
				if(i < node.Count && this.keyFields[i].Compare(ref node, value) == 0) {
					return true;
				}
				if(node.IsLeaf) {
					return false;
				}
				rowId = this.childFields[i].GetValue(ref node);
			}
		}

		public IEnumerable<RowId> Select(TField value, int version) {
			Stack<RowId> stack = new Stack<RowId>();
			stack.Push(this.Root(version));
			while(0 < stack.Count) {
				RowId rowId = stack.Pop();
				Node node;
				this.table.GetData(rowId, version, out node);
				int i, j;
				for(i = 0; i < node.Count && this.keyFields[i].Compare(ref node, value) < 0; i++);
				for(j = i; j < node.Count && this.keyFields[j].Compare(ref node, value) == 0; j++) {
					yield return this.keyFields[j].GetValue(ref node).rowId;
					if(!node.IsLeaf) {
						stack.Push(this.childFields[j].GetValue(ref node));
					}
				}
				if(!node.IsLeaf) {
					if(j < node.Count) {
						stack.Push(this.childFields[j].GetValue(ref node));
					} else if(this.keyFields[j - 1].Compare(ref node, value) <= 0) {
						stack.Push(this.childFields[j].GetValue(ref node));
					}
				}
			}
		}

		public IEnumerable<RowId> Select(TField min, TField max, int version) {
			Stack<RowId> stack = new Stack<RowId>();
			stack.Push(this.Root(version));
			while(0 < stack.Count) {
				RowId rowId = stack.Pop();
				Node node;
				this.table.GetData(rowId, version, out node);
				int i, j;
				for(i = 0; i < node.Count && this.keyFields[i].Compare(ref node, min) < 0; i++);
				for(j = i; j < node.Count && this.keyFields[j].Compare(ref node, min) >= 0 && this.keyFields[j].Compare(ref node, max) <= 0; j++) {
					yield return this.keyFields[j].GetValue(ref node).rowId;
					if(!node.IsLeaf) {
						stack.Push(this.childFields[j].GetValue(ref node));
					}
				}
				if(!node.IsLeaf) {
					if(j < node.Count) {
						stack.Push(this.childFields[j].GetValue(ref node));
					} else if(this.keyFields[j - 1].Compare(ref node, max) <= 0) {
						stack.Push(this.childFields[j].GetValue(ref node));
					}
				}
			}
		}

		#if ValidateTree
			private void Validate() {
				RowId rootId = this.Root(this.table.SnapStore.Version);
				this.Validate(rootId);
			}
			private void Validate(RowId rowId) {
				int version = this.table.SnapStore.Version;
				Node node;
				this.table.GetData(rowId, version, out node);
				Debug.Assert(0 < node.Count && node.Count <= this.keyFields.Length || node.Count == 0 && node.IsLeaf && this.Root(version) == rowId);
				Key key = node.K0;
				for(int i = 1; i < node.Count; i++) {
					Key k2 = this.keyFields[i].GetValue(ref node);
					Debug.Assert(this.keyFields[i].Compare(key, k2) < 0);
					if(!node.IsLeaf) {
						this.Validate(this.childFields[i].GetValue(ref node), key, k2);
					}
					key = k2;
				}
				if(!node.IsLeaf) {
					this.ValidateFirst(this.childFields[0].GetValue(ref node), node.K0);
					this.ValidateLast(this.childFields[node.Count].GetValue(ref node), this.keyFields[node.Count - 1].GetValue(ref node));
				}
			}
			private void Validate(RowId rowId, Key min, Key max) {
				int version = this.table.SnapStore.Version;
				Node node;
				this.table.GetData(rowId, version, out node);
				Debug.Assert(this.MinDegree <= node.Count && node.Count <= this.keyFields.Length);
				Debug.Assert(this.keyFields[0].Compare(min, this.keyFields[0].GetValue(ref node)) < 0);
				Debug.Assert(this.keyFields[0].Compare(this.keyFields[node.Count - 1].GetValue(ref node), max) < 0);
				this.Validate(rowId);
			}
			private void ValidateLast(RowId rowId, Key min) {
				int version = this.table.SnapStore.Version;
				Node node;
				this.table.GetData(rowId, version, out node);
				Debug.Assert(this.MinDegree <= node.Count && node.Count <= this.keyFields.Length);
				Debug.Assert(this.keyFields[0].Compare(min, this.keyFields[0].GetValue(ref node)) < 0);
				this.Validate(rowId);
			}
			private void ValidateFirst(RowId rowId, Key max) {
				int version = this.table.SnapStore.Version;
				Node node;
				this.table.GetData(rowId, version, out node);
				Debug.Assert(this.MinDegree <= node.Count && node.Count <= this.keyFields.Length);
				Debug.Assert(this.keyFields[0].Compare(this.keyFields[node.Count - 1].GetValue(ref node), max) < 0);
				this.Validate(rowId);
			}
		#endif

		#if DEBUG
			public string DebuggingVisualization { get { return this.BuildDebuggingVisualization(this.table.SnapStore.Version); } }
			private string BuildDebuggingVisualization(int version) {
				System.Text.StringBuilder text = new System.Text.StringBuilder();
				RowId root = this.Root(version);
				int level = 0;
				while(this.BuildDebuggingVisualization(text, version, level, root)) {
					text.AppendLine();
					level++;
				}
				return text.ToString();
			}
			private bool BuildDebuggingVisualization(System.Text.StringBuilder text, int version, int level, RowId rowId) {
				Node node;
				this.table.GetData(rowId, version, out node);
				if(0 < level) {
					if(node.IsLeaf) {
						return false;
					}
					bool result = false;
					for(int i = 0; i <= node.Count; i++) {
						result |= this.BuildDebuggingVisualization(text, version, level - 1, this.childFields[i].GetValue(ref node));
					}
					return result;
				} else {
					Debug.Assert(0 == level);
					text.AppendFormat(" {0}<{1}>(", rowId.ToString(), node.Count);
					for(int i = 0; i < node.Count; i++) {
						if(0 < i) {
							text.Append(", ");
						}
						Key key = this.keyFields[i].GetValue(ref node);
						text.AppendFormat("{{{0}, {1}}}", key.field.ToString(), key.rowId.ToString());
					}
					text.Append(")");
					return !node.IsLeaf;
				}
			}
		#endif
	}
}
