using System;
using System.Threading;
using System.Xml;

namespace PioneerAvrControlLib.DataSources {
	public abstract class DataSource : IDisposable {
		protected DataSource() {
			ReconnectBehavior = ReconnectBehavior.Reconnect;
		}

		public string Name { get; set; }
		public abstract string Type { get; }
		public int LastDataArrived { get; set; }
		
		protected bool _isEnabled;
		public virtual bool IsEnabled {
			get { return _isEnabled; }
			set {
				if (_isEnabled != value) {
					if (value) Start();
					else Stop();
					AreStartStop.WaitOne();
				}
			}
		}

		public virtual void Dispose() {
			Stop();
		}

		public virtual void Start() {
			AreStartStop.Set();
		}

		public virtual void Stop() {
			AreStartStop.Set();
		}

		public event DataReceivedEventHandler DataReceived;
		public void OnDataReceived(byte[] data) {
			if (DataReceived != null)
				DataReceived(this, new DataReceivedEventArgs(data));
		}


		#region serialization

		public static DataSource LoadFrom(XmlElement node) {
			DataSource ret = null;
			if (node.Attributes["type"].InnerText == "serialport")
				ret = SerialPortDataSource.LoadFrom(node);
			else if (node.Attributes["type"].InnerText == "tcpclient")
				ret = TcpClientDataSource.LoadFrom(node);
			else if (node.Attributes["type"].InnerText == "tcpserver")
				ret = TcpServerDataSource.LoadFrom(node);
			return ret;
		}

		public DataSource LoadBaseSettings(XmlElement x) {
			this.Name = x["name"].InnerText;
			return this; // for use-case see Publisher
		}

		public void WriteBaseSettings(XmlTextWriter xtr) {
			// for use-case see Publisher
			xtr.WriteElementString("name", Name);
		}

		public virtual void SaveTo(XmlTextWriter xtr) {
			throw new NotImplementedException();
		}

		#endregion

		#region connection flow

		public EventHandler ConnectionEstablished;
		public EventHandler ConnectionLost;

		public void OnConnectionEstablished() {
			if (ConnectionEstablished != null) ConnectionEstablished(this, EventArgs.Empty);
		}
		public void OnConnectionLost() {
			if (ConnectionLost != null) ConnectionLost(this, EventArgs.Empty);
		}

		// set this flag if you wish to forcefully close a connection without triggering reconnect behavior
		protected bool IgnoreReconnectBehavior = false;
		protected AutoResetEvent AreStartStop = new AutoResetEvent(false);
		public ReconnectBehavior ReconnectBehavior { get; set; }

		protected void ApplyReconnectBehavior() {
			if (IgnoreReconnectBehavior || ReconnectBehavior == ReconnectBehavior.Ignore) {
				Stop(); // this sets _ignoreRCB to false, Stop() needs inner scope
				return;
			}

			Stop(); // flush log file
			if (ReconnectBehavior == ReconnectBehavior.Reconnect)
				ScheduleReconnect();
			else if (ReconnectBehavior == ReconnectBehavior.Report)
				Logger.Error("Connection aborted on {0}", this.Type);
		}

		private Timer _t;

		private void ScheduleReconnect() {
			Logger.Info("Scheduling reconnect in 3s on {0}", Type);
			_t = new Timer(delegate {
				Start();
			}, null, new TimeSpan(30000000), new TimeSpan(-1));
		}
		#endregion

	}
}
