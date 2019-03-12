using SPAA05.Shared.DataSources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PioneerAvrControlLib {

	public class PioneerConnection : IDisposable {
		public event EventHandler ConnectionEstablished;
		public event EventHandler ConnectionLost;
		private readonly List<WritableDataSource> _endPoints;
		private int currentIdx = -1;
		private bool _die;
        private bool _reconnectScheduled = false;

		private WritableDataSource CurrentDataSource {
			get { return _endPoints[currentIdx]; }
		}

		public PioneerConnection(IEnumerable<WritableDataSource> endPoints) {
			this._endPoints = endPoints.ToList();
            foreach (var ds in _endPoints) {
                ds.ReconnectBehavior = ReconnectBehavior.Ignore;
				ds.ConnectingFailed += OnConnectingFailed;
				ds.ConnectionLost += OnConnectionLost;

				ds.ConnectionEstablished += OnConnectionEstablished;
                ds.DataReceived += OnDataReceived;
            }
		}

		public void Start() {
			_die = false;
			ConnectNext();
		}

		private void ConnectNext() {
			DataSource ds;
			if (currentIdx >= 0) {
				ds = _endPoints[currentIdx];
				ds.Stop();
			}

			currentIdx = (currentIdx + 1) % _endPoints.Count;
			ds = _endPoints[currentIdx];
			ds.Stop();

			System.Diagnostics.Debug.WriteLine("Attempting to start datasource " + ds);
			ds.Start();
		}

		public void ForceConnectNext() {
			ConnectNext();
		}

		public void Dispose() {
			_die = true;
            foreach (var ds in _endPoints) {
                ds.ConnectingFailed -= OnConnectingFailed;
                ds.ConnectionLost -= OnConnectionLost; 
                ds.Dispose();
            }
		}
        public async void ScheduleConnectNext() {
            if (_die || _reconnectScheduled) return;

			_reconnectScheduled = true;
			await Task.Delay(1000);
		    ConnectNext();
            _reconnectScheduled = false;
        }
		
		private void OnConnectionLost(object sender, ConnectionLostArgs e) {
			var ds = sender as DataSource;
			System.Diagnostics.Debug.WriteLine("Connection lost on " + ds);
			if (ds == _endPoints[currentIdx])
				ScheduleConnectNext();
		}

		private void OnConnectingFailed(object sender, ConnectingFailedArgs e) {
			var ds = sender as DataSource;
			System.Diagnostics.Debug.WriteLine("Connection failed on " + ds);
			if (ds == _endPoints[currentIdx])
				ScheduleConnectNext();
		}

		public void OnConnectionEstablished(object sender, EventArgs args) {
			DataSource ds = sender as DataSource;
			System.Diagnostics.Debug.WriteLine("Datasource " + ds + " started successfully");
			ConnectionEstablished?.Invoke(sender, args);
        }

	    private void OnDataReceived(object sender, DataReceivedEventArgs args) {
            AddToBuffer(args.Data);
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
