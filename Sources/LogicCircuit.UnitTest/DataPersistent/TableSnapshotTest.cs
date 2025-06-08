using LogicCircuit.DataPersistent;
using LogicCircuit.UnitTest.DataPersistent.UnitTestSnapStore;

namespace LogicCircuit.UnitTest.DataPersistent {
	[TestClass]
	public class TableSnapshotTest {
		public TestContext TestContext { get; set; }

		private struct SampleData {
			public int Id;
			public string Name;

			public class SampleIdField : IField<SampleData, int> {
				public static readonly SampleIdField Field = new SampleIdField();

				public string Name => "Id";
				public int Order { get; set; }
				public int DefaultValue => 0;
				public int GetValue(ref SampleData record) => record.Id;
				public void SetValue(ref SampleData record, int value) => record.Id = value;
				public int Compare(ref SampleData data1, ref SampleData data2) => data1.Id.CompareTo(data2.Id);
				public int Compare(int x, int y) => x.CompareTo(y);
			}

			public class SampleNameField : IField<SampleData, string> {
				public static readonly SampleNameField Field = new SampleNameField();

				public string Name => "Name";
				public int Order { get; set; }
				public string DefaultValue => string.Empty;
				public string GetValue(ref SampleData record) => record.Name;
				public void SetValue(ref SampleData record, string value) => record.Name = value;
				public int Compare(ref SampleData data1, ref SampleData data2) => string.Compare(data1.Name, data2.Name, StringComparison.Ordinal);
				public int Compare(string x, string y) => string.Compare(x, y, StringComparison.Ordinal);
			}

			public static readonly IField<SampleData>[] Fields = [SampleIdField.Field, SampleNameField.Field];

			public static TableSnapshot<SampleData> CreateTable(StoreSnapshot store, string tableName) {
				TableSnapshot<SampleData> table = new TableSnapshot<SampleData>(store, tableName, SampleData.Fields);
				return table;
			}

			public static void CreateIdIndex(TableSnapshot<SampleData> table, string indexName, bool unique, bool isPrimary) {
				Assert.IsTrue(!isPrimary || unique, "Primary key must be unique.");
				if(unique) {
					table.MakeUnique(indexName, SampleIdField.Field, isPrimary);
				} else {
					table.CreateIndex(indexName, SampleIdField.Field);
				}
			}
		}

		private void InTransaction(StoreSnapshot store, Action action) {
			int oldVersion = store.Version;
			Assert.IsTrue(store.StartTransaction(), "Failed to start transaction.");
			try {
				action();
				int version = store.Commit();
				Assert.AreEqual(oldVersion + 1, version, "Transaction commit failed to increment version.");
			} catch(Exception) {
				store.Rollback();
				throw;
			}
		}

		private Pin CreatePin(LogicalCircuit circuit) {
			Pin pin = circuit.CircuitProject.PinSet.Create(circuit, PinType.Input, 1);
			circuit.CircuitProject.CircuitSymbolSet.Create(pin, circuit, 10, 20);
			return pin;
		}

		private RowId Insert(TableSnapshot<PersonData> person, int id, string firstName, string lastName) {
			PersonData p = new PersonData() {
				PersonId = id,
				FirstName = firstName,
				LastName = lastName
			};
			return person.Insert(ref p);
		}
		private RowId Insert(TableSnapshot<PhoneData> phone, int id, int personId, string number) {
			PhoneData t = new PhoneData() {
				PhoneId = id,
				PersonId = personId,
				Number = number
			};
			return phone.Insert(ref t);
		}
		private RowId Insert(TableSnapshot<PersonRestrictData> restrict, int id, int personId, int data) {
			PersonRestrictData r = new PersonRestrictData() {
				PersonRestrictId = id,
				PersonId = personId,
				Data = data
			};
			return restrict.Insert(ref r);
		}

		private void AssertCollection<T>(IEnumerable<T> selection, IEnumerable<T> expected) {
			CollectionAssert.AreEquivalent(expected.ToArray(), selection.ToArray(), "Selection does not match expected rows.");
		}

		private void AssertSelection(IEnumerable<RowId> selection, params RowId[] expected) {
			CollectionAssert.AreEquivalent(expected, selection.ToArray(), "Selection does not match expected rows.");
		}

		[TestMethod]
		public void NameTest() {
			StoreSnapshot store = ContactsDatabase.CreateContactsDatabase();
			ITableSnapshot table1 = store.Table("Person");
			ITableSnapshot table2 = store.Table("Phone");
			Assert.IsNotNull(table1);
			Assert.IsNotNull(table2);
			Assert.AreEqual<string>("Person", table1.Name);
			Assert.AreEqual<string>("Phone", table2.Name);

			StoreSnapshot store2 = new StoreSnapshot(store);
			ITableSnapshot table12 = store2.Table(table1.Name);
			ITableSnapshot table22 = store2.Table(table2.Name);

			Assert.IsNotNull(table12);
			Assert.IsNotNull(table22);

			Assert.AreSame(table1.Name, table12.Name);
			Assert.AreSame(table2.Name, table22.Name);
		}

		private void IsEmptyTest(TableSnapshot<int> table) {
			StoreSnapshot store = table.StoreSnapshot;
			Assert.IsTrue(table.IsEmpty());
			Assert.IsTrue(store.StartTransaction());
			int data = 1;
			RowId r1 = table.Insert(ref data);
			Assert.IsFalse(table.IsEmpty());
			store.Commit();
			Assert.IsTrue(store.Undo());
			Assert.IsTrue(table.IsEmpty());
		}

