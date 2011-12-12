﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PioneerAvrControlLib {
	abstract class PioneerConnection {

		protected abstract void Open();
		protected abstract void Write(IEnumerable<byte> b);

		List<byte> rawBuff = new List<byte>();
		const byte ETX = (byte)'\n';

		protected void AddToBuffer(IEnumerable<byte> buff) {
			foreach (byte b in buff) {
				rawBuff.Add(b);

				if (b == ETX) {
					ProcesBuffer();
					rawBuff.Clear();
				}
			}
		}

		private void ProcesBuffer() {
			PioneerMessage mess = PioneerMessageFactory.Deserialize(rawBuff);
		}

	}
}