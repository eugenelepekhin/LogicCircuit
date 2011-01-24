using System;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using System.Text;

namespace LogicCircuit {
	/// <summary>
	/// Perform debugging tracing
	/// </summary>
	internal static class Tracer {
		private static readonly string LogPath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), TracerMessage.LogFileName
		);

		/// <summary>
		/// Defines current level of the tracing
		/// </summary>
		public enum Level {
			/// <summary>
			/// Fatal application error
			/// </summary>
			Fatal = 0,
			/// <summary>
			/// Application error. Usually an exception
			/// </summary>
			ApplicationError = 1,
			/// <summary>
			/// Application warning
			/// </summary>
			ApplicationWarning = 2,
			/// <summary>
			/// Application Execution flow.
			/// </summary>
			ExecutionFlow = 3,
			/// <summary>
			/// Tracing of method's parameters
			/// </summary>
			Parameter = 4,
			/// <summary>
			/// Auxiliary information
			/// </summary>
			Info = 5,
			/// <summary>
			/// Very detailed information
			/// </summary>
			FullInfo = 6
		}

		private static Level currentLevel = Level.FullInfo;
		/// <summary>
		/// Gets or Sets current application's tracing level
		/// </summary>
		public static Level CurrentLevel {
			//get { return Tracer.currentLevel; }
			set { Tracer.currentLevel = value; }
		}

		private static bool writeToLogFile = (
			#if DEBUG
				false //true
			#else
				false
			#endif
		);
		/// <summary>
		/// Gets or Sets flag if writing in log file is requared
		/// </summary>
		public static bool WriteToLogFile {
			//get { return Tracer.writeToLogFile; }
			set { Tracer.writeToLogFile = value; }
		}

		private static void Write(Level level, string category, string format, params object[] args) {
			if(level <= Tracer.currentLevel) {
				Tracer.Write(string.Format(TracerMessage.Culture, format, args), category);
			}
		}
		//public static void Report(string category, string message, Exception exception) {
		//	Tracer.Write(Level.ApplicationError, category, "{0}: {1}", message, exception);
		//}
		public static void Report(string category, Exception exception) {
			Tracer.Write(Level.ApplicationError, category, "{0}", exception);
		}
		public static void Fatal(string category, string description) {
			Tracer.Write(Level.Fatal, category, description);
		}
		//public static void Info(string category, string message, params object[] args) {
		//	Tracer.Write(Level.Info, category, message, args);
		//}
		public static void FullInfo(string category, string description, params object[] args) {
			Tracer.Write(Level.FullInfo, category, description, args);
		}

		//---------------------------------------------------------------------

		public static void Assert(bool condition, string description) {
			if(!condition) {
				throw new AssertException(description);
			}
		}
		public static void Assert(bool condition) {
			if(!condition) {
				throw new AssertException();
			}
		}
		public static void Fail(string reason) {
			throw new AssertException(reason);
		}
		//public static void Fail(string reason, params object[] args) {
		//	Tracer.Fail(string.Format(CultureInfo.CurrentUICulture, reason, args));
		//}
		public static void Fail() {
			Tracer.Fail(TracerMessage.InternalError);
		}

		//---------------------------------------------------------------------

		private static void WriteToFile(string description, string category) {
			if(!File.Exists(Tracer.LogPath)) {
				string dir = Path.GetDirectoryName(Tracer.LogPath);
				if(!Directory.Exists(dir)) {
					Directory.CreateDirectory(dir);
				}
				StreamWriter w = File.CreateText(Tracer.LogPath);
				w.Close();
			}
			StreamWriter writer = null;
			try {
				writer = File.AppendText(Tracer.LogPath);
				writer.Write(category + ": ");
				writer.WriteLine(description);
			} catch(Exception exception) {
				Tracer.writeToLogFile = false;
				Tracer.Write(exception.ToString(), "Tracer.WriteToFile");
			} finally {
				writer.Close();
			}
		}

		private static void Write(string description, string category) {
			Trace.WriteLine(description, category);
			#if UnitTest
				Log.Write("{0}: {1}", category, description);
			#else
				if(Tracer.writeToLogFile) {
					Tracer.WriteToFile(description, category);
				}
			#endif
		}
	}
}