		/// <summary>
		/// Tests IsEmpty.
		/// </summary>
		[TestMethod]
		public void TableSnapshotIsEmptyTest() {
			StoreSnapshot store = new StoreSnapshot();
			TableSnapshot<int> table1 = new TableSnapshot<int>(store, "table1", IntField.Field);
			TableSnapshot<int> table2 = new TableSnapshot<int>(store, "table2", IntField.Field);
			TableSnapshot<int> table3 = new TableSnapshot<int>(store, "table3", IntField.Field);
			TableSnapshot<int> table4 = new TableSnapshot<int>(store, "table4", IntField.Field);
			TableSnapshot<int> table5 = new TableSnapshot<int>(store, "table5", IntField.Field);

			table2.MakeAutoUnique();
			table3.MakeUnique("t3_index", IntField.Field, true);
			table4.MakeUnique("t4_index", IntField.Field, false);
			table5.CreateIndex("t5_index", IntField.Field);

			store.FreezeShape();

			this.IsEmptyTest(table1);
			this.IsEmptyTest(table2);
			this.IsEmptyTest(table3);
			this.IsEmptyTest(table4);
			this.IsEmptyTest(table5);
		}

		/// <summary>
		/// Test Fields property of table snapshot
		/// </summary>
		[TestMethod]
		public void FieldsTest() {
			StoreSnapshot store = ContactsDatabase.CreateContactsDatabase();
			this.AssertCollection(ContactsDatabase.PersonTable(store).Fields, PersonData.Fields());
			this.AssertCollection(ContactsDatabase.PhoneTable(store).Fields, PhoneData.Fields());
			this.AssertCollection(ContactsDatabase.PersonRestrictTable(store).Fields, PersonRestrictData.Fields());
			this.AssertCollection(ContactsDatabase.PersonDefaultTable(store).Fields, PersonDefaultData.Fields());
		}

		[TestMethod]
		public void ForeignKeyTest1() {
			// simple test of cascade delete
			StoreSnapshot store = ContactsDatabase.CreateContactsDatabase();
			TableSnapshot<PersonData> person = ContactsDatabase.PersonTable(store);
			TableSnapshot<PhoneData> phone = ContactsDatabase.PhoneTable(store);

			Assert.IsTrue(store.StartTransaction());
			RowId p1 = this.Insert(person, 1, "John", "Doeh");
			RowId t1 = this.Insert(phone, 1, 1, "123-456");
			Assert.AreEqual<int>(1, store.Commit());

			Assert.IsFalse(person.IsDeleted(p1));
			Assert.IsFalse(person.IsDeleted(t1));

			Assert.IsTrue(store.StartTransaction());
			person.Delete(p1);
			Assert.AreEqual<int>(2, store.Commit());

			Assert.IsTrue(person.IsDeleted(p1));
			Assert.IsTrue(phone.IsDeleted(t1));
		}

		[TestMethod]
		public void ForeignKeyTest2() {
			// simple test of restrict delete
			StoreSnapshot store = ContactsDatabase.CreateContactsDatabase();
			TableSnapshot<PersonData> person = ContactsDatabase.PersonTable(store);
			TableSnapshot<PhoneData> phone = ContactsDatabase.PhoneTable(store);
			TableSnapshot<PersonRestrictData> restrict = ContactsDatabase.PersonRestrictTable(store);

			Assert.IsTrue(store.StartTransaction());
			RowId rp1 = this.Insert(person, 1, "John", "Doeh");
			RowId rp2 = this.Insert(person, 2, "Bill", "Wats");
			RowId rt1 = this.Insert(phone, 1, 1, "345-6789");
			RowId rt2 = this.Insert(phone, 2, 2, "345-6789");
			RowId rr1 = this.Insert(restrict, 1, 2, 100);
			Assert.AreEqual<int>(1, store.Commit());

			Assert.IsFalse(person.IsDeleted(rp1));
			Assert.IsFalse(person.IsDeleted(rp2));
			Assert.IsFalse(person.IsDeleted(rt1));
			Assert.IsFalse(person.IsDeleted(rt2));
			Assert.IsFalse(person.IsDeleted(rr1));

			Assert.IsTrue(store.StartTransaction());
			person.Delete(rp1);
			Assert.Throws<ForeignKeyViolationException>(() => person.Delete(rp2));
			Assert.AreEqual<int>(2, store.Commit());

			Assert.IsTrue(person.IsDeleted(rp1));
			Assert.IsTrue(phone.IsDeleted(rt1));

			Assert.IsFalse(person.IsDeleted(rp2));
			Assert.IsFalse(phone.IsDeleted(rt2));
			Assert.IsFalse(restrict.IsDeleted(rr1));
		}

		/// <summary>
		/// Tests finding unique value
		/// </summary>
		[TestMethod]
		public void OldFindTest() {
			StoreSnapshot store = new StoreSnapshot();
			TableSnapshot<PersonData> person = new TableSnapshot<PersonData>(store, "Person", PersonData.Fields());
			person.MakeUnique<int>("PK_Person", PersonData.PersonIdField.Field, true);
			store.FreezeShape();

			Assert.IsTrue(store.StartTransaction());
			RowId rp1 = this.Insert(person, 1, "John", "Doeh");
			RowId rp2 = this.Insert(person, 2, "Bill", "Wats");
			store.Commit();

			Assert.AreEqual<RowId>(rp1, person.Find<int>(PersonData.PersonIdField.Field, 1));
			Assert.AreEqual<RowId>(rp2, person.Find<int>(PersonData.PersonIdField.Field, 2));
			Assert.IsTrue(person.Find<int>(PersonData.PersonIdField.Field, 0).IsEmpty);
			Assert.IsTrue(person.Find<int>(PersonData.PersonIdField.Field, 3).IsEmpty);

			Assert.Throws<InvalidOperationException>(() => person.Find<string>(PersonData.FirstNameField.Field, "John"));
		}

