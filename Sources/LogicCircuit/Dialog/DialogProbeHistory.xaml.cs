using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for DialogProbeHistory.xaml
	/// </summary>
	public partial class DialogProbeHistory : Window, INotifyPropertyChanged {

		public event PropertyChangedEventHandler PropertyChanged;

		private SettingsWindowLocationCache windowLocation;
		public SettingsWindowLocationCache WindowLocation { get { return this.windowLocation ?? (this.windowLocation = new SettingsWindowLocationCache(Settings.User, this)); } }

		private FunctionProbe functionProbe;
		private long[] reads;
		public int BitWidth { get; private set; }
		public IEnumerable<string> History { get; private set; }
		public bool MarkAllowed { get { return this.reads.Length < 1 || this.reads[0] != -1L; } }

		public DialogProbeHistory(FunctionProbe functionProbe) {
			this.functionProbe = functionProbe;
			this.BitWidth = this.functionProbe.BitWidth;
			this.RefreshHistory();
			this.DataContext = this;
			this.InitializeComponent();
		}

		private void RefreshHistory() {
			List<string> list = new List<string>();
			int width = this.functionProbe.BitWidth;
			this.reads = this.functionProbe.Read();
			Array.Reverse(this.reads);
			foreach(long pack in this.reads) {
				if(pack == -1L) {
					list.Add(Properties.Resources.ProbeHistoryMark);
				} else {
					list.Add(CircuitFunction.ToText(this.Unpack(pack, width), false));
				}
			}
			this.History = list;
		}

		private IEnumerable<State> Unpack(long pack, int width) {
			for(int i = 0; i < width; i++) {
				yield return this.functionProbe.Unpack(pack, i);
			}
		}

		private void NotifyPropertyChanged(string propertyName) {
			PropertyChangedEventHandler handler = this.PropertyChanged;
			if(handler != null) {
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		private void ButtonMarkClick(object sender, RoutedEventArgs e) {
			try {
				if(this.MarkAllowed) {
					this.functionProbe.Mark();
					this.RefreshHistory();
					this.NotifyPropertyChanged("History");
					this.NotifyPropertyChanged("MarkAllowed");
				}
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}
	}
}
