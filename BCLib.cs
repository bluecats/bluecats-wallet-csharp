using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using BlueCats.Serial;
using BlueCats.Serial.Commands.Responses;
using BlueCats.Serial.Events;

namespace BlueCats
{
    namespace Serial
    {
        namespace Commands
        {
            public class BCCommandBuilder
            {
                public static byte[] BuildResetCommand(bool goDFU)
                {
                    return new byte[] { BCConstants.MESSAGE_TYPE_COMMAND, 1, BCConstants.CLASS_ID_BC, BCConstants.COMMAND_RESET, Convert.ToByte(goDFU) };
                }

                public static byte[] BuildMeowCommand()
                {
                    return new byte[] { BCConstants.MESSAGE_TYPE_COMMAND, 0, BCConstants.CLASS_ID_BC, BCConstants.COMMAND_MEOW };
                }

                public static byte[] BuildReadBluetoothAddressCommand()
                {
                    return new byte[] { BCConstants.MESSAGE_TYPE_COMMAND, 0, BCConstants.CLASS_ID_BC, BCConstants.COMMAND_READ_BLUETOOTH_ADDRESS };
                }

                public static byte[] BuildReadVersionCommand()
                {
                    return new byte[] { BCConstants.MESSAGE_TYPE_COMMAND, 0, BCConstants.CLASS_ID_BC, BCConstants.COMMAND_READ_VERSION };
                }

                public static byte[] BuildWriteVersionCommand(byte version)
                {
                    return new byte[] { BCConstants.MESSAGE_TYPE_COMMAND, 0, BCConstants.CLASS_ID_BC, BCConstants.COMMAND_WRITE_VERSION, version };
                }

                public static byte[] BuildReadBeaconLoudnessCommand()
                {
                    return new byte[] { BCConstants.MESSAGE_TYPE_COMMAND, 0, BCConstants.CLASS_ID_BC, BCConstants.COMMAND_READ_BEACON_LOUDNESS };
                }

                public static byte[] BuildWriteBeaconLoudnessCommand(byte beaconLodunessLevel)
                {
                    return new byte[] { BCConstants.MESSAGE_TYPE_COMMAND, 0, BCConstants.CLASS_ID_BC, BCConstants.COMMAND_WRITE_BEACON_LOUDNESS, beaconLodunessLevel };
                }

                public static byte[] BuildReadBeaconModeCommand()
                {
                    return new byte[] { BCConstants.MESSAGE_TYPE_COMMAND, 0, BCConstants.CLASS_ID_BC, BCConstants.COMMAND_READ_BEACON_MODE };
                }

                public static byte[] BuildWriteBeaconModeCommand(byte beaconModeID)
                {
                    return new byte[] { BCConstants.MESSAGE_TYPE_COMMAND, 0, BCConstants.CLASS_ID_BC, BCConstants.COMMAND_WRITE_BEACON_MODE, beaconModeID };
                }

                public static byte[] BuildReadProximityUUIDCommand()
                {
                    return new byte[] { BCConstants.MESSAGE_TYPE_COMMAND, 0, BCConstants.CLASS_ID_BC, BCConstants.COMMAND_READ_PROXIMITY_UUID };
                }

                public static byte[] BuildWriteProximityUUIDCommand(Guid proximityUUID)
                {
                    byte[] proximityUUIDBytes = proximityUUID.ToByteArray();
                    byte[] command = new byte[BCConstants.FORMAT_HEADER_SIZE + proximityUUIDBytes.Length];
                    Array.Copy(new byte[] { BCConstants.MESSAGE_TYPE_COMMAND, (byte)proximityUUIDBytes.Length, BCConstants.CLASS_ID_BC, BCConstants.COMMAND_WRITE_PROXIMITY_UUID }, 0, command, 0, 4);
                    Array.Copy(proximityUUIDBytes, 0, command, 4, proximityUUIDBytes.Length);
                    return command;
                }

                public static byte[] BuildReadMajorCommand()
                {
                    return new byte[] { BCConstants.MESSAGE_TYPE_COMMAND, 0, BCConstants.CLASS_ID_BC, BCConstants.COMMAND_READ_MAJOR };
                }

                public static byte[] BuildWriteMajorCommand(ushort major)
                {
                    return new byte[] { BCConstants.MESSAGE_TYPE_COMMAND, 0, BCConstants.CLASS_ID_BC, BCConstants.COMMAND_WRITE_MAJOR, (byte)major, (byte)(major >> 8) };
                }

                public static byte[] BuildReadMinorCommand()
                {
                    return new byte[] { BCConstants.MESSAGE_TYPE_COMMAND, 0, BCConstants.CLASS_ID_BC, BCConstants.COMMAND_READ_MINOR };
                }

                public static byte[] BuildWriteMinorCommand(ushort minor)
                {
                    return new byte[] { BCConstants.MESSAGE_TYPE_COMMAND, 0, BCConstants.CLASS_ID_BC, BCConstants.COMMAND_WRITE_MINOR, (byte)minor, (byte)(minor >> 8) };
                }

                public static byte[] BuildReadMeasuredPowerCommand()
                {
                    return new byte[] { BCConstants.MESSAGE_TYPE_COMMAND, 0, BCConstants.CLASS_ID_BC, BCConstants.COMMAND_READ_MEASURED_POWER };
                }

                public static byte[] BuildWriteMeasuredPowerCommand(byte measuredPowerInDBm)
                {
                    byte twosComplement = measuredPowerInDBm < 0 ? (byte)(measuredPowerInDBm + 256) : measuredPowerInDBm;
                    return new byte[] { BCConstants.MESSAGE_TYPE_COMMAND, 0, BCConstants.CLASS_ID_BC, BCConstants.COMMAND_WRITE_MEASURED_POWER, twosComplement };
                }

