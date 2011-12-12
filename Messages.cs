using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PioneerAvrControlLib {

	public enum MessageType {
		// ------------------------------
		PO, // PowerOn
		PF, // PowerOff
		PZ, // PowerToggle
		P, // Power?
		PWR, // Power!
		// ------------------------------
		VU,	// Volume up
		VD,	// Volume down
		VL,	// Volume set
		V,	// Volume?
		VOL,	// Volume!
		// ------------------------------
		MO,	// Mute on
		MF,	// Mute off
		MZ,	// Mute toggle
		M,	// Mute?
		MUT,// Mute!
		// ------------------------------
		FN,	// Input change, also input!
		FU,	// Input change to next 
		FD,	// Input change to previous 
		F,	// Input? 
		// ------------------------------
		SR,	// Listening mode set
		S,	// Listening mode?
		L,	// Playing listening mode
		// ------------------------------
		LM,	// Listening mode 
		SPK,	// Speakers
		HO,	// Hdmi output select
		MC,	// Mcacc memory
		IS,	// Phase control  
		TO,	// Tone  
		BA,	// Bass  
		TR,	// Treble  
		HA,	// Hdmi audio
		PR,	// Tuner preset 
		FR,	// Tuner frequency
		XM,	// Xm channel
		SIR,	// Sirius channel
		APR,	// Zone 2 power  
		BPR,	// Zone 3 power  
		ZV,	// Zone 2 volume  
		YV,	// Zone 3 volume  
		Z2MUT,	// Zone 2 mute
		Z3MUT,	// Zone 3 mute
		Z2F,	// Zone 2 input
		Z3F,	// Zone 3 input
		PQ,	// Pqls
		CLV,	// Ch level
		VSB,	// Virtual sb
		VHT,	// Virtual height
		FL,	// Fl display information
		RGB,	// Input name information
		SSA,	// Operatiom mode
		ATA,	// Sound retriever
		VTA,	// Video parameter prohibition information
		GBS,	// Response sirius meta data
		GCS,
		GDS,
		GES,
		GBI,	// Response ipod meta data
		GCI,
		GDI,
		GEI,
		GBH,	// Response home media gallery meta data
		GCH,
		GDH,
		GEH,
		SAA,	// Dimmer
		SAB,	// Sleep
		SDA,	// Signal select
		SDB,	// Analog input att
		ATC,	// Eq
		ATD,	// Standing wave
		ATE,	// Contents phase control
		ATF,	// Sound delay
		ATG,	// Digital noise reduction
		ATH,	// Dialog enhacement
		ATI,	// Hi-bit
		ATJ,	// Dual mono
		ATK,	// Fixed pcm
		ATL,	// Drc
		ATM,	// Lfe att
		ATN,	// Sacd gain
		ATO,	// Auto delay
		ATP,	// Center width (pl2 music option)
		ATQ,	// Panorama (pl2 music option)
		ATR,	// Dimension (pl2 music option)
		ATS,	// Center image (neo:6 option)
		ATT,	// Effect
		ATU,	// Height gain (pl2z height option)
		ATV,	// Digital filter
		VDP,	// Virtual depth
		VTB,	// Video converter
		VTC,	// Resolution
		VTD,	// Pure cinema
		VTE,	// Prog. Motion
		VTF,	// Stream smoother
		VTG,	// Advanced video adjust
		VTH,	// Ynr
		VTI,	// Cnr
		VTJ,	// Bnr
		VTK,	// Mnr
		VTL,	// Detail
		VTM,	// Sharpness
		VTN,	// Brightness
		VTO,	// Contrast
		VTP,	// Hue
		VTQ,	// Chroma level
		VTR,	// Black setup
		VTS,	// Aspect
		CPL,	// Control panel
		SSD,	// Start full auto mcacc
		SSC,	// Input function's assign
		SSE,	// Osd language
		SSF,	// Speaker system
		SSG,	// Speaker setting
		SSH,	// Fl demo mode	
		STS,	// Status display
		APA,	// Audio parameter
		VPA,	// Video parameter
		HM,	// Home menu
		TQ,	// Tuner preset name
		SSN,	// Hdmi setting		
	}

	public abstract class PioneerMessage {
		public abstract MessageType Type { get; }
		protected List<string> parameters = new List<string>();
		protected PioneerMessage() { }
	}

	public abstract class PioneerResponseMessage : PioneerMessage {
		protected string response;
		static string[] names = Enum.GetNames(typeof(MessageType));

		protected PioneerResponseMessage(string message) {
			if (!message.StartsWith(Type.ToString()))
				throw new ArgumentException("Invalid message header");
			else {
				string typeName = names.FirstOrDefault(n => message.StartsWith(n));
				if (string.IsNullOrEmpty(typeName))
					throw new ArgumentException("Invalid message header");
				var type = (MessageType)Enum.Parse(typeof(MessageType), typeName);
				if (type != this.Type)
					throw new ArgumentException("Invalid message type");
			}
		}
	}

	public abstract class PioneerRequestMessage : PioneerMessage {
		public virtual string Serialize() {
			// format: ?<type><param1><param2>...<crlf>
			StringBuilder sb = new StringBuilder();
			sb.Append('?');
			sb.Append(this.Type.ToString());
			foreach (string s in parameters)
				sb.Append(s);
			sb.Append("\r\n");
			return sb.ToString();
		}
	}

	public abstract class PioneerCommandMessage : PioneerMessage {
		public virtual string Serialize() {
			// format: <param1><param2>...<type><crlf>
			StringBuilder sb = new StringBuilder();
			foreach (string s in parameters)
				sb.Append(s);
			sb.Append(this.Type.ToString());
			sb.Append("\r\n");
			return sb.ToString();
		}
	}


	#region Power messages
	public class PowerOn : PioneerCommandMessage {
		public override MessageType Type {
			get { return MessageType.PO; }
		}
	}
	public class PowerOff : PioneerCommandMessage {
		public override MessageType Type {
			get { return MessageType.PF; }
		}
	}
	public class PowerToggle : PioneerCommandMessage {
		public override MessageType Type {
			get { return MessageType.PZ; }
		}
	}
	public class PowerStatusRequest : PioneerRequestMessage {
		public override MessageType Type {
			get { return MessageType.P; }
		}
	}
	public class PowerStatusResponse : PioneerResponseMessage {
		public override MessageType Type {
			get { return MessageType.PWR; }
		}
		public bool IsOn {
			get { return parameters[0] == "1"; }
		}
		public PowerStatusResponse(string message)
			: base(message) {
			parameters.Add(message.Substring(Type.ToString().Length, 1));
		}
	}
	#endregion

	#region Volume messages
	public class VolumeUp : PioneerCommandMessage {
		public override MessageType Type {
			get { return MessageType.VU; }
		}
	}
	public class VolumeDown : PioneerCommandMessage {
		public override MessageType Type {
			get { return MessageType.VD; }
		}
	}
	public class VolumeSet : PioneerCommandMessage {
		public override MessageType Type {
			get { return MessageType.VL; }
		}
		public VolumeSet(int volume)
			: base() {
			if (0 < volume || volume > 185)
				throw new ArgumentException("Volume must be between 0 (-80dB) and 185 (+12dB)");
			this.parameters.Add(volume.ToString().PadLeft(3, '0'));
		}
	}
	public class VolumeRequest : PioneerRequestMessage {
		public override MessageType Type {
			get { return MessageType.V; }
		}
	}
	public class VolumeStatusResponse : PioneerResponseMessage {
		public override MessageType Type {
			get { return MessageType.VOL; }
		}
		public int Volume {
			get { return int.Parse(parameters[0]); }
		}
		public VolumeStatusResponse(string message)
			: base(message) {
			parameters.Add(message.Substring(Type.ToString().Length, 3));
		}
	}
	#endregion

	#region Mute messages
	public class MuteOn : PioneerCommandMessage {
		public override MessageType Type {
			get { return MessageType.MO; }
		}
	}
	public class MuteOff : PioneerCommandMessage {
		public override MessageType Type {
			get { return MessageType.MF; }
		}
	}
	public class MuteToggle : PioneerCommandMessage {
		public override MessageType Type {
			get { return MessageType.MZ; }
		}
	}
	public class MuteStatusRequest : PioneerRequestMessage {
		public override MessageType Type {
			get { return MessageType.M; }
		}
	}
	public class MuteStatusResponse : PioneerResponseMessage {
		public override MessageType Type {
			get { return MessageType.VOL; }
		}
		public bool IsOn {
			get { return parameters[0] == "1"; }
		}
		public MuteStatusResponse(string message)
			: base(message) {
			parameters.Add(message.Substring(Type.ToString().Length, 1));
		}
	}
	#endregion

	#region Input messages
	public enum InputType {
		PHONO = 0,
		CD = 1,
		TUNER = 2,
		CDR_TAPE = 3,
		DVD = 4,
		TV_SAT = 5,
		VIDEO_1 = 10,
		VIDEO_2 = 14,
		DVR_BDR = 15,
		IPOD_USB = 17,
		XM_RADIO = 18,
		HDMI_1 = 19,
		HDMI_2 = 20,
		HDMI_3 = 21,
		HDMI_4 = 22,
		HDMI_5 = 23,
		BD = 25,
		HOME_MEDIA_GALLERY = 26,
		SIRIUS = 27,
		HDMI_CYCLIC = 31,
		ADAPTER_PORT = 33
	}

	public class InputTypeChange : PioneerCommandMessage {
		public override MessageType Type {
			get { return MessageType.FN; }
		}
		public InputTypeChange(InputType input) {
			this.parameters.Add(input.ToString().PadLeft(2, '0'));
		}
	}
	public class InputTypeChangeNext : PioneerCommandMessage {
		public override MessageType Type {
			get { return MessageType.FU; }
		}
	}
	public class InputTypeChangePrevious : PioneerCommandMessage {
		public override MessageType Type {
			get { return MessageType.FD; }
		}
	}
	public class InputTypeRequest : PioneerCommandMessage {
		public override MessageType Type {
			get { return MessageType.F; }
		}
	}
	public class InputTypeResponse : PioneerResponseMessage {
		public override MessageType Type {
			get { return MessageType.FN; }
		}
		public InputType Input {
			get { return (InputType)int.Parse(parameters[0]); }
		}
		public InputTypeResponse(string message)
			: base(message) {
			parameters.Add(message.Substring(Type.ToString().Length, 1));
		}
	}
	#endregion

	#region Listening mode messages
	enum ListeningMode : ushort {
		Stereo = 0x011,
	}
	enum PlayingListeningMode : ushort {
		Stereo = 0x011,
	}
	public class ListeningModeSet : PioneerCommandMessage {
		public override MessageType Type {
			get { return MessageType.SR; }
		}
		public ListeningModeSet(ListeningMode mode) {
			parameters.Add(mode.ToString("X2").PadLeft(4, '0'));
		}
	}
	public class ListeningModeRequest : PioneerCommandMessage {
		public override MessageType Type {
			get { return MessageType.S; }
		}
	}
	public class PlayingListeningModeSet : PioneerCommandMessage {
		public override MessageType Type {
			get { return MessageType.LR; }
		}
		public PlayingListeningModeSet(PlayingListeningMode mode) {
			parameters.Add(mode.ToString("X2").PadLeft(4, '0'));
		}
	}
	public class PlayingListeningModeRequest : PioneerCommandMessage {
		public override MessageType Type {
			get { return MessageType.L; }
		}
	}
	#endregion

	#region Tone control messages
	#endregion

	#region DSP function messages
	#endregion

	#region AMP function messages
	#endregion

	#region Key lock messages
	#endregion

	#region Cursor operation messages
	#endregion

	#region Video function messages
	#endregion

	#region Zone power messages
	#endregion

	#region Zone input messages
	#endregion

	#region Zone volume messages
	#endregion

	#region Zone mute messages
	#endregion

	#region Tuner messages
	#endregion

	#region XM radio operation messages (USA model only)
	#endregion

	#region Sirius Operation messages (USA model only)
	#endregion

	#region iPod operation messages
	#endregion

	#region Home Media Gallery operation messages
	#endregion

	#region Adapter port operation messages
	#endregion

	#region Error message messages
	#endregion

	#region Keyboard operation messages
	#endregion


	#region Information request messages
	#endregion














}