		/// <summary>
		/// Tests of selecting from table with single field.
		/// </summary>
		[TestMethod]
		public void Select1Test() {
			StoreSnapshot store = new StoreSnapshot();

			TableSnapshot<PersonData> person = new TableSnapshot<PersonData>(store, "Person", PersonData.Fields());
			person.MakeUnique<int>("PK_Person", PersonData.PersonIdField.Field, true);

			TableSnapshot<PhoneData> phone = new TableSnapshot<PhoneData>(store, "Phone", PhoneData.Fields());
			phone.MakeUnique<int>("PK_Phone", PhoneData.PhoneIdField.Field, true);
			phone.CreateForeignKey<int>("FK_PersonPhone", person, PhoneData.PersonIdField.Field, ForeignKeyAction.Cascade);
			phone.CreateIndex<int>("IX_PersonPhone", PhoneData.PersonIdField.Field);

			store.FreezeShape();

			Assert.IsTrue(store.StartTransaction());
			RowId rp1 = this.Insert(person, 1, "John", "Doeh");
			RowId rp2 = this.Insert(person, 2, "Bill", "Wats");
			RowId rp3 = this.Insert(person, 3, "Bill", "Doeh");
			RowId rt1 = this.Insert(phone, 1, 1, "345-6781");
			RowId rt2 = this.Insert(phone, 2, 2, "345-6782");
			RowId rt3 = this.Insert(phone, 3, 3, "345-6783");
			RowId rt4 = this.Insert(phone, 4, 1, "345-6784");
			RowId rt5 = this.Insert(phone, 5, 2, "345-6785");
			RowId rt6 = this.Insert(phone, 6, 3, "345-6786");
			Assert.AreEqual<int>(1, store.Commit());

			// check for selecting with index
			this.AssertSelection(person.Select<int>(PersonData.PersonIdField.Field, 1), rp1);
			this.AssertSelection(person.Select<int>(PersonData.PersonIdField.Field, 2), rp2);
			this.AssertSelection(person.Select<int>(PersonData.PersonIdField.Field, 3), rp3);

			this.AssertSelection(person.Select<int>(PersonData.PersonIdField.Field, 0));
			this.AssertSelection(person.Select<int>(PersonData.PersonIdField.Field, 4));

			this.AssertSelection(phone.Select<int>(PhoneData.PersonIdField.Field, 1), rt1, rt4);
			this.AssertSelection(phone.Select<int>(PhoneData.PersonIdField.Field, 2), rt2, rt5);
			this.AssertSelection(phone.Select<int>(PhoneData.PersonIdField.Field, 3), rt3, rt6);

			// check for selecting without index
			this.AssertSelection(person.Select<string>(PersonData.FirstNameField.Field, "Bill"), rp2, rp3);
			this.AssertSelection(person.Select<string>(PersonData.FirstNameField.Field, "John"), rp1);

			this.AssertSelection(person.Select<string>(PersonData.FirstNameField.Field, "Hello"));
		}

		/// <summary>
		/// Tests of selecting from table with dual field.
		/// </summary>
		[TestMethod]
		public void Select2Test() {
			StoreSnapshot store = new StoreSnapshot();

			TableSnapshot<PersonData> person = new TableSnapshot<PersonData>(store, "Person", PersonData.Fields());
			person.MakeUnique<int>("PK_Person", PersonData.PersonIdField.Field, true);
			person.MakeUnique<string, string>("AK_PersonName", PersonData.FirstNameField.Field, PersonData.LastNameField.Field);

			TableSnapshot<PhoneData> phone = new TableSnapshot<PhoneData>(store, "Phone", PhoneData.Fields());
			phone.MakeUnique<int>("PK_Phone", PhoneData.PhoneIdField.Field, true);
			phone.CreateForeignKey<int>("FK_PersonPhone", person, PhoneData.PersonIdField.Field, ForeignKeyAction.Cascade);
			phone.CreateIndex<int, string>("IX_PersonPhone", PhoneData.PersonIdField.Field, PhoneData.NumberField.Field);

			store.FreezeShape();

			Assert.IsTrue(store.StartTransaction());
			RowId rp1 = this.Insert(person, 1, "John", "Doeh");
			RowId rp2 = this.Insert(person, 2, "Bill", "Wats");
			RowId rp3 = this.Insert(person, 3, "Bill", "Doeh");
			RowId rt1 = this.Insert(phone, 1, 1, "345-6781");
			RowId rt2 = this.Insert(phone, 2, 2, "345-6782");
			RowId rt3 = this.Insert(phone, 3, 3, "345-6783");
			RowId rt4 = this.Insert(phone, 4, 1, "345-6784");
			RowId rt5 = this.Insert(phone, 5, 2, "345-6785");
			RowId rt6 = this.Insert(phone, 6, 3, "345-6786");
			RowId rt7 = this.Insert(phone, 7, 3, "345-6786");
			Assert.AreEqual<int>(1, store.Commit());

			// check index search
			this.AssertSelection(person.Select<string, string>(PersonData.FirstNameField.Field, PersonData.LastNameField.Field, "John", "Doeh"), rp1);
			this.AssertSelection(person.Select<string, string>(PersonData.FirstNameField.Field, PersonData.LastNameField.Field, "Bill", "Wats"), rp2);
			this.AssertSelection(person.Select<string, string>(PersonData.FirstNameField.Field, PersonData.LastNameField.Field, "Bill", "Doeh"), rp3);

			this.AssertSelection(phone.Select<int, string>(PhoneData.PersonIdField.Field, PhoneData.NumberField.Field, 1, "345-6781"), rt1);
			this.AssertSelection(phone.Select<int, string>(PhoneData.PersonIdField.Field, PhoneData.NumberField.Field, 3, "345-6786"), rt6, rt7);

			//check for none index search
			this.AssertSelection(person.Select<string>(PersonData.FirstNameField.Field, "John"), rp1);
			this.AssertSelection(person.Select<string>(PersonData.FirstNameField.Field, "Bill"), rp2, rp3);

			this.AssertSelection(person.Select<int, string>(PersonData.PersonIdField.Field, PersonData.LastNameField.Field, 1, "Doeh"), rp1);
			this.AssertSelection(person.Select<int, string>(PersonData.PersonIdField.Field, PersonData.LastNameField.Field, 2, "Doeh"));
		}