                public static byte[] BuildReadModelNumberCommand()
                {
                    return new byte[] { BCConstants.MESSAGE_TYPE_COMMAND, 0, BCConstants.CLASS_ID_BC, BCConstants.COMMAND_READ_MODEL_NUMBER };
                }

                public static byte[] BuildReadFirmwareVersionCommand()
                {
                    return new byte[] { BCConstants.MESSAGE_TYPE_COMMAND, 0, BCConstants.CLASS_ID_BC, BCConstants.COMMAND_READ_FIRMARE_VERSION };
                }

                public static byte[] BuildReadAdvertisingEnabledCommand()
                {
                    return new byte[] { BCConstants.MESSAGE_TYPE_COMMAND, 0, BCConstants.CLASS_ID_BC, BCConstants.COMMAND_READ_ADVERTISING_ENABLED };
                }

                public static byte[] BuildWriteAdvertisingEnabledCommand(bool enabled)
                {
                    return new byte[] { BCConstants.MESSAGE_TYPE_COMMAND, 0, BCConstants.CLASS_ID_BC, BCConstants.COMMAND_WRITE_ADVERTISING_ENABLED, Convert.ToByte(enabled) };
                }

                public static byte[] BuildReadTargetSpeedCommand()
                {
                    return new byte[] { BCConstants.MESSAGE_TYPE_COMMAND, 0, BCConstants.CLASS_ID_BC, BCConstants.COMMAND_READ_TARGET_SPEED };
                }

                public static byte[] BuildCancelDataBlocksCommand()
                {
                    return new byte[] { BCConstants.MESSAGE_TYPE_COMMAND, 0, BCConstants.CLASS_ID_BC, BCConstants.COMMAND_CANCEL_DATA_BLOCKS };
                }

                public static byte[] BuildReadCrashReportCommand()
                {
                    return new byte[] { BCConstants.MESSAGE_TYPE_COMMAND, 0, BCConstants.CLASS_ID_BC, BCConstants.COMMAND_READ_CRASH_REPORT };
                }

                public static byte[] BuildReadEventsEnabledCommand()
                {
                    return new byte[] { BCConstants.MESSAGE_TYPE_COMMAND, 0, BCConstants.CLASS_ID_BC, BCConstants.COMMAND_READ_EVENTS_ENABLED };
                }

                public static byte[] BuildWriteEventsEnabledCommand(bool enabled)
                {
                    return new byte[] { BCConstants.MESSAGE_TYPE_COMMAND, 0, BCConstants.CLASS_ID_BC, BCConstants.COMMAND_WRITE_EVENTS_ENABLED, Convert.ToByte(enabled) };
                }

                public static byte[] BuildSendDataBlocksCommand(byte dataType, byte dataEncoding, byte repeatCount, byte[] data)
                {
                    byte[] command = new byte[BCConstants.FORMAT_HEADER_SIZE + 3 + data.Length];
                    Array.Copy(new byte[] { BCConstants.MESSAGE_TYPE_COMMAND, (byte)(3 + data.Length), BCConstants.CLASS_ID_BC, BCConstants.COMMAND_SEND_DATA_BLOCKS }, 0, command, 0, BCConstants.FORMAT_HEADER_SIZE);
                    command[4] = dataType;
                    command[5] = dataEncoding;
                    command[6] = repeatCount;
                    Array.Copy(data, 0, command, BCConstants.PAYLOAD_INDEX + 3, data.Length);
                    return command;
                }

                public static byte[] BuildRespondToBLEDataRequestCommand(byte[] data)
                {
                    byte[] command = new byte[BCConstants.FORMAT_HEADER_SIZE + data.Length];
                    Array.Copy(new byte[] { BCConstants.MESSAGE_TYPE_COMMAND, (byte)data.Length, BCConstants.CLASS_ID_BC, BCConstants.COMMAND_RESPOND_TO_BLE_DATA_REQUEST }, 0, command, 0, BCConstants.FORMAT_HEADER_SIZE);
                    Array.Copy(data, 0, command, BCConstants.PAYLOAD_INDEX, data.Length);
                    return command;
                }

                public static byte[] BuildBeginSettingsUpdateCommand()
                {
                    return new byte[] { BCConstants.MESSAGE_TYPE_COMMAND, 0, BCConstants.CLASS_ID_BC, BCConstants.COMMAND_BEGIN_SETTINGS_UPDATE };
                }

                public static byte[] BuildEndSettingsUpdateCommand()
                {
                    return new byte[] { BCConstants.MESSAGE_TYPE_COMMAND, 0, BCConstants.CLASS_ID_BC, BCConstants.COMMAND_END_SETTINGS_UPDATE };
                }
            }

            namespace Responses
            {
                public delegate void MeowEventHandler(object sender, MeowEventArgs e);
                public class MeowEventArgs : EventArgs
                {
                    public readonly byte[] Data;
                    public MeowEventArgs(byte[] data)
                    {
                        this.Data = data;
                    }
                }

                public delegate void ReadBluetoothAddressEventHandler(object sender, ReadBluetoothAddressEventArgs e);
                public class ReadBluetoothAddressEventArgs : EventArgs
                {
                    public readonly byte[] BluetoothAddressData;
                    public readonly string BluetoothAddressString;

                    public ReadBluetoothAddressEventArgs(byte[] btAddressData)
                    {
                        this.BluetoothAddressData = btAddressData;
                        this.BluetoothAddressString = BCUtilities.ConvertByteArrayToHexString(btAddressData);
                    }
                }

