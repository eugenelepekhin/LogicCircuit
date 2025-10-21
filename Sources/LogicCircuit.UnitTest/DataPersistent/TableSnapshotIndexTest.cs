using DataPersistent;

namespace LogicCircuit.UnitTest.DataPersistent {
	[TestClass]
	public class TableSnapshotIndexTest {
		private struct Record1 : IComparable<Record1> {
			public int f1;
			public int f2;
			public int f3;

			public override string ToString() => string.Format("Record1({0}, {1}, {2})", this.f1, this.f2, this.f3);

			public static readonly IField<Record1>[] Fields = new IField<Record1>[] { F1Field.Field, F2Field.Field, F3Field.Field };

			public abstract class IntField : IField<Record1, int> {
				public int DefaultValue => 0;
				public abstract int GetValue(ref Record1 record);
				public abstract void SetValue(ref Record1 record, int value);
				public string Name { get; protected set; }
				public int Order { get; set; }
				public int Compare(ref Record1 data1, ref Record1 data2) => this.Compare(this.GetValue(ref data1), this.GetValue(ref data2));
				public int Compare(int x, int y) => Math.Sign(x - (long)y);
			}
			public class F1Field : IntField {
				public static readonly F1Field Field = new F1Field();
				private F1Field() => this.Name = "f1";
				public override int GetValue(ref Record1 record) => record.f1;
				public override void SetValue(ref Record1 record, int value) => record.f1 = value;
			}
			public class F2Field : IntField {
				public static readonly F2Field Field = new F2Field();
				private F2Field() => this.Name = "f2";
				public override int GetValue(ref Record1 record) => record.f2;
				public override void SetValue(ref Record1 record, int value) => record.f2 = value;
			}
			public class F3Field : IntField {
				public static readonly F3Field Field = new F3Field();
				private F3Field() => this.Name = "f3";
				public override int GetValue(ref Record1 record) => record.f3;
				public override void SetValue(ref Record1 record, int value) => record.f3 = value;
			}

			public int CompareTo(Record1 other) {
				int result = this.f1 - other.f1;
				if(result == 0) {
					result = this.f2 - other.f2;
					if(result == 0) {
						result = this.f3 - other.f3;
					}
				}
				return result;
			}
		}

		private struct Record2 {
			public RowId fk1;
			public int f10;

			public override string ToString() => string.Format("fk1={0}, f10={1}", this.fk1, this.f10);

			public static readonly IField<Record2>[] Fields = { Fk1Field.Field, F10Field.Field };

			public class Fk1Field : RowIdField<Record2> {
				public static readonly Fk1Field Field = new Fk1Field();
				private Fk1Field() : base("fk1") { }
				public override RowId GetValue(ref Record2 record) => record.fk1;
				public override void SetValue(ref Record2 record, RowId value) => record.fk1 = value;
			}
			public class F10Field : IField<Record2, int> {
				public static readonly F10Field Field = new F10Field();
				public int DefaultValue => 0;
				public int GetValue(ref Record2 record) => record.f10;
				public void SetValue(ref Record2 record, int value) => record.f10 = value;
				public string Name => "f10";
				public int Order { get; set; }
				public int Compare(ref Record2 data1, ref Record2 data2) => data1.f10.CompareTo(data2.f10);
				public int Compare(int x, int y) => x.CompareTo(y);
			}
		}

		private RowId Insert(TableSnapshot<Record1> table, int t1, int t2, int t3) {
			Record1 r = new Record1() {
				f1 = t1,
				f2 = t2,
				f3 = t3
			};
			return table.Insert(ref r);
		}

		private RowId Insert(TableSnapshot<Record2> table, RowId fk1Value, int f10Value) {
			Record2 r = new Record2() {
				fk1 = fk1Value,
				f10 = f10Value
			};
			return table.Insert(ref r);
		}

		private void AssertSelection(IEnumerable<RowId> selection, params RowId[] expected) {
			CollectionAssert.AreEquivalent(expected, selection.ToArray(), "Selection does not match expected rows.");
		}

