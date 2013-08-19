using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace PioneerAvrControlLib {
	public static class PioneerMessageFactory {

		static List<Type> commandMessageTypes = new List<Type>();
		static List<Type> requestMessageTypes = new List<Type>();
		static List<Type> responseMessageTypes = new List<Type>();

		static List<PioneerCommandMessage> commandMessages = new List<PioneerCommandMessage>();
		static List<PioneerRequestMessage> requestMessages = new List<PioneerRequestMessage>();
		static List<PioneerResponseMessage> responseMessages = new List<PioneerResponseMessage>();

		static PioneerMessageFactory() {
			Assembly a = Assembly.GetExecutingAssembly();
			foreach (Type t in a.GetTypes()) {
				try {
					if (t.BaseType == typeof(PioneerCommandMessage)) {
						commandMessageTypes.Add(t);
						commandMessages.Add((PioneerCommandMessage)Activator.CreateInstance(t));
					}
					else if (t.BaseType == typeof(PioneerRequestMessage)) {
						requestMessageTypes.Add(t);
						requestMessages.Add((PioneerRequestMessage)Activator.CreateInstance(t));
					}
					else if (t.BaseType == typeof(PioneerResponseMessage)) {
						responseMessageTypes.Add(t);
						responseMessages.Add((PioneerResponseMessage)Activator.CreateInstance(t));
					}
				}
				catch (MissingMethodException exc) {
					throw new InvalidProgramException("Type " + t.ToString() + " requires a parameterless public constructor");
				}
			}
			// order all kinds of messages by decreasing length of type descriptor
			commandMessages  = commandMessages. OrderByDescending(m => m.Type.ToString().Length).ToList();
			requestMessages  = requestMessages. OrderByDescending(m => m.Type.ToString().Length).ToList();
			responseMessages = responseMessages.OrderByDescending(m => m.Type.ToString().Length).ToList();
		}

		public static PioneerMessage Deserialize(List<byte> rawBuff) {
			string msg = System.Text.Encoding.Default.GetString(rawBuff.ToArray(), 0, rawBuff.Count - 2);
			return Deserialize(msg);
		}

		public static PioneerMessage Deserialize(string msg) {
			// we can only deserialize responses, never requests or commands
			if (msg.Length < 2 || msg[0] == '?') // ? indicates request message
				return null; // throw new ArgumentException("Cannot deserialize request messages (yet)");

			foreach (PioneerResponseMessage m in responseMessages) {
				// all response messages start with its .Type identifier
				if (msg.StartsWith(m.Type.ToString())) {
					try {
						return (PioneerResponseMessage)Activator.CreateInstance(m.GetType(), msg);
					}
					// perhaps this messsage seems compatible with what we know, but
					// it isn't really
					catch (TargetInvocationException) { }
					catch (ArgumentException) { }
				}
			}
			// if none found this is probably a response
			// throw new ArgumentException("Message could not be deserialized");
			return null;
		}


		internal static PioneerMessage Create(MessageType type, params object[] args) {
			foreach (PioneerRequestMessage m in requestMessages) {
				// all response messages start with its .Type identifier
				if (m.Type == type) {
					return (PioneerRequestMessage)Activator.CreateInstance(m.GetType(), args);
				}
			}
			foreach (PioneerCommandMessage m in commandMessages) {
				// all response messages start with its .Type identifier
				if (m.Type == type) {
					return (PioneerCommandMessage)Activator.CreateInstance(m.GetType(), args);
				}
			}
			throw new ArgumentException("Cannot instantiate this kind of request message");
		}

		internal static MessageType GetTypeForMessage(Type messageType) {
			foreach (var m in commandMessages)
				if (m.GetType() == messageType) return m.Type;

			foreach (var m in requestMessages)
				if (m.GetType() == messageType) return m.Type;

			foreach (var m in responseMessages)
				if (m.GetType() == messageType) return m.Type;

			throw new ArgumentException("messageType");
		}
	}
}