		private void MinMaxTest(TableSnapshot<int> table) {
			StoreSnapshot store = table.StoreSnapshot;

			int seed = (int)DateTime.UtcNow.Ticks;
			//seed = -1005951636;
			this.TestContext.WriteLine("seed={0}", seed);
			Random rand = new Random(seed);
			int min = 10 + rand.Next(100);
			int max = min + 10 + rand.Next(100);

			Assert.IsTrue(store.StartTransaction());
			int count = max - min;
			for(int i = 0; i <= count; i++) {
				int data = i + min;
				table.Insert(ref data);
				Assert.AreEqual(min, table.Minimum(IntField.Field));
				Assert.AreEqual(i + min, table.Maximum(IntField.Field));
			}
			int version = store.Commit();
			Assert.AreEqual(min, table.Minimum(IntField.Field));
			Assert.AreEqual(max, table.Maximum(IntField.Field));
		}

		/// <summary>
		/// Tests of minimum and maximum value from table field.
		/// </summary>
		[TestMethod]
		public void MinMaxTest() {
			StoreSnapshot store = new StoreSnapshot();
			TableSnapshot<int> table1 = new TableSnapshot<int>(store, "table1", IntField.Field);
			TableSnapshot<int> table2 = new TableSnapshot<int>(store, "table2", IntField.Field);
			TableSnapshot<int> table3 = new TableSnapshot<int>(store, "table3", IntField.Field);
			TableSnapshot<int> table4 = new TableSnapshot<int>(store, "table4", IntField.Field);
			TableSnapshot<int> table5 = new TableSnapshot<int>(store, "table5", IntField.Field);

			table2.MakeAutoUnique();
			table3.MakeUnique("t3", IntField.Field, true);
			table4.MakeUnique("t4", IntField.Field, false);
			table5.CreateIndex("t5", IntField.Field);

			store.FreezeShape();

			this.MinMaxTest(table1);
			this.MinMaxTest(table2);
			this.MinMaxTest(table3);
			this.MinMaxTest(table4);
			this.MinMaxTest(table5);
		}

		/// <summary>
		/// Tests of eventing during transaction commit and changing versions.
		/// </summary>
		[TestMethod]
		public void VersionChangeEventTest() {
			StoreSnapshot store1 = ContactsDatabase.CreateContactsDatabase();
			TableSnapshot<PersonData> person1 = ContactsDatabase.PersonTable(store1);
			StoreSnapshot store2 = new StoreSnapshot(store1);
			TableSnapshot<PersonData> person2 = ContactsDatabase.PersonTable(store2);

			// use this event args for checking if event has happened
			EventArgs store1Commit = null;
			VersionChangeEventArgs store1Version = null;
			EventArgs store2Commit = null;
			VersionChangeEventArgs store2Version = null;

			store1.LatestVersionChanged += new EventHandler((o, e) => { Assert.AreEqual(store1, o); store1Commit = e; });
			store1.VersionChanged += new EventHandler<VersionChangeEventArgs>((o, e) => { Assert.AreEqual(store1, o); store1Version = e; });
			store2.LatestVersionChanged += new EventHandler((o, e) => { Assert.AreEqual(store2, o); store2Commit = e; });
			store2.VersionChanged += new EventHandler<VersionChangeEventArgs>((o, e) => { Assert.AreEqual(store2, o); store2Version = e; });

			Assert.IsNull(store1Commit);
			Assert.IsNull(store1Version);
			Assert.IsNull(store2Commit);
			Assert.IsNull(store2Version);

			// check event when first store get updated
			Assert.IsTrue(store1.StartTransaction());
			RowId r1 = this.Insert(person1, 1, "John", "Smith");
			RowId r2 = this.Insert(person1, 2, "Bill", "White");
			RowId r3 = this.Insert(person1, 3, "Hugo", "Black");

			Assert.IsNull(store1Commit);
			Assert.IsNull(store1Version);
			Assert.IsNull(store2Commit);
			Assert.IsNull(store2Version);

			int version = store1.Commit();
			Assert.AreEqual(1, version);
			Assert.AreEqual(1, store1.Version);
			Assert.AreEqual(0, store2.Version);
			Assert.IsNotNull(store1Commit);
			Assert.IsNotNull(store1Version);
			Assert.AreEqual(0, store1Version.OldVersion);
			Assert.AreEqual(1, store1Version.NewVersion);
			Assert.IsNotNull(store2Commit);
			Assert.IsNull(store2Version);

			// reset event args
			store1Commit = null;
			store1Version = null;
			store2Commit = null;
			store2Version = null;

			Assert.IsTrue(store2.StartTransaction());

			// check store2 auto updated to latest version
			Assert.IsNull(store1Commit);
			Assert.IsNull(store1Version);
			Assert.IsNull(store2Commit);
			Assert.IsNotNull(store2Version);

			Assert.AreEqual(1, store1.Version);
			Assert.AreEqual(2, store2.Version);
			Assert.AreEqual(0, store2Version.OldVersion);
			Assert.AreEqual(1, store2Version.NewVersion);

			// check event when store 2 gets updated
			// reset event args
			store1Commit = null;
			store1Version = null;
			store2Commit = null;
			store2Version = null;

			person2.SetField(r2, PersonData.LastNameField.Field, "Red");
			person2.SetField(r3, PersonData.FirstNameField.Field, "Sam");

			Assert.IsNull(store1Commit);
			Assert.IsNull(store1Version);
			Assert.IsNull(store2Commit);
			Assert.IsNull(store2Version);

			version = store2.Commit();
			Assert.AreEqual(2, version);

			Assert.AreEqual(1, store1.Version);
			Assert.AreEqual(2, store2.Version);
			Assert.IsNotNull(store1Commit);
			Assert.IsNull(store1Version);
			Assert.IsNotNull(store2Commit);
			Assert.IsNotNull(store2Version);
			Assert.AreEqual(1, store2Version.OldVersion);
			Assert.AreEqual(2, store2Version.NewVersion);

			//upgrade store1
			// reset event args
			store1Commit = null;
			store1Version = null;
			store2Commit = null;
			store2Version = null;

			store1.Upgrade();

			Assert.IsNull(store1Commit);
			Assert.IsNotNull(store1Version);
			Assert.IsNull(store2Commit);
			Assert.IsNull(store2Version);

			Assert.AreEqual(2, store1.Version);
			Assert.AreEqual(1, store1Version.OldVersion);
			Assert.AreEqual(2, store1Version.NewVersion);

			// check upgrade over more then one version
			// reset event args
			store1Commit = null;
			store1Version = null;
			store2Commit = null;
			store2Version = null;

			Assert.IsTrue(store1.StartTransaction());

			Assert.IsNull(store1Commit);
			Assert.IsNull(store1Version);
			Assert.IsNull(store2Commit);
			Assert.IsNull(store2Version);

			person1.SetField(r1, PersonData.LastNameField.Field, "Yellow");
			person1.SetField(r2, PersonData.FirstNameField.Field, "Oliver");

			store1.Commit();
			Assert.IsTrue(store1.StartTransaction());

			person1.SetField(r1, PersonData.LastNameField.Field, "Green");
			person1.Delete(r2);
			version = store1.Commit();

			Assert.AreEqual(4, version);
			Assert.AreEqual(4, store1.Version);
			Assert.AreEqual(2, store2.Version);
			Assert.IsNotNull(store1Commit);
			Assert.IsNotNull(store1Version);
			Assert.AreEqual(3, store1Version.OldVersion);
			Assert.AreEqual(4, store1Version.NewVersion);
			Assert.IsNotNull(store2Commit);
			Assert.IsNull(store2Version);

			// reset event args
			store1Commit = null;
			store1Version = null;
			store2Commit = null;
			store2Version = null;

			store2.Upgrade();

			Assert.IsNull(store1Commit);
			Assert.IsNull(store1Version);
			Assert.IsNull(store2Commit);
			Assert.IsNotNull(store2Version);

			Assert.AreEqual(4, store1.Version);
			Assert.AreEqual(4, store2.Version);
			Assert.AreEqual(2, store2Version.OldVersion);
			Assert.AreEqual(4, store2Version.NewVersion);
		}

