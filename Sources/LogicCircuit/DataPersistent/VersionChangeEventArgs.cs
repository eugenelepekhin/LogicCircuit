using System;

namespace LogicCircuit.DataPersistent {
	public class VersionChangeEventArgs : EventArgs {
		public int OldVersion { get; private set; }
		public int NewVersion { get; private set; }

		public VersionChangeEventArgs(int oldVersion, int newVersion) {
			this.OldVersion = oldVersion;
			this.NewVersion = newVersion;
		}
	}
}
