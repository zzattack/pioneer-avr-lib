using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace PioneerAvrControlLib {
	public class PioneerTCPConnection : PioneerConnection {
		TcpClient _tcp;
		NetworkStream _stream;
		readonly byte[] _readBuf = new byte[1024];
		readonly string _ip;
		readonly int _port;

		Queue<byte[]> _writeBuffer = new Queue<byte[]>();
		AutoResetEvent _writeEvent = new AutoResetEvent(false);
		Thread _writeThread;
		private Timer _t;

		public PioneerTCPConnection(string ip, int port = 23) {
			_ip = ip;
			_port = port;
			_writeThread = new Thread(WriteThread);
			_writeThread.Start();
		}

		public override void Open() {
			try {
				_tcp = new TcpClient();
				_tcp.BeginConnect(_ip, _port, OnConnect, null);
			}
			catch {
				ScheduleReconnect();
			}
		}

		public void OnConnect(IAsyncResult iar) {
			try {
				if (_tcp != null) {
					_tcp.EndConnect(iar);
					_stream = _tcp.GetStream();
					_stream.BeginRead(_readBuf, 0, _readBuf.Length, OnDataReceived, null);
					_writeEvent.Set();
				}
			}
			catch {
				ScheduleReconnect();
			}
		}

		void OnDataReceived(IAsyncResult r) {
			int bytesRead = 0;
			try {
				bytesRead = _stream.EndRead(r);
			}
			catch (IOException) { }
			catch (InvalidOperationException) { }
			if (bytesRead == 0) {
				if (_stream != null) _stream.Close();
				if (_tcp != null) _tcp.Close();
				ScheduleReconnect();
			}
			else {
				byte[] b = new byte[bytesRead];
				Array.Copy(_readBuf, b, bytesRead);
				AddToBuffer(b);
				_stream.BeginRead(_readBuf, 0, _readBuf.Length, OnDataReceived, null);
			}
		}

		private void ScheduleReconnect() {
			// schedule reconnect in 5 seconds
			if (_stream != null) {
				_stream.Dispose();
				_stream = null;
			}
			if (_tcp != null && _tcp.Connected) {
				_tcp.Close();
			}

			_t = new Timer(delegate {
				Open();
			}, null, new TimeSpan(30000000), new TimeSpan(-1));
		}


		protected override void Write(IEnumerable<byte> b) {
			lock (_writeBuffer) {
				_writeBuffer.Enqueue(b.ToArray());
			}
			_writeEvent.Set();
		}

		bool die = false;
		void WriteThread() {
			while (!die) {
				_writeEvent.WaitOne();

				bool continueToPush = false;
				do {
					byte[] toWrite = null;
					lock (_writeBuffer) {
						continueToPush = _writeBuffer.Count > 0;
						if (continueToPush)
							toWrite = _writeBuffer.Dequeue();
					}
					if (_stream == null || toWrite == null) continue;
					try {
						_stream.Write(toWrite, 0, toWrite.Length);
					}
					catch {
						_stream = null;
						ScheduleReconnect();
					}
				}
				while (continueToPush);
			}
		}

	}
}