		/// <summary>
		/// Tests roll back action and notification.
		/// </summary>
		[TestMethod]
		public void RollbackTest() {
			StoreSnapshot store = ContactsDatabase.CreateContactsDatabase();
			TableSnapshot<PersonData> person = ContactsDatabase.PersonTable(store);
			int rolledBackVersion = -1;
			store.RolledBack += new EventHandler<RolledBackEventArgs>((o, e) => { Assert.AreEqual(store, o); rolledBackVersion = e.Version; });

			Assert.IsTrue(store.StartTransaction());
			RowId p1 = this.Insert(person, 1, "bill", "smith");
			RowId p2 = this.Insert(person, 2, "scott", "green");
			Assert.AreEqual(1, store.Commit());
			Assert.AreEqual(-1, rolledBackVersion);

			Assert.IsTrue(store.StartTransaction());
			RowId p3 = this.Insert(person, 3, "john", "walker");
			person.Delete(p1);
			person.SetField(p2, PersonData.FirstNameField.Field, "yellow");
			store.Rollback();

			Assert.AreEqual(2, rolledBackVersion);
			Assert.IsTrue(person.IsDeleted(p3));
			Assert.IsFalse(person.IsDeleted(p1));
			Assert.IsFalse(person.IsDeleted(p2));
			Assert.AreEqual("scott", person.GetField(p2, PersonData.FirstNameField.Field));

			HashSet<RowId> rows = new HashSet<RowId>();
			IEnumerator<RowId> changes = person.GetRolledBackChanges(rolledBackVersion);
			Assert.IsNotNull(changes);
			while(changes.MoveNext()) {
				Assert.IsTrue(rows.Add(changes.Current));
			}
			Assert.AreEqual(3, rows.Count);
			Assert.IsTrue(rows.Contains(p1));
			Assert.IsTrue(rows.Contains(p2));
			Assert.IsTrue(rows.Contains(p3));
		}

		[TestMethod]
		public void MakeUniqueTest() {
			StoreSnapshot store = new StoreSnapshot();

			TableSnapshot<int> table = new TableSnapshot<int>(store, "test", IntArray.Field);

			table.MakeUnique<int>("testUnique", IntArray.Field);
			store.FreezeShape();

			Assert.IsTrue(store.StartTransaction());
			int data = 100;
			table.Insert(ref data);
			data = 20;
			table.Insert(ref data);

			Assert.Throws<UniqueViolationException>(() => table.Insert(ref data));
			data = 100;
			Assert.Throws<UniqueViolationException>(() => table.Insert(ref data));
		}

		[TestMethod]
		public void MakeUniqueInsertDeleteTest() {
			StoreSnapshot store = new StoreSnapshot();
			TableSnapshot<int> table = new TableSnapshot<int>(store, "test", IntArray.Field);
			table.MakeUnique<int>("testUnique", IntArray.Field);
			store.FreezeShape();

			Assert.IsTrue(store.StartTransaction());

			int data = 15;
			RowId r1 = table.Insert(ref data);
			data = 25;
			RowId r2 = table.Insert(ref data);
			data = 35;
			RowId r3 = table.Insert(ref data);

			table.Delete(r2);
			data = 25;
			RowId r4 = table.Insert(ref data);

			table.Delete(r1);
			table.Delete(r3);
			data = 15;
			RowId r5 = table.Insert(ref data);
			data = 35;
			RowId r6 = table.Insert(ref data);

			Assert.Throws<UniqueViolationException>(() => table.Insert(ref data));

			table.Delete(r6);
			table.Insert(ref data);
		}

