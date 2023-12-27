using System;
using System.Linq;

namespace LogicCircuit {
	internal static class EnumHelper {
		private static readonly char[] splitters = [',', ' '];

		public static T Parse<T>(string? text, T defaultValue) where T:struct {
			T value;
			if(string.IsNullOrWhiteSpace(text) || !Enum.TryParse<T>(text, true, out value) || !IsValid(value)) {
				return defaultValue;
			}
			return value;
		}

		public static bool IsValid<T>(T value) where T:struct {
			string[] name = Enum.GetNames(typeof(T));
			string text = value.ToString()!;
			StringComparer comparer = StringComparer.OrdinalIgnoreCase;
			if(!name.Contains(text, comparer)) {
				string[] part = text.Split(EnumHelper.splitters, StringSplitOptions.RemoveEmptyEntries);
				return part.All(p => name.Contains(p, comparer));
			}
			return true;
		}
	}
}