		/// <summary>
		/// Test of composite unique index
		/// </summary>
		[TestMethod]
		public void CompositeUniqueTest() {
			StoreSnapshot store = new StoreSnapshot();
			TableSnapshot<Record1> table = new TableSnapshot<Record1>(store, "Table", Record1.Fields);
			table.MakeUnique<int>("pk", Record1.F1Field.Field, true);
			table.MakeUnique<int, int>("composition1", Record1.F2Field.Field, Record1.F3Field.Field);
			store.FreezeShape();
			Assert.IsTrue(store.StartTransaction());
			RowId r1 = this.Insert(table, 1, 1, 1);
			RowId r2 = this.Insert(table, 2, 2, 1);
			RowId r3 = this.Insert(table, 3, 2, 2);
			store.Commit();
			Assert.IsTrue(store.StartTransaction());
			RowId r4 = new RowId();

			Assert.Throws<UniqueViolationException>(() => r4 = this.Insert(table, 4, 2, 1));
			Assert.AreEqual<RowId>(new RowId(), r4);

			Assert.Throws<UniqueViolationException>(() => r4 = this.Insert(table, 5, 1, 1));
			Assert.AreEqual<RowId>(new RowId(), r4);

			Assert.Throws<UniqueViolationException>(() => r4 = this.Insert(table, 6, 2, 2));
			Assert.AreEqual<RowId>(new RowId(), r4);

			store.Rollback();
			Assert.IsTrue(table.Find<int>(Record1.F1Field.Field, 4).IsEmpty);
			Assert.IsTrue(table.Find<int>(Record1.F1Field.Field, 5).IsEmpty);
			Assert.IsTrue(table.Find<int>(Record1.F1Field.Field, 6).IsEmpty);
		}

		/// <summary>
		/// Test of Find in composite unique index
		/// </summary>
		[TestMethod]
		public void Composite2UniqueFindTest() {
			StoreSnapshot store = new StoreSnapshot();
			TableSnapshot<Record1> table = new TableSnapshot<Record1>(store, "Table", Record1.Fields);
			table.MakeUnique<int>("pk", Record1.F1Field.Field, true);
			table.MakeUnique<int, int>("composition1", Record1.F2Field.Field, Record1.F3Field.Field);
			store.FreezeShape();
			Assert.IsTrue(store.StartTransaction());
			RowId r1 = this.Insert(table, 1, 1, 1);
			RowId r2 = this.Insert(table, 2, 2, 1);
			RowId r3 = this.Insert(table, 3, 2, 2);
			store.Commit();

			Assert.AreEqual<RowId>(r1, table.Find<int, int>(Record1.F2Field.Field, Record1.F3Field.Field, 1, 1));
			Assert.AreEqual<RowId>(r2, table.Find<int, int>(Record1.F2Field.Field, Record1.F3Field.Field, 2, 1));
			Assert.AreEqual<RowId>(r3, table.Find<int, int>(Record1.F2Field.Field, Record1.F3Field.Field, 2, 2));
			Assert.IsTrue(table.Find<int, int>(Record1.F2Field.Field, Record1.F3Field.Field, 1, 2).IsEmpty);
			Assert.IsTrue(table.Find<int, int>(Record1.F2Field.Field, Record1.F3Field.Field, 0, 0).IsEmpty);
		}

