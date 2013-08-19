using System.Linq;
using System.Collections.Generic;

namespace PioneerAvrControlLib.DataSources {
	public class NullDataSource : DataSource {
        public override string Type {
            get { return "NullDataSource"; }
        }

        public override void Start() {
        }

        public void Dispatch(IEnumerable<byte> data) {
            OnDataReceived(data.ToArray());
        }
    }
}
   