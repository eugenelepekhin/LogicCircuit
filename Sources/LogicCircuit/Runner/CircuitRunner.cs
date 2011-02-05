using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Threading;
using System.Windows.Threading;
using System.Diagnostics;

namespace LogicCircuit {
	public class CircuitRunner : INotifyPropertyChanged {

		public const int HistorySize = 100;

		public event PropertyChangedEventHandler PropertyChanged;

		public Editor Editor { get; private set; }
		private CircuitMap RootMap { get; set; }
		public IEnumerable<CircuitMap> Root { get { yield return this.RootMap; } }

		public CircuitMap VisibleMap { get; set; }

		public bool HasProbes { get; private set; }

		private CircuitState CircuitState { get; set; }
		private bool isMaxSpeed = false;
		private int flipCount = 0;

		private Thread evaluationThread;
		private Thread refreshingThread;
		private AutoResetEvent evaluationGate;
		private AutoResetEvent refreshingGate;

		private volatile bool refreshing = false;

		public CircuitRunner(Editor editor) {
			this.Editor = editor;
		}

		public void Start() {
			Tracer.Assert(this.evaluationThread == null);
			this.evaluationThread = new Thread(new ThreadStart(this.Run));
			this.evaluationThread.IsBackground = true;
			this.evaluationThread.Name = "EvaluationThread";
			this.evaluationThread.Priority = ThreadPriority.Normal;

			this.evaluationThread.Start();
		}

		public void Stop() {
			this.evaluationThread.Abort();
		}

		public bool IsRunning { get { return this.evaluationThread != null && this.evaluationThread.IsAlive; } }

		private static int HalfPeriod(int frequency) {
			return 500 / frequency;
		}

		private void Run() {
			PropertyChangedEventHandler editorPropertyChanged = null;
			try {
				this.Editor.Mainframe.Status = Resources.PowerOn;

				CircuitMap root = new CircuitMap(this.Editor.Project.LogicalCircuit);
				this.CircuitState = root.Apply(CircuitRunner.HistorySize);
				this.RootMap = this.VisibleMap = root;
				this.CircuitState.FunctionUpdated += new EventHandler(this.OnFunctionUpdated);
				this.HasProbes = this.CircuitState.HasProbes;

				this.Editor.Mainframe.Dispatcher.Invoke(new Action(() => {
					this.RootMap.TurnOn();
					this.Editor.Mainframe.NotifyPropertyChanged(this.PropertyChanged, this, "Root");
				}));

				this.refreshingThread = new Thread(new ThreadStart(this.MonitorUI));
				this.refreshingThread.IsBackground = true;
				this.refreshingThread.Name = "RefreshingThread";
				this.refreshingThread.Priority = ThreadPriority.BelowNormal;


				using(this.evaluationGate = new AutoResetEvent(false)) {
					using(this.refreshingGate = new AutoResetEvent(false)) {
						using (PreciseTimer timer = new PreciseTimer(this.TimerTick, CircuitRunner.HalfPeriod(this.Editor.Project.Frequency))) {
							editorPropertyChanged = new PropertyChangedEventHandler((s, e) => {
								switch(e.PropertyName) {
								case "Frequency":
									timer.Period = CircuitRunner.HalfPeriod(this.Editor.Project.Frequency);
									break;
								case "IsMaximumSpeed":
									this.isMaxSpeed = this.Editor.Project.IsMaximumSpeed;
									break;
								}
							});
							this.Editor.PropertyChanged += editorPropertyChanged;
							this.refreshingThread.Start();
							this.flipCount = 1;
							timer.Start();
							this.Run(timer);
						}
					}
				}
			} catch(ThreadAbortException) {
				// Do nothing
			} catch(Exception exception) {
				this.Editor.Mainframe.ReportException(exception);
			} finally {
				if(editorPropertyChanged != null) {
					this.Editor.PropertyChanged -= editorPropertyChanged;
				}
				if(this.refreshingThread != null) {
					this.refreshingThread.Abort();
				}
				this.Editor.Mainframe.Dispatcher.Invoke(new Action(() => this.RootMap.TurnOff()));
				this.Editor.Mainframe.Status = Resources.PowerOff;
			}
		}

