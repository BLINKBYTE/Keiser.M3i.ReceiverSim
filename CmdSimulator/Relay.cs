﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace KeiserCmd
{
    class Relay
    {
        private Thread _Thread, _DynThread;
        private volatile Boolean _KeepWorking;
        private Random random = new Random ();
        private int counter = 0;

        public static int riderCounter = 0;

        public bool running = false;
        public string ipAddress = "";
        public UInt16 ipPort;
        public bool uuidSend, versionSend, intervalSend, rssiSend, imperialUnits, randomId, realWorld, gearSend, doLog;

        public static List<Rider> riders = new List<Rider> ();

        public static Relay instance;

        public Relay ()
        {
            instance = this;
        }

        public void start (int numRiders)
        {
            _Thread = new Thread (worker);
            _KeepWorking = running = true;
            if (riders.Count < 1) {
                for (int x = 0; x < numRiders; x++) {
                    NewRider ();
                }
            }
            _Thread.Start ();
            _DynThread = new Thread (dyn_worker);
            _DynThread.Start ();
        }

        public void NewRider ()
        {
            riders.Add (new Rider (random, riders.Count, randomId, realWorld));
            Console.WriteLine ("Added Bike with ID: " + riders.Last ().id);
        }

        public void stop ()
        {
            _KeepWorking = running = false;
        }

        private void dyn_worker ()
        {
            Stopwatch runTime;
            Socket socket = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint ipEndPoint = new IPEndPoint (IPAddress.Parse ("239.10.10.10"), 35679);
            socket.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            while (_KeepWorking) {
                runTime = Stopwatch.StartNew ();
                dyn_broadcast (socket, ipEndPoint);
                runTime.Stop ();
                Thread.Sleep (Convert.ToUInt16 (30000 - runTime.ElapsedMilliseconds));
            }
            socket.Close ();
        }

        private void worker ()
        {
            Stopwatch runTime;
            Socket socket = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint ipEndPoint = new IPEndPoint (IPAddress.Parse (ipAddress), ipPort);
            socket.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            while (_KeepWorking) {
                runTime = Stopwatch.StartNew ();
                foreach (Rider rider in riders) {
                    rider.cycle ();
                }
                broadcast (socket, ipEndPoint);
                runTime.Stop ();
                int sleepTime = (realWorld) ? 500 : 2000;
                Thread.Sleep (Convert.ToUInt16 (sleepTime - runTime.ElapsedMilliseconds));
            }
            socket.Close ();
        }

        private void broadcast (Socket socket, IPEndPoint ipEndPoint)
        {
            bool emptyLog = true;
            List<byte> data = new List<byte> ();
            byte API = Convert.ToByte ((gearSend) ? 0x0B : 0x0A);
            data.Add (API);
            data.Add (getConfig ());
            foreach (Rider rider in riders) {
                if (!realWorld || (rider.cycles == 0 && random.Next (0, 20) != 0)) {
                    if (emptyLog) {
                        Log ("TX Block " + counter++);
                        emptyLog = false;
                    }
                    Log (rider.getStats ());
                    addRider (rider, data);
                }
            }
            if (data.Count > 1)
                socket.SendTo (data.ToArray (), ipEndPoint);
        }

        public void Log (string str)
        {
            if (doLog) {
                System.Console.WriteLine (str);
            }
        }

        private void dyn_broadcast (Socket socket, IPEndPoint ipEndPoint)
        {
            string API = (gearSend) ? "11" : "10";
            List<byte> data = new List<byte> ();
            push_string (data, "KEISER-RECEIVER");
            push_string (data, "|NAME:Receiver Simulator");
            push_string (data, "|API:" + API);
            push_string (data, "|IP:" + ipAddress);
            push_string (data, "|PORT:" + ipPort);
            push_string (data, "|");
            socket.SendTo (data.ToArray (), ipEndPoint);
        }

        private void push_string (List<byte> data, string str)
        {
            foreach (char c in str) {
                data.Add ((byte)c);
            }
        }

        private byte getConfig ()
        {
            byte configFlags = 0;
            if (uuidSend)
                configFlags = Convert.ToByte (configFlags | 1);
            if (versionSend)
                configFlags = Convert.ToByte (configFlags | 2);
            if (intervalSend)
                configFlags = Convert.ToByte (configFlags | 4);
            if (rssiSend)
                configFlags = Convert.ToByte (configFlags | 8);
            if (gearSend)
                configFlags = Convert.ToByte (configFlags | 16);
            if (imperialUnits)
                configFlags = Convert.ToByte (configFlags | 128);
            return configFlags;
        }

        private void addRider (Rider rider, List<byte> data)
        {
            if (!intervalSend && rider.interval != 0)
                return;

            add_1_byte (rider.id, data);

            if (uuidSend) {
                data.Add (rider.uuid [5]);
                data.Add (rider.uuid [4]);
                data.Add (rider.uuid [3]);
                data.Add (rider.uuid [2]);
                data.Add (rider.uuid [1]);
                data.Add (rider.uuid [0]);
            }

            if (versionSend) {
                add_1_byte (rider.versionMajor, data);
                add_1_byte (rider.versionMinor, data);
            }

            add_1_byte (Convert.ToUInt16 (rider.rpm / 10.0), data);
            add_1_byte (Convert.ToUInt16 (rider.hr / 10.0), data);
            add_2_byte (rider.power, data);

            if (intervalSend) {
                add_1_byte (rider.interval, data);
                add_2_byte (rider.kcal, data);
                add_2_byte (rider.clock, data);
                add_2_byte (rider.trip, data);
            }

            if (rssiSend)
                unchecked {
                    data.Add ((byte)Convert.ToSByte (rider.rssi));
                }

            if (gearSend)
                add_1_byte (rider.gear, data);

        }

        private void add_1_byte (UInt16 data, List<byte> list)
        {
            list.Add (Convert.ToByte (data & 0xFF));
        }

        private void add_2_byte (UInt16 data, List<byte> list)
        {
            list.Add (Convert.ToByte (data & 0xFF));
            list.Add (Convert.ToByte ((data >> 8) & 0xFF));
        }

    }

    class Rider
    {
        public UInt16 cycles;
        public UInt16 id, versionMajor, versionMinor, rpm, hr, power, interval, rkcal, rclock, rtrip, gear;
        public Int16 rssi = -50;
        public byte[] uuid = new byte[6];

        public float speedMultiplier = 1.0f;

        public UInt16 kcal {
            get {
                if (interval != 0 || inInterval)
                    return intKcal;
                return rkcal;
            }
        }

        public UInt16 clock {
            get {
                if (interval != 0 || inInterval)
                    return intClock;
                return rclock;
            }
        }

        public UInt16 trip {
            get {
                if (interval != 0 || inInterval)
                    return intTrip;
                return rtrip;
            }
        }

        private Random random;
        private double rtripAct, intTripAct;
        private int intervalCounter;
        private bool inInterval = false;
        private UInt16 intKcal, intClock, intTrip;
        private int age, maxHR, refresh = 2;
        private double rcal, intCal;
        private bool realWorld;
        public bool simulate = true;

        public Rider (Random parentRandom, int x, bool randomId, bool realWorld)
        {
            this.realWorld = realWorld;
            random = parentRandom;
            age = random.Next (20, 55);
            maxHR = (2200 - (age * 10));
            id = Convert.ToUInt16 (x + 1);
            versionMajor = 0x06;
            versionMinor = 0x21;
            hr = Convert.ToUInt16 (random.Next (Convert.ToUInt16 (maxHR * 0.4), Convert.ToUInt16 (maxHR * 0.9)));
            rpm = Convert.ToUInt16 (1100 * hr / maxHR);
            gear = Convert.ToUInt16 (random.Next (1, 24));
            power = getPower ();
            rcal = random.Next (0, 50000);
            interval = 0;
            intervalCounter = random.Next (1, 10);
            inInterval = false;
            rkcal = calToKcal (rcal);
            rclock = Convert.ToUInt16 (random.Next (0, 300));
            rtripAct = random.Next (0, 300);
            rtrip = Convert.ToUInt16 (rtripAct);
            cycles = Convert.ToUInt16 (random.Next (0, 3) * speedMultiplier);
            generateUUID (x, randomId);
        }

        private UInt16 getPower ()
        {
            return Convert.ToUInt16 ((gear / 64.0) * rpm);
        }

        private void generateUUID (int y, bool randomId)
        {
            if (randomId) {
                for (int x = 0; x < 6; x++) {
                    uuid [x] = Convert.ToByte (random.Next (0, 255));
                }
            } else {
                for (int x = 0; x < 6; x++) {
                    uuid [x] = Convert.ToByte (y + 1);
                }
            }
            Relay.riderCounter++;
        }

        private string getUuidString ()
        {
            string uuidString = "";
            int count = 0;
            foreach (Byte segment in uuid) {
                uuidString += string.Format ("{0:X2}", segment);
                if (count++ < 5)
                    uuidString += ":";
            }
            return uuidString;
        }

        public void cycle ()
        {
            if (realWorld) {
                if (++cycles < 4)
                    return;
                cycles = 0;
            }
            int effort = effortPredictor ();
            if (effort == 1 && simulate) {
                if (gear < 24)
                    gear++;
                if (rpm < 1100)
                    rpm += Convert.ToUInt16 (random.Next (0, 100));
                hr += Convert.ToUInt16 (random.Next (10, 30));
            }
            if (effort == -1 && simulate) {
                if (gear > 2)
                    gear--;
                if (rpm > 400)
                    rpm -= Convert.ToUInt16 (random.Next (0, 100));
                hr -= Convert.ToUInt16 (random.Next (10, 30));
            }
            power = getPower ();
            if (inInterval) {
                intCal += (power / 4.187) * 4 * refresh;
                intKcal = Convert.ToUInt16 (intCal / 1000);
                intClock += Convert.ToUInt16 (refresh);
                intTripAct += getPower () / 1000.0;
                intTrip = Convert.ToUInt16 (intTripAct);
            } else {
                rcal += (power / 4.187) * 4 * refresh;
                rkcal = Convert.ToUInt16 (rcal / 1000);
                rclock += Convert.ToUInt16 (refresh);
                rtripAct += getPower () / 1000.0;
                rtrip = Convert.ToUInt16 (rtripAct);
            }
            if (random.Next (0, 20) == 0) {
                if (inInterval) {
                    rcal += intCal;
                    rkcal += intKcal;
                    rclock += intClock;
                    rtrip += intTrip;
                    inInterval = false;
                    interval = Convert.ToUInt16 (intervalCounter++);
                } else {
                    intCal = 0;
                    intKcal = 0;
                    intClock = 0;
                    intTrip = 0;
                    inInterval = true;
                }
            } else
                interval = 0;
            if (random.Next (0, 10) > 5) {
                if (rssi < -19)
                    rssi++;
            } else {
                if (rssi > -60)
                    rssi--;
            }
        }

        private UInt16 calToKcal (double cal)
        {
            return Convert.ToUInt16 (cal / 1000);
        }

        public int effortPredictor ()
        {
            int effort = random.Next (0, 100);
            int hrRange = Convert.ToInt32 ((hr / maxHR) * 100);

            if (hrRange <= 50) {
                if (effort >= 90)
                    return 1;
                return 0;
            }
            if (hrRange <= 60) {
                if (effort >= 75)
                    return 1;
                //if (effort >= 20) return 0;
                return -1;
            }
            if (hrRange <= 80) {
                //if (effort >= 90) return 0;
                if (effort >= 50)
                    return 1;
                return -1;
            }
            if (hrRange <= 90) {
                if (effort >= 95)
                    return -1;
                if (effort >= 5)
                    return 0;
                return 1;
            }
            return -1;
        }

        public string getStats ()
        {
            //return string.Format("ID: {0,3} RPM: {1,5:0.0} HR: {2,5:0.0} POWER: {3,4:} INT: {4:2} KCAL: {5,4:} CLOCK: {6,5:} TRIP: {7,4:0.0} UUID: {8} RSSI: {9,3}dBm", id, rpm / 10.0, hr / 10.0, power, interval, kcal, clock, trip, getUuidString(), rssi);
            return string.Format ("ID: {0,3} RPM: {1,4:0.0} HR: {2,4:0.0} POWER: {3,4:} INT: {4,2} KCAL: {5,3:} CLOCK: {6,4:} TRIP: {7,3:0.0} RSSI: {8,3}dBm GEAR: {9,2}", id, rpm / 10.0, hr / 10.0, power, interval, kcal, clock, trip / 10.0, rssi, gear);
        }
    }
}
