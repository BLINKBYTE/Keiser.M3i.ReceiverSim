using System;

namespace KeiserCmd
{
    public class MainClass
    {
        private Relay relay;

        public MainClass ()
        {
            //log = new Log();
            relay = new Relay ();
        }

        public void Start ()
        {
            Config.FlushConfig ();
            relay.start (Config.Bikes);

            string readLine = "";

            Console.WriteLine ("Receiver Simulator Command-Line Version");

            while (true) {
                Console.Write ("Enter Command: ");
                readLine = System.Console.ReadLine ();
                string[] split = readLine.Split (new Char[] { ' ' }, 2);

                if (split.Length < 2) {
                    split = new string[] { split [0], "" };
                }

                switch (split [0].ToLower ()) {

                    case "config":
                        Config.ParseCommand (split [1]);
                        break;

                    case "bike":
                        Bike.ParseCommand (split [1]);
                        break;

                    case "add":
                        relay.NewRider ();
                        break;

                    case "list":
                        foreach (Rider r in Relay.riders)
                            Console.WriteLine (r.getStats ());
                        break;


                    case "start":
                        if (relay.running)
                            Console.WriteLine ("Relay already running");
                        else {
                            Console.WriteLine ("Starting Relay to " + relay.ipAddress + ":" + relay.ipPort);
                            relay.start (Config.Bikes);
                        }
                        break;

                    case "stop":
                        if (!relay.running) {
                            Console.WriteLine ("Relay not running");
                        } else {
                            Console.WriteLine ("Halting Relay");
                            relay.stop ();
                        }
                        break;

                    case "quit":
                        relay.stop ();
                        Environment.Exit (0);
                        return;

                    default:
                        System.Console.WriteLine ("Please specify a command.");
                        break;
                }
            }
        }
    }
}

