using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace LogicCircuit {
	public class DisplayCanvas : Canvas {
		private Dictionary<CircuitSymbol, FrameworkElement> symbolMap = new Dictionary<CircuitSymbol, FrameworkElement>();

		[SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		public void AddDisplay(CircuitSymbol symbol, FrameworkElement glyph) {
			if(symbol.Circuit is LogicalCircuit) {
				Tracer.Assert(glyph is DisplayCanvas);
				this.symbolMap.Add(symbol, glyph);
			} else {
				Tracer.Assert(!(glyph is DisplayCanvas));
				FrameworkElement probeView = (FrameworkElement)glyph.FindName("ProbeView");
				Tracer.Assert(probeView != null);
				this.symbolMap.Add(symbol, probeView);
			}
			this.Children.Add(glyph);
		}

		public FrameworkElement DisplayOf(IList<CircuitSymbol> symbol) {
			FrameworkElement glyph = null;
			int index = symbol.Count - 1;
			while(0 <= index && !this.symbolMap.TryGetValue(symbol[index], out glyph)) {
				index--;
			}
			Tracer.Assert(0 <= index && glyph != null);

			DisplayCanvas canvas = this;
			while(0 < index) {
				canvas = (DisplayCanvas)canvas.symbolMap[symbol[index]];
				index--;
			}
			return canvas.symbolMap[symbol[0]];
		}
	}
}
