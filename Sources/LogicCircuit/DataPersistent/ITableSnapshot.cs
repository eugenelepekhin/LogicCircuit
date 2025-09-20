using System.Collections.Generic;

namespace DataPersistent {
	public interface ITableSnapshot : IEnumerable<RowId> {
		StoreSnapshot StoreSnapshot { get; }
		string Name { get; }
		IEnumerable<RowId> Rows { get; }
		bool WasChanged(int fromVersion, int toVersion);
		bool WasAffected(int version);
	}
}
