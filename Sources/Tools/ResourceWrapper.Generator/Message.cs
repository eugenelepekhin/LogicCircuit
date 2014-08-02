using System;
using System.Text;

namespace ResourceWrapper.Generator {
	internal static class Message {
		private static StringBuilder text = new StringBuilder();

		public static void Error(string text, params object[] args) {
			Message.Flush();
			Console.Error.WriteLine(text, args);
		}

		public static void Write(string text, params object[] args) {
			Message.text.AppendFormat(text, args);
			Message.text.AppendLine();
			//Console.Out.WriteLine(text, args);
		}

		public static void Flush() {
			if(0 < Message.text.Length) {
				Console.Out.Write(Message.text);
				Message.text.Length = 0;
			}
		}

		public static void Clear() {
			Message.text.Length = 0;
		}
	}
}
