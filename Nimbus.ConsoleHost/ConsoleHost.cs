using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CommandLine;
using CommandLine.Text;
using Nimbus.Plumbing;
using Nimbus.Plumbing.Interface;

namespace Nimbus
 {
    class SimpleLog : ISimpleLog
    {
        public void Log(string origin, string message)
        {
            Console.WriteLine(
                String.Format("[Init][{0}] {1} {2} UTC: {3}",
                    origin,
                    DateTime.UtcNow.ToShortDateString(),
                    DateTime.UtcNow.ToShortTimeString(),
                    message));
        }
    }

    class ConsoleHost
    {
        class Options
        {
            [Option('s', "sleep", DefaultValue = false, HelpText = "Sleep indefinitely instead of reading key")]
            public bool SleepMode { get; set; }

            [Option('p', "http-port", DefaultValue = (short)9000, HelpText = "HTTP Listen Port (default: 9000)")]
            public short HttpListen { get; set; }

            [Option('z', "zmq-port", DefaultValue = (short)9001, HelpText = "ZMQ Listen Port (default: 9001)")]
            public short ZmqListen { get; set; }

            [Option('w', "nimbus-web-assembly", DefaultValue = "Nimbus.Web\\bin\\Nimbus.Web.dll", HelpText = "Nimbus.Web.dll file")]
            public string NimbusWebAssemblyFile { get; set; }

            [HelpOption]
            public string GetUsage()
            {
                return HelpText.AutoBuild(this,
                  (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
            }
        }
        /**
         * <summary>Nimbus ConsoleHost Entrypoint</summary>
         * <param name="args">[optional] sleep: instead of reading key, sleeps indefinitely.</param>
         */
        static int Main(string[] args)
        {
            Console.WriteLine("Nimbus ConsoleHost version " + 
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            Console.WriteLine("(c) 2013 The Nimbus Team. All Rights Reserved.\n");

            var cmdline = new Options();

            if (!CommandLine.Parser.Default.ParseArguments(args, cmdline))
            {
                //implementar interface de log
                Console.WriteLine("Could not parse command line. Exiting.");
                return 1;
            }

            try
            {
                Console.WriteLine("Starting Nimbus...");
                var initOptions = new NimbusStartup.StartupOptions()
                {
                    HttpPort = cmdline.HttpListen,
                    ZmqPort = cmdline.ZmqListen,
                    InitLog = new SimpleLog(),
                    IsDebugAllowed = true, //verificar de acordo com a cmdline!
                    IsDevEnvironment = true, //verificar de acordo com a cmdline!
                    NimbusWebAssemblyFile = cmdline.NimbusWebAssemblyFile,
                };

                NimbusStartup.Init(initOptions);

                if (cmdline.SleepMode) Thread.Sleep(Timeout.Infinite);
                else
                {
                    Console.WriteLine("Press ENTER to stop.");
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                //implementar interface de log
                Console.WriteLine("Could not initialize Nimbus. Exiting.");
                Console.WriteLine("Exception: HRESULT " + ex.HResult);
                Console.WriteLine(ex.Message);
                Console.ReadLine();
                return 1;
            }

            return 0;
        }
    }
}
