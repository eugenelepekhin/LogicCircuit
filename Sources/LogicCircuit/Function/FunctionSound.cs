using System;
using System.Diagnostics.CodeAnalysis;
using System.Media;
using System.Reflection;

namespace LogicCircuit {
	[SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
	public class FunctionSound : Probe, IFunctionVisual {
		private SoundPlayer player;

		// Note that turning sound on and off will happening only from "Run" thread.
		// Turning Visual off will happened on main thread but only after Run thread ends.
		// So overall there is no need for synchronization of incrementing and decrementing this count.
		private static int playCount = 0;

		public override string ReportName { get { return Properties.Resources.NameSound; } }

		// not used here as Visual is used to turn off sound on power off.
		public bool Invalid { get; set; }

		public FunctionSound(CircuitState circuitState, int parameter) : base(circuitState, parameter) {
			this.player = new SoundPlayer(Assembly.GetExecutingAssembly().GetManifestResourceStream("LogicCircuit.Properties.default.wav"));
			this.player.LoadAsync();
		}

		public override bool Evaluate() {
			if(this.GetState()) {
				if(this[0] == State.On1) {
					this.TurnSoundOn();
				} else {
					this.TurnSoundOff();
				}
			}
			return false;
		}

		public void TurnOn() {
			FunctionSound.playCount = 0;
		}

		public void TurnOff() {
			FunctionSound.playCount = 0;
			this.player.Stop();
			this.player.Dispose();
		}

		public void Redraw() {
			// nothing to do
		}

		private void TurnSoundOn() {
			int count = ++FunctionSound.playCount;
			if(count == 1) {
				this.player.PlayLooping();
			}
		}

		private void TurnSoundOff() {
			if(0 < FunctionSound.playCount) {
				int count = --FunctionSound.playCount;
				if(count == 0) {
					this.player.Stop();
				}
				Tracer.Assert(0 <= count);
			}
		}
	}
}
