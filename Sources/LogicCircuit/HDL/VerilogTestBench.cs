// Ignore Spelling: Verilog

using System;
using System.Collections.Generic;
using System.Globalization;

namespace LogicCircuit {
	internal class VerilogTestBench : T4Transformation {
		private string circuitName;
		private List<InputPinSocket> inputs;
		private List<OutputPinSocket> outputs;
		private IList<TruthState> table;

		public VerilogTestBench(string circuitName, List<InputPinSocket> inputs, List<OutputPinSocket> outputs, IList<TruthState> table) {
			this.circuitName = circuitName;
			this.inputs = inputs;
			this.outputs = outputs;
			this.table = table;
		}

		public override string TransformText() {
			this.WriteLine("module {0}_TestBench;", this.circuitName);

			foreach(InputPinSocket input in this.inputs) {
				this.WriteLine("\treg {0}{1};", VerilogHdl.Range(input.Pin), input.Pin.Name);
			}
            foreach(OutputPinSocket output in this.outputs) {
                this.WriteLine("\twire {0}{1};", VerilogHdl.Range(output.Pin), output.Pin.Name);
            }
			this.WriteLine();

			this.WriteLine("\t{0} u0(", this.circuitName);
			bool comma = false;
			foreach(InputPinSocket input in this.inputs) {
				if(comma) this.WriteLine(",");
				this.Write("\t\t.{0}({0})", input.Pin.Name);
				comma = true;
			}
            foreach(OutputPinSocket output in this.outputs) {
				if(comma) this.WriteLine(",");
				this.Write("\t\t.{0}({0})", output.Pin.Name);
				comma = true;
            }
			this.WriteLine();
			this.WriteLine("\t);");

			this.WriteLine();
			this.WriteLine("\tinitial begin");

			foreach(TruthState state in this.table) {
				int index = 0;
				foreach(InputPinSocket input in this.inputs) {
					this.WriteLine("\t\t{0} = {1};", input.Pin.Name, state.Input[index]);
					index++;
				}

				this.WriteLine("\t\t#10;");

				index = 0;
				foreach(OutputPinSocket output in this.outputs) {
					string value = state[index];
					string format;
					if(value.Contains('-', StringComparison.Ordinal)) {
						value = value.Replace('-', 'z');
						format = "{0}'b{1}";
					} else {
						format = "{0}'h{1}";
					}
					value = string.Format(CultureInfo.InvariantCulture, format, output.Pin.BitWidth, value);
					this.WriteLine("\t\tif({0} !== {1}) $error(\"Output {0} expected value {1} actual value is \", {0});", output.Pin.Name, value);
					index++;
				}

				this.WriteLine();
			}

			this.WriteLine("\tend");

            this.WriteLine("endmodule // {0}_TestBench", this.circuitName);

			return this.GenerationEnvironment.ToString();
		}
	}
}
