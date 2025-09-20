﻿using System;

namespace DataPersistent {
	public class RolledBackEventArgs : EventArgs {

		public int Version { get; private set; }

		public RolledBackEventArgs(int version) {
			this.Version = version;
		}
	}
}
