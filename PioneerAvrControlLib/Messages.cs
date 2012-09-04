using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace PioneerAvrControlLib.Message {

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
		LM,	// Playing listening mode
		L,	// Playing listening mode?
		// ------------------------------
		TO, // Tone!, Tone?
		BI, // BASS increment
		BD, // BASS decrement
		BA, // BASS?, BASS!
		TI, // Treble increment
		TD, // Treble decrement
		TR, // Treble set
		// ------------------------------
		SPK,// Speakers
		HO,	// Hdmi output select
		MC,	// Mcacc memory
		IS,	// Phase control  
		HA,	// Hdmi audio
		// ------------------------------
		TFI, // tuner freq increment
		TFD, // tuner freq decrement
		TPI, // tuner preset increment
		TPD, // tuner freq decrement
		TB, // tuner band
		TC, // tuner class
		TAC, // tuner access
		TP, // tuner preset?
		PR,	// Tuner preset !
		FR,	// Tuner frequency!
		TN,	// Tuner command
		// ------------------------------
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
		AST,	// Audio info
		VST,	// Video info
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
		HM,	    // Home menu
		TQ,	    // Tuner preset name
		SSN,    // Hdmi setting
		// ------------------------------	
		R, // Command ok
		E04, // Command error
		E06, // Parameter error
		B00, // Busy
		// ------------------------------
	}

	public abstract class PioneerMessage {
		public abstract MessageType Type { get; }
		protected List<string> parameters = new List<string>();
		public abstract string Serialize();
	}

	public abstract class PioneerResponseMessage : PioneerMessage {
		protected string response;
		static string[] names = Enum.GetNames(typeof(MessageType)).OrderBy(n => -n.Length).ToArray();

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
		protected PioneerResponseMessage() { }

		public override string Serialize() {
			throw new InvalidOperationException("Tried to serialize a response message");
		}
	}

	public abstract class PioneerRequestMessage : PioneerMessage {
		public override string Serialize() {
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
		public override string Serialize() {
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
		public PowerStatusResponse() { }
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
			if (0 > volume || volume > 185)
				throw new ArgumentException("Volume must be between 0 (-80dB) and 185 (+12dB)");
			this.parameters.Add(volume.ToString().PadLeft(3, '0'));
		}
		public VolumeSet() { }
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
		public VolumeStatusResponse() { }
		public override string ToString() {
			return "Volume: " + (12.0 - Volume * 0.5).ToString() + "dB";
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
		public MuteStatusResponse() { }
	}
	#endregion

	#region Input messages
	public enum InputType {
		Phono = 0,
		CD = 1,
		Tuner = 2,
		CDr_Tape = 3,
		DVD = 4,
		TV_Satelite = 5,
		Video_1 = 10,
		Video_2 = 14,
		DVR_BDR = 15,
		IPod_USB = 17,
		XM_RADIO = 18,
		HDMI_1 = 19,
		HDMI_2 = 20,
		HDMI_3 = 21,
		HDMI_4 = 22,
		HDMI_5 = 23,
		BD = 25,
		Home_Media_Gallery = 26,
		Sirius = 27,
		HDMI_Cyclic = 31,
		Adapter_Port = 33
	}

	public class InputTypeChange : PioneerCommandMessage {
		public override MessageType Type {
			get { return MessageType.FN; }
		}
		public InputTypeChange(InputType input) {
			this.parameters.Add(input.ToString().PadLeft(2, '0'));
		}
		public InputTypeChange() { }
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
	public class InputTypeRequest : PioneerRequestMessage {
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
			parameters.Add(message.Substring(Type.ToString().Length, 2));
		}
		public InputTypeResponse() { }
	}
	#endregion

	#region Listening mode messages
	public enum ListeningMode : ushort {
		Neo6_CINEMA = 0x0108,
		Neo6_MUSIC = 0x0109,
		XM_HD_Surround = 0x010a,
		NeoX_CINEMA = 0x0111,
		NeoX_MUSIC = 0x0112,
		NeoX_GAME = 0x0113,
		NEURAL_SURROUND_CINEMA = 0x0114,
		NEURAL_SURROUND_MUSIC = 0x0115,
		NEURAL_SURROUND_GAMES = 0x0116,
		DTS_Neo6_DTSHD_Neo6 = 0x1104,
		ES_NeoX = 0x110c,
		PLIIx_MOVIE_THX = 0x0301,
		PLII_MOVIE_THX = 0x0302,
		PL_THX_CINEMA = 0x0303,
		Neo6_CINEMA_THX = 0x0304,
		THX_CINEMA = 0x0305,
		PLIIx_MUSIC_THX = 0x0306,
		PLII_MUSIC_THX = 0x0307,
		PL_THX_MUSIC = 0x0308,
		Neo6_MUSIC_THX = 0x0309,
		THX_MUSIC = 0x030a,
		PLIIx_GAME_THX = 0x030b,
		PLII_GAME_THX = 0x030c,
		PL_THX_GAMES = 0x030d,
		THX_ULTRA2_GAMES = 0x030e,
		THX_SELECT2_GAMES = 0x030f,
		THX_GAMES = 0x0310,
		PLIIz_THX_CINEMA = 0x0311,
		PLIIz_THX_MUSIC = 0x0312,
		PLIIz_THX_GAMES = 0x0313,
		NeoX_CINEMA_THX_CINEMA = 0x0314,
		NeoX_MUSIC_THX_MUSIC = 0x0315,
		NeoX_GAMES_THX_GAMES = 0x0316,
		THX_Surr_EX = 0x1301,
		Neo6_THX_CINEMA = 0x1302,
		ES_MTRX_THX_CINEMA = 0x1303,
		ES_DISC_THX_CINEMA = 0x1304,
		ES_8ch_THX_CINEMA = 0x1305,
		THX_ULTRA2_CINEMA = 0x1307,
		THX_SELECT2_CINEMA = 0x1308,
		Neo6_THX_MUSIC = 0x130a,
		ES_MTRX_THX_MUSIC = 0x130b,
		ES_DISC_THX_MUSIC = 0x130c,
		ES_8ch_THX_MUSIC = 0x130d,
		THX_ULTRA2_MUSIC = 0x130f,
		THX_SELECT2_MUSIC = 0x1310,
		Neo6_THX_GAMES = 0x1312,
		ES_MTRX_THX_GAMES = 0x1313,
		ES_DISC_THX_GAMES = 0x1314,
		ES_8ch_THX_GAMES = 0x1315,
		EX_THX_GAMES = 0x1316,
		NeoX_THX_CINEMA = 0x131d,
		NeoX_THX_MUSIC = 0x131e,
		NeoX_THX_GAMES = 0x131f,
		OPTIMUM = 0x0881,
		MULTI_CH_IN = 0x0f01,
	}

	public class ListeningModeSet : PioneerCommandMessage {
		public override MessageType Type {
			get { return MessageType.SR; }
		}
		public ListeningModeSet(ListeningMode mode) {
			parameters.Add(mode.ToString("X").PadLeft(4, '0'));
		}
		public ListeningModeSet() { }
	}
	public class ListeningModeRequest : PioneerRequestMessage {
		public override MessageType Type {
			get { return MessageType.S; }
		}
	}
	public class ListeningModeResponse : PioneerResponseMessage {
		public override MessageType Type {
			get { return MessageType.SR; }
		}
		public ListeningModeResponse() { }
		public ListeningModeResponse(string message)
			: base(message) {
			parameters.Add(message.Substring(Type.ToString().Length, 4));
		}
		public ListeningMode ListeningMode {
			get {
				return (ListeningMode)int.Parse(parameters[0], NumberStyles.HexNumber);
			}
		}
		public string ListeningModeString {
			get {
				int mode = int.Parse(parameters[0], NumberStyles.HexNumber);
				return ListeningModeToString(mode);
			}
		}

		public static string ListeningModeToString(int mode) {
			if (mode == 0x0001) return "STEREO";
			else if (mode == 0x0010) return "STANDARD";
			else if (mode == 0x0009) return "STEREO";
			else if (mode == 0x0011) return "(2ch source)";
			else if (mode == 0x0013) return "PRO LOGIC2 MOVIE";
			else if (mode == 0x0018) return "PRO LOGIC2x MOVIE";
			else if (mode == 0x0014) return "PRO LOGIC2 MUSIC";
			else if (mode == 0x0019) return "PRO LOGIC2x MUSIC";
			else if (mode == 0x0015) return "PRO LOGIC2 GAME";
			else if (mode == 0x0020) return "PRO LOGIC2x GAME";
			else if (mode == 0x0031) return "PRO LOGIC2z HEIGHT";
			else if (mode == 0x0032) return "WIDE SURROUND MOVIE";
			else if (mode == 0x0033) return "WIDE SURROUND MUSIC";
			else if (mode == 0x0012) return "PRO LOGIC";
			else if (mode == 0x0016) return "Neo:6 CINEMA";
			else if (mode == 0x0017) return "Neo:6 MUSIC";
			else if (mode == 0x0028) return "XM HD SURROUND";
			else if (mode == 0x0029) return "NEURAL SURROUND";
			else if (mode == 0x0037) return "Neo:X CINEMA";
			else if (mode == 0x0038) return "Neo:X MUSIC";
			else if (mode == 0x0039) return "Neo:X GAME";
			else if (mode == 0x0040) return "NEURAL SURROUND+Neo:X CINEMA";
			else if (mode == 0x0041) return "NEURAL SURROUND+Neo:X MUSIC";
			else if (mode == 0x0042) return "NEURAL SURROUND+Neo:X GAME";
			else if (mode == 0x0021) return "(Multi ch source)";
			else if (mode == 0x0022) return "(Multi ch source)+DOLBY EX";
			else if (mode == 0x0023) return "(Multi ch source)+PRO LOGIC2x MOVIE";
			else if (mode == 0x0024) return "(Multi ch source)+PRO LOGIC2x MUSIC";
			else if (mode == 0x0034) return "(Multi-ch Source)+PRO LOGIC2z HEIGHT";
			else if (mode == 0x0035) return "(Multi-ch Source)+WIDE SURROUND MOVIE";
			else if (mode == 0x0036) return "(Multi-ch Source)+WIDE SURROUND MUSIC";
			else if (mode == 0x0025) return "(Multi ch source)DTS-ES Neo:6";
			else if (mode == 0x0026) return "(Multi ch source)DTS-ES matrix";
			else if (mode == 0x0027) return "(Multi ch source)DTS-ES discrete";
			else if (mode == 0x0030) return "(Multi ch source)DTS-ES 8ch discrete";
			else if (mode == 0x0043) return "(Multi ch source)DTS-ES Neo:X";
			else if (mode == 0x0100) return "ADVANCED SURROUND (cyclic)";
			else if (mode == 0x0101) return "ACTION";
			else if (mode == 0x0103) return "DRAMA";
			else if (mode == 0x0102) return "SCI-FI";
			else if (mode == 0x0105) return "MONO FILM";
			else if (mode == 0x0104) return "ENTERTAINMENT SHOW";
			else if (mode == 0x0106) return "EXPANDED THEATER";
			else if (mode == 0x0116) return "TV SURROUND";
			else if (mode == 0x0118) return "ADVANCED GAME";
			else if (mode == 0x0117) return "SPORTS";
			else if (mode == 0x0107) return "CLASSICAL";
			else if (mode == 0x0110) return "ROCK/POP";
			else if (mode == 0x0109) return "UNPLUGGED";
			else if (mode == 0x0112) return "EXTENDED STEREO";
			else if (mode == 0x0003) return "Front Stage Surround Advance Focus";
			else if (mode == 0x0004) return "Front Stage Surround Advance Wide";
			else if (mode == 0x0153) return "RETRIEVER AIR";
			else if (mode == 0x0113) return "PHONES SURROUND";
			else if (mode == 0x0050) return "THX (cyclic)";
			else if (mode == 0x0051) return "PROLOGIC + THX CINEMA";
			else if (mode == 0x0052) return "PL2 MOVIE + THX CINEMA";
			else if (mode == 0x0053) return "Neo:6 CINEMA + THX CINEMA";
			else if (mode == 0x0054) return "PL2x MOVIE + THX CINEMA";
			else if (mode == 0x0092) return "PL2z HEIGHT + THX CINEMA";
			else if (mode == 0x0055) return "THX SELECT2 GAMES";
			else if (mode == 0x0068) return "THX CINEMA (for 2ch)";
			else if (mode == 0x0069) return "THX MUSIC (for 2ch)";
			else if (mode == 0x0070) return "THX GAMES (for 2ch)";
			else if (mode == 0x0071) return "PL2 MUSIC + THX MUSIC";
			else if (mode == 0x0072) return "PL2x MUSIC + THX MUSIC";
			else if (mode == 0x0093) return "PL2z HEIGHT + THX MUSIC";
			else if (mode == 0x0073) return "Neo:6 MUSIC + THX MUSIC";
			else if (mode == 0x0074) return "PL2 GAME + THX GAMES";
			else if (mode == 0x0075) return "PL2x GAME + THX GAMES";
			else if (mode == 0x0094) return "PL2z HEIGHT + THX GAMES";
			else if (mode == 0x0076) return "THX ULTRA2 GAMES";
			else if (mode == 0x0077) return "PROLOGIC + THX MUSIC";
			else if (mode == 0x0078) return "PROLOGIC + THX GAMES";
			else if (mode == 0x0201) return "Neo:X CINEMA + THX CINEMA";
			else if (mode == 0x0202) return "Neo:X MUSIC + THX MUSIC";
			else if (mode == 0x0203) return "Neo:X GAME + THX GAMES";
			else if (mode == 0x0056) return "THX CINEMA (for multi ch)";
			else if (mode == 0x0057) return "THX SURROUND EX (for multi ch)";
			else if (mode == 0x0058) return "PL2x MOVIE + THX CINEMA (for multi ch)";
			else if (mode == 0x0095) return "PL2z HEIGHT + THX CINEMA (for multi ch)";
			else if (mode == 0x0059) return "ES Neo:6 + THX CINEMA (for multi ch)";
			else if (mode == 0x0060) return "ES MATRIX + THX CINEMA (for multi ch)";
			else if (mode == 0x0061) return "ES DISCRETE + THX CINEMA (for multi ch)";
			else if (mode == 0x0067) return "ES 8ch DISCRETE + THX CINEMA (for multi ch)";
			else if (mode == 0x0062) return "THX SELECT2 CINEMA (for multi ch)";
			else if (mode == 0x0063) return "THX SELECT2 MUSIC (for multi ch)";
			else if (mode == 0x0064) return "THX SELECT2 GAMES (for multi ch)";
			else if (mode == 0x0065) return "THX ULTRA2 CINEMA (for multi ch)";
			else if (mode == 0x0066) return "THX ULTRA2 MUSIC (for multi ch)";
			else if (mode == 0x0079) return "THX ULTRA2 GAMES (for multi ch)";
			else if (mode == 0x0080) return "THX MUSIC (for multi ch)";
			else if (mode == 0x0081) return "THX GAMES (for multi ch)";
			else if (mode == 0x0082) return "PL2x MUSIC + THX MUSIC (for multi ch)";
			else if (mode == 0x0096) return "PL2z HEIGHT + THX MUSIC (for multi ch)";
			else if (mode == 0x0083) return "EX + THX GAMES (for multi ch)";
			else if (mode == 0x0097) return "PL2z HEIGHT + THX GAMES (for multi ch)";
			else if (mode == 0x0084) return "Neo:6 + THX MUSIC (for multi ch)";
			else if (mode == 0x0085) return "Neo:6 + THX GAMES (for multi ch)";
			else if (mode == 0x0086) return "ES MATRIX + THX MUSIC (for multi ch)";
			else if (mode == 0x0087) return "ES MATRIX + THX GAMES (for multi ch)";
			else if (mode == 0x0088) return "ES DISCRETE + THX MUSIC (for multi ch)";
			else if (mode == 0x0089) return "ES DISCRETE + THX GAMES (for multi ch)";
			else if (mode == 0x0090) return "ES 8CH DISCRETE + THX MUSIC (for multi ch)";
			else if (mode == 0x0091) return "ES 8CH DISCRETE + THX GAMES (for multi ch)";
			else if (mode == 0x0204) return "Neo:X + THX CINEMA (for multi ch)";
			else if (mode == 0x0205) return "Neo:X + THX MUSIC (for multi ch)";
			else if (mode == 0x0206) return "Neo:X + THX GAMES (for multi ch)";
			else if (mode == 0x0005) return "AUTO SURR/STREAM DIRECT (cyclic)";
			else if (mode == 0x0006) return "AUTO SURROUND";
			else if (mode == 0x0151) return "Auto Level Control (A.L.C.)";
			else if (mode == 0x0007) return "DIRECT";
			else if (mode == 0x0008) return "PURE DIRECT";
			else if (mode == 0x0152) return "OPTIMUM SURROUND";
			else return "";
		}
	}
	public class PlayingListeningModeRequest : PioneerRequestMessage {
		public override MessageType Type {
			get { return MessageType.L; }
		}
	}
	public class PlayingListeningModeResponse : PioneerResponseMessage {
		public override MessageType Type {
			get { return MessageType.LM; }
		}
		public PlayingListeningModeResponse() { }
		public PlayingListeningModeResponse(string message)
			: base(message) {
			parameters.Add(message.Substring(Type.ToString().Length, 4));
		}

		public int PlayListeningMode {
			get { return int.Parse(parameters[0], NumberStyles.HexNumber); }
		}

		public string PlayListeningModeString {
			get {
				int mode = int.Parse(parameters[0], NumberStyles.HexNumber);
				if (mode == 0x0101) return "[)(]PLIIx MOVIE";
				else if (mode == 0x0102) return "[)(]PLII MOVIE";
				else if (mode == 0x0103) return "[)(]PLIIx MUSIC";
				else if (mode == 0x0104) return "[)(]PLII MUSIC";
				else if (mode == 0x0105) return "[)(]PLIIx GAME";
				else if (mode == 0x0106) return "[)(]PLII GAME";
				else if (mode == 0x0107) return "[)(]PROLOGIC";
				else if (mode == 0x0108) return "Neo:6 CINEMA";
				else if (mode == 0x0109) return "Neo:6 MUSIC";
				else if (mode == 0x010a) return "XM HD Surround";
				else if (mode == 0x010b) return "NEURAL SURR  ";
				else if (mode == 0x010c) return "2ch Straight Decode";
				else if (mode == 0x010d) return "[)(]PLIIz HEIGHT";
				else if (mode == 0x010e) return "WIDE SURR MOVIE";
				else if (mode == 0x010f) return "WIDE SURR MUSIC";
				else if (mode == 0x0110) return "STEREO";
				else if (mode == 0x0111) return "Neo:X CINEMA";
				else if (mode == 0x0112) return "Neo:X MUSIC";
				else if (mode == 0x0113) return "Neo:X GAME";
				else if (mode == 0x0114) return "NEURAL SURROUND+Neo:X CINEMA";
				else if (mode == 0x0115) return "NEURAL SURROUND+Neo:X MUSIC";
				else if (mode == 0x0116) return "NEURAL SURROUND+Neo:X GAMES";
				else if (mode == 0x1101) return "[)(]PLIIx MOVIE";
				else if (mode == 0x1102) return "[)(]PLIIx MUSIC";
				else if (mode == 0x1103) return "[)(]DIGITAL EX";
				else if (mode == 0x1104) return "DTS +Neo:6 / DTS-HD +Neo:6";
				else if (mode == 0x1105) return "ES MATRIX";
				else if (mode == 0x1106) return "ES DISCRETE";
				else if (mode == 0x1107) return "DTS-ES 8ch ";
				else if (mode == 0x1108) return "multi ch Straight Decode";
				else if (mode == 0x1109) return "[)(]PLIIz HEIGHT";
				else if (mode == 0x110a) return "WIDE SURR MOVIE";
				else if (mode == 0x110b) return "WIDE SURR MUSIC";
				else if (mode == 0x110c) return "ES Neo:X";
				else if (mode == 0x0201) return "ACTION";
				else if (mode == 0x0202) return "DRAMA";
				else if (mode == 0x0203) return "SCI-FI";
				else if (mode == 0x0204) return "MONOFILM";
				else if (mode == 0x0205) return "ENT.SHOW";
				else if (mode == 0x0206) return "EXPANDED";
				else if (mode == 0x0207) return "TV SURROUND";
				else if (mode == 0x0208) return "ADVANCEDGAME";
				else if (mode == 0x0209) return "SPORTS";
				else if (mode == 0x020a) return "CLASSICAL   ";
				else if (mode == 0x020b) return "ROCK/POP   ";
				else if (mode == 0x020c) return "UNPLUGGED   ";
				else if (mode == 0x020d) return "EXT.STEREO  ";
				else if (mode == 0x020e) return "PHONES SURR. ";
				else if (mode == 0x020f) return "FRONT STAGE SURROUND ADVANCE FOCUS";
				else if (mode == 0x0210) return "FRONT STAGE SURROUND ADVANCE WIDE";
				else if (mode == 0x0211) return "SOUND RETRIEVER AIR";
				else if (mode == 0x0301) return "[)(]PLIIx MOVIE +THX";
				else if (mode == 0x0302) return "[)(]PLII MOVIE +THX";
				else if (mode == 0x0303) return "[)(]PL +THX CINEMA";
				else if (mode == 0x0304) return "Neo:6 CINEMA +THX";
				else if (mode == 0x0305) return "THX CINEMA";
				else if (mode == 0x0306) return "[)(]PLIIx MUSIC +THX";
				else if (mode == 0x0307) return "[)(]PLII MUSIC +THX";
				else if (mode == 0x0308) return "[)(]PL +THX MUSIC";
				else if (mode == 0x0309) return "Neo:6 MUSIC +THX";
				else if (mode == 0x030a) return "THX MUSIC";
				else if (mode == 0x030b) return "[)(]PLIIx GAME +THX";
				else if (mode == 0x030c) return "[)(]PLII GAME +THX";
				else if (mode == 0x030d) return "[)(]PL +THX GAMES";
				else if (mode == 0x030e) return "THX ULTRA2 GAMES";
				else if (mode == 0x030f) return "THX SELECT2 GAMES";
				else if (mode == 0x0310) return "THX GAMES";
				else if (mode == 0x0311) return "[)(]PLIIz +THX CINEMA";
				else if (mode == 0x0312) return "[)(]PLIIz +THX MUSIC";
				else if (mode == 0x0313) return "[)(]PLIIz +THX GAMES";
				else if (mode == 0x0314) return "Neo:X CINEMA + THX CINEMA";
				else if (mode == 0x0315) return "Neo:X MUSIC + THX MUSIC";
				else if (mode == 0x0316) return "Neo:X GAMES + THX GAMES";
				else if (mode == 0x1301) return "THX Surr EX";
				else if (mode == 0x1302) return "Neo:6 +THX CINEMA";
				else if (mode == 0x1303) return "ES MTRX +THX CINEMA";
				else if (mode == 0x1304) return "ES DISC +THX CINEMA";
				else if (mode == 0x1305) return "ES 8ch +THX CINEMA ";
				else if (mode == 0x1306) return "[)(]PLIIx MOVIE +THX";
				else if (mode == 0x1307) return "THX ULTRA2 CINEMA";
				else if (mode == 0x1308) return "THX SELECT2 CINEMA";
				else if (mode == 0x1309) return "THX CINEMA";
				else if (mode == 0x130a) return "Neo:6 +THX MUSIC";
				else if (mode == 0x130b) return "ES MTRX +THX MUSIC";
				else if (mode == 0x130c) return "ES DISC +THX MUSIC";
				else if (mode == 0x130d) return "ES 8ch +THX MUSIC";
				else if (mode == 0x130e) return "[)(]PLIIx MUSIC +THX";
				else if (mode == 0x130f) return "THX ULTRA2 MUSIC";
				else if (mode == 0x1310) return "THX SELECT2 MUSIC";
				else if (mode == 0x1311) return "THX MUSIC";
				else if (mode == 0x1312) return "Neo:6 +THX GAMES";
				else if (mode == 0x1313) return "ES MTRX +THX GAMES";
				else if (mode == 0x1314) return "ES DISC +THX GAMES";
				else if (mode == 0x1315) return "ES 8ch +THX GAMES";
				else if (mode == 0x1316) return "[)(]EX +THX GAMES";
				else if (mode == 0x1317) return "THX ULTRA2 GAMES";
				else if (mode == 0x1318) return "THX SELECT2 GAMES";
				else if (mode == 0x1319) return "THX GAMES";
				else if (mode == 0x131a) return "[)(]PLIIz +THX CINEMA";
				else if (mode == 0x131b) return "[)(]PLIIz +THX MUSIC";
				else if (mode == 0x131c) return "[)(]PLIIz +THX GAMES";
				else if (mode == 0x131d) return "Neo:X + THX CINEMA";
				else if (mode == 0x131e) return "Neo:X + THX MUSIC";
				else if (mode == 0x131f) return "Neo:X + THX GAMES";
				else if (mode == 0x0401) return "STEREO";
				else if (mode == 0x0402) return "[)(]PLII MOVIE";
				else if (mode == 0x0403) return "[)(]PLIIx MOVIE";
				else if (mode == 0x0404) return "Neo:6 CINEMA";
				else if (mode == 0x0405) return "AUTO SURROUND Straight Decode";
				else if (mode == 0x0406) return "[)(]DIGITAL EX";
				else if (mode == 0x0407) return "[)(]PLIIx MOVIE";
				else if (mode == 0x0408) return "DTS +Neo:6";
				else if (mode == 0x0409) return "ES MATRIX";
				else if (mode == 0x040a) return "ES DISCRETE";
				else if (mode == 0x040b) return "DTS-ES 8ch ";
				else if (mode == 0x040c) return "XM HD Surround";
				else if (mode == 0x040d) return "NEURAL SURR  ";
				else if (mode == 0x040e) return "RETRIEVER AIR";
				else if (mode == 0x040f) return "Neo:X CINEMA";
				else if (mode == 0x0410) return "ES Neo:X";
				else if (mode == 0x0501) return "STEREO";
				else if (mode == 0x0502) return "[)(]PLII MOVIE";
				else if (mode == 0x0503) return "[)(]PLIIx MOVIE";
				else if (mode == 0x0504) return "Neo:6 CINEMA";
				else if (mode == 0x0505) return "ALC Straight Decode";
				else if (mode == 0x0506) return "[)(]DIGITAL EX";
				else if (mode == 0x0507) return "[)(]PLIIx MOVIE";
				else if (mode == 0x0508) return "DTS +Neo:6";
				else if (mode == 0x0509) return "ES MATRIX";
				else if (mode == 0x050a) return "ES DISCRETE";
				else if (mode == 0x050b) return "DTS-ES 8ch ";
				else if (mode == 0x050c) return "XM HD Surround";
				else if (mode == 0x050d) return "NEURAL SURR  ";
				else if (mode == 0x050e) return "RETRIEVER AIR";
				else if (mode == 0x050f) return "Neo:X CINEMA";
				else if (mode == 0x0510) return "ES Neo:X";
				else if (mode == 0x0601) return "STEREO";
				else if (mode == 0x0602) return "[)(]PLII MOVIE";
				else if (mode == 0x0603) return "[)(]PLIIx MOVIE";
				else if (mode == 0x0604) return "Neo:6 CINEMA";
				else if (mode == 0x0605) return "STREAM DIRECT NORMAL Straight Decode";
				else if (mode == 0x0606) return "[)(]DIGITAL EX";
				else if (mode == 0x0607) return "[)(]PLIIx MOVIE";
				else if (mode == 0x0608) return "(nothing)";
				else if (mode == 0x0609) return "ES MATRIX";
				else if (mode == 0x060a) return "ES DISCRETE";
				else if (mode == 0x060b) return "DTS-ES 8ch ";
				else if (mode == 0x060c) return "Neo:X CINEMA";
				else if (mode == 0x060d) return "ES Neo:X";
				else if (mode == 0x0701) return "STREAM DIRECT PURE 2ch";
				else if (mode == 0x0702) return "[)(]PLII MOVIE";
				else if (mode == 0x0703) return "[)(]PLIIx MOVIE";
				else if (mode == 0x0704) return "Neo:6 CINEMA";
				else if (mode == 0x0705) return "STREAM DIRECT PURE Straight Decode";
				else if (mode == 0x0706) return "[)(]DIGITAL EX";
				else if (mode == 0x0707) return "[)(]PLIIx MOVIE";
				else if (mode == 0x0708) return "(nothing)";
				else if (mode == 0x0709) return "ES MATRIX";
				else if (mode == 0x070a) return "ES DISCRETE";
				else if (mode == 0x070b) return "DTS-ES 8ch ";
				else if (mode == 0x070c) return "Neo:X CINEMA";
				else if (mode == 0x070d) return "ES Neo:X";
				else if (mode == 0x0881) return "OPTIMUM";
				else if (mode == 0x0e01) return "HDMI THROUGH";
				else if (mode == 0x0f01) return "MULTI CH IN";
				else return "";
			}
		}
	}
	#endregion

	#region Tone control messages
	public enum ToneSetting : byte {
		Bypass = 0,
		On = 1,
		Tone = 9
	}
	public class ToneSet : PioneerCommandMessage {
		public override MessageType Type {
			get { return MessageType.TO; }
		}
		public ToneSet(ToneSetting setting) {
			parameters.Add(setting.ToString());
		}
		public ToneSet() { }
	}
	public class ToneRequest : PioneerRequestMessage {
		public override MessageType Type {
			get { return MessageType.TO; }
		}
	}
	public class ToneResponse : PioneerResponseMessage {
		public override MessageType Type {
			get { return MessageType.TO; }
		}
		public ToneResponse() { }
		public ToneResponse(string message)
			: base(message) {
			parameters.Add(message.Substring(Type.ToString().Length, 1));
		}
		public ToneSetting ToneSetting {
			get {
				if (parameters[0] == "0") return ToneSetting.Bypass;
				if (parameters[0] == "1") return ToneSetting.On;
				else  /* 9 */ return ToneSetting.Tone;
			}
		}
	}

	public class BassIncrement : PioneerCommandMessage {
		public override MessageType Type {
			get { return MessageType.BI; }
		}
	}
	public class BassDecrement : PioneerCommandMessage {
		public override MessageType Type {
			get { return MessageType.BD; }
		}
	}
	public class BassSet : PioneerCommandMessage {
		public override MessageType Type {
			get { return MessageType.BA; }
		}
		public BassSet(int db) {
			if (-6 > db || db > 6)
				throw new ArgumentException("dB offset must be between -6 and +6 dB");
			else
				parameters.Add((db + 6).ToString().PadLeft(2, '0'));
		}
		public BassSet() { }
	}
	public class BassRequest : PioneerRequestMessage {
		public override MessageType Type {
			get { return MessageType.BA; }
		}
	}
	public class BassResponse : PioneerResponseMessage {
		public override MessageType Type {
			get { return MessageType.BA; }
		}
		public int BassLevel {
			get {
				return int.Parse(parameters[0]) - 6;
			}
		}
		public BassResponse(string message)
			: base(message) {
			parameters.Add(message.Substring(Type.ToString().Length, 2));
		}
		public BassResponse() { }
	}

	public class TrebleIncrement : PioneerCommandMessage {
		public override MessageType Type {
			get { return MessageType.TI; }
		}
	}
	public class TrebleDecrement : PioneerCommandMessage {
		public override MessageType Type {
			get { return MessageType.TD; }
		}
	}
	public class TrebleSet : PioneerCommandMessage {
		public override MessageType Type {
			get { return MessageType.TR; }
		}
		public TrebleSet(int db) {
			if (-6 > db || db > 6)
				throw new ArgumentException("dB offset must be between -6 and +6 dB");
			else
				parameters.Add((db - 6).ToString().PadLeft(2, '0'));
		}
		public TrebleSet() { }
	}
	public class TrebleRequest : PioneerRequestMessage {
		public override MessageType Type {
			get { return MessageType.TR; }
		}
	}
	public class TrebleResponse : PioneerResponseMessage {
		public override MessageType Type {
			get { return MessageType.TR; }
		}
		public int TrebleLevel {
			get {
				return int.Parse(parameters[0]) - 6;
			}
		}
		public TrebleResponse(string message)
			: base(message) {
			parameters.Add(message.Substring(Type.ToString().Length, 2));
		}
		public TrebleResponse() { }
	}
	#endregion

	#region DSP function messages
	// TODO
	#endregion

	#region AMP function messages
	// TODO
	#endregion

	#region Key lock messages
	// TODO
	#endregion

	#region Cursor operation messages
	// TODO
	#endregion

	#region Video function messages
	// TODO
	#endregion

	#region Zone power messages
	// TODO
	#endregion

	#region Zone input messages
	// TODO
	#endregion

	#region Zone volume messages
	// TODO
	#endregion

	#region Zone mute messages
	// TODO
	#endregion

	#region Tuner messages

	public class TunerFreqIncrement : PioneerCommandMessage {
		public override MessageType Type {
			get { return MessageType.TFI; }
		}
	}
	public class TunerFreqDecrement : PioneerCommandMessage {
		public override MessageType Type {
			get { return MessageType.TFD; }
		}
	}
	public class TunerFreqRequest : PioneerRequestMessage {
		public override MessageType Type {
			get { return MessageType.FR; }
		}
	}
	public enum FreqType { AM, FM }
	public class TunerFreqResponse : PioneerResponseMessage {
		public override MessageType Type {
			get { return MessageType.FR; }
		}
		public TunerFreqResponse() { }
		public TunerFreqResponse(string message)
			: base(message) {
			parameters.Add(message.Substring(Type.ToString().Length, 1)); // am or fm
			parameters.Add(message.Substring(Type.ToString().Length + 1, 5)); // freq
		}
		public FreqType FreqType { get { return parameters[0] == "A" ? FreqType.AM : FreqType.FM; } }
		public double Freq { get { return double.Parse(parameters[1]) / 100.0; } }
	}

	public class TunerPresetIncrement : PioneerCommandMessage {
		public override MessageType Type {
			get { return MessageType.TPI; }
		}
	}
	public class TunerPresetDecrement : PioneerCommandMessage {
		public override MessageType Type {
			get { return MessageType.TPD; }
		}
	}
	public class TunerPresetRequest : PioneerRequestMessage {
		public override MessageType Type {
			get { return MessageType.PR; }
		}
	}
	public class TunerPresetResponse : PioneerResponseMessage {
		public override MessageType Type {
			get { return MessageType.PR; }
		}
		public TunerPresetResponse() { }
		public TunerPresetResponse(string message)
			: base(message) {
			parameters.Add(message.Substring(Type.ToString().Length, 1)); // class
			parameters.Add(message.Substring(Type.ToString().Length + 1, 2)); // num
		}
		public char Class { get { return parameters[0][0]; } }
		public int Number { get { return int.Parse(parameters[1]); } }
	}
	public enum TunerCommandType {
		SwitchToFM = 0,
		SwitchToAM = 1,
		TunerEdit = 2,
		TunerEnter = 3,
		TunerReturn = 4,
		MpxNoiseCut = 5,
		Display = 6,
		PTY_Search = 7
	}
	public class TunerCommand : PioneerCommandMessage {
		public override MessageType Type {
			get { return MessageType.TN; }
		}
		public TunerCommand() { }
		public TunerCommand(TunerCommandType type) {
			parameters.Add(((int)type).ToString("d2"));
		}
	}
	public class TunerPresetNamesRequest : PioneerRequestMessage {
		public override MessageType Type {
			get { return MessageType.TQ; }
		}
	}
	public class TunerPresetNamesResponse : PioneerResponseMessage {
		public override MessageType Type {
			get { return MessageType.TQ; }
		}
		public TunerPresetNamesResponse() { }
		public TunerPresetNamesResponse(string message)
			: base(message) {
			parameters.Add(message.Substring(Type.ToString().Length, 2)); // class
			parameters.Add(message.Substring(Type.ToString().Length + 3, 8)); // num "NAME____"
		}
		public string Preset { get { return parameters[0]; } }
		public string Name { get { return parameters[1]; } }
	}
	#endregion

	#region XM radio operation messages (USA model only)
	// TODO
	#endregion

	#region Sirius Operation messages (USA model only)
	// TODO
	#endregion

	#region iPod operation messages
	// TODO
	#endregion

	#region Home Media Gallery operation messages
	// TODO
	#endregion

	#region Adapter port operation messages
	// TODO
	#endregion

	#region Error message messages
	public class CommandOk : PioneerResponseMessage {
		public override MessageType Type {
			get { return MessageType.R; }
		}
		public override string ToString() {
			return "Last command succeeded";
		}
		public CommandOk(string message) : base(message) { }
		public CommandOk() { }
	}
	public class CommandError : PioneerResponseMessage {
		public override MessageType Type {
			get { return MessageType.E04; }
		}
		public override string ToString() {
			return "Inappropriate command-line detected";
		}
		public CommandError(string message) : base(message) { }
		public CommandError() { }
	}
	public class ParameterError : PioneerResponseMessage {
		public override MessageType Type {
			get { return MessageType.E06; }
		}
		public override string ToString() {
			return "Inappropriate parameter detected";
		}
		public ParameterError(string message) : base(message) { }
		public ParameterError() { }
	}
	public class DeviceBusy : PioneerResponseMessage {
		public override MessageType Type {
			get { return MessageType.B00; }
		}
		public override string ToString() {
			return "Device currently busy, wait a few seconds";
		}
		public DeviceBusy(string message) : base(message) { }
		public DeviceBusy() { }
	}
	#endregion

	#region Keyboard operation messages
	// TODO
	#endregion


	#region Information request messages
	public class DisplayInformationRequest : PioneerRequestMessage {
		public override MessageType Type {
			get { return MessageType.FL; }
		}
	}
	public enum Cause {
		Volume,
		GuidIcon
	}
	public class DisplayInformationResponse : PioneerResponseMessage {
		public override MessageType Type {
			get { return MessageType.FL; }
		}
		public DisplayInformationResponse() { }
		public DisplayInformationResponse(string message)
			: base(message) {
			parameters.Add(message.Substring(Type.ToString().Length, 2));
			parameters.Add(message.Substring(Type.ToString().Length + 2));
		}
		public string DisplayMessage {
			get {
				StringBuilder sb = new StringBuilder();
				string msg = parameters[1];
				for (int i = 0; i < msg.Length - 2; i += 2) {
					sb.Append(Convert.ToChar(byte.Parse(msg.Substring(i, 2), NumberStyles.HexNumber)));
				}
				return sb.ToString();
			}
		}
		public Cause Cause {
			get { return parameters[0] == "00" ? Cause.Volume : Cause.GuidIcon; }
		}
		public override string ToString() {
			return DisplayMessage;
		}
	}

	public class AudioInformationRequest : PioneerRequestMessage {
		public override MessageType Type {
			get { return MessageType.AST; }
		}
	}
	public class AudioInformationResponse : PioneerResponseMessage {
		public override MessageType Type {
			get { return MessageType.AST; }
		}
		public AudioInformationResponse() { }
		public AudioInformationResponse(string message)
			: base(message) {

		}
	}

	public class VideoInformationRequest : PioneerRequestMessage {
		public override MessageType Type {
			get { return MessageType.VST; }
		}
	}
	public class VideoInformationResponse : PioneerResponseMessage {
		public override MessageType Type {
			get { return MessageType.VST; }
		}
		public VideoInformationResponse() { }
		public VideoInformationResponse(string message)
			: base(message) {

		}
	}
	public class InputNameInfoRequest : PioneerRequestMessage {
		public override MessageType Type {
			get { return MessageType.RGB; }
		}
	}
	public class InputNameInfoResponse : PioneerResponseMessage {
		public override MessageType Type {
			get { return MessageType.RGB; }
		}
		public InputNameInfoResponse() { }
		public InputNameInfoResponse(string message)
			: base(message) {

		}
	}
	#endregion


}