		/// <summary>
		/// Test of pseudo column primary and foreign keys
		/// </summary>
		[TestMethod]
		public void PseudoUniqueTest() {
			StoreSnapshot store = new StoreSnapshot();
			TableSnapshot<Record1> table1 = new TableSnapshot<Record1>(store, "Table1", Record1.Fields);
			table1.MakeAutoUnique();
			TableSnapshot<Record2> table2 = new TableSnapshot<Record2>(store, "Table2", Record2.Fields);
			table2.CreateForeignKey<RowId>("FK_table1_table2", table1, Record2.Fk1Field.Field, ForeignKeyAction.Cascade);
			TableSnapshot<Record2> table3 = new TableSnapshot<Record2>(store, "Table3", Record2.Fields);
			table3.CreateForeignKey<RowId>("FK_table1_table3", table1, Record2.Fk1Field.Field, ForeignKeyAction.Restrict);
			table3.CreateIndex<RowId>("AX_Record2_Fk1", Record2.Fk1Field.Field);
			store.FreezeShape();

			Assert.IsTrue(store.StartTransaction());
			RowId r11 = this.Insert(table1, 1, 2, 3);
			RowId r12 = this.Insert(table1, 10, 20, 30);
			RowId r21 = this.Insert(table2, r11, 100);
			RowId r22 = this.Insert(table2, r11, 200);
			RowId r23 = this.Insert(table2, r12, 300);
			RowId r24 = this.Insert(table2, r12, 400);
			store.Commit();

			Assert.AreEqual(2, table1.Rows.Count());
			Assert.AreEqual(4, table2.Rows.Count());
			this.AssertSelection(table1.Rows, r11, r12);
			this.AssertSelection(table2.Select<RowId>(Record2.Fk1Field.Field, r11), r21, r22);
			this.AssertSelection(table2.Select<RowId>(Record2.Fk1Field.Field, r12), r23, r24);

			Assert.AreEqual(r11, table2.GetField<RowId>(r21, Record2.Fk1Field.Field));
			Assert.AreEqual(r11, table2.GetField<RowId>(r22, Record2.Fk1Field.Field));
			Assert.AreEqual(r12, table2.GetField<RowId>(r23, Record2.Fk1Field.Field));
			Assert.AreEqual(r12, table2.GetField<RowId>(r24, Record2.Fk1Field.Field));

			// now delete the record from parent and make sure child also get deleted
			Assert.IsTrue(store.StartTransaction());
			table1.Delete(r11);
			store.Commit();

			Assert.AreEqual(1, table1.Rows.Count());
			Assert.AreEqual(2, table2.Rows.Count());
			this.AssertSelection(table1.Rows, r12);
			this.AssertSelection(table2.Select<RowId>(Record2.Fk1Field.Field, r11));
			this.AssertSelection(table2.Select<RowId>(Record2.Fk1Field.Field, r12), r23, r24);

			// now delete another parent record
			Assert.IsTrue(store.StartTransaction());
			table1.Delete(r12);
			store.Commit();

			Assert.AreEqual(0, table1.Rows.Count());
			Assert.AreEqual(0, table2.Rows.Count());
			this.AssertSelection(table1.Rows);
			this.AssertSelection(table2.Select<RowId>(Record2.Fk1Field.Field, r11));
			this.AssertSelection(table2.Select<RowId>(Record2.Fk1Field.Field, r12));

			// check restricted foreign key logic is working as well
			Assert.IsTrue(store.StartTransaction());
			r11 = this.Insert(table1, 1, 2, 3);
			r12 = this.Insert(table1, 10, 20, 30);
			r21 = this.Insert(table2, r11, 100);
			r22 = this.Insert(table2, r11, 200);
			r23 = this.Insert(table2, r12, 300);
			r24 = this.Insert(table2, r12, 400);
			RowId r31 = this.Insert(table3, r11, 1000);
			RowId r32 = this.Insert(table3, r11, 2000);
			store.Commit();

			Assert.AreEqual(2, table1.Rows.Count());
			Assert.AreEqual(4, table2.Rows.Count());
			Assert.AreEqual(2, table3.Rows.Count());
			this.AssertSelection(table1.Rows, r11, r12);
			this.AssertSelection(table2.Select<RowId>(Record2.Fk1Field.Field, r11), r21, r22);
			this.AssertSelection(table2.Select<RowId>(Record2.Fk1Field.Field, r12), r23, r24);
			this.AssertSelection(table3.Select<RowId>(Record2.Fk1Field.Field, r11), r31, r32);

			Assert.AreEqual(r11, table2.GetField<RowId>(r21, Record2.Fk1Field.Field));
			Assert.AreEqual(r11, table2.GetField<RowId>(r22, Record2.Fk1Field.Field));
			Assert.AreEqual(r12, table2.GetField<RowId>(r23, Record2.Fk1Field.Field));
			Assert.AreEqual(r12, table2.GetField<RowId>(r24, Record2.Fk1Field.Field));
			Assert.AreEqual(r11, table3.GetField<RowId>(r31, Record2.Fk1Field.Field));
			Assert.AreEqual(r11, table3.GetField<RowId>(r32, Record2.Fk1Field.Field));

			// make sure r11 can NOT be deleted now, but the r12 can be.
			Assert.IsTrue(store.StartTransaction());
			Assert.Throws<ForeignKeyViolationException>(() => table1.Delete(r11));

			Assert.AreEqual(2, table1.Rows.Count());
			Assert.AreEqual(4, table2.Rows.Count());
			Assert.AreEqual(2, table3.Rows.Count());
			this.AssertSelection(table1.Rows, r11, r12);
			this.AssertSelection(table2.Select<RowId>(Record2.Fk1Field.Field, r11), r21, r22);
			this.AssertSelection(table2.Select<RowId>(Record2.Fk1Field.Field, r12), r23, r24);
			this.AssertSelection(table3.Select<RowId>(Record2.Fk1Field.Field, r11), r31, r32);

			Assert.AreEqual(r11, table2.GetField<RowId>(r21, Record2.Fk1Field.Field));
			Assert.AreEqual(r11, table2.GetField<RowId>(r22, Record2.Fk1Field.Field));
			Assert.AreEqual(r12, table2.GetField<RowId>(r23, Record2.Fk1Field.Field));
			Assert.AreEqual(r12, table2.GetField<RowId>(r24, Record2.Fk1Field.Field));
			Assert.AreEqual(r11, table3.GetField<RowId>(r31, Record2.Fk1Field.Field));
			Assert.AreEqual(r11, table3.GetField<RowId>(r32, Record2.Fk1Field.Field));

			store.Rollback();

			Assert.IsTrue(store.StartTransaction());
			table1.Delete(r12);
			store.Commit();

			Assert.AreEqual(1, table1.Rows.Count());
			Assert.AreEqual(2, table2.Rows.Count());
			Assert.AreEqual(2, table3.Rows.Count());
			this.AssertSelection(table1.Rows, r11);
			this.AssertSelection(table2.Select<RowId>(Record2.Fk1Field.Field, r11), r21, r22);
			this.AssertSelection(table2.Select<RowId>(Record2.Fk1Field.Field, r12));
			this.AssertSelection(table3.Select<RowId>(Record2.Fk1Field.Field, r11), r31, r32);

			// now remove table3 records that prevents r11 been deleted and delete r11
			Assert.IsTrue(store.StartTransaction());
			table3.Delete(r31);
			table3.Delete(r32);
			table1.Delete(r11);
			store.Commit();

			Assert.AreEqual(0, table1.Rows.Count());
			Assert.AreEqual(0, table2.Rows.Count());
			this.AssertSelection(table1.Rows);
			this.AssertSelection(table2.Select<RowId>(Record2.Fk1Field.Field, r11));
			this.AssertSelection(table2.Select<RowId>(Record2.Fk1Field.Field, r12));
		}

