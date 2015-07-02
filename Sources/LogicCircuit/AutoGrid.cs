using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace LogicCircuit {
	/// <summary>
	/// Based on: http://www.codeproject.com/Articles/756873/GridEx-for-WPF-Automatic-Placement-of-Children
	/// </summary>
	public class AutoGrid : Grid {
		public static readonly DependencyProperty ColumnWidthsProperty = DependencyProperty.Register("ColumnWidths", typeof(string), typeof(AutoGrid));
		public string ColumnWidths {
			get { return (string)this.GetValue(AutoGrid.ColumnWidthsProperty); }
			set { this.SetValue(AutoGrid.ColumnWidthsProperty, value); }
		}

		public static readonly DependencyProperty RowHeightProperty = DependencyProperty.RegisterAttached("RowHeight", typeof(GridLength?), typeof(AutoGrid));
		public static GridLength? GetRowHeight(DependencyObject obj) {
			return (GridLength?)obj.GetValue(AutoGrid.RowHeightProperty);
		}
		public static void SetRowHeight(DependencyObject obj, GridLength? rowHeight) {
			obj.SetValue(AutoGrid.RowHeightProperty, rowHeight);
		}

		public override void EndInit() {
			this.PlaceChildren();
			this.LinkLabels();
			base.EndInit();
		}

		private void PlaceChildren() {
			int maxColumns = this.DefineColumns();
			int column = 0;
			int row = 0;
			foreach(UIElement child in this.Children) {
				int desiredColumn = AutoGrid.DesiredColumn(child);
				Debug.Assert(-1 <= desiredColumn && desiredColumn < maxColumns);
				if(0 <= desiredColumn) {
					if(desiredColumn < column) {
						row++;
					}
					column = desiredColumn;
				} else if(maxColumns <= column) {
					column = 0;
					row++;
				}
				if(this.RowDefinitions.Count <= row) {
					this.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
				}
				if(desiredColumn != column) {
					child.SetValue(Grid.ColumnProperty, column);
				}
				child.SetValue(Grid.RowProperty, row);
				int columnSpan = (int)child.GetValue(Grid.ColumnSpanProperty);
				Debug.Assert(column + columnSpan <= maxColumns);
				column += columnSpan;
				this.UpdateRowHeight(child);
			}
		}

		private void LinkLabels() {
			for(int i = 0; i < this.Children.Count; i++) {
				UIElement child = this.Children[i];
				Label label = child as Label;
				if(label != null && i + 1 < this.Children.Count && !AutoGrid.WasSet(label.Target) && label.GetBindingExpression(Label.TargetProperty) == null) {
					UIElement next = this.Children[i + 1];
					if(!(next is Panel) && !(next is GroupBox) && next.Focusable &&
						(int)label.GetValue(Grid.RowProperty) == (int)next.GetValue(Grid.RowProperty) &&
						(int)label.GetValue(Grid.ColumnProperty) == 0 &&
						(int)next.GetValue(Grid.ColumnProperty) == 1
					) {
						label.Target = next;
					}
				}
			}
		}

		private static int DesiredColumn(UIElement child) {
			object column = child.ReadLocalValue(Grid.ColumnProperty);
			if(AutoGrid.WasSet(column)) {
				return (int)column;
			}
			return -1;
		}

		private int DefineColumns() {
			Debug.Assert(this.ColumnDefinitions.Count == 0);
			Debug.Assert(this.RowDefinitions.Count == 0);
			int count = 0;
			foreach(GridLength gridLength in AutoGrid.ParseColumnWidths(this.ColumnWidths)) {
				this.ColumnDefinitions.Add(new ColumnDefinition() { Width = gridLength });
				count++;
			}
			return count;
		}

		private static IEnumerable<GridLength> ParseColumnWidths(string widths) {
			if(!string.IsNullOrWhiteSpace(widths)) {
				GridLengthConverter converter = new GridLengthConverter();
				foreach(string text in widths.Split(';').Where(str => !string.IsNullOrWhiteSpace(str)).Select(str => str.Trim())) {
					yield return (GridLength)converter.ConvertFromInvariantString(text);
				}
			} else {
				yield return GridLength.Auto;
				yield return new GridLength(1, GridUnitType.Star);
			}
		}

		private static bool WasSet(object obj) {
			return obj != null && obj != DependencyProperty.UnsetValue;
		}

		private void UpdateRowHeight(DependencyObject child) {
			object objHeight = child.GetValue(AutoGrid.RowHeightProperty);
			if(AutoGrid.WasSet(objHeight)) {
				GridLength rowHeight = (GridLength)objHeight;
				RowDefinition row = this.RowDefinitions[(int)child.GetValue(Grid.RowProperty)];
				if(rowHeight != row.Height) {
					row.Height = rowHeight;
				}
			} else {
				TextBox textBox = child as TextBox;
				if((textBox != null && textBox.AcceptsReturn && !(0 < textBox.Height || 1 < textBox.MinLines || textBox.MaxLines < int.MaxValue)) ||
					// Do not replace ListBox, ListView,... with ItemsControl. As too many of controls are ItemsControls.
					child is GroupBox || child is ListBox || child is ListView || child is RichTextBox || child is DataGrid || child is TreeView
				) {
					RowDefinition row = this.RowDefinitions[(int)child.GetValue(Grid.RowProperty)];
					if(row.Height == GridLength.Auto) {
						row.Height = new GridLength(1, GridUnitType.Star);
					}
				}
			}
		}
	}
}
