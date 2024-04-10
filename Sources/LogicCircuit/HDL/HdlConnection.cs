// Ignore Spelling: Hdl

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace LogicCircuit {
	public class HdlConnection {
		public readonly struct BitRange : IEquatable<BitRange> {
			public int First { get; }
			public int Last { get; }

			public BitRange(IList<int> bits) {
				Debug.Assert(bits != null && 0 < bits.Count);
				for(int i = 1; i < bits.Count; i++) {
					Debug.Assert(bits[i - 1] + 1 == bits[i]);
				}
				this.First = bits[0];
				this.Last = bits[bits.Count - 1];
			}

			public BitRange(Jam jam) {
				this.First = 0;
				this.Last = jam.Pin.BitWidth - 1;
			}

			public bool Equals(BitRange other) => this.First == other.First && this.Last == other.Last;
			public override bool Equals(object? obj) => obj is BitRange other && Equals(other);
			public override int GetHashCode() => HashCode.Combine(this.First, this.Last);
			public static bool operator ==(BitRange left, BitRange right) => left.Equals(right);
			public static bool operator !=(BitRange left, BitRange right) =>  !(left == right);

			public override string ToString() => string.Format(CultureInfo.InvariantCulture, (this.First == this.Last) ? "[{0}]" : "[{0}..{1}]", this.First, this.Last);
		}

		public static void Create(HdlSymbol outSymbol, HdlSymbol inSymbol, Connection connection) {
			if(outSymbol.CircuitSymbol.Circuit is Constant constant && 1 < constant.BitWidth) {
				for(int i = 0; i < constant.BitWidth; i++) {
					HdlConnection.Create(outSymbol, connection.OutJam, i, inSymbol, connection.InJam, i);
				}
			} else {
				HdlConnection hdlConnection = new HdlConnection(outSymbol, inSymbol, connection);
				outSymbol.Add(hdlConnection);
				inSymbol.Add(hdlConnection);
			}
		}

		public static void Create(HdlSymbol outSymbol, Jam outJam, int outBit, HdlSymbol inSymbol, Jam inJam, int inBit) {
			foreach(HdlConnection hdlConnection in outSymbol.Find(outJam, inJam).Where(c => c.outBits != null && c.OutHdlSymbol.CircuitSymbol.Circuit is not Constant)) {
				List<int> outBits = hdlConnection.outBits!;
				List<int> inBits = hdlConnection.inBits!;
				Debug.Assert(outBits.Count == inBits.Count);
				if(outBits[outBits.Count - 1] + 1 == outBit && inBits[inBits.Count - 1] + 1 == inBit) {
					outBits.Add(outBit);
					inBits.Add(inBit);
					return;
				}
			}
			HdlConnection connection = new HdlConnection(outSymbol, outJam, outBit, inSymbol, inJam, inBit);
			outSymbol.Add(connection);
			inSymbol.Add(connection);
		}

		private static readonly Dictionary<string, string> N2TGateName = new Dictionary<string, string>() {
			{ "x", "in" },
			{ "x1", "a" },
			{ "x2", "b" },
			{ "q", "out" },
		};

		public HdlSymbol OutHdlSymbol { get; }
		public Jam OutJam { get; }
		private readonly List<int>? outBits;
		public BitRange OutBits => (this.outBits != null) ? new BitRange(this.outBits) : new BitRange(this.OutJam);

		public HdlSymbol InHdlSymbol { get; }
		public Jam InJam { get; }
		private readonly List<int>? inBits;
		public BitRange InBits => (this.inBits != null) ? new BitRange(this.inBits) : new BitRange(this.InJam);

		public bool IsBitRange(HdlSymbol symbol) => this.outBits != null && this.outBits.Count < ((symbol == this.OutHdlSymbol) ? this.OutJam : this.InJam).Pin.BitWidth;

		/// <summary>
		/// This allow to skip specifying the same output for multiple connections, so only one will provide output and all other are not.
		/// </summary>
		public bool SkipOutput { get; set; }

		private HdlConnection(HdlSymbol outSymbol, HdlSymbol inSymbol, Connection connection) {
			this.OutHdlSymbol = outSymbol;
			this.OutJam = connection.OutJam;

			this.InHdlSymbol = inSymbol;
			this.InJam = connection.InJam;
		}
		private HdlConnection(HdlSymbol outSymbol, Jam outJam, int outBit, HdlSymbol inSymbol, Jam inJam, int inBit) {
			this.OutHdlSymbol = outSymbol;
			this.OutJam = outJam;
			this.outBits = new List<int> { outBit };

			this.InHdlSymbol = inSymbol;
			this.InJam = inJam;
			this.inBits = new List<int>() { inBit };
		}

		public bool GenerateOutput(HdlSymbol symbol) => symbol != this.OutHdlSymbol || !this.SkipOutput;

		public Jam SymbolJam(HdlSymbol symbol) {
			Debug.Assert(symbol == this.OutHdlSymbol || symbol == this.InHdlSymbol);
			if(symbol == this.OutHdlSymbol) {
				return this.OutJam;
			} else {
				return this.InJam;
			}
		}

		public Jam OtherJam(HdlSymbol symbol) {
			Debug.Assert(symbol == this.OutHdlSymbol || symbol == this.InHdlSymbol);
			if(symbol == this.OutHdlSymbol) {
				return this.InJam;
			} else {
				return this.OutJam;
			}
		}

		public string SymbolJamName(HdlSymbol symbol) {
			Debug.Assert(symbol.CircuitSymbol.Circuit is not Pin);
			Jam jam = this.SymbolJam(symbol);
			string? name = null;
			bool isN2t = this.OutHdlSymbol.HdlExport.IsNand2Tetris;
			if(!isN2t || jam.CircuitSymbol.Circuit is not Gate || !HdlConnection.N2TGateName.TryGetValue(jam.Pin.Name, out name)) {
				name = jam.Pin.Name;
			}
			Debug.Assert(name != null);
			if(this.IsBitRange(symbol)) {
				BitRange bits = (jam == this.OutJam) ? this.OutBits : this.InBits;
				name += bits.ToString();
			}
			return name;
		}

		public string PinName(HdlSymbol symbol) {
			Jam jam = this.OtherJam(symbol);
			HdlSymbol other;
			BitRange bits;
			if(symbol == this.OutHdlSymbol) {
				other = this.InHdlSymbol;
				bits = this.InBits;
			} else {
				other = this.OutHdlSymbol;
				bits = this.OutBits;
			}
			if(jam.CircuitSymbol.Circuit is Pin pin) {
				string name = pin.Name;
				if(this.IsBitRange(other)) {
					name += bits.ToString();
				}
				return name;
			}
			if(jam.CircuitSymbol.Circuit is Constant constant) {
				Debug.Assert(bits.First == bits.Last);
				int value = (constant.ConstantValue >> bits.First) & 1;
				return (value == 0) ? "false" : "true";
			}
			GridPoint point = this.OutJam.AbsolutePoint;
			return string.Format(CultureInfo.InvariantCulture, "Pin{0}x{1}", point.X, point.Y);
		}

		public bool ConnectsInputWithOutput() =>
			(this.InJam.CircuitSymbol.Circuit is Pin) &&
			(this.OutJam.CircuitSymbol.Circuit is Pin)
		;

		#if DEBUG
			public override string ToString() => $"HdlConnection ({this.OutJam.ToString()})->({this.InJam.ToString()})";
		#endif
	}
}
