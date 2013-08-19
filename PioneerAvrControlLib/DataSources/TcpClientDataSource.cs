using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Xml;

namespace PioneerAvrControlLib.DataSources {
	public class TcpClientDataSource : WritableDataSource {
		private TcpClient _tcp;
		private NetworkStream _stream;
		private readonly byte[] _readBuf = new byte[1024];

		public IPAddress IP { get { return _endPoints[_currentEndPointIdx].Address; } }
		public int Port { get { return _endPoints[_currentEndPointIdx].Port; } }
		private int _currentEndPointIdx = 0;
		private readonly List<IPEndPoint> _endPoints = new List<IPEndPoint>();

		public TcpClientDataSource(string ip, int port = 6498) {
			_endPoints.Add(new IPEndPoint(IPAddress.Parse(ip), port));
		}
		public TcpClientDataSource(IEnumerable<IPEndPoint> endpoints) {
			_endPoints.AddRange(endpoints);
		}

		private void CycleEndpoints() {
			_currentEndPointIdx = (_currentEndPointIdx + 1)%_endPoints.Count;
		}

		public override string Type {
			get { return "TCP datasource"; }
		}

		public override void Start() {
			IgnoreReconnectBehavior = false;
			try {
				if (_stream != null) {
					_stream.Dispose();
					_stream = null;
				}
				if (_tcp != null) {
					if (_tcp.Connected)
						_tcp.Close();
					_tcp = null;
				}

				if (_tcp == null) {
					_tcp = new TcpClient();
					_tcp.BeginConnect(IP, Port, OnConnect, null);
				}
			}
			catch {
				Logger.Error("TCP connection failed");
				ApplyReconnectBehavior();
			}

			base.Start(); // start UniversalLogfileWriter
		}

		public override void Stop() {
			IgnoreReconnectBehavior = true;

			if (_stream != null) {
				_stream.Close();
				_stream.Dispose();
			}
			if (_tcp != null) {
				_tcp.Close();
			}

			_tcp = null;
			_stream = null;
			_isEnabled = false;

			Logger.Info("Stopping TCP DataProvider");
			OnConnectionLost();
			CycleEndpoints();

			base.Stop();
		}

		public override void Dispose() {
			Stop();
			base.Dispose();
		}

		public void OnConnect(IAsyncResult iar) {
			try {
				if (_tcp != null) {
					_tcp.EndConnect(iar);
					_stream = _tcp.GetStream();
					_stream.BeginRead(_readBuf, 0, _readBuf.Length, TcpOnDataReceived, null);
					_isEnabled = true;
					Logger.Success("ConnectionEstablished TCP socket");
					OnConnectionEstablished();
				}
			}
			catch (SocketException) {
				Logger.Error("Couldn't connect TCP socket");
				ApplyReconnectBehavior();
			}
		}

		private void TcpOnDataReceived(IAsyncResult r) {
			if (_stream == null)
				return;

			try {
				int bytesRead = _stream.EndRead(r);

				if (bytesRead == 0) {
					Logger.Error("No bytes read! Connection aborted?");
					if (_stream != null)
						_stream.Close();
					if (_tcp != null)
						_tcp.Close();

					Stop();

					ApplyReconnectBehavior();
				}
				else {
					byte[] b = new byte[bytesRead];
					Array.Copy(_readBuf, b, bytesRead);
					OnDataReceived(b);
					_stream.BeginRead(_readBuf, 0, _readBuf.Length, TcpOnDataReceived, null);
				}
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
		}

		#region xml serializing/deserializing

		public static DataSource LoadFrom(XmlElement node) {
			return (new TcpClientDataSource(
				node["ip"].InnerText,
				int.Parse(node["port"].InnerText)
				)).LoadBaseSettings(node);
		}

		public override void SaveTo(XmlTextWriter xtr) {
			xtr.WriteStartElement("datasource");
			xtr.WriteAttributeString("type", "tcpclient");
			xtr.WriteElementString("ip", IP.ToString());
			xtr.WriteElementString("port", Port.ToString(CultureInfo.InvariantCulture));
			WriteBaseSettings(xtr);
			xtr.WriteEndElement();
		}

		#endregion

		public override void Write(byte[] data) {
			if (_stream != null && _stream.CanWrite)
				_stream.Write(data, 0, data.Length);
		}
	}
}
