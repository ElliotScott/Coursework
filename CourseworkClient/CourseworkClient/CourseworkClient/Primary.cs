using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Security.Cryptography;
using System.Text;
using CourseworkClient.Gui;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace CourseworkClient
{
    public enum ScreenRatio
    {
        FourByThree,
        SixteenByNine,
        Other
    }
    public class Primary : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        HMACMD5 hasher = new HMACMD5();
        public KeyPressHandler keypresshandler = new KeyPressHandler();
        ScreenRatio ratio;
        public Texture2D loginScreenBackground;
        public Texture2D textFieldTexture;
        public Texture2D title;
        public Texture2D buttonTexture;
        public Texture2D textFieldInfoTab;
        public Texture2D greenArrowTexture, grayArrowTexture;
        public SpriteFont mainFont;
        public static Primary game;
        public Form currentForm;
        public FriendManager friendManager;
        TcpClient client;
        MemoryStream readMemoryStream, writeMemoryStream;
        BinaryReader binaryReader;
        BinaryWriter binaryWriter;
        const string ip = "192.168.1.71";
        const int port = 1337;
        const int bufferSize = 2048;
        byte[] readBuffer;
        public bool connected = false;
        public int connectTimer = 0;
        public string username;

        static void Main(string[] args)
        {
            game = new Primary();
            game.Run();
        }
        public Primary()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferHeight = 600;
            graphics.PreferredBackBufferWidth = 800;
            Window.Title = "Hearthclone";

        }

        protected override void Initialize()
        {

            client = new TcpClient();
            client.NoDelay = true;
            readBuffer = new byte[bufferSize];
            readMemoryStream = new MemoryStream();
            writeMemoryStream = new MemoryStream();
            binaryReader = new BinaryReader(readMemoryStream);
            binaryWriter = new BinaryWriter(writeMemoryStream);
            IsMouseVisible = true;
            ratio = CalculateRatio();
            new Thread(new ThreadStart(ConnectClient)).Start();
            //friendManager = new FriendManager();
            base.Initialize();
        }
        protected override void LoadContent()
        {
            loginScreenBackground = LoadLoadingScreenBackground();
            textFieldTexture = Content.Load<Texture2D>("TextFieldBox");
            buttonTexture = Content.Load<Texture2D>("ButtonIcon");
            currentForm = new LoginScreenForm();
            spriteBatch = new SpriteBatch(GraphicsDevice);
            mainFont = Content.Load<SpriteFont>("Mainfont");
            title = Content.Load<Texture2D>("Title");
            textFieldInfoTab = Content.Load<Texture2D>("InfoTab");
            greenArrowTexture = Content.Load<Texture2D>("Scroll Arrow");
            grayArrowTexture = Content.Load<Texture2D>("Grayed Out Scroll Arrow");
        }

        protected override void Update(GameTime gameTime)
        {
            currentForm.Update();
            base.Update(gameTime);
            keypresshandler.UpdateOldState();
            GuiItem.UpdateOldState();
            if (!connected)
            {
                if (connectTimer++ == 100)
                {

                    new Thread(new ThreadStart(ConnectClient)).Start();
                    connectTimer = 0;
                }
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.LightGoldenrodYellow);
            spriteBatch.Begin();
            currentForm.Draw(spriteBatch);
            spriteBatch.End();

            base.Draw(gameTime);

        }
        public string ComputeHash(string s)
        {
            int l = 16;
            char[] f = s.ToCharArray();
            string o = "";
            for (int i = 0; i < l; i++)
            {
                int r = f[i % f.Length];

                o += ((r ^ i) % 16).ToString("X"); ;
            }
            return o;

        }
        public ScreenRatio CalculateRatio()
        {
            if ((3d * GraphicsDevice.Viewport.Width) / (4d * GraphicsDevice.Viewport.Height) == 1d)
            {
                return ScreenRatio.FourByThree;
            }
            else if ((9d * GraphicsDevice.Viewport.Width) / (16d * GraphicsDevice.Viewport.Height) == 1d)
            {
                return ScreenRatio.SixteenByNine;
            }
            else return ScreenRatio.Other;
        }
        public Texture2D LoadLoadingScreenBackground()
        {
            switch (ratio)
            {
                case ScreenRatio.FourByThree:
                    return Content.Load<Texture2D>("4x3 Background");
                case ScreenRatio.SixteenByNine:
                case ScreenRatio.Other:
                    return Content.Load<Texture2D>("16x9 Background");
            }
            throw new InvalidOperationException("Something is very wrong here");
        }
        public void ConnectClient()
        {
            try
            {
                client.Connect(ip, port);
                client.GetStream().BeginRead(readBuffer, 0, bufferSize, StreamReceived, null);
                try
                {
                    if (currentForm.GetType() == typeof(CreateAccountForm))
                    {
                        ((CreateAccountForm)currentForm).errorMessageText = "";
                    }
                    else if (currentForm.GetType() == typeof(LoginScreenForm))
                    {
                        ((LoginScreenForm)currentForm).errorMessageText = "";
                    }
                }
                catch (NullReferenceException) { }

            }
            catch
            {
                connected = false;
                return;
            }
            connected = true;
        }
        private void StreamReceived(IAsyncResult ar)
        {
            int bytesRead = 0;
            try
            {
                bytesRead = client.GetStream().EndRead(ar);
            }
            catch
            {
            }
            byte[] data = new byte[bytesRead];
            for (int i = 0; i < bytesRead; i++)
                data[i] = readBuffer[i];
            ProcessData(data);
            try
            {
                client.GetStream().BeginRead(readBuffer, 0, bufferSize, StreamReceived, null);
            }
            catch
            {
                connected = false;
                ExitGame();
            }
        }
        public void ExitGame()
        {
            System.Windows.Forms.MessageBox.Show("Game has encountered issue. Closing now");
            Environment.Exit(0);
        }
        private void ProcessData(byte[] data)
        {
            try
            {
                readMemoryStream.SetLength(0);
                readMemoryStream.Position = 0;

                readMemoryStream.Write(data, 0, data.Length);
                readMemoryStream.Position = 0;
                Protocol p;
                p = (Protocol)binaryReader.ReadByte();
                HandleData(p);
            }
            catch
            {
            }

        }

        private void HandleData(Protocol p)
        {
            switch (p)
            {
                case Protocol.UsernameTaken:
                    ((CreateAccountForm)currentForm).errorMessageText = "Username taken";
                    break;
                case Protocol.BadCredentials:
                    ((LoginScreenForm)currentForm).errorMessageText = "Username or Password is incorrect";
                    break;
                case Protocol.GoodCredentials:
                    username = ((TextField)(((LoginScreenForm)currentForm).formItems[0])).text;
                    currentForm = new MainMenuForm();
                    break;
                case Protocol.FriendStatus:
                    string friendUserName = binaryReader.ReadString();
                    byte status = binaryReader.ReadByte();
                    //Add more stuff here later
                    break;
                case Protocol.LoggedIn:
                    ((LoginScreenForm)currentForm).errorMessageText = "You are already logged in on another instance.\nTest";
                    break;
                case Protocol.EnterMatch:
                    ShowMessage("Entering match");
                    break;
                    //case Protocol.
                default:
                    ExitGame();
                    break;

            }
        }
        public static void ShowMessage(string s)
        {
            System.Windows.Forms.MessageBox.Show(s);
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
        public void SendData(byte[] b)
        {
            try
            {
                lock (client.GetStream())
                {
                    client.GetStream().BeginWrite(b, 0, b.Length, null, null);
                }
            }
            catch
            {
                ////Console.WriteLine("Error sending data");
            }
        }
        public void WriteDataToStream(Protocol p)
        {


            writeMemoryStream.Position = 0;
            binaryWriter.Write((byte)p);
            SendData(GetDataFromMemoryStream(writeMemoryStream));



        }
        public void WriteDataToStream(Protocol p, params string[] o)
        {
            writeMemoryStream.Position = 0;
            binaryWriter.Write((byte)p);
            foreach (string e in o)
            {
                binaryWriter.Write(e);
                SendData(GetDataFromMemoryStream(writeMemoryStream));
                writeMemoryStream.Position = 0;
                binaryWriter.Write((byte)p);

            }

        }
    }
}
