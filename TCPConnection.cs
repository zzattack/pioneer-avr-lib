using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace PioneerAvrControlLib {
	public class TCPConnection : PioneerConnection {
		TcpClient tcp;
		NetworkStream stream;
		byte[] readBuf = new byte[1024];
		string ip;
		int port;

		public TCPConnection(string ip, int port = 23) {
			this.ip = ip;
			this.port = port;
			this.tcp = new TcpClient();
		}
		
		public override void Open() {
			tcp.Connect(ip, port);
			stream = tcp.GetStream();
			stream.BeginRead(readBuf, 0, readBuf.Length, OnDataReceived, null);
		}

		void OnDataReceived(IAsyncResult r) {
			int bytesRead = stream.EndRead(r);
			byte[] b = new byte[bytesRead];
			Array.Copy(readBuf, b, bytesRead);
			AddToBuffer(b);
			
			stream.BeginRead(readBuf, 0, readBuf.Length, OnDataReceived, null);
		}

		protected override void Write(IEnumerable<byte> b) {
			var arr = b.ToArray();
			stream.Write(arr, 0, arr.Length);
		}

	}
}
