using System.Diagnostics;
using System.Drawing;

namespace PioneerAvrControlLib {
	public static class Logger {
		public delegate void LogDelegate(string line, Color c);

		public static event LogDelegate LogLine;

		public static Color SuccessColor = Color.Green;
		public static Color InfoColor = Color.Black;
		public static Color WarnColor = Color.Orange;
		public static Color ErrorColor = Color.Red;

		public static void Success(string line, params object[] parameters) {
			string str = string.Format(line, parameters);
			if (LogLine != null)
				LogLine(str, SuccessColor);
			Debug.Write("[SUCCESS] ");
			Debug.WriteLine(str);
		}

		public static void Info(string line, params object[] parameters) {
			string str = string.Format(line, parameters);
			if (LogLine != null)
				LogLine(str, InfoColor);
			Debug.Write("[INFO] ");
			Debug.WriteLine(str);
		}

		public static void Warn(string line, params object[] parameters) {
			string str = string.Format(line, parameters);
			if (LogLine != null)
				LogLine(str, WarnColor);
			Debug.Write("[WARN] ");
			Debug.WriteLine(str);
		}

		public static void Error(string line, params object[] parameters) {
			string str = string.Format(line, parameters);
			if (LogLine != null)
				LogLine(str, ErrorColor);
			Debug.Write("[ERROR] ");
			Debug.WriteLine(str);
		}
	}
}