                public delegate void ReadVersionEventHandler(object sender, ReadVersionEventArgs e);
                public class ReadVersionEventArgs : EventArgs
                {
                    public readonly byte Version;
                    public ReadVersionEventArgs(byte version)
                    {
                        this.Version = version;
                    }
                }

                public delegate void ReadBeaconLoudnessEventHandler(object sender, ReadBeaconLoudnessEventArgs e);
                public class ReadBeaconLoudnessEventArgs : EventArgs
                {
                    public readonly byte BeaconLoudnessLevel;
                    public ReadBeaconLoudnessEventArgs(byte beaconLoudnessLevel)
                    {
                        this.BeaconLoudnessLevel = beaconLoudnessLevel;
                    }
                }

                public delegate void ReadBeaconModeEventHandler(object sender, ReadBeaconModeEventArgs e);
                public class ReadBeaconModeEventArgs : EventArgs
                {
                    public readonly byte BeaconModeID;
                    public ReadBeaconModeEventArgs(byte beaconModeID)
                    {
                        this.BeaconModeID = beaconModeID;
                    }
                }

                public delegate void ReadProximityUUIDEventHandler(object sender, ReadProximityUUIDEventArgs e);
                public class ReadProximityUUIDEventArgs : EventArgs
                {
                    public readonly Guid ProximityUUID;
                    public ReadProximityUUIDEventArgs(byte[] proximityUUIDBytes)
                    {
                        this.ProximityUUID = new Guid(proximityUUIDBytes);
                    }
                }

                public delegate void ReadMajorEventHandler(object sender, ReadMajorEventArgs e);
                public class ReadMajorEventArgs : EventArgs
                {
                    public readonly ushort Major;
                    public ReadMajorEventArgs(ushort major)
                    {
                        this.Major = major;
                    }
                }

                public delegate void ReadMinorEventHandler(object sender, ReadMinorEventArgs e);
                public class ReadMinorEventArgs : EventArgs
                {
                    public readonly ushort Minor;
                    public ReadMinorEventArgs(ushort minor)
                    {
                        this.Minor = minor;
                    }
                }

                public delegate void ReadMeasuredPowerEventHandler(object sender, ReadMeasuredPowerEventArgs e);
                public class ReadMeasuredPowerEventArgs : EventArgs
                {
                    public readonly byte MeasuredPowerInDBm;
                    public ReadMeasuredPowerEventArgs(byte twosComplementOfMeasuredPower)
                    {
                        this.MeasuredPowerInDBm = twosComplementOfMeasuredPower >= 127 ? (byte)(twosComplementOfMeasuredPower - 256) : twosComplementOfMeasuredPower;
                    }
                }

                public delegate void ReadModelNumberEventHandler(object sender, ReadModelNumberEventArgs e);
                public class ReadModelNumberEventArgs : EventArgs
                {
                    public readonly string ModelNumber;
                    public ReadModelNumberEventArgs(byte[] modelNumberBytes)
                    {
                        string hexString = BCUtilities.ConvertByteArrayToHexString(modelNumberBytes);
                        this.ModelNumber = "BC" + hexString.Insert(hexString.Length - 1, "-");
                    }
                }

                public delegate void ReadFirmwareVersionEventHandler(object sender, ReadFirmwareVersionEventArgs e);
                public class ReadFirmwareVersionEventArgs : EventArgs
                {
                    public readonly string FirmwareVersion;
                    public ReadFirmwareVersionEventArgs(byte[] firmwareVersionBytes)
                    {
                        this.FirmwareVersion = BCUtilities.ConvertByteArrayToFirmwareVersion(firmwareVersionBytes);
                    }
                }

                public delegate void ReadAdvertisingEnabledEventHandler(object sender, AdvertisingEnabledEventArgs e);
                public class AdvertisingEnabledEventArgs : EventArgs
                {
                    public readonly bool Enabled;
                    public AdvertisingEnabledEventArgs(byte enabledData)
                    {
                        this.Enabled = Convert.ToBoolean(enabledData);
                    }
                }

                public delegate void ReadTargetSpeedEventHandler(object sender, ReadTargetSpeedEventArgs e);
                public class ReadTargetSpeedEventArgs : EventArgs
                {
                    public readonly UInt16 TargetSpeedInMilliseconds;
                    public ReadTargetSpeedEventArgs(ushort targetSpeedInMilliseconds)
                    {
                        this.TargetSpeedInMilliseconds = targetSpeedInMilliseconds;
                    }
                }

                public delegate void ReadCrashReportEventHandler(object sender, ReadCrashReportEventArgs e);
                public class ReadCrashReportEventArgs : EventArgs
                {
                    public readonly UInt16 CrashReport;
                    public ReadCrashReportEventArgs(UInt16 crashReport)
                    {
                        this.CrashReport = crashReport;
                    }
                }

                public delegate void ReadEventsEnabledEventHandler(object sender, EventsEnabledEventArgs e);
                public class EventsEnabledEventArgs : EventArgs
                {
                    public readonly bool Enabled;
                    public EventsEnabledEventArgs(byte enabledData)
                    {
                        this.Enabled = Convert.ToBoolean(enabledData);
                    }
                }

                public class CommandResponseArgs : EventArgs
                {
                    public readonly byte ResponseCode;
                    public CommandResponseArgs(byte responseCode)
                    {
                        this.ResponseCode = responseCode;
                    }
                }

                public delegate void RespondToBleDataRequestEventHandler(object sender, CommandResponseArgs e);
                
