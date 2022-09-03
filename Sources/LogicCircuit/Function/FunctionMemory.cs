using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace LogicCircuit {
	public abstract class FunctionMemory : CircuitFunction, IFunctionMemory {
		private readonly int[] address;
		private readonly int[] address2;
		private readonly int[] inputData;
		private readonly int[] outputData;
		private readonly int[] outputData2;
		private readonly int write;
		private readonly State writeOn;
		private State oldWriteState = State.Off;
		private readonly byte[] data;
		public Memory Memory { get; private set; }

		protected FunctionMemory(
			CircuitState circuitState, int[] address, int[] inputData, int[] outputData, int[] address2, int[] outputData2, int write, Memory memory
		) : base(circuitState, FunctionMemory.Input(address, address2, inputData, write), FunctionMemory.Output(outputData, outputData2)) {
			this.address = address;
			this.address2 = address2;
			this.inputData = inputData;
			this.outputData = outputData;
			this.outputData2 = outputData2;
			this.Memory = memory;
			if(inputData != null) {
				Tracer.Assert(memory.Writable);
				Tracer.Assert(this.inputData.Length == this.outputData.Length);
				this.write = write;
				this.writeOn = this.Memory.WriteOn1 ? State.On1 : State.On0;
				switch(this.Memory.OnStart) {
				case MemoryOnStart.Random:
					this.data = this.Allocate();
					circuitState.Random.NextBytes(this.data);
					break;
				case MemoryOnStart.Zeros:
					this.data = this.Allocate();
					break;
				case MemoryOnStart.Ones:
					this.data = this.Allocate();
					for(int i = 0; i < this.data.Length; i++) {
						this.data[i] = 0xFF;
					}
					break;
				case MemoryOnStart.Data:
					this.data = memory.MemoryValue();
					break;
				default:
					Tracer.Fail();
					break;
				}
			} else {
				Tracer.Assert(!memory.Writable);
				this.write = -1;
				this.data = memory.MemoryValue();
			}
		}

		private byte[] Allocate() {
			return new byte[Memory.BytesPerCellFor(this.inputData.Length) * Memory.NumberCellsFor(this.address.Length)];
		}

		internal static int[] Input(int[] address, int[] address2, int[] inputData, int clock) {
			if(address2 == null && inputData == null) {
				return address;
			}
			int a2 = address2 == null ? 0 : address2.Length;
			int d = inputData == null ? 0 : inputData.Length;
			int c = inputData == null ? 0 : 1;
			int[] input = new int[address.Length + a2 + d + c];
			Array.Copy(address, input, address.Length);
			if(address2 != null) {
				Array.Copy(address2, 0, input, address.Length, a2);
			}
			if(inputData != null) {
				Array.Copy(inputData, 0, input, address.Length + a2, inputData.Length);
				input[address.Length + a2 + inputData.Length] = clock;
			}
			return input;
		}

		internal static int[] Output(int[] outputData, int[] outputData2) {
			if(outputData == null) {
				return null;
			}
			if(outputData2 == null) {
				return outputData;
			}
			int[] output = new int[outputData.Length + outputData2.Length];
			Array.Copy(outputData, output, outputData.Length);
			Array.Copy(outputData2, 0, output, outputData.Length, outputData2.Length);
			return output;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void Write() {
			Memory.SetCellValue(this.data, this.inputData.Length, this.ReadNumericState(this.address), this.ReadNumericState(this.inputData));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected bool Read() {
			int state = Memory.CellValue(this.data, this.DataBitWidth, this.ReadNumericState(this.address));
			if(this.address2 == null) {
				return this.SetResult(state);
			} else {
				bool changed = false;
				for(int i = 0; i < this.outputData.Length; i++) {
					changed |= this.SetResult(i, CircuitFunction.FromBool((state & (1 << i)) != 0));
				}
				state = Memory.CellValue(this.data, this.DataBitWidth, this.ReadNumericState(this.address2));
				for(int i = 0; i < this.outputData2.Length; i++) {
					changed |= this.SetResult(i + this.outputData.Length, CircuitFunction.FromBool((state & (1 << i)) != 0));
				}
				return changed;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected bool IsWriteAllowed() {
			State state = this.CircuitState[this.write];
			bool allowed = (state == this.writeOn && CircuitFunction.Not(state) == this.oldWriteState);
			this.oldWriteState = state;
			return allowed;
		}

		public int AddressBitWidth { get { return this.address.Length; } }
		public int DataBitWidth { get { return this.outputData.Length; } }

		public int this[int index] {
			get { return Memory.CellValue(this.data, this.DataBitWidth, index); }
			set { Memory.SetCellValue(this.data, this.DataBitWidth, index, value); }
		}

		[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "TurnOff")]
		public void TurnOff() {
			if(this.Memory.Writable && this.Memory.OnStart == MemoryOnStart.Data) {
				this.Memory.SetMemoryValue(this.data);
			}
		}
	}
}
