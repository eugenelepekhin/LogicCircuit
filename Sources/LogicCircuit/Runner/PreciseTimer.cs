﻿using System;
using System.Timers;

namespace LogicCircuit {
	internal sealed class PreciseTimer : IDisposable {

		private const int CicleCount = 32;

		private readonly Action action;
		private readonly Timer timer;

		private TimerState? state;

		public PreciseTimer(Action action, int period) {
			this.action = action;
			this.Period = period;
			this.timer = new Timer(period);
			this.timer.Elapsed += new ElapsedEventHandler(this.TimerElapsed);
		}

		public int Period {
			get { return (int)(this.state!.period / TimeSpan.TicksPerMillisecond); }
			set {
				Tracer.Assert(3 < value && value <= 10000);
				if(this.state == null || this.state.period != value * TimeSpan.TicksPerMillisecond) {
					TimerState s = new TimerState {
						period = value * TimeSpan.TicksPerMillisecond,
						cicle = 0,
						interval = int.MaxValue / 2,
						start = DateTime.UtcNow.Ticks
					};
					this.state = s;
				}
			}
		}

		public void Start() {
			if(!this.timer.Enabled) {
				TimerState s = this.state!;
				s.cicle = 0;
				s.start = DateTime.UtcNow.Ticks;
				this.timer.Start();
			}
		}

		//public void Stop() {
		//    if(this.timer.Enabled) {
		//        this.timer.Stop();
		//    }
		//}

		private void TimerElapsed(object? sender, ElapsedEventArgs e) {
			TimerState s = this.state!;
			s.cicle++;
			if((s.cicle % PreciseTimer.CicleCount) == 1) {
				int i = Math.Max(1,
					(int)((s.period + (s.period * s.cicle -  DateTime.UtcNow.Ticks + s.start) / PreciseTimer.CicleCount) / TimeSpan.TicksPerMillisecond)
				);
				if(2 < Math.Abs(i - s.interval)) {
					this.timer.Interval = s.interval = i;
				}
			}
			this.action();
		}

		private sealed class TimerState {
			public long period;
			public long start;
			public long cicle;
			public int interval;
		}

		public void Dispose() {
			this.timer.Dispose();
		}
	}
}
