﻿//#define DUMP_STATE

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.CompilerServices;

namespace LogicCircuit {
	public class CircuitState {

		public event EventHandler FunctionUpdated;

		private DirtyList dirty;
		private State[] state = null;
		public int Count { get; private set; }

		private List<CircuitFunction>[] dependant = null;
		private List<CircuitFunction> functions = new List<CircuitFunction>();
		public IEnumerable<CircuitFunction> Functions { get { return this.functions; } }

		private HashSet<CircuitFunction> updated = new HashSet<CircuitFunction>();
		private List<FunctionClock> clockList = new List<FunctionClock>();
		private List<FunctionProbe> probeList = new List<FunctionProbe>();
		private volatile bool invalidating = false;
		private volatile HashSet<IFunctionVisual> invalid = new HashSet<IFunctionVisual>();
		private HashSet<IFunctionVisual> invalidEmpty = new HashSet<IFunctionVisual>();

		public Random Random { get; private set; }

		public CircuitState(int reserveState) {
			int seed = (int)DateTime.UtcNow.Ticks;
			//seed = 2087553645;
			Tracer.FullInfo("CircuitState", "CircuitState.seed={0}", seed);
			this.dirty = new DirtyList(seed);
			this.Random = new Random(seed);
			this.Count = reserveState;
		}

		public State this[int index] {
			get { return this.state[index]; }
			set { this.state[index] = value; }
		}

		public int ReserveState() {
			return this.Count++;
		}

		public void DefineFunction(CircuitFunction function) {
			if(this.state == null) {
				this.state = new State[this.Count];
				this.dependant = new List<CircuitFunction>[this.Count];
				for(int i = 0; i < this.dependant.Length; i++) {
					this.dependant[i] = new List<CircuitFunction>();
				}
			}
			this.functions.Add(function);
			int count = 0;
			foreach(int parameter in function.Parameter) {
				this.dependant[parameter].Add(function);
				count++;
			}
			if(count <= 0) {
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

		public void EndDefinition() {
			if(this.state == null) {
				this.state = new State[this.Count];
				Tracer.Assert(this.functions.Count == 0);
			}
			foreach(CircuitFunction function in this.functions) {
				if(function.Result.Any()) {
					function.Dependant = function.Result.Select(r => (IEnumerable<CircuitFunction>)this.dependant[r]).Aggregate((x, y) => x.Union(y)).ToArray();
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
			RuntimeHelpers.PrepareConstrainedRegions();
			try {
				this.invalidating = true;
				Thread.MemoryBarrier();
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
			int oscilation = this.state.Length * this.state.Length;
			while(!this.dirty.IsEmpty) {
				CircuitFunction function = this.dirty.Get();
				if(function.Evaluate()) {
					if(function.Dependant != null) {
						this.dirty.Add(function.Dependant);
					}
					if(oscilation-- < 0) {
						if(maxRetry <= attempt) {
							return false;
						}
						oscilation = this.state.Length * this.state.Length;
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

		#if DUMP_STATE
			private string ShowParam(CircuitFunction f) {
				StringBuilder text = new StringBuilder();
				text.Append(' ', this.Count);
				foreach(int p in f.Parameter) {
					text[p] = '^';
				}
				foreach(int r in f.Result) {
					text[r] = 'v';
				}
				return text.ToString();
			}
		#endif

		public override string ToString() {
			StringBuilder text = new StringBuilder();
			foreach(State s in this.state) {
				text.Append(CircuitFunction.ToChar(s));
			}
			return text.ToString();
		}

		private class DirtyList {
			private FunctionList current = new FunctionList(1024);
			private FunctionList next = new FunctionList(1024);
			private int seed;
			public int Delay { get; set; }
			private long iteration = 1;

			private int offset = 0;
			private int index = 0;

			public DirtyList(int seed) {
				this.seed = seed;
				this.Delay = 0;
			}

			public void Add(CircuitFunction function) {
				if(function.Iteration < this.iteration) {
					function.Iteration = this.iteration;
					this.next.Add(function);
				}
			}

			public void Add(IEnumerable<CircuitFunction> function) {
				foreach(CircuitFunction f in function) {
					this.Add(f);
				}
			}

			public void Add(CircuitFunction[] function) {
				for(int i = 0; i < function.Length; i++) {
					this.Add(function[i]);
				}
			}

			public CircuitFunction Get() {
				if(this.current.Count <= this.index) {
					if(this.next.Count <= 0) {
						throw new InvalidOperationException(Resources.ErrorDirtyListIsEmpty);
					}

					this.iteration++;
					this.current.Clear();
					FunctionList temp = this.next;
					this.next = this.current;
					this.current = temp;

					this.index = 0;
					this.seed = 214013 * this.seed + 2531011;
					this.offset = (int.MaxValue & this.seed) % this.current.Count;

					if(0 < this.Delay && 1 < this.current.Count) {
						int max = Math.Min(this.Delay, this.current.Count - 1);
						for(int i = 0; i < max; i++) {
							this.Add(this.Get());
						}
					}
				}
				// This expression will iterate for this.index from 0 to this.current.Count - 1 and walk through each element
				// of this.current exactly once.
				// The constant should be a primary number in order to walk through entire current list
				// The expression inside parentesis may overflow int and become negative, so it will only work for
				// not big prime numbers and not big list of dirty functions.
				return this.current[(521 * (this.index++) + this.offset) % this.current.Count];
			}

			public bool IsEmpty { get { return this.current.Count <= this.index && this.next.Count <= 0; } }

			private struct FunctionList {
				private CircuitFunction[] list;
				public int Count { get; private set; }

				public FunctionList(int size) : this() {
					this.list = new CircuitFunction[size];
				}

				public CircuitFunction this[int index] {
					get { return this.list[index]; }
				}

				public void Add(CircuitFunction f) {
					if(this.list.Length <= this.Count) {
						Tracer.Assert(this.Count < this.list.Length * 2, "size overflowed");
						Array.Resize(ref this.list, this.list.Length * 2);
					}
					this.list[this.Count++] = f;
				}

				public void Clear() {
					this.Count = 0;
				}
			}
		}
	}
}
