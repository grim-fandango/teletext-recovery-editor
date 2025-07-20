using System;
//using System.Windows.Media.Imaging;
//using System.Windows.Controls;
//using System.Windows.Media;
//using System.Globalization;
using System.Windows;
using System.Windows.Forms;

namespace ExtensionMethods
{
    public static class MyExtensions
    {
        public static Boolean isHex(this char digit)
        {
            System.Diagnostics.Debug.WriteLine(Convert.ToInt32(digit));
            if (char.IsNumber(digit) || (Convert.ToInt32(digit) >= 65 && Convert.ToInt32(digit) <= 70))
                return true;
            else
                return false;
        }

        public static String toHex(this byte Integer, Int32 Length)
        {
            String outStr = Convert.ToString(Integer, 16);
            //String zeroes = "";

            //for (Int32 i = 0; i < Length; i++)
            //{
            //    zeroes += "0";
            //}

            //outStr = zeroes.Substring(0, Length - outStr.Length) + outStr;
            outStr = outStr.ZeroPadding(Length);
            return outStr;
        }

        public static String toHex(this Int32 Integer, Int32 Length)
        {
            String outStr = Convert.ToString(Integer, 16);
            //String zeroes = "";

            //for (Int32 i = 0; i < Length; i++)
            //{
            //    zeroes += "0";
            //}

            //outStr = zeroes.Substring(0, Length - outStr.Length) + outStr;
            outStr = outStr.ZeroPadding(2);
            return outStr;
        }

        public static String Reverse(this string s)
        {
            String outStr = "";

            for (Int32 n = s.Length - 1; n>-1; n--)
                outStr += s.Substring(n, 1);

            return outStr;
            
        }

        public static String ZeroPadding(this String inVal, Int32 Length)
        {
            string inString = inVal.ToString();
            String zeroes = "";
            if (inVal.Length < Length)
            {
                for (Int32 i = 0; i < Length; i++)
                {
                    zeroes += "0";
                }

                return zeroes.Substring(0, Length - inString.Length) + inString;
            }
            else
                return inVal;
        }

        public static Int32 ThreeDigitHexNoPosition(this Byte[] bytes)
        {
            Int32 position = 0;
            for (int start = 0; start < bytes.Length - 5; start++)
            {
                if ((bytes[start] & 127) < 0x21 && (bytes[start + 4] & 127) < 0x21)
                {
                    char hChar = Convert.ToChar(bytes[start + 1] & 127);
                    char tChar = Convert.ToChar(bytes[start + 2] & 127);
                    char uChar = Convert.ToChar(bytes[start + 3] & 127);

                    if (char.IsNumber(hChar) && tChar.isHex() && uChar.isHex())
                        position = start + 1;
                }
            }

            if (position == 0)
                System.Diagnostics.Debug.WriteLine("No page number found");

            return position;
        }

        /*public static WriteableBitmap DrawChar(this WriteableBitmap wb, String str, Int32 x, Int32 y)
        {
            Image renderImage = new Image();
            FontFamily ff = new FontFamily("Bedstead");
            FormattedText text = new FormattedText(str,
                    new CultureInfo("en-us"),
                    FlowDirection.LeftToRight,
                    new Typeface(ff, FontStyles.Normal, FontWeights.Normal, new FontStretch()),
                    14,
                    Brushes.White);

            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            drawingContext.DrawText(text, new Point(2, 2));
            drawingContext.Close();

            RenderTargetBitmap bmp = new RenderTargetBitmap(180, 180, 120, 96, PixelFormats.Pbgra32);
            bmp.Render(drawingVisual);
            renderImage.Source = bmp;

            return wb;
        }*/

        /*
        -- Nice,but seems to return a character array for some reason
        public static String Reverse(this String inStr)
        {
            Char[] strCharArray = inStr.ToCharArray();
            Array.Reverse(strCharArray, 0, strCharArray.Length);
            String returnStr = strCharArray.ToString();
            return returnStr;
        }*/

    }
}
