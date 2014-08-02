using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace LogicCircuit {
	public class GridLengthAnimation : AnimationTimeline {

		protected override Freezable CreateInstanceCore() {
			return new GridLengthAnimation();
		}

		public static readonly DependencyProperty FromProperty = DependencyProperty.Register("From", typeof(GridLength), typeof(GridLengthAnimation));
		public static readonly DependencyProperty ToProperty = DependencyProperty.Register("To", typeof(GridLength), typeof(GridLengthAnimation));

		public GridLength From {
			get { return (GridLength)this.GetValue(GridLengthAnimation.FromProperty); }
			set { this.SetValue(GridLengthAnimation.FromProperty, value); }
		}

		public GridLength To {
			get { return (GridLength)this.GetValue(GridLengthAnimation.ToProperty); }
			set { this.SetValue(GridLengthAnimation.ToProperty, value); }
		}

		public override Type TargetPropertyType {
			get { return typeof(GridLength); }
		}

		public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock) {
			double from = this.From.Value;
			double to = this.To.Value;
			double? clock = animationClock.CurrentProgress;
			double value;
			if(clock.HasValue) {
				if(from < to) {
					value = clock.Value * (to - from ) + from;
				} else {
					value = (1 - clock.Value) * (from - to) + to;
				}
			} else {
				value = (from + to) / 2;
			}
			//Tracer.FullInfo("GridLengthAnimation.GetCurrentValue", "from={0}, to={1}, clock={2}, value={3}", from, to, clock, value);
			return new GridLength(value, GridUnitType.Star);
		}
	}
}
