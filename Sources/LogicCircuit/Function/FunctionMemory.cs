using System;
using System.Collections.Generic;

namespace LogicCircuit {
	public abstract class FunctionMemory : CircuitFunction {
		private int[] address;
		private int[] inputData;
		private int[] outputData;
		private int write;
		private State writeOn;
		private State oldWriteState = State.Off;
		private byte[] data;

		protected FunctionMemory(
			CircuitState circuitState, int[] address, int[] inputData, int[] outputData, int write, bool writeOn1
		) : base(circuitState, FunctionMemory.Input(address, inputData, write), outputData) {
			this.address = address;
			this.inputData = inputData;
			this.outputData = outputData;
			if(inputData != null) {
				Tracer.Assert(this.inputData.Length == this.outputData.Length);
				this.data = new byte[Memory.BytesPerCellFor(this.inputData.Length) * Memory.NumberCellsFor(this.address.Length)];
				circuitState.Random.NextBytes(this.data);
				this.write = write;
				this.writeOn = writeOn1 ? State.On1 : State.On0;
			} else {
				this.write = -1;
			}
		}

		private static int[] Input(int[] address, int[] inputData, int clock) {
			if(inputData == null) {
				return address;
			}
			int[] input = new int[address.Length + inputData.Length + 1];
			Array.Copy(address, input, address.Length);
			Array.Copy(inputData, 0, input, address.Length, inputData.Length);
			input[address.Length + inputData.Length] = clock;
			return input;
		}

		public void Write(byte[] newData) {
			Tracer.Assert(this.inputData == null && this.data == null);
			this.data = newData;
		}

		private int ReadState(int[] parameter) {
			int state = 0;
			for(int i = 0; i < parameter.Length; i++) {
				if(this.CircuitState[parameter[i]] == State.On1) {
					state |= 1 << i;
				}
			}
			return state;
		}

		protected void Write() {
			Memory.SetCellValue(this.data, this.inputData.Length, this.ReadState(this.address), this.ReadState(this.inputData));
		}

		protected bool Read() {
			return this.SetResult(Memory.CellValue(this.data, this.DataBitWidth, this.ReadState(this.address)));
		}

		protected bool IsWriteAllowed {
			get {
				State state = this.CircuitState[this.write];
				bool allowed = (state == this.writeOn && CircuitFunction.Not(state) == this.oldWriteState);
				this.oldWriteState = state;
				return allowed;
			}
		}

		public int AddressBitWidth { get { return this.address.Length; } }
		public int DataBitWidth { get { return this.outputData.Length; } }

		public int this[int index] {
			get { return Memory.CellValue(this.data, this.DataBitWidth, index); }
		}
	}
}
