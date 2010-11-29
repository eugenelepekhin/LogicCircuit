using System;
using System.Collections.Generic;
using System.Text;

namespace ResourceWrapper.Generator {
	internal static class Message {
		private static StringBuilder text = new StringBuilder();

		public static void Error(string code, params object[] args) {
			Message.Flush();
			Console.Error.WriteLine(TextMessage.ResourceManager.GetString(code, TextMessage.Culture), args);
		}

		public static void Write(string code, params object[] args) {
			Message.text.AppendFormat(TextMessage.ResourceManager.GetString(code, TextMessage.Culture), args);
			Message.text.AppendLine();
			//Console.Out.WriteLine(TextMessage.ResourceManager.GetString(code, TextMessage.Culture), args);
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
