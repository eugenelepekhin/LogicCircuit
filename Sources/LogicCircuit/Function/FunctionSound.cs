using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Windows;

namespace LogicCircuit {
	[SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
	public class FunctionSound : Probe, IFunctionVisual {
		private const string DefaultSound = "LogicCircuit.Properties.default.wav";

		private SoundPlayer player;
		private bool looping;

		public override string ReportName { get { return Properties.Resources.NameSound; } }

		// not used here as Visual is used to turn off sound on power off.
		public bool Invalid { get; set; }

		public FunctionSound(CircuitState circuitState, int parameter, Sound sound) : base(circuitState, parameter) {
			string data = sound.Data;
			if(string.IsNullOrEmpty(data)) {
				this.player = new SoundPlayer(Assembly.GetExecutingAssembly().GetManifestResourceStream(FunctionSound.DefaultSound));
				player.LoadAsync();
				this.looping = sound.Looping;
			}
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
			// nothing to do
		}

		public void TurnOff() {
			this.TurnSoundOff();
			this.player.Dispose();
		}

		public void Redraw() {
			// nothing to do
		}

		private void TurnSoundOn() {
			if(this.looping) {
				this.player.PlayLooping();
			} else {
				this.player.Play();
			}
		}

		private void TurnSoundOff() {
			this.player.Stop();
		}
	}
}
