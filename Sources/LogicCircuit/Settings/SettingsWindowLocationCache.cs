﻿using System;
using System.Windows;

namespace LogicCircuit {
	public class SettingsWindowLocationCache {
		private readonly SettingsDoubleCache x;
		private readonly SettingsDoubleCache y;
		private readonly SettingsDoubleCache width;
		private readonly SettingsDoubleCache height;
		private readonly SettingsWindowStateCache state;

		public SettingsWindowLocationCache(Settings settings, Window window, double width, double height) {
			if(double.IsNaN(width)) {
				width = window.Width;
			}
			if(double.IsNaN(height)) {
				height = window.Height;
			}
			string windowName = window.GetType().Name;
			this.x = new SettingsDoubleCache(settings, windowName + ".X", 0, SystemParameters.VirtualScreenWidth - 30, window.Left, true);
			this.y = new SettingsDoubleCache(settings, windowName + ".Y", 0, SystemParameters.VirtualScreenHeight - 30, window.Top, true);
			this.width = new SettingsDoubleCache(settings, windowName + ".Width", window.MinWidth, window.MaxWidth, width, true);
			this.height = new SettingsDoubleCache(settings, windowName + ".Height", window.MinHeight, window.MaxHeight, height, true);
			this.state = new SettingsWindowStateCache(settings, windowName + ".WindowState");
		}

		public SettingsWindowLocationCache(Settings settings, Window window) : this(settings, window, 0, 0) {
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

		public WindowState WindowState {
			get { return this.state.Value; }
			set { this.state.Value = value; }
		}

		private sealed class SettingsWindowStateCache : SettingsEnumCache<WindowState> {
			public SettingsWindowStateCache(Settings settings, string key) : base(settings, key, WindowState.Normal) {
				if(this.Value == WindowState.Minimized) {
					this.Value = WindowState.Normal;
				}
			}
		}
	}
}
