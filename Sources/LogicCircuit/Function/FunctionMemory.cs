using System;
using System.Diagnostics.CodeAnalysis;

namespace LogicCircuit {
	public abstract class FunctionMemory : CircuitFunction, IFunctionMemory {
		private int[] address;
		private int[] inputData;
		private int[] outputData;
		private int write;
		private State writeOn;
		private State oldWriteState = State.Off;
		private byte[] data;
		public Memory Memory { get; private set; }

		protected FunctionMemory(
			CircuitState circuitState, int[] address, int[] inputData, int[] outputData, int write, Memory memory
		) : base(circuitState, FunctionMemory.Input(address, inputData, write), outputData) {
			this.address = address;
			this.inputData = inputData;
			this.outputData = outputData;
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

		internal static int[] Input(int[] address, int[] inputData, int clock) {
			if(inputData == null) {
				return address;
			}
			int[] input = new int[address.Length + inputData.Length + 1];
			Array.Copy(address, input, address.Length);
			Array.Copy(inputData, 0, input, address.Length, inputData.Length);
			input[address.Length + inputData.Length] = clock;
			return input;
		}

		protected void Write() {
			Memory.SetCellValue(this.data, this.inputData.Length, this.ReadNumericState(this.address), this.ReadNumericState(this.inputData));
		}

		protected bool Read() {
			return this.SetResult(Memory.CellValue(this.data, this.DataBitWidth, this.ReadNumericState(this.address)));
		}

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
