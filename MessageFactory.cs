using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using PioneerAvrControlLib.Message;

namespace PioneerAvrControlLib {
	public static class PioneerMessageFactory {

		static List<Type> commandMessageTypes = new List<Type>();
		static List<Type> requestMessageTypes = new List<Type>();
		static List<Type> responseMessageTypes = new List<Type>();

		static List<PioneerResponseMessage> responseMessages = new List<PioneerResponseMessage>();

		static PioneerMessageFactory() {
			Assembly a = Assembly.GetExecutingAssembly();
			foreach (Type t in a.GetTypes()) {
				if (t.BaseType == typeof(PioneerCommandMessage))
					commandMessageTypes.Add(t);
				else if (t.BaseType == typeof(PioneerRequestMessage))
					requestMessageTypes.Add(t);
				else if (t.BaseType == typeof(PioneerResponseMessage)) {
					responseMessageTypes.Add(t);
					responseMessages.Add((PioneerResponseMessage)Activator.CreateInstance(t));
				}
			}
		}

		public static PioneerMessage Deserialize(List<byte> rawBuff) {
			string s = System.Text.Encoding.Default.GetString(rawBuff.ToArray(), 0, rawBuff.Count - 2);
			return Deserialize(s);
		}
		public static PioneerMessage Deserialize(string s) {
			// we can only deserialize responses, never requests or commands
			if (s[0] == '?')
				throw new ArgumentException("Cannot deserialize request messages (yet)");

			foreach (PioneerResponseMessage m in responseMessages) {
				if (s.StartsWith(m.Type.ToString())) {
					return (PioneerResponseMessage)Activator.CreateInstance(m.GetType(), s);
				}
			}

			throw new ArgumentException("Message could not be deserialized");
		}

	}
}
