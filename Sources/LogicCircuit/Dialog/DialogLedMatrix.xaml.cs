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
		public int CellType { get; set; }
		public int Rows { get; set; }
		public int Columns { get; set; }
		public string Note { get; set; }

		public DialogLedMatrix(LedMatrix ledMatrix) {
			this.LedMatrix = ledMatrix;
			this.MatrixType = (int)this.LedMatrix.MatrixType;
			this.CellType = (int)this.LedMatrix.CellType;
			this.Rows = this.LedMatrix.Rows;
			this.Columns =this.LedMatrix.Columns;
			this.Note = this.LedMatrix.Note;

			this.DataContext = this;
			this.InitializeComponent();
		}

		private void ButtonOkClick(object sender, RoutedEventArgs e) {
			try {
				LedMatrixType ledMatrixType = (LedMatrixType)this.MatrixType;
				LedMatrixCellType ledMatrixCellType = (LedMatrixCellType)this.CellType;
				string note = this.Note.Trim();

				if(	ledMatrixType != this.LedMatrix.MatrixType ||
					ledMatrixCellType != this.LedMatrix.CellType ||
					this.Rows != this.LedMatrix.Rows ||
					this.Columns != this.LedMatrix.Columns ||
					note != this.LedMatrix.Note
				) {
					this.LedMatrix.CircuitProject.InTransaction(() => {
						this.LedMatrix.MatrixType = ledMatrixType;
						this.LedMatrix.CellType = ledMatrixCellType;
						this.LedMatrix.Rows = this.Rows;
						this.LedMatrix.Columns = this.Columns;
						this.LedMatrix.Note = note;
						this.LedMatrix.UpdatePins();
					});
				} 
				this.Close();
			} catch(Exception exception) {
				App.Mainframe.ReportException(exception);
			}
		}
	}
}
