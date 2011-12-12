using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace PioneerAvrControlLib {
	class TCPConnection : PioneerConnection {
		TcpClient tcp;
		IPEndPoint ipe;
		NetworkStream stream;
		byte[] readBuf = new byte[1024];

		public TCPConnection(string ip, int port = 23)
			: this(new IPEndPoint(IPAddress.Parse(ip), port)) {
		}

		public TCPConnection(IPEndPoint ip) {
			this.ipe = ip;
		}

		protected override void Open() {
			tcp.Connect(ipe);
			stream = tcp.GetStream();
			stream.BeginRead(readBuf, 0, readBuf.Length, OnDataReceived, null);
		}

		void OnDataReceived(IAsyncResult r) {
			int bytesRead = stream.EndRead(r);
			byte[] b = new byte[bytesRead];
			Array.Copy(readBuf, b, bytesRead);
			AddToBuffer(b);
		}

		protected override void Write(IEnumerable<byte> b) {
			var arr = b.ToArray();
			stream.Write(arr, 0, arr.Length);
		}

	}
}
