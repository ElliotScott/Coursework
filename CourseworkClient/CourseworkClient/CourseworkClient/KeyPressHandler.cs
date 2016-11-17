﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace CourseworkClient
{
    public class KeyPressHandler
    {
        KeyboardState oldKeyboardState;
        const int ASCII_A = 'A';
        const int ASCII_Z = 'Z';
        List<KeyPressTimer> timers = new List<KeyPressTimer>();
        #region dict
        Dictionary[] dictionary = { new Dictionary(Keys.OemPipe, '\\', '|'),
            new Dictionary(Keys.OemComma, ',', '<'),
            new Dictionary(Keys.OemPeriod, '.', '>'),
            new Dictionary(Keys.OemSemicolon, ';', ':'),
            new Dictionary(Keys.OemQuestion, '/', '?'),
            new Dictionary(Keys.OemTilde, '\'', '@'),
            new Dictionary(Keys.OemQuotes, '#', '~'),
            new Dictionary(Keys.D1, '1', '!'),
            new Dictionary(Keys.D2, '2', '"'),
            new Dictionary(Keys.D3, '3', '£'),
            new Dictionary(Keys.D4, '4', '$'),
            new Dictionary(Keys.D5, '5', '%'),
            new Dictionary(Keys.D6, '6', '^'),
            new Dictionary(Keys.D7, '7', '&'),
            new Dictionary(Keys.D8, '8', '*'),
            new Dictionary(Keys.D9, '9', '('),
            new Dictionary(Keys.D0, '0', ')'),
            new Dictionary(Keys.OemOpenBrackets, '[', '{'),
            new Dictionary(Keys.OemCloseBrackets, ']', '}'),
            new Dictionary(Keys.OemMinus, '-', '_'),
            new Dictionary(Keys.OemPlus, '=', '+'),
            new Dictionary(Keys.Space, ' ', ' ')
        };
        #endregion

        public string NewTypedString(string initstring, int maxLength)
        {
            KeyboardState currentKeyboardState = Keyboard.GetState();
            Keys[] newKeys = currentKeyboardState.GetPressedKeys();
            Keys[] oldKeys = oldKeyboardState.GetPressedKeys();
            bool shift = false;
            if (currentKeyboardState.IsKeyDown(Keys.LeftShift) || currentKeyboardState.IsKeyDown(Keys.RightShift)) shift = true;
            foreach (Keys key in newKeys)
            {
                if (!oldKeys.Contains(key))
                {
                    if (key == Keys.Back)
                    {
                        if (initstring.Length > 0) initstring = initstring.Substring(0, initstring.Length - 1);
                    }
                    else if (initstring.Length < maxLength) initstring += OnKeyDownType(key, shift);
                    timers.Add(new KeyPressTimer(key));
                }
                else
                {
                    int index = GetIndexOfTimer(key);
                    timers[index]++;
                }
            }
            foreach (Keys key in oldKeys)
            {
                if (!newKeys.Contains(key))
                {
                    int index = GetIndexOfTimer(key);
                    timers.RemoveAt(index);
                }
            }
            oldKeyboardState = currentKeyboardState;
            return initstring;
            
        }
        string OnKeyDownType(Keys key, bool shift)
        {
            string keyname = key.ToString();
            char[] chars = keyname.ToCharArray();
            if (chars.Length == 1 && chars[0] >= ASCII_A && chars[0] <= ASCII_Z) return shift ? chars[0].ToString() : chars[0].ToString().ToLower();
            else foreach (Dictionary d in dictionary)
                if (d.key == key) return shift ? d.shiftCharacter.ToString() : d.character.ToString();
            return "";
        }
        int GetIndexOfTimer(Keys k)
        {
            for (int i = 0; i < timers.Count; i++)
            {
                if (timers[i].key == k) return i;
            }
            return -1;
        }

    }
    class Dictionary
    {
        public Keys key;
        public char character;
        public char shiftCharacter;
        public Dictionary(Keys k, char c, char sc)
        {
            key = k;
            character = c;
            shiftCharacter = sc;
        }
    }
    class KeyPressTimer
    {
        public int timer;
        public Keys key;
        public KeyPressTimer(Keys k)
        {
            key = k;
            timer = 0;
        }
        public static implicit operator int(KeyPressTimer k) => k.timer;
        public static KeyPressTimer operator ++(KeyPressTimer k)
        {
            k.timer++;
            return k;
        }
    }
    
}
