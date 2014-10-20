using System;
using System.Linq;
using System.Collections.Generic;

namespace KeiserCmd
{
    public class Bike
    {
        public static bool ParseCommand (string command)
        {
            string[] split = command.Split (new Char[] { ' ' }, 3);

            if (split.Length < 2) {
                Console.WriteLine ("Please specify a sub-option: mode, rpm, gear");
                return true;
            }

            List<Rider> riders = new List<Rider> ();

            if (split [0].ToLower () == "all")
                riders.AddRange (Relay.riders);
            else {
                int bikeId;
                if (!int.TryParse (split [0], out bikeId)) {
                    Console.WriteLine ("Invalid Bike ID");
                    return true;
                }
                riders.Add (Relay.riders.Find (r => r.id == bikeId));
            }

            if (riders.Count == 0) {
                Console.WriteLine ("Invalid Bike ID or no bikes available");
                return true;
            }

            switch (split [1].ToLower ()) {
                case "mode":
                    if (split.Length < 3) {
                        foreach (Rider r in riders)
                            Console.WriteLine ("Bike " + r.id + ": " + (r.simulate ? "simulate" : "static"));
                    } else if (split [2].ToLower () == "simulate") {
                        foreach (Rider r in riders)
                            r.simulate = true;
                    } else if (split [2].ToLower () == "static") {
                        foreach (Rider r in riders)
                            r.simulate = false;
                    } else {
                        Console.WriteLine ("Invalid mode, valid modes are: simulate, static");
                    }
                    return true;

                case "rpm":
                    ushort newrpm = 0;
                    if (split.Length < 3) {
                        foreach (Rider r in riders)
                            Console.WriteLine ("Bike " + r.id + ": " + r.rpm);
                    } else if (UInt16.TryParse (split [2].ToLower (), out newrpm)) {
                        foreach (Rider r in riders)
                            r.rpm = newrpm;
                    } else {
                        Console.WriteLine ("Invalid integer");
                    }
                    return true;

                case "gear":
                    ushort newgear = 0;
                    if (split.Length < 3) {
                        foreach (Rider r in riders)
                            Console.WriteLine ("Bike " + r.id + ": " + r.gear);
                    } else if (UInt16.TryParse (split [2].ToLower (), out newgear)) {
                        foreach (Rider r in riders)
                            r.gear = newgear;
                    } else {
                        Console.WriteLine ("Invalid integer");
                    }
                    return true;
            }

            return false;
        }
    }
}