                public delegate void SendDataBlocksEventHandler(object sender, CommandResponseArgs e);
                public delegate void CancelDataBlocksEventHandler(object sender, CommandResponseArgs e);

                public delegate void WriteAdvertisingEnabledEventHandler(object sender, AdvertisingEnabledEventArgs e);
                public delegate void WriteEventsEnabledEventHandler(object sender, EventsEnabledEventArgs e);

                public delegate void ResetEventHandler(object sender, CommandResponseArgs e);

                public delegate void BeginSettingsUpdateEventHandler(object sender, CommandResponseArgs e);
                public delegate void WriteBeaconModeEventHandler(object sender, CommandResponseArgs e);
                public delegate void WriteBeaconLoudnessEventHandler(object sender, CommandResponseArgs e);
                public delegate void WriteProximityUUIDEventHandler(object sender, CommandResponseArgs e);
                public delegate void WriteMajorEventHandler(object sender, CommandResponseArgs e);
                public delegate void WriteMinorEventHandler(object sender, CommandResponseArgs e);
                public delegate void WriteMeasuredPowerEventHandler(object sender, CommandResponseArgs e);
                public delegate void WriteVersionEventHandler(object sender, CommandResponseArgs e);
                public delegate void EndSettingsUpdateEventHandler(object sender, CommandResponseArgs e);
            }
        }

        namespace Events
        {
            public delegate void BleConnectedEventHandler(object sender, EventArgs e);
            public delegate void BleDisconnectedEventHandler(object sender, EventArgs e);
            public delegate void BleAuthenticationSucceededEventHandler(object sender, EventArgs e);
            public delegate void BleAuthenticationFailedEventHandler(object sender, EventArgs e);
            public delegate void SettingsSavedEventHandler(object sender, EventArgs e);
            public delegate void DataBlocksSentEventHandler(object sender, EventArgs e);
            public delegate void BleDataIndicatedEventHandler(object sender, EventArgs e);

            public delegate void BleDataRequestEventHandler(object sender, BleDataRequestEventArgs e);
            public class BleDataRequestEventArgs : EventArgs
            {
                public readonly byte[] Data;
                public BleDataRequestEventArgs(byte[] data)
                {
                    this.Data = data;
                }
            }

            public delegate void ErrorEventHandler(object sender, ErrorEventArgs e);
            public class ErrorEventArgs : EventArgs
            {
                public readonly byte ErrorCode;
                public ErrorEventArgs(byte errorCode)
                {
                    this.ErrorCode = errorCode;
                }
            }
        }

        public static class BCConstants
        {
        	public const string BCLIB_VERSION = "1.0.1";

            public const byte FORMAT_HEADER_SIZE = 4;
            public const byte MESSAGE_TYPE_INDEX = 0;
            public const byte PAYLOAD_LENGTH_INDEX = 1;
            public const byte CLASS_ID_INDEX = 2;
            public const byte COMMAND_ID_INDEX = 3;
            public const byte EVENT_ID_INDEX = 3;
            public const byte PAYLOAD_INDEX = 4;

            public const byte MESSAGE_TYPE_COMMAND = 0x00;
            public const byte MESSAGE_TYPE_EVENT = 0x80;

            public const byte CLASS_ID_BC = 0xBC;

            // command ids
            public const byte COMMAND_RESET = 0x00;
            public const byte COMMAND_MEOW = 0x01;
            public const byte COMMAND_READ_BLUETOOTH_ADDRESS = 0x02;
            public const byte COMMAND_READ_VERSION = 0x03;
            public const byte COMMAND_WRITE_VERSION = 0x04;
            public const byte COMMAND_READ_BEACON_LOUDNESS = 0x05;
            public const byte COMMAND_WRITE_BEACON_LOUDNESS = 0x06;
            public const byte COMMAND_READ_BEACON_MODE = 0x07;
            public const byte COMMAND_WRITE_BEACON_MODE = 0x08;
            public const byte COMMAND_READ_PROXIMITY_UUID = 0x09;
            public const byte COMMAND_WRITE_PROXIMITY_UUID = 0x0A;
            public const byte COMMAND_READ_MAJOR = 0x0B;
            public const byte COMMAND_WRITE_MAJOR = 0x0C;
            public const byte COMMAND_READ_MINOR = 0x0D;
            public const byte COMMAND_WRITE_MINOR = 0x0E;
            public const byte COMMAND_READ_MEASURED_POWER = 0x0F;
            public const byte COMMAND_WRITE_MEASURED_POWER = 0x10;
            public const byte COMMAND_READ_MODEL_NUMBER = 0x11;
            public const byte COMMAND_READ_FIRMARE_VERSION = 0x12;
            public const byte COMMAND_READ_ADVERTISING_ENABLED = 0x13;
            public const byte COMMAND_WRITE_ADVERTISING_ENABLED = 0x14;
            public const byte COMMAND_READ_TARGET_SPEED = 0x15;
            public const byte COMMAND_WRITE_TARGET_SPEED = 0x16;
            public const byte COMMAND_SEND_DATA_BLOCKS = 0x17;
            public const byte COMMAND_RESPOND_TO_BLE_DATA_REQUEST = 0x18;
            public const byte COMMAND_CANCEL_DATA_BLOCKS = 0x19;
            public const byte COMMAND_READ_CRASH_REPORT = 0x1A;
            public const byte COMMAND_READ_EVENTS_ENABLED = 0x1B;
            public const byte COMMAND_WRITE_EVENTS_ENABLED = 0x1C;
            public const byte COMMAND_BEGIN_SETTINGS_UPDATE = 0x1D;
            public const byte COMMAND_END_SETTINGS_UPDATE = 0x1E;