		[TestMethod]
		public void MakeUniqueFindTest() {
			StoreSnapshot store = new StoreSnapshot();
			TableSnapshot<int> table = new TableSnapshot<int>(store, "test", IntArray.Field);
			table.MakeUnique<int>("testUnique", IntArray.Field);
			store.FreezeShape();

			Assert.IsTrue(store.StartTransaction());

			int data = 19;
			RowId r1 = table.Insert(ref data);
			data = 71;
			RowId r2 = table.Insert(ref data);
			data = 4;
			RowId r3 = table.Insert(ref data);

			Assert.AreEqual<RowId>(r1, table.Find<int>(IntArray.Field, 19));
			Assert.AreEqual<RowId>(r2, table.Find<int>(IntArray.Field, 71));
			Assert.AreEqual<RowId>(r3, table.Find<int>(IntArray.Field, 4));

			Assert.AreEqual<RowId>(RowId.Empty, table.Find<int>(IntArray.Field, 0));
			Assert.AreEqual<RowId>(RowId.Empty, table.Find<int>(IntArray.Field, -1));
			Assert.AreEqual<RowId>(RowId.Empty, table.Find<int>(IntArray.Field, -130));
			Assert.AreEqual<RowId>(RowId.Empty, table.Find<int>(IntArray.Field, 50));
			Assert.AreEqual<RowId>(RowId.Empty, table.Find<int>(IntArray.Field, 70));
			Assert.AreEqual<RowId>(RowId.Empty, table.Find<int>(IntArray.Field, 72));
		}

		[TestMethod]
		public void MakeUniqueUpdateTest() {
			StoreSnapshot store = new StoreSnapshot();
			TableSnapshot<int> table = new TableSnapshot<int>(store, "test", IntArray.Field);
			table.MakeUnique<int>("testUnique", IntArray.Field);
			store.FreezeShape();

			Assert.IsTrue(store.StartTransaction());

			int data = 315;
			RowId r1 = table.Insert(ref data);
			data = 234;
			RowId r2 = table.Insert(ref data);
			data = 917;
			RowId r3 = table.Insert(ref data);

			Assert.AreEqual<RowId>(r1, table.Find<int>(IntArray.Field, 315));
			Assert.AreEqual<RowId>(r2, table.Find<int>(IntArray.Field, 234));
			Assert.AreEqual<RowId>(r3, table.Find<int>(IntArray.Field, 917));

			table.SetField<int>(r2, IntArray.Field, 14);

			Assert.AreEqual<RowId>(r1, table.Find<int>(IntArray.Field, 315));
			Assert.AreEqual<RowId>(r2, table.Find<int>(IntArray.Field, 14));
			Assert.AreEqual<RowId>(r3, table.Find<int>(IntArray.Field, 917));

			table.SetField<int>(r1, IntArray.Field, 9);

			Assert.AreEqual<RowId>(r1, table.Find<int>(IntArray.Field, 9));
			Assert.AreEqual<RowId>(r2, table.Find<int>(IntArray.Field, 14));
			Assert.AreEqual<RowId>(r3, table.Find<int>(IntArray.Field, 917));

			table.SetField<int>(r3, IntArray.Field, 300);

			Assert.AreEqual<RowId>(r1, table.Find<int>(IntArray.Field, 9));
			Assert.AreEqual<RowId>(r2, table.Find<int>(IntArray.Field, 14));
			Assert.AreEqual<RowId>(r3, table.Find<int>(IntArray.Field, 300));
		}

		[TestMethod]
		public void ForeignKeyTest() {
			StoreSnapshot store = new StoreSnapshot();
			TableSnapshot<SampleData> table1 = SampleData.CreateTable(store, "table1");
			TableSnapshot<SampleData> table2 = SampleData.CreateTable(store, "table2");
			TableSnapshot<SampleData> table3 = SampleData.CreateTable(store, "table3");
			table1.MakeUnique("PK_Id", SampleData.SampleIdField.Field, true);
			table2.CreateForeignKey("FK_12Id", table1, SampleData.SampleIdField.Field, ForeignKeyAction.Cascade, false);
			table3.CreateForeignKey("FK_13Id", table1, SampleData.SampleIdField.Field, ForeignKeyAction.Restrict, true);
			store.FreezeShape();

			SampleData data;
			List<RowId> rowIds = [];
			RowId row2 = RowId.Empty, row3 = RowId.Empty;
			this.InTransaction(store, () => {
				for(int i = 0; i < 5; i++) {
					data.Id = i;
					data.Name = $"Test{i}";
					rowIds.Add(table1.Insert(ref data));
				}

				data.Id = 2;
				data.Name = "Test22";
				row2 = table2.Insert(ref data);

				data.Id = 3;
				data.Name = "Test33";
				row3 = table3.Insert(ref data);
			});

			this.InTransaction(store, () => {
				table1.Delete(rowIds[2]);
			});
			Assert.IsTrue(table2.IsDeleted(row2), "Row in table2 should be deleted due to foreign key cascade action.");

			this.InTransaction(store, () => {
				// This should throw an exception because of foreign key restriction
				Assert.Throws<ForeignKeyViolationException>(() => table1.Delete(rowIds[3]), "Deleting row in table1 should fail due to foreign key restriction.");
			});

			data.Id = 100;
			data.Name = "Test100";
			Assert.Throws<ForeignKeyViolationException>(() => this.InTransaction(store, () => table2.Insert(ref data)), "Inserting into table2 with non-existing foreign key should fail.");
		}

