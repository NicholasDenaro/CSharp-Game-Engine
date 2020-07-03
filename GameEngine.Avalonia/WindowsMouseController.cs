﻿using Avalonia.Input;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace GameEngine.UI.AvaloniaUI
{
    public class WindowsMouseController : Controller
    {
        private Dictionary<int, int> keymap;
        private bool hooked = false;

        public WindowsMouseController() : base()
        {

        }

        public WindowsMouseController(Dictionary<int, int> keymap) : base(keymap.Values)
        {
            this.keymap = keymap;
        }

        public void Hook(GameFrame frame)
        {
            if (frame.window == null)
            {
                return;
            }

            frame.Pane.HookMouse(Frame_KeyInfo, Frame_KeyDown, Frame_KeyUp);
            hooked = true;
        }

        public bool IsHooked()
        {
            return hooked;
        }

        private void Frame_KeyUp(object sender, MouseEventArgs e)
        {
            if (keymap.ContainsKey(e.Button))
            {
                ActionEnd(keymap[e.Button], new MouseControllerInfo(new Point(e.X, e.Y)));
            }
        }

        private void Frame_KeyDown(object sender, MouseEventArgs e)
        {
            if (keymap.ContainsKey(e.Button))
            {
                ActionStart(keymap[e.Button], new MouseControllerInfo(new Point(e.X, e.Y)));
            }
        }

        private void Frame_KeyInfo(object sender, MouseEventArgs e)
        {
            if (keymap.ContainsKey(e.Button))
            {
                ActionInfo(keymap[e.Button], new MouseControllerInfo(new Point(e.X, e.Y)));
            }
        }

        public override string Serialize()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(this.GetType().FullName);
            sb.Append(":");
            sb.Append("{");
            sb.Append(base.Serialize());
            sb.Append(",");
            sb.Append(StringConverter.Serialize<int, int>(keymap));
            sb.Append("}");
            return sb.ToString();
        }

        public override void Deserialize(string state)
        {
            List<string> tokens = StringConverter.DeserializeTokens(state);
            base.Deserialize(tokens[0]);
            keymap = StringConverter.Deserialize<int, int>(tokens[1], str => int.Parse(str), str => int.Parse(str));
        }
    }
}
