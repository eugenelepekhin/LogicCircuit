using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;

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
		public IEnumerable<string> History { get; private set; }
		public bool MarkAllowed { get { return this.reads.Length < 1 || this.reads[0] != -1L; } }

		public DialogProbeHistory(FunctionProbe functionProbe) {
			this.functionProbe = functionProbe;
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
					list.Add(this.Hex(pack, width));
				}
			}
			this.History = list;
		}

		private string Hex(long pack, int width) {
			int value = 0;
			for(int i = 0; i < width; i++) {
				switch(this.functionProbe.Unpack(pack, i)) {
				case State.Off:
					return this.Bin(pack, width);
				case State.On0:
					break;
				case State.On1:
					value |= (1 << i);
					break;
				default:
					Tracer.Fail();
					break;
				}
			}
			return Properties.Resources.ProbeHistoryHex(value);
		}

		private string Bin(long pack, int width) {
			char[] text = new char[width];
			for(int i = 0; i < width; i++) {
				text[i] = CircuitFunction.ToChar(this.functionProbe.Unpack(pack, i));
			}
			Array.Reverse(text);
			return new string(text);
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