		[TestMethod]
		public void SelectFieldValueTest() {
			string name(int i) => $"X {i}";
			CircuitProject project = CircuitProject.Create(null);
			List<LogicalCircuit> list = [];
			project.InTransaction(() => {
				for(int i = 0; i < 5; i++) {
					LogicalCircuit circuit = project.LogicalCircuitSet.Create();
					list.Add(circuit);
					circuit.Note = circuit.Name = name(i);
				}
			});

			TableSnapshot<LogicalCircuitData> table = project.LogicalCircuitSet.Table;

			LogicalCircuit toFind = list[3];
			// check selection by value with index
			List<RowId> found = table.Select(LogicalCircuitData.NameField.Field, toFind.Name).ToList();
			Assert.IsNotNull(found);
			Assert.AreEqual(1, found.Count);
			Assert.AreEqual(toFind.LogicalCircuitRowId, found[0]);

			// check selection by value without index
			found = table.Select(LogicalCircuitData.NoteField.Field, toFind.Note).ToList();
			Assert.IsNotNull(found);
			Assert.AreEqual(1, found.Count);
			Assert.AreEqual(list[3].LogicalCircuitRowId, found[0]);
		}

		[TestMethod]
		public void SelectByRange() {
			string pinName(int i) => $"Pin {i}";
			CircuitProject project = CircuitProject.Create(null);
			LogicalCircuit circuit = project.ProjectSet.Project.LogicalCircuit;
			List<Pin> pins = [];
			project.InTransaction(() => {
				for(int i = 0; i < 5; i++) {
					Pin pin = this.CreatePin(circuit);
					pin.Name = pin.Note = pinName(i);
					pins.Add(pin);
				}
			});

			TableSnapshot<PinData> table = (TableSnapshot<PinData>)project.Table("Pin");

			// check selection by range without index
			List<RowId> range = table.Select(PinData.NoteField.Field, pinName(1), pinName(3)).ToList();
			Assert.IsNotNull(range);
			Assert.AreEqual(3, range.Count);
			for(int i = 1; i <= 3; i++) {
				Assert.IsTrue(range.Contains(pins[i].PinRowId), $"Pin {i} not found in range selection.");
			}

			// check selection by range with index without range support, so essentially same as above
			range = table.Select(PinData.NameField.Field, pinName(1), pinName(3)).ToList();
			Assert.IsNotNull(range);
			Assert.AreEqual(3, range.Count);
			for(int i = 1; i <= 3; i++) {
				Assert.IsTrue(range.Contains(pins[i].PinRowId), $"Pin {i} not found in range selection.");
			}
		}

		[TestMethod]
		public void SelectByRangeWithIndex() {
			StoreSnapshot store = new StoreSnapshot();
			TableSnapshot<int> table = new TableSnapshot<int>(store, "test", IntArray.Field);
			table.CreateIndex("index", IntArray.Field);
			store.FreezeShape();

			Assert.IsTrue(store.StartTransaction());
			for(int i = 0; i < 10; i++) {
				table.Insert(ref i);
			}
			store.Commit();
			List<RowId> range = table.Select(IntArray.Field, 3, 7).ToList();
			Assert.IsNotNull(range);
			Assert.AreEqual(5, range.Count);
			for(int i = 3; i <= 7; i++) {
				Assert.IsTrue(range.Contains(new RowId(i)), $"Value {i} not found in range selection.");
			}
		}

		[TestMethod]
		public void FindTest() {
			string name(int i) => $"X {i}";
			CircuitProject project = CircuitProject.Create(null);
			List<LogicalCircuit> list = [];
			project.InTransaction(() => {
				for(int i = 0; i < 5; i++) {
					LogicalCircuit circuit = project.LogicalCircuitSet.Create();
					list.Add(circuit);
					circuit.Note = circuit.Name = name(i);
				}
			});

			TableSnapshot<LogicalCircuitData> table = project.LogicalCircuitSet.Table;

			RowId rowId = table.Find(LogicalCircuitData.NameField.Field, list[3].Name);
			Assert.AreEqual(list[3].LogicalCircuitRowId, rowId, "Find by name failed.");

			rowId = RowId.Empty;
			Assert.Throws<InvalidOperationException>(() => {
				// Find by value without index should throw an exception
				rowId = table.Find(LogicalCircuitData.NoteField.Field, list[3].Note);
			}, "Find by note without index should throw an exception.");
			Assert.AreEqual(RowId.Empty, rowId, "Find by note without index should return empty RowId.");

			rowId = table.Find(LogicalCircuitData.NameField.Field, "NonExistentName");
			Assert.AreEqual(RowId.Empty, rowId, "Find by note without index should return empty RowId.");
		}

		[TestMethod]
		public void Find2Test() {
			StoreSnapshot store = new StoreSnapshot();
			TableSnapshot<SampleData> table1 = SampleData.CreateTable(store, "table1");
			table1.MakeUnique("2 fields", SampleData.SampleIdField.Field, SampleData.SampleNameField.Field);
			TableSnapshot<SampleData> table2 = SampleData.CreateTable(store, "table2");
			store.FreezeShape();

			RowId rowId = table1.Find(SampleData.SampleIdField.Field, SampleData.SampleNameField.Field, 1, "hello");
			Assert.AreEqual(RowId.Empty, rowId, "Find should return empty RowId for non-existing record.");
			Assert.Throws<InvalidOperationException>(() => {
				// Find by value without index should throw an exception
				rowId = table2.Find(SampleData.SampleIdField.Field, SampleData.SampleNameField.Field, 1, "hello");
			}, "Find by value without index should throw an exception.");

			SampleData data;
			this.InTransaction(store, () => {
				for(int i = 0; i < 10; i++) {
					data.Id = i;
					data.Name = $"Test{i}";
					table1.Insert(ref data);
					table2.Insert(ref data);
				}
			});
			rowId = table1.Find(SampleData.SampleIdField.Field, SampleData.SampleNameField.Field, 3, "Test3");
			Assert.AreEqual(new RowId(3), rowId, "Find should return correct RowId for existing record.");
			Assert.Throws<InvalidOperationException>(() => {
				// Find by value without index should throw an exception
				rowId = table2.Find(SampleData.SampleIdField.Field, SampleData.SampleNameField.Field, 1, "hello");
			}, "Find by value without index should throw an exception.");
		}

