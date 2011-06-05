using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace LogicCircuit {
	public static class CustomCommands {

		private static RoutedUICommand Create(string name, params InputGesture[] gestures) {
			return new RoutedUICommand(
				Resources.ResourceManager.GetString(name, Resources.Culture),
				name,
				typeof(CustomCommands),
				new InputGestureCollection(gestures)
			);
		}

		public static readonly RoutedUICommand FileImport = Create("CommandFileFileImport");
		public static readonly RoutedUICommand FileExportImage = Create("CommandFileExportImage");

		public static readonly RoutedUICommand EditSelectAllWires = Create("CommandEditSelectAllWires");
		public static readonly RoutedUICommand EditSelectFreeWires = Create("CommandEditSelectFreeWires");
		public static readonly RoutedUICommand EditSelectFloatingSymbols = Create("CommandEditSelectFloatingSymbols");
		public static readonly RoutedUICommand EditSelectAllButWires = Create("CommandEditSelectAllButWires");
		public static readonly RoutedUICommand EditUnselectAllWires = Create("CommandEditUnselectAllWires");
		public static readonly RoutedUICommand EditUnselectAllButWires = Create("CommandEditUnselectAllButWires");
		public static readonly RoutedUICommand EditSelectAllProbes = Create("CommandEditSelectAllProbes");
		public static readonly RoutedUICommand EditSelectAllProbesWithWire = Create("CommandEditSelectAllProbesWithWire");

		public static readonly RoutedUICommand CircuitProject = Create("CommandCircuitProject");
		public static readonly RoutedUICommand CircuitCurrent = Create("CommandCircuitCurrent");
		public static readonly RoutedUICommand CircuitNew = Create("CommandCircuitNew");
		public static readonly RoutedUICommand CircuitDelete = Create("CommandCircuitDelete");
		public static readonly RoutedUICommand CircuitPower = Create("CommandCircuitPower", new KeyGesture(Key.W, ModifierKeys.Control));

		public static readonly RoutedUICommand ToolsReport = Create("CommandToolsReport");
		public static readonly RoutedUICommand ToolsOscilloscope = Create("CommandToolsOscilloscope");
		public static readonly RoutedUICommand ToolsOptions = Create("CommandToolsOptions");

		public static readonly RoutedUICommand HelpAbout = Create("CommandHelpAbout");
	}
}
