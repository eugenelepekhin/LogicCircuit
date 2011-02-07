using System;
using System.Collections.Generic;

namespace LogicCircuit {
	public class Oscilloscope {

		private Dictionary<string, List<string>> probeLabels = new Dictionary<string, List<string>>();
		private List<string> probes = new List<string>();
		private Dictionary<string, State[]> history = new Dictionary<string, State[]>();

		public Oscilloscope(CircuitRunner circuitRunner) {
			foreach(FunctionProbe probe in circuitRunner.CircuitState.Probes) {
				if(probe.BitWidth == 1) {
					this.probes.Add(probe.Label);
					this.history.Add(probe.Label, new State[CircuitRunner.HistorySize]);
				} else {
					List<string> list = new List<string>();
					this.probeLabels.Add(probe.Label, list);
					for(int i = 0; i < probe.BitWidth; i++) {
						string label = probe.Label + "[" + i + "]";
						list.Add(label);
						this.probes.Add(label);
						this.history.Add(label, new State[CircuitRunner.HistorySize]);
					}
				}
			}
			this.probes.Sort();
		}

		public IEnumerable<string> Probes { get { return probes; } }

		//TODO: revisit this suppression
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public State[] this[string probe] { get { return this.history[probe]; } }

		public void Read(FunctionProbe probe) {
			if(probe.BitWidth == 1) {
				probe.Read(0, this.history[probe.Label]);
			} else {
				List<string> list;
				this.probeLabels.TryGetValue(probe.Label, out list);
				Tracer.Assert(list != null && list.Count == probe.BitWidth);
				for(int i = 0; i < probe.BitWidth; i++) {
					probe.Read(i, this.history[list[i]]);
				}
			}
		}
	}
}
