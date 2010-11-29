using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LogicCircuit.DataPersistent {
	internal partial class Unique<TField> {
		private struct Bucket {
			public static readonly RowId Empty = new RowId(-1);
			public static readonly RowId Deleted = new RowId(-2);

			public TField Value;
			
			private int hash;
			public int Hash {
				get { return this.hash & int.MaxValue; }
				set { this.hash = (value | (this.hash & ~int.MaxValue)); }
			}
			public bool HasCollision {
				get { return (this.hash & ~int.MaxValue) != 0; }
				set {
					if(value) {
						this.hash |= ~int.MaxValue;
					} else {
						this.hash &= int.MaxValue;
					}
				}
			}

			private int index;

			public RowId RowId {
				get { return new RowId(this.index - 1); }
				set { this.index = value.Value + 1; }
			}

			public bool IsEmpty { get { return this.index == 0; } }
			public bool IsDeleted { get { return this.index < 0; } }
			public bool IsFree { get { return this.index <= 0; } }

			public bool IsDirty { get { return this.hash != 0 || this.index != 0; } }

			#if DEBUG
				public override string ToString() {
					return string.Format(System.Globalization.CultureInfo.InvariantCulture,
						"[Key={0}, Hash={1}, Collision={2}, {3}", this.Value, this.Hash, this.HasCollision, this.RowId
					);
				}
			#endif
		}

		private struct Hash {
			private int seed;
			private int factor;
			private int size;

			public Hash(TField value, int size) {
				Debug.Assert(1 < size && size < int.MaxValue - 1);
				this.seed = value.GetHashCode() & int.MaxValue;
				this.factor = 1 + ((this.seed >> 5) + 1) % (size - 1);
				this.size = size;
			}

			public int Code { get { return this.seed; } }

			public RowId Bucket(int iteration) {
				Debug.Assert(iteration < this.size);
				return new RowId((int)(((long)this.seed + (long)iteration * (long)this.factor) % (long)this.size));
			}

			public int Size { get { return this.size; } }
		}

		private class ValueField : IField<Bucket, TField> {
			private readonly IComparer<TField> comparer;
			public ValueField(IComparer<TField> comparer) {
				this.comparer = comparer;
			}
			public string Name { get { return "Value"; } }
			public int Order { get; set; }
			public TField DefaultValue { get { return default(TField); } }
			public TField GetValue(ref Bucket record) {
				return record.Value;
			}
			public void SetValue(ref Bucket record, TField value) {
				record.Value = value;
			}
			public int Compare(ref Bucket data1, ref Bucket data2) {
				return this.comparer.Compare(data1.Value, data2.Value);
			}
			public int Compare(TField x, TField y) {
				return this.comparer.Compare(x, y);
			}
		}

		private class RowIdField : IField<Bucket, RowId> {
			public static readonly RowIdField Field = new RowIdField();
			public string Name { get { return "RowId"; } }
			public int Order { get; set; }
			public RowId DefaultValue { get { return RowId.Empty; } }
			public RowId GetValue(ref Bucket record) {
				return record.RowId;
			}
			public void SetValue(ref Bucket record, RowId value) {
				record.RowId = value;
			}
			public int Compare(ref Bucket data1, ref Bucket data2) {
				return data1.RowId.Value - data2.RowId.Value;
			}
			public int Compare(RowId x, RowId y) {
				return x.Value - y.Value;
			}
		}

		private class HashField : IField<Bucket, int> {
			public static readonly HashField Field = new HashField();
			public string Name { get { return "Hash"; } }
			public int Order { get; set; }
			public int DefaultValue { get { return 0; } }
			public int GetValue(ref Bucket record) {
				return record.Hash;
			}
			public void SetValue(ref Bucket record, int value) {
				record.Hash = value;
			}
			public int Compare(ref Bucket data1, ref Bucket data2) {
				return data1.Hash - data2.Hash;
			}
			public int Compare(int x, int y) {
				return x - y;
			}
		}

		private class CollisionField : IField<Bucket, bool> {
			public static readonly CollisionField Field = new CollisionField();
			public bool DefaultValue { get { return false; } }
			public bool GetValue(ref Bucket record) {
				return record.HasCollision;
			}
			public void SetValue(ref Bucket record, bool value) {
				record.HasCollision = value;
			}
			public string Name { get { return "HasCollision"; } }
			public int Order { get; set; }
			public int Compare(ref Bucket data1, ref Bucket data2) {
				return data1.HasCollision.CompareTo(data2.HasCollision);
			}
			public int Compare(bool x, bool y) {
				return x.CompareTo(y);
			}
		}

		private const int MinSize = 7;
		private readonly SnapTable<Bucket> table;
		private readonly ValueField valueField;
		private readonly IntArray variables;
		private readonly float loadFactor;
		public SnapStore SnapStore { get { return this.table.SnapStore; } }

		public Unique(SnapStore store, string name, IComparer<TField> comparer, float loadFactor) {
			if(!(loadFactor >= 0.1f && loadFactor <= 1.0f)) {
				throw new ArgumentOutOfRangeException("loadFactor");
			}
			this.valueField = new ValueField(comparer);
			this.table = new SnapTable<Bucket>(store, name, Unique<TField>.MinSize,
				new IField<Bucket>[] { this.valueField, RowIdField.Field, HashField.Field, CollisionField.Field },
				false
			);
			// allocate 3 int values:
			// [0] - is the count of items inserted
			// [1] - is known size of the bucket store which is different from table size, as rollbacks or undos will delete inserted rows
			// [2] - is occupancy - total number of collision bits set
			this.variables = new IntArray(store, name + "~Variables~", 3);
			this.loadFactor = loadFactor;
		}

		public Unique(SnapStore store, string name, IComparer<TField> comparer) : this(store, name, comparer, 0.75f) {
		}

		public int Count(int version) {
			return this.variables.Value(0, version);
		}

		private void SetCount(int value) {
			Debug.Assert(0 <= value && value < this.Size(this.table.SnapStore.Version), "Attempt to set invalid count");
			this.variables.SetValue(0, value);
		}

		private int Size(int version) {
			return Math.Max(Unique<TField>.MinSize, this.variables.Value(1, version));
		}

		private void SetSize(int value) {
			Debug.Assert(Unique<TField>.MinSize <= value);
			this.variables.SetValue(1, value);
		}

		private int Occupancy(int version) {
			return this.variables.Value(2, version);
		}

		private void SetOccupancy(int value) {
			this.variables.SetValue(2, value);
		}

		public RowId Find(TField value, int version) {
			Hash hash = new Hash(value, this.Size(version));
			for(int i = 0; i < hash.Size; i++) {
				Bucket bucket;
				this.table.GetData(hash.Bucket(i), version, out bucket);
				if(!bucket.IsFree && bucket.Hash == hash.Code && this.valueField.Compare(bucket.Value, value) == 0) {
					return bucket.RowId;
				}
				if(!bucket.HasCollision) {
					return RowId.Empty;
				}
			}
			return RowId.Empty;
		}

		public void Insert(TField value, RowId rowId) {
			Debug.Assert(0 <= rowId.Value, "There is no reason to insert invalid row ids in the unique index.");
			int version = this.SnapStore.Version;
			int count = this.Count(version);
			if(this.MaxLoadSize <= count) {
				this.Expand();
			} else if(this.MaxLoadSize <= this.Occupancy(version)) {
				this.Rehash();
			}
			Bucket bucket = new Bucket();
			RowId emptyBucket = RowId.Empty;
			bool emptyBucketCollision = false;
			Hash hash = new Hash(value, this.Size(version));
			for(int i = 0; i < hash.Size; i++) {
				RowId bucketIndex = hash.Bucket(i);
				this.table.GetLatestData(bucketIndex, out bucket);
				if(bucket.IsFree) {
					if(!bucket.HasCollision) {
						if(emptyBucket.IsEmpty) {
							emptyBucket = bucketIndex;
						}
						break;
					} else if(emptyBucket.IsEmpty) {
						emptyBucket = bucketIndex;
						emptyBucketCollision = true;
					}
				} else if(bucket.Hash == hash.Code && this.valueField.Compare(bucket.Value, value) == 0) {
					throw new UniqueViolationException(this.table.Name);
				} else if(!bucket.HasCollision && emptyBucket.IsEmpty) {
					this.table.SetField<bool>(bucketIndex, CollisionField.Field, true);
					this.SetOccupancy(this.Occupancy(version) + 1);
				}
			}
			if(emptyBucket.IsEmpty) {
				Debug.Fail("hash insert failed");
				throw new InvalidOperationException(Properties.Resources.ErrorHashInsertFailed);
			} else {
				bucket.Value = value;
				bucket.Hash = hash.Code;
				bucket.RowId = rowId;
				bucket.HasCollision = emptyBucketCollision;
				this.table.SetData(emptyBucket, ref bucket);
				this.SetCount(count + 1);
			}
		}

		public bool Remove(TField value) {
			int version = this.SnapStore.Version;
			Hash hash = new Hash(value, this.Size(version));
			for(int i = 0; i < hash.Size; i++) {
				RowId bucketIndex = hash.Bucket(i);
				Bucket bucket;
				this.table.GetLatestData(bucketIndex, out bucket);
				if(bucket.IsEmpty) {
					return false;
				}
				if(!bucket.IsFree && bucket.Hash == hash.Code && this.valueField.Compare(bucket.Value, value) == 0) {
					this.table.SetField<RowId>(bucketIndex, RowIdField.Field, bucket.HasCollision ? Bucket.Deleted : Bucket.Empty);
					this.SetCount(this.Count(version) - 1);
					return true;
				}
			}
			return false;
		}

		private int MaxLoadSize { get { return (int)(this.loadFactor * this.Size(this.SnapStore.Version)); } }

		private void Expand() {
			int newSize = this.Size(this.SnapStore.Version) * 2;
			if(newSize <= 0) {
				throw new InvalidOperationException(Properties.Resources.ErrorHashExpandFailed);
			}
			this.Rehash(GetPrime(newSize));
		}

		private void Rehash() {
			this.Rehash(this.Size(this.SnapStore.Version));
		}

		private void Rehash(int newSize) {
			int version = this.SnapStore.Version;
			int oldSize = this.Size(version);
			Debug.Assert(oldSize <= newSize);
			int count = this.Count(version);
			List<KeyValuePair<TField, RowId>> list = new List<KeyValuePair<TField, RowId>>(count);
			Bucket empty = new Bucket();
			for(int i = 0; i < oldSize; i++) {
				Bucket bucket;
				this.table.GetLatestData(new RowId(i), out bucket);
				if(!bucket.IsFree) {
					list.Add(new KeyValuePair<TField, RowId>(bucket.Value, bucket.RowId));
				}
				if(bucket.IsDirty) {
					this.table.SetData(new RowId(i), ref empty);
				}
			}
			int currentSize = Math.Min(newSize, this.table.LatestCount());
			for(int i = oldSize; i < currentSize; i++) {
				RowId rowId = new RowId(i);
				if(this.table.IsLatestDeleted(rowId)) {
					this.table.UnDelete(rowId);
				}
				this.table.SetData(rowId, ref empty);
			}
			for(int i = currentSize; i < newSize; i++) {
				this.table.Insert(ref empty);
			}
			this.SetSize(newSize);
			this.SetCount(0);
			this.SetOccupancy(0);
			foreach(KeyValuePair<TField, RowId> pair in list) {
				this.Insert(pair.Key, pair.Value);
			}
			Debug.Assert(count == this.Count(version), "Count been changed by rehashing");
		}

		private static readonly int[] primes = {
			3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919,
			1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591,
			17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437,
			187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263,
			1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369
		};

		private static bool IsPrime(int candidate) {
			if((candidate & 1) != 0) {
				int limit = (int)Math.Sqrt(candidate);
				for(int divisor = 3; divisor <= limit; divisor += 2) {
					if((candidate % divisor) == 0) {
						return false;
					}
				}
				return true;
			}
			return candidate == 2;
		}

		private static int GetPrime(int min) {
			for(int i = 0; i < primes.Length; i++) {
				int prime = primes[i];
				if(prime >= min) {
					return prime;
				}
			}
			// outside of our predefined table.
			// compute the hard way.
			for(int i = (min | 1); i < int.MaxValue; i += 2) {
				if(IsPrime(i)) {
					return i;
				}
			}
			throw new InvalidOperationException(Properties.Resources.ErrorHashPrimeFailed);
		}
	}
}
