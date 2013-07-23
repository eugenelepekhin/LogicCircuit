using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Windows.Threading;

namespace LogicCircuit {
	public class CircuitRunner {

		public const int HistorySize = 100;

		public Editor Editor { get; private set; }
		private CircuitMap RootMap { get; set; }
		public IEnumerable<CircuitMap> Root { get { yield return this.RootMap; } }

		public CircuitMap VisibleMap {
			get {
				if(this.RootMap != null) {
					return this.RootMap.Visible;
				}
				return null;
			}
			set {
				Tracer.Assert(this.RootMap != null);
				this.RootMap.Visible = value;
			}
		}

		public bool HasProbes { get; private set; }

		public CircuitState CircuitState { get; private set; }
		private bool isMaxSpeed = false;
		private int flipCount = 0;
		private double actualFrequency = 0;
		private double lastActualFrequency = 0;

		private Thread evaluationThread;
		private Thread refreshingThread;
		private AutoResetEvent evaluationGate;
		private AutoResetEvent refreshingGate;

		private volatile bool running = false;
		private volatile bool refreshing = false;
		private volatile bool updatingUI = false;

		private SettingsBoolCache oscilloscoping;
		public bool Oscilloscoping {
			get { return this.oscilloscoping.Value; }
			set { this.oscilloscoping.Value = value; }
		}
		public DialogOscilloscope DialogOscilloscope { get; set; }
		public Oscilloscope Oscilloscope { get; set; }

		public CircuitRunner(Editor editor) {
			this.Editor = editor;
			this.isMaxSpeed = this.Editor.IsMaximumSpeed;
			this.oscilloscoping = new SettingsBoolCache(Settings.Session, "Oscilloscoping" + this.Editor.Project.ProjectId.ToString(), false);
			this.RootMap = new CircuitMap(this.Editor.Project.LogicalCircuit);
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

		public bool IsRunning { get { return this.evaluationThread != null && this.evaluationThread.IsAlive && this.running; } }

		private static int HalfPeriod(int frequency) {
			return 500 / frequency;
		}

		public void ShowOscilloscope() {
			if(this.DialogOscilloscope == null) {
				this.DialogOscilloscope = new DialogOscilloscope(this);
				this.DialogOscilloscope.Owner = this.Editor.Mainframe;
				this.DialogOscilloscope.Show();
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		private void Run() {
			PropertyChangedEventHandler editorPropertyChanged = null;
			try {
				this.running = true;
				this.Editor.Mainframe.Status = Properties.Resources.PowerOn;

				this.CircuitState = this.RootMap.Apply(CircuitRunner.HistorySize);
				this.CircuitState.FunctionUpdated += new EventHandler(this.OnFunctionUpdated);
				this.HasProbes = this.CircuitState.HasProbes;

				this.Editor.Mainframe.Dispatcher.Invoke(new Action(() => this.RootMap.TurnOn()));
				this.Editor.ActualFrequency = this.Editor.Frequency;

				if(this.Oscilloscoping && this.CircuitState.HasProbes) {
					Tracer.Assert(this.DialogOscilloscope == null);
					this.Editor.Mainframe.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(this.ShowOscilloscope));
				} else {
					this.Oscilloscoping = false;
				}

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
									if(this.isMaxSpeed) {
										AutoResetEvent resetEvent = this.evaluationGate;
										if(resetEvent != null) {
											resetEvent.Set();
										}
									}
									break;
								}
							});
							this.Editor.PropertyChanged += editorPropertyChanged;
							this.refreshingThread.Start();
							this.flipCount = 1;
							timer.Start();
							this.RunCircuit();
						}
					}
				}
			} catch(ThreadAbortException) {
				// Do nothing
			} catch(Exception exception) {
				this.Editor.Mainframe.ReportException(exception);
			} finally {
				this.running = false;
				if(this.refreshingThread != null) {
					this.refreshingThread.Abort();
				}
				if(editorPropertyChanged != null) {
					this.Editor.PropertyChanged -= editorPropertyChanged;
				}
				if(this.DialogOscilloscope != null) {
					this.DialogOscilloscope.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
						new Action(this.DialogOscilloscope.Close)
					);
				}
				if(this.RootMap != null) {
					while(this.updatingUI);
					this.Editor.Mainframe.Dispatcher.BeginInvoke(
						new Action(() => this.RootMap.Circuit.CircuitProject.InTransaction(() => this.RootMap.TurnOff())),
						DispatcherPriority.ApplicationIdle
					);
				}
				this.Editor.Mainframe.Status = Properties.Resources.PowerOff;
			}
		}

		private void RunCircuit() {
			bool singleCPU = (Environment.ProcessorCount < 2);
			bool hasProbes = this.CircuitState.HasProbes;
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			long tickCount = 0;
			for(;;) {
				bool flipClock = (0 < this.flipCount);
				bool maxSpeed = this.isMaxSpeed;
				if(!this.CircuitState.Evaluate(maxSpeed || flipClock)) {
					break;
				}
				if(maxSpeed || flipClock) {
					tickCount++;
					long ms = stopwatch.ElapsedMilliseconds;
					if(1500 <= ms) {
						this.actualFrequency = Math.Round((double)(tickCount * 500L) / ms, 1);
						tickCount = 0;
						stopwatch.Restart();
					}
				}
				if(!this.refreshing) {
					this.refreshingGate.Set();
				}
				if(this.Oscilloscoping) {
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
				}
				if(!maxSpeed && Interlocked.Decrement(ref this.flipCount) <= 0) {
					if(this.flipCount < 0) {
						this.flipCount = 0;
					}
					this.evaluationGate.WaitOne();
				} else if(maxSpeed && singleCPU) {
					// On single CPU machine when circuit is running on max speed lets yield for UI thread to refresh displays.
					Thread.Yield();
				}
			}
			this.Editor.Mainframe.ErrorMessage(Properties.Resources.Oscillation);
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

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		private void OnFunctionUpdated(object sender, EventArgs e) {
			try {
				this.evaluationGate.Set();
			} catch(ThreadAbortException) {
			} catch(ObjectDisposedException) {
			} catch(Exception exception) {
				this.Editor.Mainframe.ReportException(exception);
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		private void MonitorUI() {
			try {
				Action refresh = new Action(this.RefreshUI);
				while(this.running && this.refreshingGate != null && this.refreshingThread != null && this.evaluationThread != null) {
					this.refreshingGate.WaitOne();
					if(this.running && this.refreshingThread != null && this.evaluationThread != null && !this.refreshing) {
						this.refreshing = true;
						Thread.MemoryBarrier();
						this.Editor.Mainframe.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, refresh);
					}
				}
			} catch(ThreadAbortException) {
			} catch(Exception exception) {
				this.Stop();
				this.Editor.Mainframe.ReportException(exception);
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		private void RefreshUI() {
			try {
				if(this.running) {
					this.updatingUI = true;
					Thread.MemoryBarrier();
					this.VisibleMap.Redraw(false);
					if(this.actualFrequency != this.lastActualFrequency) {
						this.lastActualFrequency = this.actualFrequency;
						this.Editor.ActualFrequency = this.actualFrequency;
					}
				}
			} catch(ThreadAbortException) {
			} catch(Exception exception) {
				this.Editor.Mainframe.ReportException(exception);
			} finally {
				this.refreshing = false;
				this.updatingUI = false;
				Thread.MemoryBarrier();
			}
		}
	}
}