		private void Run(PreciseTimer timer) {
			int slownesCount = 0;
			int slownesMax = 2;
			bool notifyPerf = false;
			Stopwatch stopwatch = new Stopwatch();
			for(;;) {
				bool flipClock = (0 < this.flipCount);
				bool maxSpeed = this.Editor.Project.IsMaximumSpeed;
				if(!maxSpeed) {
					stopwatch.Reset();
					stopwatch.Start();
				}
				if(!this.CircuitState.Evaluate(maxSpeed || flipClock)) {
					break;
				}
				if(!this.refreshing) {
					this.refreshingGate.Set();
				}
				if(!maxSpeed) {
					if(timer.Period < stopwatch.Elapsed.TotalMilliseconds) {
						slownesCount++;
						if(slownesMax < slownesCount) {
							slownesCount = slownesMax;
							if(!notifyPerf) {
								notifyPerf = true;
								//this.Editor.Mainframe.NotifyPerformance(true);
							}
						}
					} else {
						if(0 < slownesCount) {
							slownesCount--;
							if(slownesCount == 0) {
								notifyPerf = false;
								//this.Editor.Mainframe.NotifyPerformance(false);
							}
						}
					}
				}
				/*if(this.Oscilloscoping) {
					if(flipClock && hasProbes) {
						foreach(FunctionProbe probe in this.CircuitState.Probes) {
							probe.Tick();
						}
					}
					if(this.Oscilloscope != null) {
						foreach(FunctionProbe probe in this.CircuitState.Probes) {
							this.Oscilloscope.Read(probe);
						}
						this.Oscilloscope = null;
					}
				}*/
				if(!maxSpeed && Interlocked.Decrement(ref this.flipCount) <= 0) {
					if(this.flipCount < 0) {
						this.flipCount = 0;
					}
					this.evaluationGate.WaitOne();
				}
			}
			//this.Editor.Mainframe.ErrorMessage(Resources.Oscillation);
		}

		private void TimerTick() {
			if(!this.isMaxSpeed) {
				AutoResetEvent e = this.evaluationGate;
				if(e != null) {
					Interlocked.Increment(ref this.flipCount);
					e.Set();
				}
			}
		}

		private void OnFunctionUpdated(object sender, EventArgs e) {
			try {
				this.evaluationGate.Set();
			} catch(ThreadAbortException) {
			} catch(Exception exception) {
				this.Editor.Mainframe.ReportException(exception);
			}
		}

		private void MonitorUI() {
			try {
				while(this.refreshingGate != null && this.refreshingThread != null && this.evaluationThread != null) {
					this.refreshingGate.WaitOne();
					if(this.refreshingThread != null && this.evaluationThread != null && !this.refreshing) {
						this.refreshing = true;
						Thread.MemoryBarrier();
						this.Editor.Mainframe.Dispatcher.BeginInvoke(DispatcherPriority.SystemIdle, new Action(this.RefreshUI));
					}
				}
			} catch(ThreadAbortException) {
			} catch(Exception exception) {
				this.Stop();
				this.Editor.Mainframe.ReportException(exception);
			}
		}

		private void RefreshUI() {
			try {
				if(this.CircuitState != null) {
					IEnumerable<IFunctionVisual> invalidVisuals = this.CircuitState.InvalidVisuals();
					if(invalidVisuals != null) {
						foreach(IFunctionVisual function in invalidVisuals) {
							if(this.VisibleMap.IsVisible(function)) {
								function.Redraw();
							}
						}
					}
				}
			} catch(ThreadAbortException) {
			} catch(Exception exception) {
				this.Editor.Mainframe.ReportException(exception);
			} finally {
				this.refreshing = false;
				Thread.MemoryBarrier();
			}
		}
	}
}
