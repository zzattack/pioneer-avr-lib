using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PioneerAvrControlLib.DataSources;

namespace PioneerAvrControlLib {

	public class PioneerConnection : IDisposable {
		public event EventHandler ConnectionEstablished;
		public event EventHandler ConnectionLost;
		private readonly List<WritableDataSource> _endPoints;
		private int currentIdx = -1;
		private bool _die;

		private WritableDataSource CurrentDataSource {
			get { return _endPoints[currentIdx]; }
		}

		public PioneerConnection(IEnumerable<WritableDataSource> endPoints) {
			this._endPoints = endPoints.ToList();
			foreach (var ds in _endPoints)
				ds.ReconnectBehavior = ReconnectBehavior.Ignore;
		}

		public void Start() {
			_die = false;
			ConnectNext();
		}

		private void ConnectNext() {
			currentIdx = (currentIdx + 1) % _endPoints.Count;
			var ds = _endPoints[currentIdx];

			ds.ConnectionEstablished += (sender, args) => ConnectionEstablished(sender, args);
			ds.ConnectionLost += (sender, args) => {
				ConnectionLost(sender, args);
				if (!_die)
					ConnectNext();
			};
			ds.DataReceived += (source, args) => AddToBuffer(args.NewData);
			ds.Start();
		}
		public void Dispose() {
			_die = true;
			foreach (var ds in _endPoints)
				ds.Dispose();
		}

		#region buffer handling

		readonly List<byte> _rawBuff = new List<byte>();
		const byte ETX = (byte)'\n';

		protected void AddToBuffer(IEnumerable<byte> buff) {
			lock (_rawBuff) {
				foreach (byte b in buff) {
					_rawBuff.Add(b);

					if (b == ETX) {
						ProcesBuffer();
						_rawBuff.Clear();
					}
				}
			}
		}
		public event EventHandler<MessageReceivedEventArgs> MessageReceived;
		private void ProcesBuffer() {
			PioneerMessage msg = PioneerMessageFactory.Deserialize(_rawBuff);
			if (msg != null && MessageReceived != null)
				MessageReceived(this, new MessageReceivedEventArgs(msg));
		}
		#endregion

		#region message sending
		public void SendMessage(Type messageType, params object[] args) {
			if (messageType.BaseType.BaseType != typeof(PioneerMessage))
				throw new InvalidCastException("messageType must be of type PioneerMessage");
			// get type for message
			var type = PioneerMessageFactory.GetTypeForMessage(messageType);
			SendMessage(type, args);
		}

		public void SendMessage(MessageType type, params object[] args) {
			byte[] b = null;
			try {
				PioneerMessage mess = PioneerMessageFactory.Create(type, args);
				b = Encoding.Default.GetBytes(mess.Serialize());
			}
			catch (ArgumentException) { }
			if (b != null)
				CurrentDataSource.Write(b);
		}

		public void SendMessage(PioneerMessage mess) {
			var b = Encoding.Default.GetBytes(mess.Serialize());
			CurrentDataSource.Write(b);
		}
		#endregion
	}

	public class MessageReceivedEventArgs : EventArgs {
		public readonly PioneerMessage message;
		public MessageReceivedEventArgs(PioneerMessage msg) {
			this.message = msg;
		}
	}

}
