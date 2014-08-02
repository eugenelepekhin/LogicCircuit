//#define DUMP_STATE
//#define VALIDATE_GET_FUNCTION

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogicCircuit {
	public class CircuitState {

		public event EventHandler FunctionUpdated;

		private DirtyList dirty;
		private State[] state = null;
		public int Count { get; private set; }

		private List<CircuitFunction>[] dependent = null;
		private List<CircuitFunction> functions = new List<CircuitFunction>();
		public IEnumerable<CircuitFunction> Functions { get { return this.functions; } }

		private HashSet<CircuitFunction> updated = new HashSet<CircuitFunction>();
		private List<CircuitFunction> clockList = new List<CircuitFunction>();
		private List<FunctionProbe> probeList = new List<FunctionProbe>();

		public Random Random { get; private set; }

		public CircuitState(int reserveState) {
			int seed = (int)DateTime.UtcNow.Ticks;
			//seed = -142808611;
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
				this.dependent = new List<CircuitFunction>[this.Count];
				for(int i = 0; i < this.dependent.Length; i++) {
					this.dependent[i] = new List<CircuitFunction>();
				}
			}
			this.functions.Add(function);
			if(function is IFunctionClock) {
				this.clockList.Add(function);
			}
			int count = 0;
			foreach(int parameter in function.Parameter) {
				this.dependent[parameter].Add(function);
				count++;
			}
			if(count <= 0) {
				this.dirty.Add(function);
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
					function.Dependent = function.Result.Select(r => (IEnumerable<CircuitFunction>)this.dependent[r]).Aggregate((x, y) => x.Union(y)).ToArray();
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
				foreach(CircuitFunction c in this.clockList) {
					if(((IFunctionClock)c).Flip()) {
						this.dirty.Add(c);
					}
				}
			}
			int maxRetry = 3;
			int attempt = 0;
			this.dirty.Delay = attempt;
			long oscillation = (long)this.state.Length * (long)this.state.Length;
			CircuitFunction function;
			while((function = this.dirty.Get()) != null) {
				if(function.Evaluate()) {
					if(function.Dependent != null) {
						this.dirty.Add(function.Dependent);
					}
					if(oscillation-- < 0) {
						if(maxRetry <= attempt) {
							return false;
						}
						oscillation = (long)this.state.Length * (long)this.state.Length;
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

		#if DEBUG
			public override string ToString() {
				StringBuilder text = new StringBuilder();
				foreach(State s in this.state) {
					text.Append(CircuitFunction.ToChar(s));
				}
				return text.ToString();
			}
		#endif

		private class DirtyList {
			private FunctionList current = new FunctionList(1024);
			private FunctionList next = new FunctionList(1024);
			private int seed;
			public int Delay { get; set; }
			private long iteration = 1;

			private int index = 0;
			private int offset = 0;
			private int step = 0;

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

			#if VALIDATE_GET_FUNCTION
				private int functionCount = 0;
				private HashSet<int> functionIndex = new HashSet<int>();
			#endif

			public CircuitFunction Get() {
				int random;
				int count = this.current.Count;
				if(count <= this.index) {
					count = this.next.Count;
					if(count <= 0) {
						return null;
					}

					#if VALIDATE_GET_FUNCTION
						Tracer.Assert(this.functionCount == this.functionIndex.Count);
						Tracer.Assert(this.functionCount == 0 || (this.functionIndex.Contains(0) && this.functionIndex.Contains(this.functionCount - 1)));
						this.functionCount = count;
						this.functionIndex.Clear();
					#endif

					this.iteration++;
					this.current.Clear();
					FunctionList temp = this.next;
					this.next = this.current;
					this.current = temp;

					this.index = 1;

					// This expression for step will allow to iterate for this.index from 0 to this.current.Count - 1 and walk through each element
					// of this.current exactly once because int.MaxValue is prime number
					this.step = int.MaxValue % count;

					// simple but fast random number generator
					this.seed = 214013 * this.seed + 2531011;

					// this will just make sure: 0 <= random < count
					random = (int.MaxValue & this.seed) % count;
					this.offset = random;

					if(0 < this.Delay && 1 < count) {
						int max = Math.Min(this.Delay, count - 1);
						for(int i = 0; i < max; i++) {
							this.Add(this.Get());
						}
					}
				} else {
					this.index++;
					random = this.offset - this.step;
					if(random < 0) {
						random += count;
					}
					this.offset = random;
				}

				#if VALIDATE_GET_FUNCTION
					Tracer.Assert(0 <= random && random < this.functionCount && this.functionIndex.Add(random));
				#endif

				return this.current[random];
			}

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
