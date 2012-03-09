using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for DialogLedMatrix.xaml
	/// </summary>
	public partial class DialogLedMatrix : Window {

		private SettingsWindowLocationCache windowLocation;
		public SettingsWindowLocationCache WindowLocation { get { return this.windowLocation ?? (this.windowLocation = new SettingsWindowLocationCache(Settings.User, this)); } }

		public LedMatrix LedMatrix { get; private set; }
		public int MatrixType { get; set; }
		public int Rows { get; set; }
		public int Columns { get; set; }

		public DialogLedMatrix(LedMatrix ledMatrix) {
			this.LedMatrix = ledMatrix;
			this.MatrixType = (int)this.LedMatrix.MatrixType;
			this.Rows = this.LedMatrix.Rows;
			this.Columns =this.LedMatrix.Columns;
			this.DataContext = this;
			this.InitializeComponent();
		}

		private void ButtonOkClick(object sender, RoutedEventArgs e) {

		}
	}
}
