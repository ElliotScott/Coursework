﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading;

namespace CourseworkServer
{
    public delegate void OnConnect(Client user);
    public delegate void OnDataReceived(Client sender, byte[] data);

    class Server
    {

        public Listener listener;
        public ActionQueue queue = new ActionQueue();
        public DatabaseHandler dbHandler = new DatabaseHandler();
        public static Server server;
        public List<Client> connectedClients;
        MemoryStream readStream;
        MemoryStream writeStream;
        BinaryReader reader;
        BinaryWriter writer;
        const string laptopConnectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=\"C:\\Users\\Lisa\\Source\\Repos\\Coursework\\Coursework Server\\CourseworkDB.mdf\";Integrated Security=True";
        const string desktopConnectionString = "Data Source=.\\SQLEXPRESS;AttachDbFilename=\"C:\\Users\\Robert\\Source\\Repos\\Coursework\\Coursework Server\\CourseworkDB.mdf\";Integrated Security=True;User Instance=True";
        public static string connectionString;

        static void Main(string[] args)
        {
            #region Change this before hand-in
            Console.WriteLine("Use Laptop or Desktop connection string? L/D");
            switch (Console.Read())
            {
                case 'l':
                case 'L':
                    connectionString = laptopConnectionString;
                    break;
                case 'd':
                case 'D':
                    connectionString = desktopConnectionString;
                    break;
                default:
                    Console.WriteLine("Invalid input");
                    Main(args);
                    break;
            }
            #endregion
            server = new Server();
            Console.WriteLine("Server online");
            while (true)
            {
                Thread t = new Thread(new ThreadStart(ReadCommand));
                t.Start();
                Thread t2 = new Thread(new ThreadStart(UpdateQueues));
                t2.Start();
                Thread.Sleep(1000);
                
            }
        }
        public static int GetRangeFromTime(uint t)
        {
            return (int) (50 * Math.Atan((double)(t / 15) - 6) + 120);
        }
        public static void UpdateQueues()
        {
            for (int i = 0; i < server.connectedClients.Count; i++)
            {
                if (server.connectedClients[i].status == Status.InQueue)
                {

                }
            }
        }
        public static void ReadCommand()
        {
            server.ExecuteCommand(Console.ReadLine());
        }
        public Server()
        {
            if (!File.Exists("Messages.txt"))
            {
                File.Create("Messages.txt");
            }
            Console.WriteLine("Initialising Server");
            listener = new Listener();
            connectedClients = new List<Client>();
            Console.WriteLine("Initialising writers and readers");
            readStream = new MemoryStream();
            writeStream = new MemoryStream();
            reader = new BinaryReader(readStream);
            writer = new BinaryWriter(writeStream);
            listener.userAdded += new OnConnect(listener_userAdded);
        }

        public int getAvailableID()
        {
            Console.WriteLine("Searching for available ID for new connection");
            for (int i = 0; i <= connectedClients.Count; i++)
            {
                bool idTaken = false;
                foreach (Client c in connectedClients)
                {
                    if (c.id == i)
                    {
                        idTaken = true;
                        break;
                    }
                }
                if (!idTaken) return i;
            }
            throw new Exception("An unexpected error has occurred. The program will now terminate");
        }
        public void listener_userAdded(Client user)
        {
            Console.WriteLine("Adding user to list of users");
            user.DataReceivedEvent += new OnDataReceived(user_DataReceived);
            user.DisconnectEvent += new OnConnect(user_UserDisconnected);

            connectedClients.Add(user);
        }

        public void user_UserDisconnected(Client user)
        {
            Console.WriteLine("Removing user");
            connectedClients.Remove(user);
        }
        private void ExecuteCommand(string s)
        {
            string[] splitted = s.Split(' ');
            switch (splitted[0])
            {
                case "/sql":
                    string genericSQLString = "";
                    for (int i = 1; i < splitted.Length; i++) genericSQLString += " " + splitted[i];
                    try
                    {
                        Console.WriteLine(dbHandler.DoSQLCommand(genericSQLString.Trim()) + " row(s) affected");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error encountered: " + e);
                    }
                    break;
                case "/sqlQuery":
                    string sqlQuery = "";
                    for (int i = 1; i < splitted.Length; i++) sqlQuery += " " + splitted[i];
                    try
                    {
                        DataTable table = dbHandler.DoSQLQuery(sqlQuery);
                        foreach (DataRow row in table.Rows)
                        {
                            for (int i = 0; i < table.Columns.Count; i++)
                            {
                                Console.Write(row[i].ToString() + " ");
                            }
                            Console.WriteLine();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error encountered: " + e);
                    }
                    break;

                case "/checkCredentials":
                    //bool b = dbHandler.CheckLoginCredentials(splitted[1], splitted[2]);
                    //if (b) Console.WriteLine("Credentials valid");
                    //else Console.WriteLine("Invalid credentials");
                    break;
                case "/close":
                    Environment.Exit(0);
                    break;
                case "/help":
                    Console.WriteLine("/sql <command> : Executes the command given and prints the number of rows affected");
                    Console.WriteLine("/checkCredentials <Username> <PasswordHash> : Checks to see if the given user exists in the DB");
                    Console.WriteLine("/help : Prints all usable commands to the console");
                    Console.WriteLine("/close : Exits the program safely");
                    break;
                default:
                    Console.WriteLine("Command not found. Enter /help for all commands");
                    break;
            }
        }
        private void user_DataReceived(Client sender, byte[] data)
        {
            Protocol p = (Protocol)data[0];

            Console.WriteLine(p);
            foreach (byte b in data) Console.Write(" " + b);
            //SendData(data, sender);
            char[] chars = new char[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                chars[i] = (char)data[i];
            }
            string s = new string(chars);
            Console.WriteLine(s);

            switch (p)
            {
                case Protocol.CreateAccount:
                    queue.Enqueue(new ActionItem(Operation.AddNewAccount, s, sender));
                    break;
                case Protocol.LogIn:
                    queue.Enqueue(new ActionItem(Operation.CheckCredentials, s, sender));
                    break;
                case Protocol.FriendStatus:
                    queue.Enqueue(new ActionItem(Operation.CheckFriendStatus, s, sender));
                    break;
            }
        }

        private byte[] GetDataFromMemoryStream(MemoryStream ms)
        {
            byte[] result;

            lock (ms)
            {
                int bytesWritten = (int)ms.Position;
                result = new byte[bytesWritten];

                ms.Position = 0;
                ms.Read(result, 0, bytesWritten);
            }

            return result;
        }
        public Status StatusOf(string s)
        {
            foreach (Client c in connectedClients)
            {
                if (c.userName == s) return c.status;
            }
            return Status.Offline;
        }
        

    }
}
