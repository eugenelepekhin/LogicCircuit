using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace LogicCircuit.DataPersistent {
	[SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	public interface ITableSnapshot : IEnumerable<RowId> {
		StoreSnapshot StoreSnapshot { get; }
		string Name { get; }
		IEnumerable<RowId> Rows { get; }
		bool WasChanged(int fromVersion, int toVersion);
		bool WasAffected(int version);
	}
}
