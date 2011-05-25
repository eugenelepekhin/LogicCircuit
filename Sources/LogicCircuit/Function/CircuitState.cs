//#define DUMP_STATE

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;

namespace LogicCircuit {
	public class CircuitState {

		public event EventHandler FunctionUpdated;

		private int randomSeed;
		private DirtyList dirty;
		private List<State> state = new List<State>();
		private List<CircuitFunction> terminal = new List<CircuitFunction>();
		private List<List<CircuitFunction>> dependant = new List<List<CircuitFunction>>();
		private HashSet<CircuitFunction> updated = new HashSet<CircuitFunction>();
		private List<FunctionClock> clockList = new List<FunctionClock>();
		private List<FunctionProbe> probeList = new List<FunctionProbe>();
		private volatile bool invalidating = false;
		private volatile HashSet<IFunctionVisual> invalid = new HashSet<IFunctionVisual>();
		private HashSet<IFunctionVisual> invalidEmpty = new HashSet<IFunctionVisual>();

		public CircuitState() {
			this.randomSeed = (int)DateTime.UtcNow.Ticks;
			//this.randomSeed = 196081105;
			Tracer.FullInfo("CircuitState", "CircuitState.randomSeed=" + this.randomSeed.ToString(CultureInfo.InvariantCulture));
			this.dirty = new DirtyList(this.randomSeed);
		}

		public int Count { get { return this.state.Count; } }

		public State this[int index] {
			get { return this.state[index]; }
			set { this.state[index] = value; }
		}

		public int ReserveState(int count) {
			int min = this.state.Count;
			for(int i = 0; i < count; i++) {
				this.state.Add(State.Off);
				this.dependant.Add(new List<CircuitFunction>());
			}
			return min;
		}

		public void DefineFunction(CircuitFunction function) {
			int count = 0;
			foreach(int parameter in function.Parameter) {
				this.dependant[parameter].Add(function);
				count++;
			}
			if(count <= 0) {
				this.terminal.Add(function);
				this.dirty.Add(function);
				FunctionClock clock = function as FunctionClock;
				if(clock != null) {
					this.clockList.Add(clock);
				}
			} else {
				FunctionProbe probe = function as FunctionProbe;
				if(probe != null) {
					this.probeList.Add(probe);
				}
			}
		}

		public void MarkUpdated(CircuitFunction function) {
			bool wasUpdated;
			lock(this.updated) {
				wasUpdated = this.updated.Add(function);
			}
			if(wasUpdated && this.FunctionUpdated != null) {
				this.FunctionUpdated(this, EventArgs.Empty);
			}
		}

		public void Invalidate(IFunctionVisual function) {
			this.invalidating = true;
			Thread.MemoryBarrier();
			try {
				this.invalid.Add(function);
			} finally {
				this.invalidating = false;
				Thread.MemoryBarrier();
			}
		}

		public IEnumerable<IFunctionVisual> InvalidVisuals() {
			if(0 < this.invalid.Count) {
				HashSet<IFunctionVisual> current = this.invalid;
				this.invalid = this.invalidEmpty;
				Thread.MemoryBarrier();
				while(this.invalidating);
				Tracer.Assert(current != this.invalid);
				List<IFunctionVisual> list = new List<IFunctionVisual>(current);
				current.Clear();
				this.invalidEmpty = current;
				return list;
			} else {
				return null;
			}
		}

		public bool HasProbes {
			get { return 0 < this.probeList.Count; }
		}

		public IEnumerable<FunctionProbe> Probes {
			get {
				Tracer.Assert(this.HasProbes);
				return this.probeList;
			}
		}

		public IEnumerable<CircuitFunction> Functions {
			get {
				HashSet<CircuitFunction> list = new HashSet<CircuitFunction>();
				list.UnionWith(this.terminal);
				foreach(List<CircuitFunction> d in this.dependant) {
					list.UnionWith(d);
				}
				return list;
			}
		}

		public bool Evaluate(bool flipClock) {
			if(0 < this.updated.Count) {
				lock(this.updated) {
					this.dirty.Add(this.updated);
					this.updated.Clear();
				}
			}
			if(flipClock) {
				foreach(FunctionClock c in this.clockList) {
					c.Flip();
					this.dirty.Add(c);
				}
			}
			int maxRetry = 3;
			int attempt = 0;
			this.dirty.Delay = attempt;
			int oscilation = this.state.Count * this.state.Count;
			while(!this.dirty.IsEmpty) {
				CircuitFunction function = this.dirty.Get();
				if(function.Evaluate()) {
					foreach(int result in function.Result) {
						this.dirty.Add(this.dependant[result]);
					}
					if(oscilation-- < 0) {
						if(maxRetry <= attempt) {
							return false;
						}
						oscilation = this.state.Count * this.state.Count;
						this.dirty.Delay = ++attempt;
					}
				}
				#if DUMP_STATE
					Tracer.FullInfo("CircuitState.Evaluate", "{0} {1}", this.ShowParam(function), function.ToString());
					Tracer.FullInfo("CircuitState.Evaluate", this.ToString());
				#endif
			}
			return true;
		}

		/*public bool Evaluate() {
			return this.Evaluate(true);
		}*/

		/*private string ShowParam(Function f) {
			StringBuilder text = new StringBuilder();
			text.Append(' ', this.Count);
			foreach(int p in f.Parameter) {
				text[p] = '^';
			}
			foreach(int r in f.Result) {
				text[r] = 'v';
			}
			return text.ToString();
		}*/

		public Random Random { get { return this.dirty.Random; } }

		public override string ToString() {
			StringBuilder text = new StringBuilder();
			foreach(State s in this.state) {
				text.Append(CircuitFunction.ToChar(s));
			}
			return text.ToString();
		}

		private class DirtyList {
			private List<CircuitFunction> current = new List<CircuitFunction>();
			private int head = 0;
			private HashSet<CircuitFunction> next = new HashSet<CircuitFunction>();
			private Random random;
			public int Delay { get; set; }

			public DirtyList(int seed) {
				this.random = new Random(seed);
				this.Delay = 0;
			}

			public void Add(CircuitFunction function) {
				this.next.Add(function);
			}

			public void Add(IEnumerable<CircuitFunction> function) {
				this.next.UnionWith(function);
			}

			public CircuitFunction Get() {
				if(this.current.Count <= this.head) {
					if(this.next.Count <= 0) {
						throw new InvalidOperationException(Resources.ErrorDirtyListIsEmpty);
					}
					this.current.Clear();
					this.head = 0;
					for(int i = 0; i < this.next.Count; i++) {
						this.current.Add(null);
					}
					foreach(CircuitFunction f in this.next) {
						int index = this.random.Next(this.current.Count);
						while(this.current[index] != null) {
							index++;
							if(this.current.Count <= index) {
								index = 0;
							}
						}
						this.current[index] = f;
					}
					this.next.Clear();
					if(0 < this.Delay && 1 < this.current.Count) {
						int max = Math.Min(this.Delay, this.current.Count - 1);
						for(int i = 0; i < max; i++) {
							this.next.Add(this.Get());
						}
					}
				}
				return this.current[this.head++];
			}

			public bool IsEmpty { get { return this.current.Count - this.head + this.next.Count <= 0; } }

			public Random Random { get { return this.random; } }
		}
	}
}
