// Comment out any of #define to turn off unneeded parameter type.
#define HaveFlagParam
#define HaveStringParam
//#define HaveIntParam

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CommandLineParser {
	/// <summary>
	/// Command line parser
	/// Inspired by Mono.Options but simpler and easier to use.
	/// </summary>
	internal class CommandLine {
		private ParameterList parameterList = new ParameterList();

		#if HaveFlagParam
			/// <summary>
			/// Defines flag parameter
			/// </summary>
			/// <param name="name">Full name of the parameter</param>
			/// <param name="alias">Short name of the parameter</param>
			/// <param name="note">Help text for the parameter</param>
			/// <param name="required">True if the parameter is mandatory</param>
			/// <param name="assign">Method to assign flag parameter back to the application variable</param>
			/// <returns>Returns this reference</returns>
			public CommandLine AddFlag(string name, string alias, string note, bool required, Action<bool> assign) {
				this.parameterList.Add(new ParameterFlag(name, alias, note, required, assign));
				return this;
			}
		#endif

		#if HaveStringParam
			/// <summary>
			/// Defines string parameter
			/// </summary>
			/// <param name="name">Full name of the parameter</param>
			/// <param name="alias">Short name of the parameter</param>
			/// <param name="value">Help text that will represent the value passed to the parameter</param>
			/// <param name="note">Help text for the parameter</param>
			/// <param name="required">True if the parameter is mandatory</param>
			/// <param name="assign">Method to assign string parameter back to the application variable</param>
			/// <returns>Returns this reference</returns>
			public CommandLine AddString(string name, string alias, string value, string note, bool required, Action<string> assign) {
				this.parameterList.Add(new ParameterString(name, alias, value, note, required, assign));
				return this;
			}
		#endif

		#if HaveIntParam
			/// <summary>
			/// Defines integer parameter
			/// </summary>
			/// <param name="name">Full name of the parameter</param>
			/// <param name="alias">Short name of the parameter</param>
			/// <param name="value">Help text that will represent the value passed to the parameter</param>
			/// <param name="note">Help text for the parameter</param>
			/// <param name="required">True if the parameter is mandatory</param>
			/// <param name="min">Min value</param>
			/// <param name="max">Max value</param>
			/// <param name="assign">Method to assign int parameter back to the application variable</param>
			/// <returns>Returns this reference</returns>
			public CommandLine AddInt(string name, string alias, string value, string note, bool required, int min, int max, Action<int> assign) {
				this.parameterList.Add(new ParameterInt(name, alias, value, note, required, min, max, assign));
				return this;
			}

			/// <summary>
			/// Defines int parameter allowing any integer value
			/// </summary>
			/// <param name="name">Full name of the parameter</param>
			/// <param name="alias">Short name of the parameter</param>
			/// <param name="value">Help text that will represent the value passed to the parameter</param>
			/// <param name="note">Help text for the parameter</param>
			/// <param name="required">True if the parameter is mandatory</param>
			/// <param name="assign">Method to assign int parameter back to the application variable</param>
			/// <returns>Returns this reference</returns>
			public CommandLine AddInt(string name, string alias, string value, string note, bool required, Action<int> assign) {
				return this.AddInt(name, alias, value, note, required, int.MinValue, int.MaxValue, assign);
			}
		#endif

		/// <summary>
		/// Parses the array of command line parameters and calling all the assign methods to set values of parsed parameters.
		/// </summary>
		/// <param name="args">Command line arguments</param>
		/// <param name="assingUnmatched">Assign all unmatched parameters. If this parameter is null then no unmatched parameters are allowed.</param>
		/// <returns>null if parsing is successful, error messages if unsuccessful.</returns>
		public string Parse(string[] args, Action<IEnumerable<string>> assingUnmatched) {
			this.parameterList.Reset();
			List<string> errors = new List<string>();
			List<string> unmatched = new List<string>();
			if(args != null && 0 < args.Length) {
				// -v+ /hello:- --world=test someArbitraryString
				Regex regex = new Regex(@"^(?<prefix>(/|-|--)?)(?<name>[^=:+-]+)(?<separator>[=:]?)(?<value>.*)$",
					RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline
				);
				for(int i = 0; i < args.Length; i++) {
					string text = Parameter.Trim(args[i]);
					Match match = regex.Match(text);
					if(match.Success) {
						Parameter parameter = this.parameterList.Find(Parameter.Trim(match.Groups["name"].Value));
						if(parameter == null) {
							if(Parameter.Trim(match.Groups["prefix"].Value).Length == 0) {
								unmatched.Add(text);
							} else {
								errors.Add(string.Format(CultureInfo.InvariantCulture, "Unknown parameter: {0}", text));
								break;
							}
						} else {
							bool separatorIsEmpty = (Parameter.Trim(match.Groups["separator"].Value).Length == 0);
							string value = Parameter.Trim(match.Groups["value"].Value);
							if(separatorIsEmpty && value.Length == 0 && parameter.ExpectValue()) {
								if(i + 1 < args.Length) {
									value = Parameter.Trim(args[++i]); // assume next argument is the value. Note! the index of the loop is advanced here.
								} else {
									errors.Add(string.Format(CultureInfo.InvariantCulture, "Parameter \"{0}\" is missing its value", text));
									break;
								}
							}
							string setError = parameter.SetValue(value);
							if(!string.IsNullOrEmpty(setError)) {
								errors.Add(setError);
								break;
							}
						}
					} else {
						unmatched.Add(text);
					}
				}
			}
			if(errors.Count == 0) {
				foreach(Parameter parameter in this.parameterList.Where(p => p.Required && !p.HasValue)) {
					errors.Add(string.Format(CultureInfo.InvariantCulture, "Required parameter \"{0}\" is missing", parameter.Name));
				}
			}
			if(errors.Count == 0) {
				if(assingUnmatched != null) {
					assingUnmatched(unmatched);
				} else if(0 < unmatched.Count) {
					errors.Add(string.Format(CultureInfo.InvariantCulture, "Unrecognized parameter: {0}", unmatched[0]));
				}
			}
			return errors.Aggregate((string)null, (left, right) => string.IsNullOrEmpty(left) ? right : left + "\n" + right);
		}

		/// <summary>
		/// Constructs help text for the parameters to use in get help of the program
		/// </summary>
		/// <returns>Constructed help string</returns>
		public string Help() {
			this.parameterList.EnsureDefined();
			Func<Parameter, string> value = parameter => parameter.Value != null ? " " + parameter.Value : string.Empty;
			Func<Parameter, string> format = parameter =>
				(parameter.Alias == null)
				? string.Format(CultureInfo.InvariantCulture, "/{0}{1}", parameter.Name, value(parameter))
				: string.Format(CultureInfo.InvariantCulture, "/{0} -{1}{2}", parameter.Alias, parameter.Name, value(parameter))
			;
			int width = this.parameterList.Select(parameter => format(parameter).Length).Max();
			StringBuilder text = new StringBuilder();
			this.parameterList.ForEach(parameter => {
				string help = format(parameter);
				text.Append(help);
				text.Append(' ', width - help.Length);
				text.Append(" - ");
				if(parameter.Required) {
					text.Append("required: ");
				}
				text.AppendLine(parameter.Note);
			});
			return text.ToString();
		}

		private abstract class Parameter {
			public string Name { get; private set; }
			public string Alias { get; private set; }
			public string Value { get; private set; }
			public string Note { get; private set; }
			public bool Required { get; private set; }
			public bool HasValue { get; set; }

			protected Parameter(string name, string alias, string value, string note, bool required) {
				Debug.Assert(!string.IsNullOrWhiteSpace(name) && name == name.Trim(), "Invalid parameter name: " + name);
				Debug.Assert(alias == null || (alias == alias.Trim() && 0 < alias.Length), "Invalid parameter alias: " + alias);
				Debug.Assert(alias != name, "alias == name for parameter " + name);
				Debug.Assert(value == null || (value == value.Trim() && 0 < value.Length), "Invalid parameter value: " + value);
				Debug.Assert(!string.IsNullOrWhiteSpace(note) && note == note.Trim(), "Invalid parameter note: " + note);

				this.Name = name;
				this.Alias = alias;
				this.Value = value;
				this.Note = note;
				this.Required = required;
			}

			public abstract string SetValue(string value);
			public virtual bool ExpectValue() {
				return true;
			}
			public static string Trim(string text) {
				return string.IsNullOrWhiteSpace(text) ? string.Empty : text.Trim();
			}
		}

		private sealed class ParameterList : List<Parameter> {
			public void EnsureDefined() {
				if(0 == this.Count) {
					throw new InvalidOperationException("Command line parameters are not defined");
				}
			}

			public void Reset() {
				this.EnsureDefined();
				this.ForEach(parameter => parameter.HasValue = false);
			}

			public new void Add(Parameter parameter) {
				if(this.Find(parameter.Name) != null || (parameter.Alias != null && this.Find(parameter.Alias) != null)) {
					throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Parameter with such name or alias already defined: name={0}, alias={1}", parameter.Name, parameter.Alias));
				}
				base.Add(parameter);
			}

			public Parameter Find(string name) {
				StringComparer comparer = StringComparer.OrdinalIgnoreCase;
				return this.FirstOrDefault(
					parameter => comparer.Equals(parameter.Name, name) || comparer.Equals(parameter.Alias, name)
				);
			}
		}

		private abstract class Parameter<T> : Parameter {
			private readonly Action<T> assign;

			protected Parameter(string name, string alias, string value, string note, bool required, Action<T> assign) : base(name, alias, value, note, required) {
				Debug.Assert(assign != null, "Assign parameter is missing");
				this.assign = assign;
			}

			public string AssignValue(T value) {
				this.assign(value);
				this.HasValue = true;
				return null;
			}
		}

		#if HaveFlagParam
			private sealed class ParameterFlag : Parameter<bool> {
				public ParameterFlag(string name, string alias, string note, bool required, Action<bool> assign) : base(name, alias, null, note, required, assign) {
				}

				public override string SetValue(string value) {
					bool flag = false;
					if(string.IsNullOrWhiteSpace(value)) {
						flag = true;
					} else {
						switch(value.ToUpperInvariant()) {
						case "+":
						case "YES":
						case "TRUE":
						case "ON":
						case "1":
							flag = true;
							break;
						case "-":
						case "NO":
						case "FALSE":
						case "OFF":
						case "0":
							flag = false;
							break;
						default:
							return string.Format(CultureInfo.InvariantCulture, "Parameter {0} has invalid value {1}", this.Name, value);
						}
					}
					return this.AssignValue(flag);
				}

				public override bool ExpectValue() {
					return false;
				}
			}
		#endif

		#if HaveStringParam
			private sealed class ParameterString : Parameter<string> {
				public ParameterString(string name, string alias, string value, string note, bool required, Action<string> assign) : base(name, alias, value, note, required, assign) {
				}

				public override string SetValue(string value) {
					return this.AssignValue(value);
				}
			}
		#endif

		#if HaveIntParam
			private sealed class ParameterInt : Parameter<int> {
				private int min;
				private int max;

				public ParameterInt(string name, string alias, string value, string note, bool required, int min, int max, Action<int> assign) : base(name, alias, value, note, required, assign) {
					Debug.Assert(min < max, "min should be less than max for integer parameter.");
					this.min = min;
					this.max = max;
				}

				public override string SetValue(string value) {
					int parsedValue;
					if(int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsedValue)) {
						if(this.min <= parsedValue && parsedValue <= this.max) {
							return this.AssignValue(parsedValue);
						}
						return string.Format(CultureInfo.InvariantCulture, "Provided value {0} of parameter {1} expected to be in range: {2} <= {3} <= {4}.", value, this.Name, this.min, this.Value, this.max);
					}
					return string.Format(CultureInfo.InvariantCulture, "Parameter {0} has invalid value {1}. This parameter is expecting numerical value.", this.Name, value);
				}
			}
		#endif
	}
}
