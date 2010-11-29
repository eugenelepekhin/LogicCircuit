using System;
using System.Collections.Generic;

namespace LogicCircuit.DataPersistent {
	/// <summary>
	/// Describes Field of the table
	/// </summary>
	public interface IField<TRecord> where TRecord:struct {
		/// <summary>
		/// Gets name of the filed
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Gets or sets order number of the field. The setter is not intended for public use.
		/// </summary>
		int Order { get; set; }

		/// <summary>
		/// Compares two values of the provided data
		/// </summary>
		/// <param name="data1"></param>
		/// <param name="data2"></param>
		/// <returns></returns>
		int Compare(ref TRecord data1, ref TRecord data2);
	}

	/// <summary>
	/// Defines operations on one field of the table.
	/// </summary>
	/// <typeparam name="TRecord"></typeparam>
	/// <typeparam name="TField"></typeparam>
	public interface IField<TRecord, TField> : IField<TRecord>, IComparer<TField> where TRecord:struct {
		/// <summary>
		/// Gets default value for the field
		/// </summary>
		TField DefaultValue { get; }

		/// <summary>
		/// Gets the value of the field from provided record
		/// </summary>
		/// <param name="record"></param>
		/// <returns></returns>
		TField GetValue(ref TRecord record);

		/// <summary>
		/// Sets value of the field
		/// </summary>
		/// <param name="record"></param>
		/// <param name="value"></param>
		void SetValue(ref TRecord record, TField value);
	}
}
