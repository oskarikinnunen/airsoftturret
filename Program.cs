using Network;
using Network.Converter;
using Network.Enums;
using Network.Extensions;
using Network.Packets;
using System;
using System.Linq;
using System.Reflection;
using System.Timers;
using SerialPort = System.IO.Ports.SerialPort;
using System.IO;


namespace LadaTurretServer
{
	/* This is the server program running on the raspberry pi.
	 * This receives packets from a unity client, which handles the input from the xbox controller and keyboard. */
	class Program
	{
		private const int baudrate = 38400;
		static ServerConnectionContainer scContainer;
		static TcpConnection connection;
		static string controlString = "x000dr090cfldmd"; //TODO: convert this to binary, using a whole string is wasteful
		static string arduinoPortName = "COM3";
		private static SerialPort arduinoPort;
		static string[] sPortNames;

		static void Main(string[] args)
		{
			StartServer();
			SerialQuery();

			while (true) ;
		}

		static void SerialQuery()
		{
			if (sPortNames == null)
				sPortNames = SerialPort.GetPortNames();

			if (sPortNames.Length > 0)
			{
				string folder = Assembly.GetEntryAssembly().Location.Replace("ConsoleNetwork2.dll", "");
				Console.WriteLine("path = " + folder);
				arduinoPortName = File.ReadAllText(folder + "ARDUINOPORT").Replace("\n", "").Replace("\r", "");
				Console.WriteLine("ARDUINOPORT is " + arduinoPortName);
				Console.WriteLine("Found some ports:");
				foreach (string s in sPortNames)
					Console.WriteLine(s);

				if (sPortNames.Contains(arduinoPortName))
				{
					TryOpenPort(arduinoPortName);
				}
				else
				{
					Console.WriteLine($"But not {arduinoPortName}, which is specified as the arduino port.");
					Console.WriteLine("Select new arduinoport:");
					string newPortName = Console.ReadLine();
					arduinoPortName = newPortName;
					TryOpenPort(arduinoPortName);
				}
			}
		}

		static void TryOpenPort(string name)
		{
			arduinoPort = new SerialPort(arduinoPortName, baudrate)
			{
				ReadTimeout = 1500,
				WriteTimeout = 1500
			};

			try
			{
				arduinoPort.Open();
			}
			catch (System.IO.IOException e)
			{
				Console.WriteLine("Error when opening port: " + e.Message);
			}

			if (arduinoPort.IsOpen)
				Console.WriteLine("SerialPort " + arduinoPortName + " opened successfully.");
			else
				Console.WriteLine("SerialPort " + arduinoPortName + " couldn't be opened.");
		}

		static void OnClientTick(Object source, ElapsedEventArgs e)
		{
			if (Console.KeyAvailable)
			{
				Console.WriteLine("key");
				string r = Console.ReadKey().KeyChar.ToString();
				connection.SendRawData(RawDataConverter.FromUTF8String("Control", "clientdata: " + r));
			}
		}

		static void StartServer()
		{
			scContainer = ConnectionFactory.CreateServerConnectionContainer(1234, true);
			#region Optional settings
			scContainer.ConnectionLost += (a, b, c) => Console.WriteLine($"{scContainer.Count} {b} Connection lost {a.IPRemoteEndPoint.Port}. Reason {c.ToString()}");
			scContainer.ConnectionEstablished += connectionEstablished;
			scContainer.AllowUDPConnections = true;
			scContainer.UDPConnectionLimit = 2;
			#endregion Optional settings
			scContainer.Start();
			Console.WriteLine("Server started.");
		}

		static void connectionEstablished(Connection connection, ConnectionType type)
		{
			Console.WriteLine($"{scContainer.Count} {connection.GetType()} connected on port {connection.IPRemoteEndPoint.Port}");

			//3. Register packet listeners.
			Timer t1 = new Timer(100f);
			t1.Elapsed += OnServerTick;
			t1.Start();
			connection.RegisterRawDataHandler("Control", ServerPacketHandler);
		}

		private static void ServerPacketHandler(RawData packet, Connection connection)
		{
			string data = packet.ToUTF8String();
			controlString = data;
			Console.SetCursorPosition(0, 14);
			Console.Write($"RawDataPacket received. Data: {controlString}");
		}

		static void OnServerTick(Object source, ElapsedEventArgs e)
		{
			if (arduinoPort != null && arduinoPort.IsOpen)
				arduinoPort.WriteLine(controlString);
		}
	}
}