            // command response codes
            public const byte COMMAND_RESPONSE_SUCCESS = 0x00;
            public const byte COMMAND_RESPONSE_ERROR_BUSY = 0x01;
            public const byte COMMAND_RESPONSE_ERROR_ADV_DISABLED = 0x02;
            public const byte COMMAND_RESPONSE_ERROR_BUF_OVERFLOW = 0x03;
            public const byte COMMAND_RESPONSE_ERROR_REMOTE_HANGUP = 0x04;
            public const byte COMMAND_RESPONSE_ERROR_GATT_WRITE = 0x05;
            public const byte COMMAND_RESPONSE_ERROR_INVALID_PARAMETER = 0x06;
            public const byte COMMAND_RESPONSE_ERROR_NOT_SUPPORTED = 0x07;
            public const byte COMMAND_RESPONSE_ERROR_VER_REQUIRED = 0x08;

            // event ids
            public const byte EVENT_BLE_CONNECT = 0x01;
            public const byte EVENT_BLE_DISCONNECT = 0x02;
            public const byte EVENT_DEBUG = 0x03;
            public const byte EVENT_BLE_AUTH_SUCCEEDED = 0x04;
            public const byte EVENT_BLE_AUTH_FAILED = 0x05;
            public const byte EVENT_ERROR = 0x06;
            public const byte EVENT_SETTINGS_SAVED = 0x07;
            public const byte EVENT_BLE_DATA_REQ = 0x08;
            public const byte EVENT_DATA_BLOCKS_SENT = 0x09;
            public const byte EVENT_BLE_DATA_INDICATED = 0x0A;

            // event error
            public const byte EVENT_ERR_UNRECOGNISED_COMMAND = 0x84;
        }

        public static class BCUtilities
        {
            public static string ConvertByteArrayToHexString(byte[] data)
            {
                if (data == null || data.Length <= 0)
                    return string.Empty;

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    sb.Append(string.Format("{0:X2}", data[i]));
                }
                return sb.ToString();
            }

