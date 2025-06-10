﻿using System;

namespace LogicCircuit.DataPersistent {
	partial class SnapTable<TRecord> {
		/// <summary>
		/// Pseudo field to use in <seealso cref="UniquePseudoIndex"/>.
		/// </summary>
		internal sealed class RowIdPseudoField : IField<TRecord, RowId> {
			public static readonly RowIdPseudoField Field = new RowIdPseudoField();

			public RowId DefaultValue => RowId.Empty;

			public RowId GetValue(ref TRecord record) {
				throw new InvalidOperationException();
			}

			public void SetValue(ref TRecord record, RowId value) {
				throw new InvalidOperationException();
			}

			public string Name => "RowIdPseudoField";

			public int Order {
				get => -1;
				set => throw new InvalidOperationException();
			}

			public int Compare(ref TRecord data1, ref TRecord data2) {
				throw new InvalidOperationException();
			}

			public int Compare(RowId x, RowId y) => x.CompareTo(y);
		}
	}
}