		/// <summary>
		/// Test of range selection from table
		/// </summary>
		[TestMethod]
		public void SelectRangeTest() {
			StoreSnapshot store = new StoreSnapshot();
			TableSnapshot<Record1> table1 = new TableSnapshot<Record1>(store, "Table1", Record1.Fields);
			TableSnapshot<Record1> table2 = new TableSnapshot<Record1>(store, "Table2", Record1.Fields);
			table2.CreateIndex("AX_Record1_f2", Record1.F2Field.Field);
			store.FreezeShape();

			List<RowId> map = new List<RowId>();
			Assert.IsTrue(store.StartTransaction());
			int count = 60;
			for(int i = 0; i < count; i++) {
				RowId r1 = this.Insert(table1, i + 10, i, i + 100);
				RowId r2 = this.Insert(table2, i + 10, i, i + 100);
				Assert.AreEqual(r1, r2);
				map.Add(r1);
			}
			store.Commit();

			Action<TableSnapshot<Record1>, int, int> validate = (table, min, max) => {
				HashSet<RowId> distinct = new HashSet<RowId>();
				foreach(RowId rowId in table.Select(Record1.F2Field.Field, min, max)) {
					Assert.IsTrue(distinct.Add(rowId), "Duplicated value selected");
					int index = map.IndexOf(rowId);
					Assert.IsTrue(min <= index && index <= max, "Wrong value selected");
				}
				Assert.HasCount(Math.Max(Math.Min(max, count - 1) - Math.Max(min, 0) + 1, 0), distinct, "Wrong number of rows selected");
			};

			for(int i = -count; i < 2 * count; i++) {
				for(int j = i; j < 3 * count; j++) {
					validate(table1, i, j);
					validate(table2, i, j);
				}
			}
		}

