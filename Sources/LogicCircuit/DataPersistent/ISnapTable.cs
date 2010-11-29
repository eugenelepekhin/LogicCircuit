using System;
using System.Collections.Generic;

namespace LogicCircuit.DataPersistent {
	internal interface ISnapTable {
		SnapStore SnapStore { get; }
		string Name { get; }
		void Rollback();
		void Revert(int version);
		ITableSnapshot CreateTableSnapshot(StoreSnapshot storeSnapshot);
		List<IForeignKey> Children { get; }
		bool IsUserTable { get; }
		bool WasChangedIn(int version);
	}
}
