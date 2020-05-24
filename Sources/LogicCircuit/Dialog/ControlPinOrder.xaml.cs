using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace LogicCircuit {
	/// <summary>
	/// Interaction logic for ControlPinOrder.xaml
	/// </summary>
	public partial class ControlPinOrder : UserControl {
		private static readonly DependencyProperty PinListProperty = DependencyProperty.Register(nameof(PinList), typeof(ListCollectionView), typeof(ControlPinOrder));
		public ListCollectionView PinList {
			get => (ListCollectionView)this.GetValue(ControlPinOrder.PinListProperty);
			private set => this.SetValue(ControlPinOrder.PinListProperty, value);
		}

		public LambdaUICommand CommandLeft { get; }
		public LambdaUICommand CommandRight { get; }

		private List<PinOrderDescriptor> list;

		public ControlPinOrder() {
			this.CommandLeft = new LambdaUICommand("_<", o => this.CanLeft(), o => this.Left());
			this.CommandRight = new LambdaUICommand("_>", o => this.CanRight(), o => this.Right());
			this.InitializeComponent();
		}

		public void SetPins(IEnumerable<Pin> pins) {
			this.list = pins.Select(pin => new PinOrderDescriptor(pin)).ToList();
			this.list.Sort(PinOrderDescriptor.Comparer);
			this.PinList = (ListCollectionView)CollectionViewSource.GetDefaultView(this.list);
			this.PinList.CustomSort = (IComparer)PinOrderDescriptor.Comparer;
			this.PinList.CurrentChanged += (sender, e) => this.UpdateCommands();
			this.GotFocus += (sender, e) => this.UpdateCommands();
			this.LostFocus += (sender, e) => this.UpdateCommands();
		}

		public bool HasChanges() => this.list.Any(d => d.Index != d.Circuit.Index);

		public void Update() {
			this.list.ForEach(d => d.Circuit.Index = d.Index);
		}

		public bool IsOrderFixed() => this.list.Any(d => 0 < d.Index);

		public void Reset() {
			this.list.ForEach(d => d.Index = 0);
			this.list.Sort(PinOrderDescriptor.Comparer);
			this.PinList.Refresh();
			this.UpdateCommands();
		}

		public void FixOrder() {
			for(int i = 0; i < this.list.Count; i++) {
				this.list[i].Index = i;
			}
		}

		private void UpdateCommands() {
			this.CommandLeft.NotifyCanExecuteChanged();
			this.CommandRight.NotifyCanExecuteChanged();
		}

		private bool CanLeft() {
			return this.IsKeyboardFocusWithin && 0 < this.PinList.CurrentPosition;
		}

		private bool CanRight() {
			return this.IsKeyboardFocusWithin && this.PinList.CurrentPosition < this.PinList.Count - 1;
		}

		private void Left() {
			int current = this.PinList.CurrentPosition;
			for(int i = 0; i < this.list.Count; i++) {
				this.list[i].Index = i;
			}
			this.list[current - 1].Index = current;
			this.list[current].Index = current - 1;
			this.list.Sort(PinOrderDescriptor.Comparer);
			this.PinList.Refresh();
			this.PinList.MoveCurrentTo(this.list[current - 1]);
			this.listBox.Focus();
			this.UpdateCommands();
		}

		private void Right() {
			int current = this.PinList.CurrentPosition;
			for(int i = 0; i < this.list.Count; i++) {
				this.list[i].Index = i;
			}
			this.list[current].Index = current + 1;
			this.list[current + 1].Index = current;
			this.list.Sort(PinOrderDescriptor.Comparer);
			this.PinList.Refresh();
			this.PinList.MoveCurrentTo(this.list[current + 1]);
			this.listBox.Focus();
			this.UpdateCommands();
		}
	}
}
