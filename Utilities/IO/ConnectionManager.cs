using BCWallet.Utilities.Threading;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Threading;

namespace BCWallet.Utilities.IO
{
    public static class ConnectionManager
    {
        private static ConcurrentDictionary<string, SerialPort> _attachedPorts = new ConcurrentDictionary<string, SerialPort>();
        private static List<SerialPortInfo> _portsPrevDiscoveredByConnectListener = new List<SerialPortInfo>();
        private static List<SerialPortInfo> _portsPrevDiscoveredByDisconnectListener = new List<SerialPortInfo>();
        private static readonly Object _connectPadlock = new Object();
        private static readonly Object _disconnectPadlock = new Object();
        private static readonly Object _serialPortPadlock = new Object();

        public static int AttachedComportCount
        {
            get { return _attachedPorts.Count; }
        }

        public static List<SerialPortInfo> DiscoverSerialPorts()
        {
            var serialPorts = new List<SerialPortInfo>();
            try
            {
                var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_SerialPort");
                foreach (var query_obj in searcher.Get())
                {
                    serialPorts.Add(new SerialPortInfo
                    {
                        Name = query_obj["Caption"].ToString(),
                        Port = query_obj["DeviceID"].ToString()
                    });
                }
            }
            catch (ManagementException)
            {
                Debug.Print("Error trying to discover local serial ports");
                return new List<SerialPortInfo>();
            }
            return serialPorts;
        }

        public static void DetachFromSerialPort(string portName)
        {
            lock (_serialPortPadlock)
            {
                if (portName == null)
                    return;

                SerialPort value;
                if (_attachedPorts.TryGetValue(portName, out value))
                {
                    var port = value;
                    if (port.IsOpen)
                    {
                        try
                        {
                            Debug.Print("Closing serial port: '{0}'", portName);
                            port.DiscardInBuffer();
                            port.DiscardOutBuffer();
                            port.Close();
                            Debug.Print("Closed serial port: '{0}'", portName);
                            Thread.Sleep(500); // Safety recommendation from Microsoft best practice doc to wait 1/2 sec
                        }
                        catch (IOException ex)
                        {
                            //Debug.Print("Closing serial port '{0}' failed with error: '{1}'", portName, ex);
                        }
                        finally
                        {
                            var successful_remove = _attachedPorts.TryRemove(portName, out port);
                            if (successful_remove)
                                Debug.Print("Removed Port '{0}', attached ports: {1}", portName, AttachedComportCount);
                        }
                    }
                }
            }
        }

        public static SerialPort AttachToSerialPort(string portName, int baudRate = 9600)
        {
            lock (_serialPortPadlock)
            {
                if (portName == null)
                    throw new ArgumentNullException("portName");

                SerialPort serialPort = null;
                DetachFromSerialPort(portName);
                try
                {
                    serialPort = CreateSerialPort(portName, baudRate);
                }
                catch (Exception ex)
                {
                    if (serialPort != null)
                        serialPort.Dispose();
                    throw ex;
                }

                var addedPort = _attachedPorts.TryAdd(portName, serialPort);
                if (addedPort) Debug.Print("Added serial port \'{0}\', attached ports: {1}", portName, AttachedComportCount);

                return _attachedPorts[portName];
            }
        }

        public static void ListenForSerialDeviceConnection(Action<string> wasConnectedCallback, int timeoutInMs = Timeout.Infinite, bool oneshot = true, CancellationToken cancelToken = new CancellationToken())
        {
            lock (_connectPadlock) _portsPrevDiscoveredByConnectListener = DiscoverSerialPorts();
            var newly_connected_port = String.Empty;

            Func<bool> IsNewSerialPortConnected = () =>
            {
                var current_ports = DiscoverSerialPorts();

                var new_ports =
                    (from c_port in current_ports
                     where _portsPrevDiscoveredByConnectListener.All((p_port) => { return !p_port.Name.Equals(c_port.Name); })
                     select new SerialPortInfo
                     {
                         Port = c_port.Port,
                         Name = c_port.Name
                     }).ToList();

                if (_portsPrevDiscoveredByConnectListener.Count != current_ports.Count)
                    lock (_connectPadlock) _portsPrevDiscoveredByConnectListener = current_ports.ToList();

                if (new_ports.Count >= 1)
                {
                    newly_connected_port = new_ports[0].Port ?? String.Empty;
                    Debug.Print("(Event: Serial Device Connected) Port= {0}", newly_connected_port);
                    return true;
                }
                else
                {
                    return false;
                }
            };

            AsyncHelper.BackgroundPoll(
                IsNewSerialPortConnected,
                () => wasConnectedCallback(newly_connected_port),
                500,
                timeoutInMs,
                oneshot,
                cancelToken);
        }

        public static void ListenForSerialDeviceDisconnection(Action<string> disconnectedAction, int timeoutInMillis = Timeout.Infinite, bool isOneShot = true, CancellationToken cancelToken = new CancellationToken())
        {
            lock (_disconnectPadlock) _portsPrevDiscoveredByDisconnectListener = DiscoverSerialPorts();
            var newly_disconnected_port = String.Empty;

            Func<bool> IsNewSerialPortDisconnected = () =>
            {
                var currentSerialPorts = DiscoverSerialPorts();

                var removedSerialPorts =
                    (from p_port in _portsPrevDiscoveredByDisconnectListener
                     where currentSerialPorts.All((c_port) => { return !c_port.Name.Equals(p_port.Name); })
                     select new SerialPortInfo
                     {
                         Port = p_port.Port,
                         Name = p_port.Name
                     }).ToList();

                if (_portsPrevDiscoveredByDisconnectListener.Count != currentSerialPorts.Count)
                    lock (_disconnectPadlock) _portsPrevDiscoveredByDisconnectListener = currentSerialPorts.ToList();

                if (removedSerialPorts.Count >= 1)
                {
                    newly_disconnected_port = removedSerialPorts[0].Port ?? String.Empty;
                    Debug.Print("(Event: Serial Device Disconnected) Port= {0}", newly_disconnected_port);
                    return true;
                }
                else
                {
                    return false;
                }
            };

            AsyncHelper.BackgroundPoll(
                IsNewSerialPortDisconnected,
                () => { disconnectedAction(newly_disconnected_port); },
                500,
                timeoutInMillis,
                isOneShot,
                cancelToken);
        }

        private static SerialPort CreateSerialPort(string portName, int baudRate = 9600)
        {
            lock (_serialPortPadlock)
            {
                var serialPort = new SerialPort();
                try
                {
                    serialPort.Handshake = Handshake.RequestToSend;
                    serialPort.BaudRate = baudRate;
                    serialPort.DataBits = 8;
                    serialPort.StopBits = StopBits.One;
                    serialPort.Parity = Parity.None;
                    serialPort.PortName = portName;
                    serialPort.Open();
                    serialPort.DiscardInBuffer();
                    serialPort.DiscardOutBuffer();
                    Thread.Sleep(500); // Safety recommendation from Microsoft best practice doc to wait 1/2 sec
                    Debug.Print("Created serial port: '{0}'", portName);
                }
                catch (Exception ex)
                {
                    Debug.Print("Failed to create and/or open serial port '{0}': '{1}'", portName, ex.GetBaseException().Message);
                    throw ex;
                }
                return serialPort;
            }
        }
    }
}
