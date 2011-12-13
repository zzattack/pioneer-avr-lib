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
		HM,	// Home menu
		TQ,	// Tuner preset name
		SSN,	// Hdmi setting		
		E04, // Command error
		E06, // Parameter error
		B00, // Busy
	}

	public abstract class PioneerMessage {
		public abstract MessageType Type { get; }
		protected List<string> parameters = new List<string>();
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
			parameters.Add(mode.ToString("X2").PadLeft(4, '0'));
		}
	}
	public class ListeningModeRequest : PioneerCommandMessage {
		public override MessageType Type {
			get { return MessageType.S; }
		}
	}
	public class ListeningModeResponse : PioneerResponseMessage {
		public override MessageType Type {
			get { return MessageType.S; }
		}
		public ListeningMode Mode {
			get {
				return (ListeningMode)int.Parse(parameters[0], NumberStyles.HexNumber);
			}
		}
		public ListeningModeResponse(string message)
			: base(message) {
			parameters.Add(message.Substring(Type.ToString().Length, 4));
		}
		public ListeningModeResponse() { }
	}
	public class PlayingListeningModeRequest : PioneerCommandMessage {
		public override MessageType Type {
			get { return MessageType.L; }
		}
	}
	public class PlayingListeningModeRespone : PioneerResponseMessage {
		public override MessageType Type {
			get { return MessageType.LM; }
		}
		public PlayingListeningModeRespone(string message)
			: base(message) {
			parameters.Add(message.Substring(Type.ToString().Length, 4));
		}
		public PlayingListeningModeRespone() { }
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
		public ToneResponse(string message)
			: base(message) {

		}
		public ToneResponse() { }
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
				return int.Parse(parameters[0]) + 6;
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
				for (int i = 0; i < msg.Length; i += 2) {
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