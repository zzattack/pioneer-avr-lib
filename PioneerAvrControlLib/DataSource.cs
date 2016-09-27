using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace PioneerAvrControlLib {

	public abstract class DataSource : IDisposable {
		public string Name { get; set; }

		public int LastDataArrived { get; set; }

		public long TotalReceived { get; protected set; }

		protected AutoResetEvent LineReadyEvent = new AutoResetEvent(false);

		private ConnectionState _state = ConnectionState.Disconnected;
		private readonly AutoResetEvent _areStartStop = new AutoResetEvent(false);

		protected bool ExpectDisconnect = false;
		public virtual ConnectionState State {
			get { return _state; }
		}

		public abstract string DetailedConfig { get; }

		public event EventHandler ConnectionEstablished;
		public event EventHandler ConnectionPending;
		public event EventHandler<ConnectingFailedArgs> ConnectingFailed;
		public event EventHandler<ConnectionLostArgs> ConnectionLost;
		public event EventHandler<DataReceivedEventArgs> DataReceived;

		protected DataSource() {
			ReconnectBehavior = ReconnectBehavior.ReconnectAlways;
		}

		public ReconnectBehavior ReconnectBehavior { get; set; }
		public virtual void Dispose() {
			Stop();
			if (_reconnectTimer != null) {
				_reconnectTimer.Change(-1, -1);
				_reconnectTimer.Dispose();
				_reconnectTimer = null;
			}
		}

		protected virtual void OnDataReceived(IEnumerable<byte> data) {
			// Util.DebugLog("<< DS: " + Util.ByteArrayToHexString(data));
			LastDataArrived = Environment.TickCount;
			TotalReceived += data.Count();
			if (DataReceived != null)
				FireDataReceived(this, new DataReceivedEventArgs(data));
		}

		protected virtual void FireDataReceived(object sender, DataReceivedEventArgs args) {
			if (DataReceived != null)
				DataReceived(sender, args);
		}


		/// <summary>
		/// Enabled a datasource. This function always calls either OnConnect() or OnConnectFailed().
		/// Upon success, should call OnConnect(). In case of failure should call OnConnectFailed().
		/// </summary>
		/// <param name="failSilently">Whether a message may be shown in case the datasource cannot be enabled.</param>
		/// <returns></returns>
		public abstract bool Start(bool failSilently = false);
		public virtual void Stop() {
			TotalReceived = 0;
			ExpectDisconnect = true;
			Cleanup();
			OnDisconnect(true);
		}
		public virtual void Cleanup() {
			_areStartStop.Set();
		}
		
		protected virtual void OnConnected() {
			if (_state != ConnectionState.Connected) {
				_state = ConnectionState.Connected;
			}
			_areStartStop.Set();

			if (ConnectionEstablished != null)
				ConnectionEstablished(this, EventArgs.Empty);
		}

		/// <summary>
		/// Signals connecting on Start() failed.
		/// </summary>
		/// <param name="failSilently">Whether the connection attempt was automatically triggered or user-triggered.
		/// If not user-triggered, reconnection behaviour should not be broken.</param>
		protected virtual void OnConnectFailed(bool failSilently, Exception exc = null) {
			var sb = new StringBuilder();
			sb.Append("Connection failed");
			if (exc != null) {
				sb.Append(": ");
				sb.Append(exc.Message);
				if (exc.InnerException != null)
					sb.AppendFormat(" [{0}]", exc.InnerException);
			}

			if (ReconnectBehavior == ReconnectBehavior.ReconnectAlways) {
				_state = ConnectionState.Connecting;
				ApplyReconnectBehavior();
			}
			else
				_state = ConnectionState.Disconnected;

			_areStartStop.Set();

			if (ConnectingFailed != null)
				ConnectingFailed(this, new ConnectingFailedArgs(sb.ToString(), exc, failSilently));
		}

		protected virtual void OnConnectPending() {
			if (_state < ConnectionState.Connecting) {
				_state = ConnectionState.Connecting;
				if (ConnectionPending != null)
					ConnectionPending(this, EventArgs.Empty);
			}
			_areStartStop.Set();
		}

		protected virtual void OnDisconnect(bool expected, bool forceEventFire = false) {
			bool wasConnected = State == ConnectionState.Connected;
			_state = ConnectionState.Disconnected;
			// else: ApplyReconnectBehavior will set it to Connecting

			// if state wasn't already "Disconnected", fire event
			if ((wasConnected || forceEventFire) && ConnectionLost != null) {
				ConnectionLost(this, new ConnectionLostArgs(
					expected ? ReconnectBehavior.Ignore : this.ReconnectBehavior, null));
			}

			if (!expected) {
				Cleanup(); // enforce a proper disconnect
				ApplyReconnectBehavior();
			}
			else { 
				// make sure pending reconnects are skipped
				_abortReconnect = true;
			}

		}

		protected void ApplyReconnectBehavior() {
			if (ReconnectBehavior == ReconnectBehavior.Ignore) {
				// _logger.Error("Applying reconnect behavior {0}: ignoring", Name);
			}
			else if (ReconnectBehavior == ReconnectBehavior.ReconnectAlways || 
				ReconnectBehavior == ReconnectBehavior.Reestablish) {
				ScheduleReconnect();
			}
		}

		private Timer _reconnectTimer;
		private bool _abortReconnect; // set to true to "unschedule" reconnect
		private void ScheduleReconnect() {
			// prevent multiple timers from being scheduled simultaneously
			if (_reconnectTimer != null) {
				// _logger.Debug("Skipping reconnect attempt scheduling (already planned)");
			}
			// prevent "reconnecting" if somehow already connected
			else if (State == ConnectionState.Connected) {
				// _logger.Debug("Skipping reconnect attempt scheduling (already reconnected)");
			}
			else {
				// _logger.Debug("Scheduling reconnect in 2s on {0}", Name);
				OnConnectPending();

				_abortReconnect = false;
				_reconnectTimer = new Timer(delegate {
					_reconnectTimer = null;
					if (_abortReconnect) { 
						// _logger.Debug("Aborting scheduled reconnect attempt", Name);
						return;
					}
					// if the state is not connected, we don't intend to be connected
					// if the state is connected, some previous attempt already succeeded by now
					if (!Start(true)) {
						// _logger.Debug("Scheduled reconnect failed on {0}", Name);
						ScheduleReconnect();
					}

				}, null, TimeSpan.FromSeconds(2), new TimeSpan(-1));
			}
		}
		
		public virtual string DisplayName {
			get { return ToString(); }
		}

		public override string ToString() {
			return Name;
		}

	}
	
	public class DataReceivedEventArgs : EventArgs {
		public readonly byte[] Data;

		public DataReceivedEventArgs(IEnumerable<byte> data) {
			Data = data.ToArray();
		}
	}

	public class ConnectingFailedArgs : EventArgs {
		public string Message { get; private set; }
		public Exception ExceptionObject { get; private set; }
		public bool Silent { get; private set; }

		public ConnectingFailedArgs(string msg, Exception exc, bool silent) {
			Message = msg;
			ExceptionObject = exc;
			Silent = silent;
		}
	}
	public class ConnectionLostArgs : EventArgs {
		public ReconnectBehavior ReconnectOption { get; private set; }
		public Exception ExceptionObject { get; private set; }
		public ConnectionLostArgs(ReconnectBehavior option, Exception exc) {
			ReconnectOption = option;
			ExceptionObject = exc;
		}
	}

	public abstract class WritableDataSource : DataSource {
		/// <summary>
		/// Write data to stream.
		/// </summary>
		public virtual bool Write(byte[] buffer, int offset, int count) {
			TotalTransmitted += count;
			return true;
		}

		public bool Write(byte[] buffer) {
			return Write(buffer, 0, buffer.Length);
		}

		public bool Write(IEnumerable<byte> buffer) {
			var b = buffer.ToArray();
			return Write(b, 0, b.Length);
		}

		public override void Stop() {
			base.Stop();
			TotalTransmitted = 0;
			ReconnectBehavior = ReconnectBehavior.Ignore;
		}

		public int LastDataSent { get; protected set; }

		public long TotalTransmitted { get; protected set; }
	}
	
	public class TcpClientDataSource : WritableDataSource {


		private TcpClient _tcp;
		readonly byte[] _readBuf = new byte[1024];
		readonly object _syncLock = new object();

		private string _hostname;

		public string Hostname {
			get { return _hostname; }
			set {
				/*if (_hostname != value && State != ConnectionState.Disconnected)
					MessageBox.Show("Cannot change property while datasource is enabled");
				else if (!string.IsNullOrEmpty(value) && Uri.CheckHostName(value) == UriHostNameType.Unknown)
					MessageBox.Show("Invalid hostname given, please correct");
				else*/
					_hostname = value;
			}
		}

		private int _port;


		public int Port {
			get { return _port; }
			set {
				//if (_port != value && State != ConnectionState.Disconnected)
				//	MessageBox.Show("Cannot change property while datasource is enabled");
				// if (20 <= value && value <= 65535)
					_port = value;
				// else 
				//	MessageBox.Show("Invalid port entered, must be between 24-65535, please correct");
			}
		}

		public TcpClientDataSource() : this("127.0.0.1") { }
		public TcpClientDataSource(string hostname, int port = 6498) {
			Hostname = hostname;
			Port = port;
		}

		public override string DetailedConfig {
			get {
				return "TCP client @ " + (Hostname ?? "<no host>") + ":" + Port;
			}
		}

		/// <summary>
		/// Remote IP, available once connected
		/// </summary>
		public IPAddress RemoteIp { get; private set; }


		public override bool Start(bool failSilently = false) {
			try {
				if (_tcp != null && _tcp.Connected) {
					var ipEndpoint = _tcp.Client.RemoteEndPoint as IPEndPoint;
					// already connected?
					if (ipEndpoint != null && (ipEndpoint.Address.ToString().Equals(Hostname) || ipEndpoint.Address.Equals(Dns.Resolve(Hostname))))
						return true;
				}

				Cleanup();
				ExpectDisconnect = false;
				var tcp = _tcp = new TcpClient(AddressFamily.InterNetwork);

				if (failSilently) {
					// async connect
					_tcp.BeginConnect(Hostname, Port, OnConnect, tcp);
					OnConnectPending();
					return true;
				}

				else {
					// connect synchronously with 2 second timeout
					IAsyncResult ar = _tcp.BeginConnect(Hostname, Port, null, tcp);
					OnConnectPending();
					
					WaitHandle wh = ar.AsyncWaitHandle;
					try {
						bool waitFinished = ar.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(2), false);
						if (!waitFinished) {
							OnConnectFailed(false, new TimeoutException());
							return false;
						}
						else if (_tcp == null || !_tcp.Connected) {
							OnConnectFailed(false, new SocketException(10060)); // 10060 is a guess
							return false;
						}
						else {
							OnConnect(ar);
							return true;
						}
					}
					finally {
						wh.Close();
					}  
				}
			}
			catch (Exception e) {
				OnConnectFailed(failSilently, e);
				return false;
			}
		}

		public override void Cleanup() {
			// _logger.Info("Cleaning up TCP DataProvider");
			RemoteIp = null;
			lock (_syncLock) {
				if (_tcp != null) {
					_tcp.Close();
					_tcp = null;
				}
			}
			base.Cleanup();
		}

		public void OnConnect(IAsyncResult iar) {
			try {
				var tcp = iar.AsyncState as TcpClient;
				if (tcp != null && tcp.Client != null) {
					if (!tcp.Connected) {
						OnDisconnect(ExpectDisconnect);
						return;
					}
					tcp.EndConnect(iar);
					lock (_syncLock) {
						_tcp = tcp;
						var stream = tcp.GetStream();
						stream.BeginRead(_readBuf, 0, _readBuf.Length, TcpOnDataReceived, tcp);
					}
					if (tcp.Client.RemoteEndPoint is IPEndPoint)
						RemoteIp = (tcp.Client.RemoteEndPoint as IPEndPoint).Address;
					// _logger.Info("Connected TCP socket");
					base.OnConnected();
				}
			}
			catch (Exception exc) {
				// _logger.Error("Couldn't connect TCP socket", exc);
				OnConnectFailed(true, exc);
			}
		}

		void TcpOnDataReceived(IAsyncResult r) {
			try {
				var tcp = r.AsyncState as TcpClient;
				if (tcp == null || !tcp.Connected) {
					OnDisconnect(ExpectDisconnect);
					return;
				}
				var stream = tcp.GetStream();
				int bytesRead = stream.EndRead(r);
				if (bytesRead == 0) {
					OnDisconnect(ExpectDisconnect);
					return;
				}

				var chunk = new byte[bytesRead];
				Array.Copy(_readBuf, chunk, bytesRead);
				OnDataReceived(chunk);
				
				stream.BeginRead(_readBuf, 0, _readBuf.Length, TcpOnDataReceived, tcp);
			}
			catch (IOException) {
				// _logger.Error("IOException in TcpOnDataReceived");
				OnDisconnect(ExpectDisconnect);
			}
			catch (InvalidOperationException) {
				// _logger.Error("InvalidOperationException in TcpOnDataReceived");
				OnDisconnect(ExpectDisconnect);
			}
			catch (NullReferenceException) {
				// _logger.Error("NullReferenceException in TcpOnDataReceived");
			}
			catch (SocketException) {
				// _logger.Error("SocketException in TcpOnDataReceived");
			}
		}

		public override bool Write(byte[] buffer, int offset, int count) {
			try {
				if (_tcp == null || !_tcp.Connected) return false;
				var stream = _tcp.GetStream();
				if (!stream.CanWrite)
					return false;
				else
					stream.Write(buffer, 0, buffer.Length);
				return base.Write(buffer, offset, count);
			}
			catch (IOException) { OnDisconnect(false); }
			catch (InvalidOperationException) { OnDisconnect(false); }
			catch (SocketException) { OnDisconnect(false); }
			return false;
		}

		public override string ToString() {
			return string.IsNullOrEmpty(Name) ? string.Format("TCP {0}:{1}", Hostname ?? "", Port) : Name;
		}
		
	}

	public enum ConnectionState {
		Disconnected = 0,
		Connecting = 1,
		Connected = 2,
	}

	public enum ReconnectBehavior {
		ReconnectAlways, // always attempt to reconnect
		Reestablish, // reconnect only if initial connnect is successful
		Ignore, // ignore
	}
}
