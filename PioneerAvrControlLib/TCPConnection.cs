using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace PioneerAvrControlLib {
	public class PioneerTCPConnection : PioneerConnection {
		TcpClient tcp;
		NetworkStream stream;
		byte[] readBuf = new byte[1024];
		string ip;
		int port;

		Queue<byte[]> writeBuffer = new Queue<byte[]>();
		AutoResetEvent writeEvent = new AutoResetEvent(false);
		Thread writeThread;

		public PioneerTCPConnection(string ip, int port = 23) {
			this.ip = ip;
			this.port = port;
			this.tcp = new TcpClient();
			this.writeThread = new Thread(WriteThread);
			this.writeThread.Start();
		}

		public override void Open() {
			tcp.Connect(ip, port);
			stream = tcp.GetStream();
			stream.BeginRead(readBuf, 0, readBuf.Length, OnDataReceived, null);
		}

		void OnDataReceived(IAsyncResult r) {
			int bytesRead;
			try {
				bytesRead = stream.EndRead(r);
			}
			catch {
				tcp.Close();
				stream.Dispose();
				Open();
				return;
			}
			byte[] b = new byte[bytesRead];
			Array.Copy(readBuf, b, bytesRead);			
			AddToBuffer(b);

			stream.BeginRead(readBuf, 0, readBuf.Length, OnDataReceived, null);
		}

		protected override void Write(IEnumerable<byte> b) {
			lock (writeBuffer) {
				writeBuffer.Enqueue(b.ToArray());
			}
			writeEvent.Set();
		}

		bool die = false;
		void WriteThread() {
			while (!die) {
				writeEvent.WaitOne();
				bool continueToPush = false;
				do {
					byte[] toWrite = null;
					lock (writeBuffer) {
						continueToPush = writeBuffer.Count > 0;
						if (continueToPush)
							toWrite = writeBuffer.Dequeue();
					}
					if (toWrite != null)
						stream.Write(toWrite, 0, toWrite.Length);
				}
				while (continueToPush);
			}
		}

	}
}
