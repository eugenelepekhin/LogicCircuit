using System;
using System.Windows;

namespace LogicCircuit {
	public class SettingsWindowLocationCache {
		private SettingsDoubleCache x;
		private SettingsDoubleCache y;
		private SettingsDoubleCache width;
		private SettingsDoubleCache height;
		private SettingsEnumCache<WindowState> state;

		public SettingsWindowLocationCache(Settings settings, string windowName) {
			this.x = new SettingsDoubleCache(settings, windowName + ".X", 0, SystemParameters.VirtualScreenWidth - 30, 0);
			this.y = new SettingsDoubleCache(settings, windowName + ".Y", 0, SystemParameters.VirtualScreenHeight - 30, 0);
			this.width = new SettingsDoubleCache(settings, windowName + ".Width", 0, SystemParameters.VirtualScreenWidth, 0);
			this.height = new SettingsDoubleCache(settings, windowName + ".Height", 0, SystemParameters.VirtualScreenHeight, 0);
			this.state = new SettingsEnumCache<WindowState>(settings, windowName + ".WindowState", WindowState.Normal);
		}

		public SettingsWindowLocationCache(Settings settings, Window window) : this(settings, window.GetType().Name) {
		}

		public double X {
			get { return this.x.Value; }
			set { this.x.Value = value; }
		}

		public double Y {
			get { return this.y.Value; }
			set { this.y.Value = value; }
		}

		public double Width {
			get { return this.width.Value; }
			set { this.width.Value = value; }
		}

		public double Height {
			get { return this.height.Value; }
			set { this.height.Value = value; }
		}

		private WindowState Normalize(WindowState state) {
			switch(state) {
			case WindowState.Normal:
			case WindowState.Maximized:
				return state;
			default:
				return WindowState.Normal;
			}
		}

		public WindowState WindowState {
			get { return this.Normalize(this.state.Value); }
			set { this.state.Value = this.Normalize(value); }
		}
	}
}
