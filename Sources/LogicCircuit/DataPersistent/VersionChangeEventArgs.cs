using System;

namespace DataPersistent {
	public class VersionChangeEventArgs : EventArgs {
		public int OldVersion { get; }
		public int NewVersion { get; }

		public VersionChangeEventArgs(int oldVersion, int newVersion) {
			this.OldVersion = oldVersion;
			this.NewVersion = newVersion;
		}
	}
}
