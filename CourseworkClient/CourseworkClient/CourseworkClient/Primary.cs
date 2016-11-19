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
        Texture2D loginScreenBackground;
        public SpriteFont mainFont;
        public static Primary game;

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
            ratio = CalculateRatio();
            base.Initialize();
        }
        protected override void LoadContent()
        {
            loginScreenBackground = LoadLoadingScreenBackground();
            spriteBatch = new SpriteBatch(GraphicsDevice);
            mainFont = Content.Load<SpriteFont>("Mainfont");
        }

        protected override void Update(GameTime gameTime)
        {
            Window.Title = keypresshandler.NewTypedString(Window.Title, 50);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.LightGoldenrodYellow);
            spriteBatch.Begin();
            spriteBatch.Draw(loginScreenBackground, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
            for (int i = 0; i < keypresshandler.timers.Count; i++)
            {
                spriteBatch.DrawString(mainFont, keypresshandler.timers[i].ToString(), new Vector2(100, 50 + (i * 50)), Color.Black);
            }
            spriteBatch.End();
            base.Draw(gameTime);
        }
        public string ComputeHash(string s)
        {
            byte[] b = hasher.ComputeHash(Encoding.UTF8.GetBytes(s));
            string output = "";
            foreach (byte e in b)
            {
                output += e.ToString("x0");
            }
            return output;
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

    }
}
