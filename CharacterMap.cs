using System;
using System.Drawing;
using System.Windows.Forms;

namespace Teletext
{
    public class CharacterMap : System.Windows.Forms.PictureBox
    {

        private Int32 cBmpX = 0;
        private Int32 cBmpY = 0;

        public Byte CharWidth = 40;
        public Byte CharHeight = 25;

        public Byte CharPixelWidth = 12;
        public Byte CharPixelHeight = 20;

        private System.Windows.Forms.Timer tmrFlash = new System.Windows.Forms.Timer();
        private bool cursorState = true;
        private Bitmap bmpCursorLayer = new Bitmap(480, 500, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        

        private Bitmap textCursor;
        private Bitmap graphicsCursor;
        private Bitmap bmpCursor;

        public int l1CanvasXOrigin = 100;
        public int l1CanvasYOrigin = 60;

        private Boolean gCursor;
        public Boolean GraphicsCursor
        {
            get
            {
                return gCursor;
            }
            set
            {
                gCursor = value;
            }
        }


        private Byte cursorHeight = 1;
        public Byte CursorHeightMultiplier
        {
            get
            {
                return cursorHeight;
            }
            set
            {
                cursorHeight = value;
            }
        }

        private Byte cx = 0;
 
        public Byte cursorX
        {
            get
            {
                return cx;
            }
            set 
            {
                cx = value;
                cBmpX = (cx * CharPixelWidth) + l1CanvasXOrigin;
                if (textCursor != null) MoveCursor();
            }
        }

        private Byte cy = 0;
        public Byte cursorY 
        { 
            get
            {
                return cy;
            }
            set 
            {
                cy = value;
                cBmpY = (cy * CharPixelHeight) + l1CanvasYOrigin;
                if (textCursor != null) MoveCursor();
            }
        }

        private bool _borders = false;
        public Boolean Borders
        {
            get
            {
                return _borders;
            }
            set
            {
                _borders = value;
                 
            }
        }

        private bool cursorAllocated = false;

        public void InitCursor()
        {
            if (!cursorAllocated)
            {
                bmpCursorLayer = new Bitmap(BackgroundImage.Width, BackgroundImage.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                if (_borders)
                {
                    l1CanvasXOrigin = 100;
                    l1CanvasYOrigin = 60;
                }
                else
                {
                    l1CanvasXOrigin = 0;
                    l1CanvasYOrigin = 0;
                }

                if (bmpCursor == null)
                {
                    textCursor = CreateTextCursor();
                    graphicsCursor = CreateGraphicsCursor();
                    bmpCursor = (gCursor == false) ? textCursor : graphicsCursor;
                }
                cursorState = true;
                tmrFlash.Interval = 500;
                tmrFlash.Tick += tmrFlash_Tick;
                tmrFlash.Start();


                bmpCursorLayer.MakeTransparent(Color.Black);

                this.Invalidate();
                this.Update();
                this.Refresh();

                cursorAllocated = true;
            }
        }



        public void DeallocateCursor()
        {
            if (cursorAllocated)
            {
                tmrFlash.Stop();
                tmrFlash.Tick -= tmrFlash_Tick;
                tmrFlash.Dispose();
                clearCursorLayer();

                cursorAllocated = false;
            }
        }

        private void tmrFlash_Tick(object sender, EventArgs e)
        {
            if (cursorState)
            {
                DrawCursor();
            }
            else
            {
                clearCursorLayer();
                
            }
            cursorState = !cursorState;

            bmpCursor = (gCursor == false) ? textCursor : graphicsCursor;

            this.Invalidate();
            this.Update();
            this.Refresh();
        }

        private void clearCursorLayer()
        {
            Graphics g = Graphics.FromImage(bmpCursorLayer);
            g.Clear(Color.Black);
            bmpCursorLayer.MakeTransparent(Color.Black);
            this.Image = bmpCursorLayer;
            g.Dispose();
        }


       public void MoveCursor()
        {
            clearCursorLayer();

            cursorState = true;

            bmpCursor = (gCursor == false) ? textCursor : graphicsCursor;
            DrawCursor();
        }

        public void DrawCursor()
        {
            clearCursorLayer();
            Graphics g = Graphics.FromImage(bmpCursorLayer);

            Rectangle rect= new Rectangle(0, 0, bmpCursor.Width, bmpCursor.Height * cursorHeight);
            Bitmap tempCursor = new Bitmap(bmpCursor, bmpCursor.Width, bmpCursor.Height * cursorHeight);
            Graphics gBitmap = Graphics.FromImage(tempCursor);
            gBitmap.DrawImage(bmpCursor, bmpCursor.Width, bmpCursor.Height * cursorHeight);
            gBitmap.Dispose();

            g.DrawImage(tempCursor, cBmpX, cBmpY);
            
            g.Dispose();
            this.Invalidate();
            this.Update();
            this.Refresh();
        }

        private Bitmap CreateTextCursor()
        {
            //create cursor bitmap
            Bitmap crsr = new Bitmap(CharPixelWidth, CharPixelHeight, this.Image.PixelFormat);

            Graphics g = Graphics.FromImage(crsr);
            Brush b = new SolidBrush(Color.White);
            Pen p = new Pen(Color.White);
            Rectangle r = new Rectangle(0, 0, CharPixelWidth, CharPixelHeight);
            g.FillRectangle(b, r);
            g.Dispose();

            return crsr;
        }

        private Bitmap CreateGraphicsCursor()
        {
            //create cursor bitmap
            Bitmap crsr = new Bitmap(CharPixelWidth, CharPixelHeight, this.Image.PixelFormat);

            Graphics g = Graphics.FromImage(crsr);
            Brush b = new SolidBrush(Color.White);
            Pen p = new Pen(Color.LightSlateGray);
            Rectangle r = new Rectangle(0, 0, CharPixelWidth - 1, CharPixelHeight - 1);
            g.DrawRectangle(p, r);
            g.DrawLine(p, CharPixelWidth / 2 - 1, 0, CharPixelWidth / 2 - 1, CharPixelHeight - 1);
            g.DrawLine(p, 0, CharPixelHeight / 3, CharPixelWidth - 1, CharPixelHeight / 3);
            g.DrawLine(p, 0, CharPixelHeight / 3 * 2 + 1, CharPixelWidth - 1, CharPixelHeight / 3 * 2 + 1);

            g.Dispose();

            return crsr;
        }

        public void Key(Keys k)
        {
            switch (k)
            {
                case Keys.Down:
                    if (cursorY < CharHeight - cursorHeight)
                        cursorY += cursorHeight;
                    break;

                case Keys.Up:
                    if (cursorY - cursorHeight >= 0)
                        cursorY -= cursorHeight;
                    break;

                case Keys.Left:
                    if (cursorX == 0 && cursorY > 0)
                    {
                        cursorX = 39;
                        cursorY -= 1;
                    }
                    else
                        if (cursorX > 0)
                        {
                            cursorX -= 1;
                        }
                    break;

                case Keys.Right:
                    if (cursorX == CharWidth - 1 && cursorY < CharHeight - 1)
                    {
                        cursorX = 0;
                        cursorY += 1;
                    }
                    else
                        if (cursorX < CharWidth - 1)
                        {
                            cursorX += 1;
                        }
                    break;
            }
        }
        
    }
}
