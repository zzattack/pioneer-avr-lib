using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PioneerAvrControlLib.Message;

namespace PioneerAvrControlLib {

	public abstract class PioneerConnection {

		public abstract void Open();
		protected abstract void Write(IEnumerable<byte> b);

		List<byte> rawBuff = new List<byte>();
		const byte ETX = (byte)'\n';

		protected void AddToBuffer(IEnumerable<byte> buff) {
			lock (rawBuff) {
				foreach (byte b in buff) {
					rawBuff.Add(b);

					if (b == ETX) {
						ProcesBuffer();
						rawBuff.Clear();
					}
				}
			}
		}

		public event EventHandler<MessageReceivedEventArgs> MessageReceived;

		private void ProcesBuffer() {
			PioneerMessage msg = PioneerMessageFactory.Deserialize(rawBuff);
			if (msg != null && MessageReceived != null)
				MessageReceived(this, new MessageReceivedEventArgs(msg));
		}

		public void SendMessage(Type messageType, params object[] args) {
			if (messageType.BaseType.BaseType != typeof(PioneerMessage))
				throw new InvalidCastException("messageType must be of type PioneerMessage");
			// get type for message
			var type = PioneerMessageFactory.GetTypeForMessage(messageType);
			SendMessage(type, args);
		}

		public void SendMessage(MessageType type, params object[] args) {
			PioneerMessage mess = PioneerMessageFactory.Create(type, args);
			var b = Encoding.Default.GetBytes(mess.Serialize());
			this.Write(b);
		}

		public void SendMessage(PioneerMessage mess) {
			var b = Encoding.Default.GetBytes(mess.Serialize());
			this.Write(b);
		}
	}

	public class MessageReceivedEventArgs : EventArgs {
		public readonly PioneerMessage message;
		public MessageReceivedEventArgs(PioneerMessage msg) {
			this.message = msg;
		}
	}

}
