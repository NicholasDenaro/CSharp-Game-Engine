﻿using GameEngine._2D;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace GameEngine.UI.WinForms
{
    public class GamePanel : Panel, IGamePanel
    {
        public bool Drawing { get; private set; }
        private Bitmap[] buffers;
        private byte currentBuffer;
        public double ScaleX { get; private set; }
        public double ScaleY { get; private set; }
        Semaphore sem = new Semaphore(1, 1);

        public GamePanel(int width, int height, double xScale, double yScale)
        {
            Drawing = false;
            this.ScaleX = xScale;
            this.ScaleY = yScale;
            this.Width = (int)(width * xScale);
            this.Height = (int)(height * yScale);
            currentBuffer = 0;
            buffers = new Bitmap[] { BitmapExtensions.CreateBitmap(this.Width, this.Height), BitmapExtensions.CreateBitmap(this.Width, this.Height) };
            this.DoubleBuffered = true;
        }

        public void Resize(int width, int height)
        {
            this.Width = width;
            this.Height = height;
            buffers = new Bitmap[] { BitmapExtensions.CreateBitmap(this.Width, this.Height), BitmapExtensions.CreateBitmap(this.Width, this.Height) };
        }

        public void Draw(GameView2D view)
        {
            Drawing = true;

            if (sem.WaitOne())
            {
                Drawer2DSystemDrawing d = view.Drawer as Drawer2DSystemDrawing;
                Graphics gfx = Graphics.FromImage(buffers[++currentBuffer % 2]);
                gfx.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

                //gfx.DrawImage(img, 0, 0, this.Width + 2, this.Height + 2);
                gfx.DrawImage(d.Image(currentBuffer % 2).Image, 1, 1, this.Width, this.Height);
                gfx.DrawImage(d.Overlay(currentBuffer % 2).Image, 1, 1, d.Overlay(currentBuffer % 2).Width, d.Overlay(currentBuffer % 2).Height);
                //gfx.DrawRectangle(Pens.Cyan, 0, 0, width - 1, height - 1);

                Invalidate();

                while (Drawing)
                {
                    Thread.Sleep(TimeSpan.FromTicks(1000));
                }

                sem.Release();
            }
        }

        public void DrawHandle(object sender, View view)
        {
            GameView2D view2D = view as GameView2D;
            if (view2D != null)
            {
                Draw(view2D);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            bool selfDraw = Drawing;
            try
            {
                if (selfDraw || sem.WaitOne())
                {
                    e.Graphics.DrawImage(buffers[currentBuffer % 2], 0, 0, this.Width, this.Height);
                }
            }
            catch
            {

            }
            finally
            {
                if (!selfDraw)
                {
                    sem.Release();
                }
                Drawing = false;
            }
        }
    }
}
