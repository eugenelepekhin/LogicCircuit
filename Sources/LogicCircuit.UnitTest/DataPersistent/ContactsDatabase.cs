using DataPersistent;

namespace LogicCircuit.UnitTest.DataPersistent {
	namespace UnitTestSnapStore {
		public struct PersonData {
			public int PersonId;
			public string FirstName;
			public string LastName;

			public static IField<PersonData>[] Fields() => new IField<PersonData>[] { PersonIdField.Field, FirstNameField.Field, LastNameField.Field };

			public class PersonIdField : IField<PersonData, int> {
				public static readonly PersonIdField Field = new PersonIdField();

				public int DefaultValue => 0;
				public int GetValue(ref PersonData record) => record.PersonId;
				public void SetValue(ref PersonData record, int value) => record.PersonId = value;
				public int Compare(int x, int y) => x - y;
				public string Name => "PersonId";
				public int Order { get; set; }
				public int Compare(ref PersonData data1, ref PersonData data2) => data1.PersonId - data2.PersonId;
			}

			public abstract class StringField : IField<PersonData, string> {
				protected StringField(string name) => this.Name = name;
				public abstract string GetValue(ref PersonData record);
				public abstract void SetValue(ref PersonData record, string value);
				public int Compare(string x, string y) => StringComparer.Ordinal.Compare(x, y);
				public string Name { get; private set; }
				public int Order { get; set; }
				public int Compare(ref PersonData data1, ref PersonData data2) => this.Compare(this.GetValue(ref data1), this.GetValue(ref data2));
				public string DefaultValue => null;
			}

			public class FirstNameField : StringField {
				public static readonly FirstNameField Field = new FirstNameField();
				private FirstNameField() : base("FirstName") { }
				public override string GetValue(ref PersonData record) => record.FirstName;
				public override void SetValue(ref PersonData record, string value) => record.FirstName = value;
			}

			public class LastNameField : StringField {
				public static readonly LastNameField Field = new LastNameField();
				private LastNameField() : base("LastName") { }
				public override string GetValue(ref PersonData record) => record.LastName;
				public override void SetValue(ref PersonData record, string value) => record.LastName = value;
			}
		}

		public struct PhoneData {
			public int PhoneId;
			public int PersonId;
			public string Number;

			public static IField<PhoneData>[] Fields() => new IField<PhoneData>[] { PhoneIdField.Field, PersonIdField.Field, NumberField.Field };

			public abstract class IntField : IField<PhoneData, int> {
				protected IntField(string name) => this.Name = name;
				public abstract int GetValue(ref PhoneData record);
				public abstract void SetValue(ref PhoneData record, int value);
				public int Compare(int x, int y) => x - y;
				public string Name { get; private set; }
				public int Order { get; set; }
				public int Compare(ref PhoneData data1, ref PhoneData data2) => this.Compare(this.GetValue(ref data1), this.GetValue(ref data2));
				public int DefaultValue => 0;
			}

			public class PhoneIdField : IntField {
				public static readonly PhoneIdField Field = new PhoneIdField();
				private PhoneIdField() : base("PhoneId") { }
				public override int GetValue(ref PhoneData record) => record.PhoneId;
				public override void SetValue(ref PhoneData record, int value) => record.PhoneId = value;
			}

			public class PersonIdField : IntField {
				public static readonly PersonIdField Field = new PersonIdField();

				private PersonIdField() : base("PersonId") { }
				public override int GetValue(ref PhoneData record) => record.PersonId;
				public override void SetValue(ref PhoneData record, int value) => record.PersonId = value;
			}

			public class NumberField : IField<PhoneData, string> {
				public static readonly NumberField Field = new NumberField();
				public string GetValue(ref PhoneData record) => record.Number;
				public void SetValue(ref PhoneData record, string value) => record.Number = value;
				public int Compare(string x, string y) => StringComparer.OrdinalIgnoreCase.Compare(x, y);
				public string Name => "Number";
				public int Order { get; set; }
				public int Compare(ref PhoneData data1, ref PhoneData data2) => this.Compare(data1.Number, data2.Number);
				public string DefaultValue => null;
			}
		}

		public struct PersonRestrictData {
			public int PersonRestrictId;
			public int PersonId;
			public int Data;

			public static IField<PersonRestrictData>[] Fields() => new IField<PersonRestrictData>[] { PersonRestrictIdField.Field, PersonIdField.Field, DataField.Field };

			public abstract class IntField : IField<PersonRestrictData, int> {
				protected IntField(string name) => this.Name = name;
				public abstract int GetValue(ref PersonRestrictData record);
				public abstract void SetValue(ref PersonRestrictData record, int value);
				public int Compare(int x, int y) => x - y;
				public string Name { get; private set; }
				public int Order { get; set; }
				public int Compare(ref PersonRestrictData data1, ref PersonRestrictData data2) => this.Compare(this.GetValue(ref data1), this.GetValue(ref data2));
				public int DefaultValue => 0;
			}

			public class PersonRestrictIdField : IntField {
				public static readonly PersonRestrictIdField Field = new PersonRestrictIdField();

				private PersonRestrictIdField() : base("PersonRestrictId") { }
				public override int GetValue(ref PersonRestrictData record) => record.PersonRestrictId;
				public override void SetValue(ref PersonRestrictData record, int value) => record.PersonRestrictId = value;
			}

			public class PersonIdField : IntField {
				public static readonly PersonIdField Field = new PersonIdField();

