// Ignore Spelling: Hdl

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace LogicCircuit {
	internal sealed class HdlConnection : IEquatable<HdlConnection> {
		internal readonly struct BitRange : IEquatable<BitRange> {
			public int First { get; }
			public int Last { get; }
			public int BitWidth => this.Last - this.First + 1;

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

			private BitRange(int first, int last) {
				Tracer.Assert(0 <= first && first <= last && last <= 32);
				this.First = first;
				this.Last = last;
			}

			public bool Equals(BitRange other) => this.First == other.First && this.Last == other.Last;
			public override bool Equals(object? obj) => obj is BitRange other && Equals(other);
			public override int GetHashCode() => HashCode.Combine(this.First, this.Last);
			public static bool operator ==(BitRange left, BitRange right) => left.Equals(right);
			public static bool operator !=(BitRange left, BitRange right) =>  !(left == right);


			public int Extract(int value) => BitRange.Extract(value, this.First, this.Last);
			public static int Extract(int value, int first, int last) {
				long f = (1L << first) - 1;
				long l = (1L << (last + 1)) - 1;
				int mask = (int)(f ^ l);
				return (mask & value) >> first;
			}

			public bool CanAdd(BitRange other) => other.First <= this.First && this.First <= other.Last + 1 || other.First <= this.Last + 1 && this.Last <= other.Last;
			public BitRange Add(BitRange other) {
				Tracer.Assert(this.CanAdd(other));
				return new BitRange(Math.Min(this.First, other.First), Math.Max(this.Last, other.Last));
			}

			#if DEBUG
				public override string ToString() => $"[{this.Last}:{this.First}]";
			#endif
		}

		public static void Create(HdlSymbol outSymbol, HdlSymbol inSymbol, Connection connection) {
			Debug.Assert(outSymbol.CircuitSymbol == connection.OutJam.CircuitSymbol);
			Debug.Assert(inSymbol.CircuitSymbol == connection.InJam.CircuitSymbol);
			HdlConnection hdlConnection = new HdlConnection(outSymbol, inSymbol, connection);
			outSymbol.Add(hdlConnection);
			inSymbol.Add(hdlConnection);
		}

		public static void Create(HdlSymbol outSymbol, Jam outJam, int outBit, HdlSymbol inSymbol, Jam inJam, int inBit) {
			foreach(HdlConnection hdlConnection in outSymbol.Find(outJam, inJam).Where(c => c.outBits != null)) {
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

		private HdlConnection(HdlSymbol outSymbol, Jam outJam, List<int>? outBits, HdlSymbol inSymbol, Jam inJam, List<int>? inBits) {
			this.OutHdlSymbol = outSymbol;
			this.OutJam = outJam;
			this.outBits = (outBits == null) ? outBits : new List<int>(outBits);

			this.InHdlSymbol = inSymbol;
			this.InJam = inJam;
			this.inBits = (inBits == null) ? inBits : new List<int>(inBits);
		}

		public HdlConnection CreateCopy(HdlSymbol outSymbol, Jam outJam, HdlSymbol inSymbol, Jam inJam) {
			return new HdlConnection(outSymbol, outJam, this.outBits, inSymbol, inJam, this.inBits) {
				SkipOutput = this.SkipOutput
			};
		}

		public bool Equals(HdlConnection? other) => other != null && 
			this.OutHdlSymbol == other.OutHdlSymbol &&
			this.InHdlSymbol == other.InHdlSymbol &&
			this.OutJam == other.OutJam &&
			this.InJam == other.InJam &&
			this.InBits == other.InBits &&
			this.OutBits == other.OutBits
		;

		public override bool Equals(object? obj) => this.Equals(obj as HdlConnection);

		public override int GetHashCode() => base.GetHashCode();

		public bool GenerateOutput(HdlSymbol symbol) => symbol != this.OutHdlSymbol || !this.SkipOutput;

		public bool ConnectsInputWithOutput() =>
			(this.InJam.CircuitSymbol.Circuit is Pin) &&
			(this.OutJam.CircuitSymbol.Circuit is Pin)
		;

		public Jam SymbolJam(HdlSymbol symbol) {
			Debug.Assert(symbol == this.OutHdlSymbol || symbol == this.InHdlSymbol);
			if(symbol == this.OutHdlSymbol) {
				return this.OutJam;
			} else {
				return this.InJam;
			}
		}

		public HdlSymbol OtherSymbol(HdlSymbol symbol) {
			Debug.Assert(symbol == this.OutHdlSymbol || symbol == this.InHdlSymbol);
			return (symbol == this.OutHdlSymbol) ? this.InHdlSymbol : this.OutHdlSymbol;
		}
		public Jam OtherJam(Jam jam) {
			Debug.Assert(jam == this.OutJam || jam == this.InJam);
			return (jam == this.OutJam) ? this.InJam : this.OutJam;
		}

		public HdlSymbol Symbol(Jam jam) {
			Debug.Assert(jam == this.OutJam || jam == this.InJam);
			return (jam == this.OutJam) ? this.OutHdlSymbol : this.InHdlSymbol;
		}

		public BitRange JamRange(Jam jam) {
			Debug.Assert(jam == this.OutJam || jam == this.InJam);
			return (jam == this.OutJam) ? this.OutBits : this.InBits;
		}

		#if DEBUG
			public override string ToString() => $"HdlConnection ({this.OutJam.ToString()})->({this.InJam.ToString()})";
		#endif
	}
}
