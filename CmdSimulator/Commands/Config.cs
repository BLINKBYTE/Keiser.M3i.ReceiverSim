using System;
using System.Net;
using System.Linq;

namespace KeiserCmd
{
    public class Config
    {
        static string _IP = "239.10.10.10";

        public static string IP {
            get { return _IP; }
            set {
                _IP = value;
                Relay.instance.ipAddress = value;
            }
        }

        static int _Port = 35680;

        public static int Port {
            get { return _Port; }
            set {
                _Port = value;
                Relay.instance.ipPort = Convert.ToUInt16 (value);
            }
        }

        static int _Bikes = 5;

        public static int Bikes {
            get { return _Bikes; }
            set {
                _Bikes = value;
                Relay.instance.stop ();
                Relay.instance.start (value);
            }
        }

        static bool _sendUUID = true;

        public static bool SendUUID {
            get { return _sendUUID; }
            set {
                _sendUUID = value;
                Relay.instance.uuidSend = value;
            }
        }

        static bool _sendVersion = true;

        public static bool SendVersion {
            get { return _sendVersion; }
            set {
                _sendVersion = value;
                Relay.instance.versionSend = value;
            }
        }

        static bool _sendInterval = true;

        public static bool SendInterval {
            get { return _sendInterval; }
            set {
                _sendInterval = value;
                Relay.instance.intervalSend = value;
            }
        }

        static bool _sendRSSI = true;

        public static bool SendRSSI {
            get { return _sendRSSI; }
            set {
                _sendRSSI = value;
                Relay.instance.rssiSend = value;
            }
        }

        static bool _sendGear = true;

        public static bool SendGear {
            get { return _sendGear; }
            set {
                _sendGear = value;
                Relay.instance.gearSend = value;
            }
        }

        static bool _randomId = true;

        public static bool RandomID {
            get { return _randomId; }
            set {
                _randomId = value;
                Relay.instance.randomId = value;
            }
        }

        static bool _realWorld = true;

        public static bool RealWorld {
            get { return _realWorld; }
            set {
                _realWorld = value;
                Relay.instance.realWorld = value;
            }
        }

        static bool _doLog = false;

        public static bool DoLog {
            get { return _doLog; }
            set {
                _doLog = value;
                Relay.instance.doLog = value;
            }
        }

        public static void FlushConfig ()
        {
            Relay.instance.ipAddress = _IP;
            Relay.instance.ipPort = Convert.ToUInt16 (_Port);
            Relay.instance.doLog = _doLog;
            Relay.instance.uuidSend = _sendUUID;
            Relay.instance.gearSend = _sendGear;
            Relay.instance.randomId = _randomId;
            Relay.instance.versionSend = _sendVersion;
            Relay.instance.realWorld = _realWorld;
            Relay.instance.randomId = _randomId;
            Relay.instance.rssiSend = _sendRSSI;
            Relay.instance.intervalSend = _sendInterval;
        }

        public static bool ParseCommand (string command)
        {
            string[] split = command.Split (new Char[] { ' ' }, 2);

            switch (split [0].ToLower ()) {
                case "ip":
                    if (split.Length == 2) {
                        IPAddress unused;
                        if (IPAddress.TryParse (split [1], out unused)) {
                            IP = split [1];
                            System.Console.WriteLine ("IP set to " + IP);
                        } else {
                            System.Console.WriteLine ("Invalid IP Address");
                        }
                    } else {
                        System.Console.WriteLine (IP);
                    }
                    return true;

                case "port":
                    if (split.Length == 2) {
                        int port = Convert.ToInt32 (split [1]);
                        if (port > 0 && port < 65535) {
                            Port = port;
                            System.Console.WriteLine ("Port set to " + port);
                        } else {
                            System.Console.WriteLine ("Please specify a valid port");
                        }
                    } else {
                        System.Console.WriteLine (Port);
                    }
                    return true;

                case "bikes":
                    if (split.Length == 2) {
                        int bikes = Convert.ToInt32 (split [1]);
                        if (bikes > 0) {
                            Bikes = bikes;
                            System.Console.WriteLine ("Bikes set to " + Bikes);
                        } else {
                            System.Console.WriteLine ("Please specify a number great than 0");
                        }
                    } else {
                        System.Console.WriteLine ("Invalid number specified");
                    }
                    return true;

                case "uuid": 
                    if (split.Length == 2) {
                        SendUUID = split [1] == "1";
                    } else {
                        System.Console.WriteLine (SendUUID);
                    }
                    return true;
            }

            return false;
        }
    }
}