		[TestMethod]
		public void ExistsTest() {
			string name(int i) => $"X {i}";
			CircuitProject project = CircuitProject.Create(null);
			List<LogicalCircuit> list = [];
			project.InTransaction(() => {
				for(int i = 0; i < 5; i++) {
					LogicalCircuit circuit = project.LogicalCircuitSet.Create();
					list.Add(circuit);
					circuit.Note = circuit.Name = name(i);
				}
			});

			TableSnapshot<LogicalCircuitData> table = project.LogicalCircuitSet.Table;

			bool exists = table.Exists(LogicalCircuitData.NameField.Field, list[3].Name);
			Assert.IsTrue(exists);

			exists = table.Exists(LogicalCircuitData.NoteField.Field, list[3].Note);
			Assert.IsTrue(exists);

			exists = table.Exists(LogicalCircuitData.NameField.Field, "NonExistentName");
			Assert.IsFalse(exists);
		}

		[TestMethod]
		public void WasChangedTest() {
			StoreSnapshot store = new StoreSnapshot();
			TableSnapshot<SampleData> table1 = SampleData.CreateTable(store, "table1");
			TableSnapshot<SampleData> table2 = SampleData.CreateTable(store, "table2");
			store.FreezeShape();
			SampleData data = new SampleData { Id = 1, Name = "Test1" };
			this.InTransaction(store, () => {
				table1.Insert(ref data);
			});
			this.InTransaction(store, () => {
				data.Id = 2;
				data.Name = "Test2";
				table2.Insert(ref data);
			});
			this.InTransaction(store, () => {
				data.Id = 3;
				data.Name = "Test3";
				table1.Insert(ref data);
				data.Id = 4;
				data.Name = "Test4";
				table2.Insert(ref data);
			});

			Assert.IsTrue(table1.WasChanged(0, 1), "Table1 should have been changed between versions 0 and 1.");
			Assert.IsFalse(table1.WasChanged(2, 2), "Table1 should not have been changed between versions 2 and 2.");
			Assert.IsFalse(table2.WasChanged(0, 1), "Table1 should not have been changed between versions 0 and 1.");
			Assert.IsTrue(table2.WasChanged(1, 2), "Table2 should have been changed between versions 1 and 2.");
			Assert.IsTrue(table1.WasChanged(1, 3), "Table2 should have been changed between versions 2 and 3.");

			Assert.Throws<ArgumentOutOfRangeException>(() => {
				// Version 5 does not exist
				table1.WasChanged(-1, 5);
			}, "WasChanged should throw ArgumentOutOfRangeException for non-existing version.");

			Assert.Throws<ArgumentOutOfRangeException>(() => {
				// Version 5 does not exist
				table1.WasChanged(5, 6);
			}, "WasChanged should throw ArgumentOutOfRangeException for non-existing version.");

			Assert.Throws<ArgumentOutOfRangeException>(() => {
				// Version 5 does not exist
				table1.WasChanged(2, 1);
			}, "WasChanged should throw ArgumentOutOfRangeException for non-existing version.");

			Assert.Throws<ArgumentOutOfRangeException>(() => {
				// Version 5 does not exist
				table1.WasChanged(1, 5);
			}, "WasChanged should throw ArgumentOutOfRangeException for non-existing version.");
		}

		[TestMethod]
		public void GetChangesTest() {
			StoreSnapshot store = new StoreSnapshot();
			TableSnapshot<int> table = new TableSnapshot<int>(store, "test", IntArray.Field);
			store.FreezeShape();

			int data;

			// transaction 1
			Assert.IsTrue(store.StartTransaction());
			data = 100;
			table.Insert(ref data);
			store.Commit();

			// transaction 2
			Assert.IsTrue(store.StartTransaction());
			data = 200;
			table.Insert(ref data);
			table.SetField<int>(new RowId(0), IntArray.Field, 101);
			store.Commit();

			// transaction 3
			Assert.IsTrue(store.StartTransaction());
			table.Delete(new RowId(0));
			table.SetField<int>(new RowId(1), IntArray.Field, 201);
			store.Commit();

			// verify state
			Assert.IsTrue(table.IsDeleted(new RowId(0)));
			Assert.AreEqual<int>(201, table.GetField<int>(new RowId(1), IntArray.Field));

			IEnumerator<TableChange<int>> enumerator = table.GetChanges(1, 3);
			Assert.IsNotNull(enumerator);
			Assert.IsTrue(enumerator.MoveNext());
			Assert.AreEqual<int>(1, enumerator.Current.RowId.Value);
			Assert.AreEqual<SnapTableAction>(SnapTableAction.Insert, enumerator.Current.Action);
			Assert.IsFalse(enumerator.MoveNext());

			// transaction 4: undo the transaction 3
			store.Undo();

			// verify the state
			Assert.IsFalse(table.IsDeleted(new RowId(0)));
			Assert.AreEqual<int>(101, table.GetField<int>(new RowId(0), IntArray.Field));
			Assert.AreEqual<int>(200, table.GetField<int>(new RowId(1), IntArray.Field));

			enumerator = table.GetChanges(1, 4);
			Assert.IsNotNull(enumerator);
			int count = 0;
			while(enumerator.MoveNext()) {
				count++;
				Assert.AreEqual<SnapTableAction>(SnapTableAction.Insert, enumerator.Current.Action);
				Assert.IsTrue(0 == enumerator.Current.RowId.Value || 1 == enumerator.Current.RowId.Value);
			}
			Assert.AreEqual<int>(2, count);

			// operation with undo should give us an empty changes
			enumerator = table.GetChanges(3, 4);
			Assert.IsNotNull(enumerator);
			Assert.IsTrue(enumerator.MoveNext());
			int oldData, newData;
			enumerator.Current.GetOldData(out oldData);
			enumerator.Current.GetNewData(out newData);
			Assert.AreEqual(oldData, newData);
			Assert.IsFalse(enumerator.MoveNext());
		}
	}
}
