using System;
using System.Media;
using System.Reflection;

namespace LogicCircuit {
	public class FunctionSound : Probe, IFunctionVisual {

		// SoundPlayer does not support multiple WAVs to played, so play only one sound, no matter how many sound function is running.
		private static SoundPlayer player;

		// Note that turning sound on and off will happening only from "Run" thread.
		// Turning Visual off will happened on main thread but only after Run thread ends.
		// So overall there is no need for synchronization of incrementing and decrementing this count.
		private static int playCount = 0;

		public override string ReportName { get { return Properties.Resources.NameSound; } }

		// not used here as Visual is used to turn off sound on power off.
		public bool Invalid { get; set; }

		public FunctionSound(CircuitState circuitState, int parameter) : base(circuitState, parameter) {
			if(FunctionSound.player == null) {
				FunctionSound.player = new SoundPlayer(Assembly.GetExecutingAssembly().GetManifestResourceStream("LogicCircuit.Properties.default.wav"));
				FunctionSound.player.LoadAsync();
			}
			FunctionSound.playCount = 0;
		}

		public override bool Evaluate() {
			if(this.GetState()) {
				if(this[0] == State.On1) {
					int count = ++FunctionSound.playCount;
					if(count == 1) {
						FunctionSound.player.PlayLooping();
					}
				} else if(0 < FunctionSound.playCount) {
					int count = --FunctionSound.playCount;
					if(count == 0) {
						FunctionSound.player.Stop();
					}
					Tracer.Assert(0 <= count);
				}
			}
			return false;
		}

		public void TurnOn() {
			// Nothing to do
		}

		public void TurnOff() {
			FunctionSound.playCount = 0;
			SoundPlayer p = FunctionSound.player;
			FunctionSound.player = null;
			if(p != null) {
				p.Stop();
				p.Dispose();
			}
		}

		public void Redraw() {
			// nothing to do
		}
	}
}