            public static string ConvertByteArrayToFirmwareVersion(byte[] data)
            {
                string hexString = ConvertByteArrayToHexString(data);
                if (hexString.Length > 1 && hexString[0] == '0') {
                    hexString = hexString.Substring(1);
                }
    
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hexString.Length; i++)
                {
                    sb.Append(hexString[i]);
                    if (i < hexString.Length - 1) 
                    {
                        sb.Append('.');
                    }
                }
                return sb.ToString();
            }

        }

        public class BCLib
        {
            // events
            public event BleAuthenticationFailedEventHandler BleAuthenticationFailedEvent;
            public event BleAuthenticationSucceededEventHandler BleAuthenticationSucceededEvent;
            public event BleConnectedEventHandler BleConnectedEvent;
            public event DataBlocksSentEventHandler BleDataBlocksSentEvent;
            public event BleDataIndicatedEventHandler BleDataIndicatedEvent;
            public event BleDataRequestEventHandler BleDataRequestEvent;
            public event BleDisconnectedEventHandler BleDisconnectedEvent;
            public event SettingsSavedEventHandler SettingsSavedEvent;
            public event ErrorEventHandler ErrorEvent;

            // commands
            public event BeginSettingsUpdateEventHandler BeginSettingsUpdateCommandResponse;
            public event CancelDataBlocksEventHandler CancelDataBlocksCommandResponse;
            public event ResetEventHandler ResetCommandResponse;
            public event EndSettingsUpdateEventHandler EndSettingsUpdateCommandResponse;
            public event MeowEventHandler MeowCommandResponse;
            public event ReadAdvertisingEnabledEventHandler ReadAdvertisingEnabledCommandResponse;
            public event ReadBluetoothAddressEventHandler ReadBluetoothAddressCommandResponse;
            public event ReadCrashReportEventHandler ReadCrashReportCommandResponse;
            public event ReadEventsEnabledEventHandler ReadEventsEnabledCommandResponse;
            public event ReadFirmwareVersionEventHandler ReadFirmwareVersionCommandResponse;
            public event ReadBeaconLoudnessEventHandler ReadBeaconLoudnessCommandResponse;
            public event ReadMajorEventHandler ReadMajorCommandResponse;
            public event ReadMeasuredPowerEventHandler ReadMeasuredPowerCommandResponse;
            public event ReadMinorEventHandler ReadMinorCommandResponse;
            public event ReadBeaconModeEventHandler ReadBeaconModeCommandResponse;
            public event ReadModelNumberEventHandler ReadModelNumberCommandResponse;
            public event ReadProximityUUIDEventHandler ReadProximityUUIDCommandResponse;
            public event ReadTargetSpeedEventHandler ReadTargetSpeedCommandResponse;
            public event ReadVersionEventHandler ReadVersionCommandResponse;
            public event RespondToBleDataRequestEventHandler RespondToBleDataRequestCommandResponse;
            public event SendDataBlocksEventHandler SendDataBlocksCommandResponse;
            public event WriteAdvertisingEnabledEventHandler WriteAdvertisingEnabledCommandResponse;
            public event WriteEventsEnabledEventHandler WriteEventsEnabledCommandResponse;
            public event WriteBeaconLoudnessEventHandler WriteBeaconLoudnessCommandResponse;
            public event WriteMajorEventHandler WriteMajorCommandResponse;
            public event WriteMeasuredPowerEventHandler WriteMeasuredPowerCommandResponse;
            public event WriteMinorEventHandler WriteMinorCommandResponse;
            public event WriteBeaconModeEventHandler WriteBeaconModeCommandResponse;
            public event WriteProximityUUIDEventHandler WriteProximityUUIDCommandResponse;
            public event WriteVersionEventHandler WriteVersionCommandResponse;

            private byte[] RxBuffer = new byte[256];
            private int RxBufferIndex = 0;
            private int PayloadLength = 0;
            private Boolean parserBusy = false;

            public void SetBusy(Boolean isBusy)
            {
                this.parserBusy = isBusy;
            }

            public Boolean IsBusy()
            {
                return parserBusy;
            }

            public UInt16 Parse(byte nextByte)
            {
                if (RxBufferIndex == 0)
                { 
                    // find message type for commands or events
                    if ((nextByte & 0x7F) == 0x00)
                    {
                        RxBuffer[RxBufferIndex++] = nextByte; // put byte in rx buffer
                    }
                    else
                    {
                        return 1; // packet format error
                    }
                }
                else
                { 
                    RxBuffer[RxBufferIndex++] = nextByte;
                    if (RxBufferIndex == (BCConstants.PAYLOAD_LENGTH_INDEX + 1))
                    { 
                        PayloadLength = nextByte;
                    }
                    else if (RxBufferIndex == PayloadLength + BCConstants.FORMAT_HEADER_SIZE)
                    { 
                        //rx last byte
                        if (RxBuffer[0] == BCConstants.MESSAGE_TYPE_COMMAND)
                        { // command response
                            if (RxBuffer[BCConstants.CLASS_ID_INDEX] == BCConstants.CLASS_ID_BC)
                            {
                                if (RxBuffer[BCConstants.COMMAND_ID_INDEX] == BCConstants.COMMAND_MEOW)
                                {
                                    if (MeowCommandResponse != null)
                                    {
                                        MeowCommandResponse(this, new MeowEventArgs(
                                            (byte[])(RxBuffer.Skip(4).Take(5).ToArray())
                                        ));
                                    }
                                }
                                else if (RxBuffer[BCConstants.COMMAND_ID_INDEX] == BCConstants.COMMAND_READ_BLUETOOTH_ADDRESS)
                                {
                                    if (ReadBluetoothAddressCommandResponse != null)
                                    {
                                        ReadBluetoothAddressCommandResponse(this, new ReadBluetoothAddressEventArgs(
                                            (byte[])(RxBuffer.Skip(4).Take(6).ToArray())
                                        ));
                                    }
                                }
                                else if (RxBuffer[BCConstants.COMMAND_ID_INDEX] == BCConstants.COMMAND_READ_VERSION)
                                {
                                    if (ReadVersionCommandResponse != null)
                                    {
                                        ReadVersionCommandResponse(this, new ReadVersionEventArgs(
                                            RxBuffer[4]
                                        ));
                                    }
                                }
                                else if (RxBuffer[BCConstants.COMMAND_ID_INDEX] == BCConstants.COMMAND_READ_BEACON_MODE)
                                {
                                    if (ReadBeaconModeCommandResponse != null)
                                    {
                                        ReadBeaconModeCommandResponse(this, new ReadBeaconModeEventArgs(
                                            RxBuffer[4]
                                        ));
                                    }
                                }
                                else if (RxBuffer[BCConstants.COMMAND_ID_INDEX] == BCConstants.COMMAND_READ_BEACON_LOUDNESS)
                                {
                                    if (ReadBeaconLoudnessCommandResponse != null)
                                    {
                                        ReadBeaconLoudnessCommandResponse(this, new ReadBeaconLoudnessEventArgs(
                                            RxBuffer[4]
                                        ));
                                    }
                                }
                                else if (RxBuffer[BCConstants.COMMAND_ID_INDEX] == BCConstants.COMMAND_READ_PROXIMITY_UUID)
                                {
                                    if (ReadProximityUUIDCommandResponse != null)
                                    {
                                        ReadProximityUUIDCommandResponse(this, new ReadProximityUUIDEventArgs(
                                            (byte[])(RxBuffer.Skip(4).Take(16).ToArray())
                                        ));
                                    }
                                }
                                else if (RxBuffer[BCConstants.COMMAND_ID_INDEX] == BCConstants.COMMAND_READ_MAJOR)
                                {
                                    if (ReadMajorCommandResponse != null)
                                    {
                                        ReadMajorCommandResponse(this, new ReadMajorEventArgs(
                                            (UInt16)((RxBuffer[4] << 8) | RxBuffer[5])
                                        ));
                                    }
                                }
                                else if (RxBuffer[BCConstants.COMMAND_ID_INDEX] == BCConstants.COMMAND_READ_MINOR)
                                {
                                    if (ReadMinorCommandResponse != null)
                                    {
                                        ReadMinorCommandResponse(this, new ReadMinorEventArgs(
                                            (UInt16)((RxBuffer[4] << 8) | RxBuffer[5])
                                        ));
                                    }
                                }
                                else if (RxBuffer[BCConstants.COMMAND_ID_INDEX] == BCConstants.COMMAND_READ_MEASURED_POWER)
                                {
                                    if (ReadMeasuredPowerCommandResponse != null)
                                    {
                                        ReadMeasuredPowerCommandResponse(this, new ReadMeasuredPowerEventArgs(RxBuffer[4]));
                                    }
                                }
                                else if (RxBuffer[BCConstants.COMMAND_ID_INDEX] == BCConstants.COMMAND_READ_MODEL_NUMBER)
                                {
                                    if (ReadModelNumberCommandResponse != null)
                                    {
                                        ReadModelNumberCommandResponse(this, new ReadModelNumberEventArgs(new byte[] { (byte)(RxBuffer[4] << 8), RxBuffer[5] }));
                                    }
                                }
                                else if (RxBuffer[BCConstants.COMMAND_ID_INDEX] == BCConstants.COMMAND_READ_FIRMARE_VERSION)
                                {
                                    if (ReadFirmwareVersionCommandResponse != null)
                                    {
                                        ReadFirmwareVersionCommandResponse(this, new ReadFirmwareVersionEventArgs(new byte[] { (byte)(RxBuffer[4] << 8), RxBuffer[5] }));
                                    }
                                }
                                else if (RxBuffer[BCConstants.COMMAND_ID_INDEX] == BCConstants.COMMAND_READ_ADVERTISING_ENABLED)
                                {
                                    if (ReadAdvertisingEnabledCommandResponse != null)
                                    {
                                        ReadAdvertisingEnabledCommandResponse(this, new AdvertisingEnabledEventArgs(RxBuffer[4]));
                                    }
                                }
                                else if (RxBuffer[BCConstants.COMMAND_ID_INDEX] == BCConstants.COMMAND_READ_TARGET_SPEED)
                                {
                                    if (ReadTargetSpeedCommandResponse != null)
                                    {
                                        ReadTargetSpeedCommandResponse(this, new ReadTargetSpeedEventArgs(
                                            (UInt16)((RxBuffer[4] << 8) | RxBuffer[5])
                                        ));
                                    }
                                }
                                else if (RxBuffer[BCConstants.COMMAND_ID_INDEX] == BCConstants.COMMAND_READ_CRASH_REPORT)
                                {
                                    if (ReadCrashReportCommandResponse != null)
                                    {
                                        ReadCrashReportCommandResponse(this, new ReadCrashReportEventArgs(
                                            (UInt16)((RxBuffer[4] << 8) | RxBuffer[5])
                                        ));
                                    }
                                }
                                else if (RxBuffer[BCConstants.COMMAND_ID_INDEX] == BCConstants.COMMAND_READ_EVENTS_ENABLED)
                                {
                                    if (ReadEventsEnabledCommandResponse != null)
                                    {
                                        ReadEventsEnabledCommandResponse(this, new EventsEnabledEventArgs(RxBuffer[4]));
                                    }
                                }
                                else if (RxBuffer[BCConstants.COMMAND_ID_INDEX] == BCConstants.COMMAND_BEGIN_SETTINGS_UPDATE)
                                {
                                    if (BeginSettingsUpdateCommandResponse != null)
                                    {
                                        BeginSettingsUpdateCommandResponse(this, new CommandResponseArgs(RxBuffer[4]));
                                    }
                                }
                                else if (RxBuffer[BCConstants.COMMAND_ID_INDEX] == BCConstants.COMMAND_CANCEL_DATA_BLOCKS)
                                {
                                    if (CancelDataBlocksCommandResponse != null)
                                    {
                                        CancelDataBlocksCommandResponse(this, new CommandResponseArgs(RxBuffer[4]));
                                    }
                                }
                                else if (RxBuffer[BCConstants.COMMAND_ID_INDEX] == BCConstants.COMMAND_RESET)
                                {
                                    if (ResetCommandResponse != null)
                                    {
                                        ResetCommandResponse(this, new CommandResponseArgs(RxBuffer[4]));
                                    }
                                }
                                else if (RxBuffer[BCConstants.COMMAND_ID_INDEX] == BCConstants.COMMAND_END_SETTINGS_UPDATE)
                                {
                                    if (EndSettingsUpdateCommandResponse != null)
                                    {
                                        EndSettingsUpdateCommandResponse(this, new CommandResponseArgs(RxBuffer[4]));
                                    }
                                }
                                else if (RxBuffer[BCConstants.COMMAND_ID_INDEX] == BCConstants.COMMAND_RESPOND_TO_BLE_DATA_REQUEST)
                                {
                                    if (RespondToBleDataRequestCommandResponse != null)
                                    {
                                        RespondToBleDataRequestCommandResponse(this, new CommandResponseArgs(RxBuffer[4]));
                                    }
                                }
                                else if (RxBuffer[BCConstants.COMMAND_ID_INDEX] == BCConstants.COMMAND_SEND_DATA_BLOCKS)
                                {
                                    if (SendDataBlocksCommandResponse != null)
                                    {
                                        SendDataBlocksCommandResponse(this, new CommandResponseArgs(RxBuffer[4]));
                                    }
                                }
                                else if (RxBuffer[BCConstants.COMMAND_ID_INDEX] == BCConstants.COMMAND_WRITE_ADVERTISING_ENABLED)
                                {
                                    if (WriteAdvertisingEnabledCommandResponse != null)
                                    {
                                        WriteAdvertisingEnabledCommandResponse(this, new AdvertisingEnabledEventArgs(RxBuffer[4]));
                                    }
                                }
                                else if (RxBuffer[BCConstants.COMMAND_ID_INDEX] == BCConstants.COMMAND_WRITE_EVENTS_ENABLED)
                                {
                                    if (WriteEventsEnabledCommandResponse != null)
                                    {
                                        WriteEventsEnabledCommandResponse(this, new EventsEnabledEventArgs(RxBuffer[4]));
                                    }
                                }
                                else if (RxBuffer[BCConstants.COMMAND_ID_INDEX] == BCConstants.COMMAND_WRITE_BEACON_LOUDNESS)
                                {
                                    if (WriteBeaconLoudnessCommandResponse != null)
                                    {
                                        WriteBeaconLoudnessCommandResponse(this, new CommandResponseArgs(RxBuffer[4]));
                                    }
                                }
                                else if (RxBuffer[BCConstants.COMMAND_ID_INDEX] == BCConstants.COMMAND_WRITE_MAJOR)
                                {
                                    if (WriteMajorCommandResponse != null)
                                    {
                                        WriteMajorCommandResponse(this, new CommandResponseArgs(RxBuffer[4]));
                                    }
                                }
                                else if (RxBuffer[BCConstants.COMMAND_ID_INDEX] == BCConstants.COMMAND_WRITE_MEASURED_POWER)
                                {
                                    if (WriteMeasuredPowerCommandResponse != null)
                                    {
                                        WriteMeasuredPowerCommandResponse(this, new CommandResponseArgs(RxBuffer[4]));
                                    }
                                }
                                else if (RxBuffer[BCConstants.COMMAND_ID_INDEX] == BCConstants.COMMAND_WRITE_MINOR)
                                {
                                    if (WriteMinorCommandResponse != null)
                                    {
                                        WriteMinorCommandResponse(this, new CommandResponseArgs(RxBuffer[4]));
                                    }
                                }
                                else if (RxBuffer[BCConstants.COMMAND_ID_INDEX] == BCConstants.COMMAND_WRITE_BEACON_MODE)
                                {
                                    if (WriteBeaconModeCommandResponse != null)
                                    {
                                        WriteBeaconModeCommandResponse(this, new CommandResponseArgs(RxBuffer[4]));
                                    }
                                }
                                else if (RxBuffer[BCConstants.COMMAND_ID_INDEX] == BCConstants.COMMAND_WRITE_PROXIMITY_UUID)
                                {
                                    if (WriteProximityUUIDCommandResponse != null)
                                    {
                                        WriteProximityUUIDCommandResponse(this, new CommandResponseArgs(RxBuffer[4]));
                                    }
                                }
                                else if (RxBuffer[BCConstants.COMMAND_ID_INDEX] == BCConstants.COMMAND_WRITE_VERSION)
                                {
                                    if (WriteVersionCommandResponse != null)
                                    {
                                        WriteVersionCommandResponse(this, new CommandResponseArgs(RxBuffer[4]));
                                    }
                                }
                            }
                            SetBusy(false);
                        }
                        else if (RxBuffer[0] == BCConstants.MESSAGE_TYPE_EVENT)
                        { // event
                            if (RxBuffer[BCConstants.CLASS_ID_INDEX] == BCConstants.CLASS_ID_BC)
                            {
                                if (RxBuffer[BCConstants.EVENT_ID_INDEX] == BCConstants.EVENT_BLE_CONNECT)
                                {
                                    if (BleConnectedEvent != null)
                                    {
                                        BleConnectedEvent(this, new EventArgs());
                                    }
                                }
                                else if (RxBuffer[BCConstants.EVENT_ID_INDEX] == BCConstants.EVENT_BLE_DISCONNECT)
                                {
                                    if (BleDisconnectedEvent != null)
                                    {
                                        BleDisconnectedEvent(this, new EventArgs());
                                    }
                                }
                                else if (RxBuffer[BCConstants.EVENT_ID_INDEX] == BCConstants.EVENT_BLE_AUTH_SUCCEEDED)
                                {
                                    if (BleAuthenticationSucceededEvent != null)
                                    {
                                        BleAuthenticationSucceededEvent(this, new EventArgs());
                                    }
                                }
                                else if (RxBuffer[BCConstants.EVENT_ID_INDEX] == BCConstants.EVENT_BLE_AUTH_FAILED)
                                {
                                    if (BleAuthenticationFailedEvent != null)
                                    {
                                        BleAuthenticationFailedEvent(this, new EventArgs());
                                    }
                                }
                                else if (RxBuffer[BCConstants.EVENT_ID_INDEX] == BCConstants.EVENT_SETTINGS_SAVED)
                                {
                                    if (SettingsSavedEvent != null)
                                    {
                                        SettingsSavedEvent(this, new EventArgs());
                                    }
                                }
                                else if (RxBuffer[BCConstants.EVENT_ID_INDEX] == BCConstants.EVENT_DATA_BLOCKS_SENT)
                                {
                                    if (BleDataBlocksSentEvent != null)
                                    {
                                        BleDataBlocksSentEvent(this, new EventArgs());
                                    }
                                }
                                else if (RxBuffer[BCConstants.EVENT_ID_INDEX] == BCConstants.EVENT_BLE_DATA_INDICATED)
                                {
                                    if (BleDataIndicatedEvent != null)
                                    {
                                        BleDataIndicatedEvent(this, new EventArgs());
                                    }
                                }
                                else if (RxBuffer[BCConstants.EVENT_ID_INDEX] == BCConstants.EVENT_BLE_DATA_REQ)
                                {
                                    if (BleDataRequestEvent != null)
                                    {
                                        BleDataRequestEvent(this, new BleDataRequestEventArgs(
                                            (byte[])(RxBuffer.Skip(BCConstants.FORMAT_HEADER_SIZE).Take(RxBuffer[1]).ToArray())
                                        ));
                                    }
                                }
                                else if (RxBuffer[BCConstants.EVENT_ID_INDEX] == BCConstants.EVENT_ERROR)
                                {
                                    if (ErrorEvent != null)
                                    {
                                        ErrorEvent(this, new ErrorEventArgs(RxBuffer[4]));
                                    }
                                }
                            }
                        }

                        RxBufferIndex = 0; // reset ex buffer position to be ready for next packet
                    }
                }

                return 0; // parsed successfully
            }

            public UInt16 SendCommand(SerialPort port, byte[] cmd)
            {
                SetBusy(true);
                port.Write(cmd, 0, cmd.Length);
                return 0; // no error handling yet
            }

        }
    }
}


