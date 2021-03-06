using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace Digger
{
    public class DiggerWindow : Form
    {
        private readonly Dictionary<string, Bitmap> bitmaps = new Dictionary<string, Bitmap>();
        private readonly GameState gameState;
        private readonly HashSet<Keys> pressedKeys = new HashSet<Keys>();
        private int tickCount;


        public DiggerWindow(DirectoryInfo imagesDirectory = null)
        {
            gameState = new GameState();
            ClientSize = new Size(
                GameState.ElementSize * Game.MapWidth,
                GameState.ElementSize * Game.MapHeight + GameState.ElementSize);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            if (imagesDirectory == null)
                imagesDirectory = new DirectoryInfo("Images");
            foreach (var e in imagesDirectory.GetFiles("*.png"))
                bitmaps[e.Name] = (Bitmap) Image.FromFile(e.FullName);
            var timer = new Timer();
            timer.Interval = 15;
            timer.Tick += TimerTick;
            timer.Start();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Text = "Digger";
            DoubleBuffered = true;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            pressedKeys.Add(e.KeyCode);
            Game.KeyPressed = e.KeyCode;
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            pressedKeys.Remove(e.KeyCode);
            Game.KeyPressed = pressedKeys.Any() ? pressedKeys.Min() : Keys.None;
        }

        private Point GetPlayerPosition()
        {
            foreach (var obj in gameState.Animations)
            {
                if (obj.Creature is Player)
                {
                    return obj.Location;
                }   
            }

            return new Point();
        }
    
        protected override void OnPaint(PaintEventArgs e)
        {
            
            var headPos = GetPlayerPosition();
            var offsetX = -headPos.X + Size.Width/2;
            var offsetY = -headPos.Y + Size.Height/2;
            //??????????
            e.Graphics.FillRectangle(
                Brushes.Black, 0, 0, GameState.ElementSize * Game.MapWidth+GameState.ElementSize,
                GameState.ElementSize * Game.MapHeight+GameState.ElementSize);
            
            if(headPos.X < Size.Width/2 && headPos.Y < Size.Height/2)
                e.Graphics.TranslateTransform(0, 0);
            else if(headPos.X < Size.Width/2)
                e.Graphics.TranslateTransform(0, offsetY);
            else if(headPos.Y < Size.Height/2)
                e.Graphics.TranslateTransform(offsetX, 0);
            else 
                e.Graphics.TranslateTransform(offsetX, offsetY);

            foreach (var obj in gameState.Animations)
            {
                 if(obj.Creature is Player)
                {
                    var photo = (Bitmap)bitmaps[obj.Creature.GetImageFileName()].Clone();
                    if(Player.dx < 0)  
                        photo.RotateFlip(RotateFlipType.Rotate180FlipY);
                    e.Graphics.DrawImage(photo, obj.Location);
                }else if (obj.Creature is SpecialMonster)
                 {
                     var photo = (Bitmap)bitmaps[obj.Creature.GetImageFileName()].Clone();
                     var monster = obj.Creature as SpecialMonster;
                     if(monster.dx > 0)  
                         photo.RotateFlip(RotateFlipType.Rotate180FlipY);
                     e.Graphics.DrawImage(photo, obj.Location);
                 }
                else
                {
                    e.Graphics.DrawImage(bitmaps[obj.Creature.GetImageFileName()], obj.Location);
                }
            }
            e.Graphics.DrawRectangle(Pens.Gray,0, 0, GameState.ElementSize * Game.MapWidth,
                GameState.ElementSize * Game.MapHeight);
            e.Graphics.ResetTransform();
            e.Graphics.DrawString(Game.Scores.ToString(), new Font("Arial", 16), Brushes.Green, 0, 0);
        }

        private void TimerTick(object sender, EventArgs args)
        {
            if (tickCount == 0) gameState.BeginAct();
            foreach (var e in gameState.Animations)
                e.Location = new Point(e.Location.X + 4 * e.Command.DeltaX, e.Location.Y + 4 * e.Command.DeltaY);
            if (tickCount == 7)
                gameState.EndAct();
            tickCount++;
            if (tickCount == 8) tickCount = 0;
            Invalidate();
        }
    }
}