				private PersonIdField() : base("PersonId") { }
				public override int GetValue(ref PersonRestrictData record) => record.PersonId;
				public override void SetValue(ref PersonRestrictData record, int value) => record.PersonId = value;
			}

			public class DataField : IntField {
				public static readonly DataField Field = new DataField();

				private DataField() : base("Data") { }
				public override int GetValue(ref PersonRestrictData record) => record.Data;
				public override void SetValue(ref PersonRestrictData record, int value) => record.Data = value;
			}
		}

		public struct PersonDefaultData {
			public int PersonDefaultId;
			public int PersonId;
			public int Data;

			public static IField<PersonDefaultData>[] Fields() => new IField<PersonDefaultData>[] { PersonDefaultIdField.Field, PersonIdField.Field, DataField.Field };

			public abstract class IntField : IField<PersonDefaultData, int> {
				protected IntField(string name) => this.Name = name;
				public abstract int GetValue(ref PersonDefaultData record);
				public abstract void SetValue(ref PersonDefaultData record, int value);
				public int Compare(int x, int y) => x - y;
				public string Name { get; private set; }
				public int Order { get; set; }
				public int Compare(ref PersonDefaultData data1, ref PersonDefaultData data2) => this.Compare(this.GetValue(ref data1), this.GetValue(ref data2));
				public int DefaultValue => 0;
			}

			public class PersonDefaultIdField : IntField {
				public static readonly PersonDefaultIdField Field = new PersonDefaultIdField();

				private PersonDefaultIdField() : base("PersonDefaultId") { }
				public override int GetValue(ref PersonDefaultData record) => record.PersonDefaultId;
				public override void SetValue(ref PersonDefaultData record, int value) => record.PersonDefaultId = value;
			}

			public class PersonIdField : IntField {
				public static readonly PersonIdField Field = new PersonIdField();

				private PersonIdField() : base("PersonId") { }
				public override int GetValue(ref PersonDefaultData record) => record.PersonId;
				public override void SetValue(ref PersonDefaultData record, int value) => record.PersonId = value;
			}

			public class DataField : IntField {
				public static readonly DataField Field = new DataField();

				private DataField() : base("Data") { }
				public override int GetValue(ref PersonDefaultData record) => record.Data;
				public override void SetValue(ref PersonDefaultData record, int value) => record.Data = value;
			}
		}

		public class ContactsDatabase {
			public static StoreSnapshot CreateContactsDatabase() {
				StoreSnapshot store = new StoreSnapshot();
				TableSnapshot<PersonData> person = new TableSnapshot<PersonData>(store, "Person", PersonData.Fields());
				person.MakeUnique<int>("PK_Person", PersonData.PersonIdField.Field, true);

				TableSnapshot<PhoneData> phone = new TableSnapshot<PhoneData>(store, "Phone", PhoneData.Fields());
				phone.MakeUnique<int>("PK_Phone", PhoneData.PhoneIdField.Field, true);
				phone.CreateForeignKey<int>("FK_PersonPhone", person, PhoneData.PersonIdField.Field, ForeignKeyAction.Cascade);
				phone.CreateIndex<int>("IX_PersonPhone", PhoneData.PersonIdField.Field);

				TableSnapshot<PersonRestrictData> restrict = new TableSnapshot<PersonRestrictData>(store, "PersonRestrict", PersonRestrictData.Fields());
				restrict.MakeUnique<int>("PK_PersonRestrict", PersonRestrictData.PersonRestrictIdField.Field, true);
				restrict.CreateForeignKey<int>("FK_Person_PersonRestrict", person, PersonRestrictData.PersonIdField.Field, ForeignKeyAction.Restrict);
				restrict.CreateIndex<int>("IX_PresonRestrict", PersonRestrictData.PersonIdField.Field);

				TableSnapshot<PersonDefaultData> def = new TableSnapshot<PersonDefaultData>(store, "PersonDefault", PersonDefaultData.Fields());
				def.MakeUnique<int>("PK_PersonDefault", PersonDefaultData.PersonDefaultIdField.Field, true);
				def.CreateForeignKey<int>("FK_Person_PersonDefault", person, PersonDefaultData.PersonIdField.Field, ForeignKeyAction.Restrict);
				def.CreateIndex<int>("IX_PersonDefault", PersonDefaultData.PersonIdField.Field);

				store.FreezeShape();

				return store;
			}

			public static TableSnapshot<PersonData> PersonTable(StoreSnapshot store) {
				TableSnapshot<PersonData> table = (TableSnapshot<PersonData>)store.Table("Person");
				Assert.IsNotNull(table);
				return table;
			}

			public static TableSnapshot<PhoneData> PhoneTable(StoreSnapshot store) {
				TableSnapshot<PhoneData> table = (TableSnapshot<PhoneData>)store.Table("Phone");
				Assert.IsNotNull(table);
				return table;
			}

			public static TableSnapshot<PersonRestrictData> PersonRestrictTable(StoreSnapshot store) {
				TableSnapshot<PersonRestrictData> table = (TableSnapshot<PersonRestrictData>)store.Table("PersonRestrict");
				Assert.IsNotNull(table);
				return table;
			}

			public static TableSnapshot<PersonDefaultData> PersonDefaultTable(StoreSnapshot store) {
				TableSnapshot<PersonDefaultData> table = (TableSnapshot<PersonDefaultData>)store.Table("PersonDefault");
				Assert.IsNotNull(table);
				return table;
			}
		}
	}
}