		private void IndexUpdateTest(TableSnapshot<Record1> table1) {
			StoreSnapshot store = table1.StoreSnapshot;

			Assert.IsTrue(store.StartTransaction());
			RowId r1 = this.Insert(table1, 1, 2, 3);
			RowId r2 = this.Insert(table1, 10, 20, 30);
			RowId r3 = this.Insert(table1, 100, 200, 300);
			RowId r4 = this.Insert(table1, 1000, 2000, 3000);
			RowId r5 = this.Insert(table1, 10000, 20000, 30000);
			store.Commit();

			this.AssertSelection(table1, r1, r2, r3, r4, r5);
			this.AssertSelection(table1.Select<int>(Record1.F2Field.Field, 2), r1);
			this.AssertSelection(table1.Select<int>(Record1.F2Field.Field, 20), r2);
			this.AssertSelection(table1.Select<int>(Record1.F2Field.Field, 200), r3);
			this.AssertSelection(table1.Select<int>(Record1.F2Field.Field, 2000), r4);
			this.AssertSelection(table1.Select<int>(Record1.F2Field.Field, 20000), r5);

			Assert.IsTrue(store.StartTransaction());
			table1.SetField<int>(r3, Record1.F2Field.Field, 201);
			store.Commit();

			this.AssertSelection(table1, r1, r2, r3, r4, r5);
			this.AssertSelection(table1.Select<int>(Record1.F2Field.Field, 2), r1);
			this.AssertSelection(table1.Select<int>(Record1.F2Field.Field, 20), r2);
			this.AssertSelection(table1.Select<int>(Record1.F2Field.Field, 200));
			this.AssertSelection(table1.Select<int>(Record1.F2Field.Field, 201), r3);
			this.AssertSelection(table1.Select<int>(Record1.F2Field.Field, 2000), r4);
			this.AssertSelection(table1.Select<int>(Record1.F2Field.Field, 20000), r5);

			Assert.IsTrue(store.StartTransaction());
			table1.SetField<int>(r3, Record1.F2Field.Field, 202);
			store.Commit();

			this.AssertSelection(table1, r1, r2, r3, r4, r5);
			this.AssertSelection(table1.Select<int>(Record1.F2Field.Field, 2), r1);
			this.AssertSelection(table1.Select<int>(Record1.F2Field.Field, 20), r2);
			this.AssertSelection(table1.Select<int>(Record1.F2Field.Field, 200));
			this.AssertSelection(table1.Select<int>(Record1.F2Field.Field, 201));
			this.AssertSelection(table1.Select<int>(Record1.F2Field.Field, 202), r3);
			this.AssertSelection(table1.Select<int>(Record1.F2Field.Field, 2000), r4);
			this.AssertSelection(table1.Select<int>(Record1.F2Field.Field, 20000), r5);

			Assert.IsTrue(store.StartTransaction());
			table1.SetField<int>(r1, Record1.F2Field.Field, 5);
			table1.SetField<int>(r2, Record1.F2Field.Field, 25);
			table1.SetField<int>(r3, Record1.F2Field.Field, 205);
			table1.SetField<int>(r4, Record1.F2Field.Field, 2005);
			table1.SetField<int>(r5, Record1.F2Field.Field, 20005);
			store.Commit();

			this.AssertSelection(table1, r1, r2, r3, r4, r5);
			this.AssertSelection(table1.Select<int>(Record1.F2Field.Field, 2));
			this.AssertSelection(table1.Select<int>(Record1.F2Field.Field, 5), r1);
			this.AssertSelection(table1.Select<int>(Record1.F2Field.Field, 20));
			this.AssertSelection(table1.Select<int>(Record1.F2Field.Field, 25), r2);
			this.AssertSelection(table1.Select<int>(Record1.F2Field.Field, 200));
			this.AssertSelection(table1.Select<int>(Record1.F2Field.Field, 201));
			this.AssertSelection(table1.Select<int>(Record1.F2Field.Field, 202));
			this.AssertSelection(table1.Select<int>(Record1.F2Field.Field, 205), r3);
			this.AssertSelection(table1.Select<int>(Record1.F2Field.Field, 2000));
			this.AssertSelection(table1.Select<int>(Record1.F2Field.Field, 2005), r4);
			this.AssertSelection(table1.Select<int>(Record1.F2Field.Field, 20000));
			this.AssertSelection(table1.Select<int>(Record1.F2Field.Field, 20005), r5);
		}

		/// <summary>
		/// Test of updating index
		/// </summary>
		[TestMethod]
		public void IndexUpdateTest() {
			// none unique index
			StoreSnapshot store = new StoreSnapshot();
			TableSnapshot<Record1> table1 = new TableSnapshot<Record1>(store, "Table1", Record1.Fields);
			table1.CreateIndex<int>("f2.index", Record1.F2Field.Field);
			store.FreezeShape();
			this.IndexUpdateTest(table1);

			//unique index
			store = new StoreSnapshot();
			table1 = new TableSnapshot<Record1>(store, "Table1", Record1.Fields);
			table1.MakeUnique<int>("f2.index", Record1.F2Field.Field, true);
			store.FreezeShape();
			this.IndexUpdateTest(table1);
		}
	}
}
