using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Xml;

namespace PioneerAvrControlLib.DataSources {
	public class TcpServerDataSource : DataSource {
		TcpListener _listener;
		List<TcpClient> _clients = new List<TcpClient>();
		public int Port { get; set; }

		public TcpServerDataSource(int listingPort = 12912) {
			Port = listingPort;
			_listener = new TcpListener(IPAddress.Any, Port);
		}

		public override void Start() {
			try {
				_listener = new TcpListener(IPAddress.Any, Port);

				_listener.Start(999);
				_listener.BeginAcceptTcpClient(OnConnect, null);
			}
			catch (SocketException) {
				Logger.Error("Cannot start tcp listener");
			}

			base.Start(); // start universal log writer
		}

		public override void Stop() {
			if (_listener != null) {
				if (_listener.Server.Connected) 
					_listener.Server.Disconnect(false);
				_listener.Stop();
				_listener = null;

				lock (_clients) {
					foreach (var client in _clients) {
						try {
							if (client != null && client.Connected) {
								client.GetStream().Close();
								client.Close();
							}
						}
						catch (SocketException) { }
						catch (InvalidOperationException) { }
					}
					_clients.Clear();
				}
			}
			base.Stop();
		}

		public void OnConnect(IAsyncResult iar) {
			try {
				if (_listener == null)
					return;

				TcpClient client = _listener.EndAcceptTcpClient(iar);
				_listener.BeginAcceptTcpClient(OnConnect, null);

				lock (_clients) {
					_clients.Add(client);
				}

				var stream = client.GetStream();
				stream.BeginRead(_readBuf, 0, _readBuf.Length, TcpOnDataReceived, stream);
			}
			catch (SocketException) {
			}
			catch (InvalidOperationException) {
			}
		}

		byte[] _readBuf = new byte[1024];
		void TcpOnDataReceived(IAsyncResult r) {
			var stream = r.AsyncState as NetworkStream;
			if (stream == null) return;

			int bytesRead = 0;
			try {
				bytesRead = stream.EndRead(r);
			}
			catch (IOException) {
				Logger.Error("IOException in TcpOnDataReceived");
			}
			catch (InvalidOperationException) {
				Logger.Error("InvalidOperationException in TcpOnDataReceived");
			}
			catch (NullReferenceException) {
				Logger.Error("NullReferenceException in TcpOnDataReceived");
			}
			if (bytesRead == 0) {
				Logger.Error("No bytes read! Connection aborted?");
				stream.Close();

				ApplyReconnectBehavior();
			}
			else {
				byte[] b = new byte[bytesRead];
				Array.Copy(_readBuf, b, bytesRead);
				OnDataReceived(b);
				stream.BeginRead(_readBuf, 0, _readBuf.Length, TcpOnDataReceived, stream);
			}
		}

		public override string Type {
			get { return "TcpServer"; }
		}

		#region xml serializing/deserializing
		public static DataSource LoadFrom(XmlElement node) {
			return (new TcpServerDataSource(
				int.Parse(node["port"].InnerText)
			)).LoadBaseSettings(node);
		}

		public override void SaveTo(XmlTextWriter xtr) {
			xtr.WriteStartElement("datasource");
			xtr.WriteAttributeString("type", "tcpserver");
			xtr.WriteElementString("port", Port.ToString(CultureInfo.InvariantCulture));
			WriteBaseSettings(xtr);
			xtr.WriteEndElement();
		}
		#endregion

	}
}
