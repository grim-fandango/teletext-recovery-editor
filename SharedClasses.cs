using System;
using System.Drawing;
using System.Drawing.Imaging;
using ExtensionMethods;
using LineTypes = Teletext.SharedResources.LineTypes;

namespace TeletextStreamCreator
{
    public class Page
    {
        public Line[] Lines = new Line[256];
        public Mode[,] modeMap = new Mode[40, 25];
        public Mode[,] modeMapL2 = new Mode[40, 25];

        public void Clear()
        {
            for (Int32 n = 0; n < 256; n++)
            {
                this.Lines[n].Clear();
            }
        }

        public Line GetPacket(Int32 packet)
        {
            Line ret = new Line();

            for (Int32 n = 0; n<256; n++)
            {
                if (this.Lines[n].Row == packet && this.Lines[n].Bytes != null)
                    ret = this.Lines[n];

            }

            return ret;
        }
    }



    public struct Line
    {
        public Int32 Magazine;
        public Int32 Row;
        public String Page;
        public String TimeCode;
        public LineTypes Type;
        public String Text;
        public Int64 StartPos;
        public Int64 EndPos;
        public byte LineNo;  //the "VBI" line as passed from the card (not actually VBIs but some internal construct)
        public Int32 Frame;
        public ControlFlags Flags;
        public Byte MRAG1, MRAG2;
        public Byte PU, PT, MU, MT, HU, HT, CA, CB;
        public Byte[] Bytes;

        public void Clear()
        {
            Magazine = 0;
            Row = 0;
            Page = "";
            TimeCode = "00:00";
            Type = "";
            Text = "";
            StartPos = 0;
            EndPos = 0;
            LineNo = 0;
            Frame = 0;
            Flags = new ControlFlags();
            MRAG1 = MRAG2 = PU = PT = MU = MT = HU = HT = CA = CB = 0;
            Bytes = new Byte[44];
        }

        public void CalcHammingCodes()
        {
            //Convert data to binary
            String magBin = Convert.ToString((Byte)this.Magazine % 8, 2).PadLeft(3, Convert.ToChar("0"));
            String rowBin = Convert.ToString((Byte)this.Row % 32, 2).PadLeft(5, Convert.ToChar("0"));


            //Char[] magBinRevC = (Char[])magBin;//.Reverse();
            //Char[] rowBinRevC = (Char[])rowBin.ToCharArray();//.Reverse();

            //reverse bits to convert to transmission order
            String magBinRev = magBin.Substring(2, 1) + magBin.Substring(1, 1) + magBin.Substring(0, 1);
            String rowBinRev = rowBin.Substring(4, 1) + rowBin.Substring(3, 1) + rowBin.Substring(2, 1) + rowBin.Substring(1, 1) + rowBin.Substring(0, 1);

            String nybble1 = magBinRev + rowBinRev.Substring(0, 1);
            String nybble2 = rowBinRev.Substring(1);

            //Work out MRAG
            this.MRAG1 = hammingEncode(nybble1);
            this.Bytes[1] = MRAG1;
            this.MRAG2 = hammingEncode(nybble2);
            this.Bytes[2] = MRAG2;

            


            //Check if line is Header or Line
            switch (this.Type)
            {
                case "Header":
                    Byte hammed;
                    String strPageUnits = Convert.ToString(Convert.ToByte(this.Page.Substring(1,1), 16), 2).PadLeft(4, Convert.ToChar("0"));
                    hammed = hammingEncode(strPageUnits.Reverse());
                    this.Bytes[3] = hammed;
                    this.PU = hammed;

                    String strPageTens = Convert.ToString(Convert.ToByte(this.Page.Substring(0, 1), 16), 2).PadLeft(4, Convert.ToChar("0"));
                    hammed = hammingEncode(strPageTens.Reverse());
                    this.Bytes[4] = hammed;
                    this.PT = hammed;

                    String strTimeCodeMinutesUnits = Convert.ToString(Convert.ToByte(this.TimeCode.Substring(4, 1), 16), 2).PadLeft(4, Convert.ToChar("0"));
                    hammed = hammingEncode(strTimeCodeMinutesUnits.Reverse());
                    this.Bytes[5] = hammed;
                    this.MU = hammed;

                    String strTimeCodeMinutesTens = Convert.ToString(Convert.ToByte(this.TimeCode.Substring(3, 1), 16), 2).PadLeft(4, Convert.ToChar("0")).Substring(1, 3);
                    strTimeCodeMinutesTens = (this.Flags.C4_Erase ? "1" : "0") + strTimeCodeMinutesTens;
                    hammed = hammingEncode(strTimeCodeMinutesTens.Reverse());
                    this.Bytes[6] = hammed;
                    this.MT = hammed; 

                    String strTimeCodeHoursUnits = Convert.ToString(Convert.ToByte(this.TimeCode.Substring(1, 1), 16), 2).PadLeft(4, Convert.ToChar("0"));
                    hammed = hammingEncode(strTimeCodeHoursUnits.Reverse());
                    this.Bytes[7] = hammed;
                    this.HU = hammed;

                    String strTimeCodeHoursTens = Convert.ToString(Convert.ToByte(this.TimeCode.Substring(0, 1), 16), 2).PadLeft(4, Convert.ToChar("0")).Substring(2, 2);
                    strTimeCodeHoursTens = (this.Flags.C6_Subtitle ? "1" : "0") + (this.Flags.C5_Newsflash ? "1" : "0") + strTimeCodeHoursTens;
                    hammed = hammingEncode(strTimeCodeHoursTens.Reverse());
                    this.Bytes[8] = hammed;
                    this.HT = hammed;
                    break;
                case "Line":
                    break;
                default:
                    break;
            }

        }

        private Byte hammingEncode(String nybble)
        {
            //Not right, do bits need mirroring?
            String outByte = "";
            Boolean p1, p2, p3, p4, d1, d2, d3, d4;

            // Extract bits from string
            d1 = (nybble.Substring(0, 1) == "1") ? true : false;
            d2 = (nybble.Substring(1, 1) == "1") ? true : false;
            d3 = (nybble.Substring(2, 1) == "1") ? true : false;
            d4 = (nybble.Substring(3, 1) == "1") ? true : false;

            // Do Hamming
            p1 = true ^ d1 ^ d3 ^ d4;
            p2 = true ^ d1 ^ d2 ^ d4;
            p3 = true ^ d1 ^ d2 ^ d3;
            p4 = true ^ p1 ^ d1 ^ p2 ^ d2 ^ p3 ^ d3 ^ d4;

            // Assemble output Byte
            outByte += (d4) ? "1" : "0";
            outByte += (p4) ? "1" : "0";
            outByte += (d3) ? "1" : "0";
            outByte += (p3) ? "1" : "0";
            outByte += (d2) ? "1" : "0";
            outByte += (p2) ? "1" : "0";
            outByte += (d1) ? "1" : "0";
            outByte += (p1) ? "1" : "0";
            //outByte.Rev();

            /*outByte = "";
            outByte += (p1) ? "1" : "0";
            outByte += (d1) ? "1" : "0";
            outByte += (p2) ? "1" : "0";
            outByte += (d2) ? "1" : "0";
            outByte += (p3) ? "1" : "0";
            outByte += (d3) ? "1" : "0";
            outByte += (p4) ? "1" : "0";
            outByte += (d4) ? "1" : "0";*/

            return Convert.ToByte(outByte, 2);
        }

    }

    public struct Header
    {
        public byte mrag1;
        public byte mrag2;
        public byte pu;
        public byte pt;
        public byte mu;
        public byte mt;
        public byte hu;
        public byte ht;
        public byte ca;
        public byte cb;
    }

    public class ControlFlags
    {
        public Boolean C4_Erase;
        public Boolean C5_Newsflash;
        public Boolean C6_Subtitle;
        public Boolean C7_SuppressHeader;
        public Boolean C8_Update;
        public Boolean C9_InterruptedSequence;
        public Boolean C10_InhibitDisplay;
        public Boolean C11_MagazineSerial;
        public Boolean C12;
        public Boolean C13;
        public Boolean C14;

        public ControlFlags()
        {
            C4_Erase = false;
            C5_Newsflash = false;
            C6_Subtitle = false;
            C7_SuppressHeader = false;
            C8_Update = false;
            C9_InterruptedSequence = false;
            C10_InhibitDisplay = false;
            C11_MagazineSerial = false;
            C12 = false;
            C13 = false;
            C14 = false;
        }
    }

    public class Frame
    {
        public string[] LineConverted = new string[32];
        public byte[,] LineOriginal = new byte[32, 44];
    }

    public struct Mode
    {
        public Color ForeColour;
        public Byte ForeColourCode;
        public Color BgndColour;
        public Byte BgndColourCode;
        public Boolean Graphics;
        public Boolean Flash;
        public Boolean Boxed;
        public Boolean DoubleHeight;
        public Boolean DoubleHeightFlag;
        public Boolean DoubleHeight2ndRow;
        public Boolean SeparatedGraphics;
        public Boolean Hold;
        public Boolean NewBgnd;
        public Color LastColour;
        public Byte LastColourCode;
        public Byte LastBit6;
        public Byte HeldGraphicsChar;
        public Boolean Conceal;
        public Byte Character;
        public Byte BgndCLUT;
        public Byte ForeCLUT;
        public Boolean DoubleWidth;
        public Boolean DoubleSize;
        public Boolean Underlined;
        public String CharacterSet;
        public Boolean FullRowColour;

        public void Default(Color? defaultBgnd = null)
        {
            ForeColour = Color.White;
            ForeColourCode = 7;
            BgndColour = defaultBgnd ?? Color.Black;
            BgndColourCode = 0;
            Graphics = false;
            Flash = false;
            Boxed = false;
            DoubleHeight = false;
            DoubleHeightFlag = false;
            SeparatedGraphics = false;
            NewBgnd = false;
            Hold = false;
            LastColour = Color.White;
            LastColourCode = 7;
            LastBit6 = 0;
            HeldGraphicsChar = 0x20;
            Conceal = false;
            BgndCLUT = 0;
            ForeCLUT = 0;
            DoubleWidth = false;
            DoubleSize = false;
            Underlined = false;
            CharacterSet = "G0";
            FullRowColour = false;

        }
    }

    class RenderedLayers
    {
        //public Bitmap L25Background = new Bitmap(480, 500, PixelFormat.Format32bppArgb);
        public Bitmap Background = new Bitmap(480, 500, PixelFormat.Format32bppArgb);
        public Bitmap Foreground = new Bitmap(480, 500, PixelFormat.Format32bppArgb);
        //public Bitmap Flash;
        //public Bitmap Conceal;
        //public Bitmap FlashConceal;


        public Boolean[] doubleHeight2ndRows = new Boolean[31];

        public void ResetDoubleHeights()
        {
            for (int n = doubleHeight2ndRows.GetLowerBound(0); n <= doubleHeight2ndRows.GetUpperBound(0); n++)
                doubleHeight2ndRows[n] = false;
        }

    }

    class HammingResults84
    {
        public byte Value;
        public String binRev;
        public String ErrorString;

        public HammingResults84 HammingCheck84(byte field)
        {
            //Convert to binary string
            string bin = Convert.ToString(field, 2);
            string binRev = "";
            string hammingStatus = "OK";

            // pad to 8 bits
            bin = bin.PadLeft(8, Convert.ToChar("0"));

            // reverse string as spec uses LSB first
            for (int i = bin.Length - 1; i > -1; i--)
            {
                binRev += bin.Substring(i, 1);
            }

            // apply Hamming error connection if enabled
            if (true)
            {
                Boolean parityA, parityB, parityC, parityD;
                String binRevA = Convert.ToString(field & Convert.ToByte("11000101", 2), 2);
                parityA = (binRevA.Replace("0", "").Length % 2 == 0);

                String binRevB = Convert.ToString(field & Convert.ToByte("01110001", 2), 2);
                parityB = (binRevB.Replace("0", "").Length % 2 == 0);

                String binRevC = Convert.ToString(field & Convert.ToByte("01011100", 2), 2);
                parityC = (binRevC.Replace("0", "").Length % 2 == 0);

                parityD = (binRev.Replace("0", "").Length % 2 == 0);

                if (parityA && parityB && parityC && !parityD)
                    hammingStatus = "Error in P4, but accept data";

                if ((!parityA || !parityB || !parityC) && parityD)
                    hammingStatus = "Double Error, reject data";

                if ((!parityA || !parityB || !parityC) && !parityD)
                {
                    hammingStatus = "Single Error: " + binRev + " ";
                    if (!parityA && !parityB && !parityC)
                    {
                        hammingStatus += ": D1";
                        string subs = "0";
                        if (binRev.Substring(1, 1) == "0")
                            subs = "1";
                        binRev = binRev.Substring(0, 1) + subs + binRev.Substring(2);
                    }
                    if (parityA && !parityB && !parityC)
                    {
                        hammingStatus += ": D2";
                        string subs = "0";

                        if (binRev.Substring(3, 1) == "0")
                            subs = "1";
                        binRev = binRev.Substring(0, 3) + subs + binRev.Substring(4);
                    }

                    if (!parityA && parityB && !parityC)
                    {
                        hammingStatus += ": D3";
                        string subs = "0";
                        if (binRev.Substring(5, 1) == "0")
                            subs = "1";
                        binRev = binRev.Substring(0, 5) + subs + binRev.Substring(6);
                    }

                    if (!parityA && !parityB && parityC)
                    {
                        hammingStatus += ": D4";
                        string subs = "0";
                        if (binRev.Substring(7, 1) == "0")
                            subs = "1";
                        binRev = binRev.Substring(0, 7) + subs;
                    }

                    hammingStatus += " " + binRev;
                }
            }

            HammingResults84 hr = new HammingResults84();
            hr.binRev = binRev;
            hr.ErrorString = hammingStatus;

            string valueBin = binRev.Substring(5, 1) + binRev.Substring(3, 1) + binRev.Substring(1, 1);

            hr.Value = Convert.ToByte(valueBin, 2);

            return hr;
        }

    }

    class HammingResults2418
    {
        public String Address, Mode, Data, Bits;
        public String Result, ErrorString;
        public Int32 RowColumn;
        public Boolean FatalError;
        public HammingResults2418 HammingCheck2418(byte byte1, byte byte2, byte byte3)
        {
            //Convert bytes to binary strings
            string arse = Convert.ToString(byte1, 2).PadLeft(8, Convert.ToChar("0"));
            string bin1 = Convert.ToString(byte1, 2).PadLeft(8, Convert.ToChar("0"));
            string bin2 = Convert.ToString(byte2, 2).PadLeft(8, Convert.ToChar("0"));
            string bin3 = Convert.ToString(byte3, 2).PadLeft(8, Convert.ToChar("0"));

            //string binRev1 = "", binRev2 = "", binRev3 = "";
            string hammingStatus = "OK";

            // reverse strings as spec uses LSB first
            bin1 = bin1.Reverse();
            bin2 = bin2.Reverse();
            bin3 = bin3.Reverse();

            // apply Hamming error detection - let's see where the errors are
            // note: Parity checks A, B, C, D and F are the same for all bytes

            //Byte 1
            Boolean parityA1, parityB1, parityC1, parityD1, parityE1, parityF1;

            String binA1 = Convert.ToString(byte1 & Convert.ToByte("10101010", 2), 2);
            parityA1 = (binA1.Replace("0", "").Length % 2 == 0);

            String binB1 = Convert.ToString(byte1 & Convert.ToByte("01100110", 2), 2);
            parityB1 = (binB1.Replace("0", "").Length % 2 == 0);

            String binC1 = Convert.ToString(byte1 & Convert.ToByte("00011110", 2), 2);
            parityC1 = (binC1.Replace("0", "").Length % 2 == 0);

            String binD1 = Convert.ToString(byte1 & Convert.ToByte("00000001", 2), 2);
            parityD1 = (binD1.Replace("0", "").Length % 2 == 0);

            String binE1 = Convert.ToString(byte1 & Convert.ToByte("00000000", 2), 2);
            parityE1 = (binE1.Replace("0", "").Length % 2 == 0);

            parityF1 = (Convert.ToString(byte1, 2).Replace("0", "").Length % 2 == 0);

            //Byte 2
            Boolean parityA2, parityB2, parityC2, parityD2, parityE2, parityF2;

            String binA2 = Convert.ToString(byte2 & Convert.ToByte("10101010", 2), 2);
            parityA2 = (binA2.Replace("0", "").Length % 2 == 0);

            String binB2 = Convert.ToString(byte2 & Convert.ToByte("01100110", 2), 2);
            parityB2 = (binB2.Replace("0", "").Length % 2 == 0);

            String binC2 = Convert.ToString(byte2 & Convert.ToByte("00011110", 2), 2);
            parityC2 = (binC2.Replace("0", "").Length % 2 == 0);

            String binD2 = Convert.ToString(byte2 & Convert.ToByte("11111110", 2), 2);
            parityD2 = (binD2.Replace("0", "").Length % 2 == 0);

            String binE2 = Convert.ToString(byte2 & Convert.ToByte("00000001", 2), 2);
            parityE2 = (binE2.Replace("0", "").Length % 2 == 0);

            parityF2 = (Convert.ToString(byte2, 2).Replace("0", "").Length % 2 == 0);

            //Byte 3
            Boolean parityA3, parityB3, parityC3, parityD3, parityE3, parityF3;

            String binA3 = Convert.ToString(byte3 & Convert.ToByte("10101010", 2), 2);
            parityA3 = (binA3.Replace("0", "").Length % 2 == 0);

            String binB3 = Convert.ToString(byte3 & Convert.ToByte("01100110", 2), 2);
            parityB3 = (binB3.Replace("0", "").Length % 2 == 0);

            String binC3 = Convert.ToString(byte3 & Convert.ToByte("00011110", 2), 2);
            parityC3 = (binC3.Replace("0", "").Length % 2 == 0);

            String binD3 = Convert.ToString(byte3 & Convert.ToByte("00000000", 2), 2);
            parityD3 = (binD3.Replace("0", "").Length % 2 == 0);

            String binE3 = Convert.ToString(byte3 & Convert.ToByte("11111110", 2), 2);
            parityE3 = (binE3.Replace("0", "").Length % 2 == 0);

            parityF3 = (Convert.ToString(byte1, 2).Replace("0", "").Length % 2 == 0);


            HammingResults2418 hr = new HammingResults2418();
            hr.FatalError = false;

            if (parityA1 && parityB1 && parityC1 && parityD1 && parityE1 &&
                parityA2 && parityB2 && parityC2 && parityD2 && parityE2 &&
                parityA3 && parityB3 && parityC3 && parityD3 && parityE3 &&
                (!parityF1 || !parityF2 || !parityF3)
                )
                hammingStatus = "Error in P6, but accept data";

            if (parityF1 && parityF2 && parityF3 &&
                (
                !parityA1 || !parityB1 || !parityC1 || !parityD1 || !parityE1 ||
                !parityA2 || !parityB2 || !parityC2 || !parityD2 || !parityE2 ||
                !parityA3 || !parityB3 || !parityC3 || !parityD3 || !parityE3)
                )
            {
                hammingStatus = "Double error, reject data bits";
                hr.FatalError = true;
            }

            if (
                (!parityF1 || !parityF2 || !parityF3) &&
                (
                !parityA1 || !parityB1 || !parityC1 || !parityD1 || !parityE1 ||
                !parityA2 || !parityB2 || !parityC2 || !parityD2 || !parityE2 ||
                !parityA3 || !parityB3 || !parityC3 || !parityD3 || !parityE3)
                )
            {
                hammingStatus = "Single Error, ";

                //Get results of tests across all three bytes
                Boolean resultA = ((binA1 + binA2 + binA3).Replace("0", "").Length % 2 == 0);
                Boolean resultB = ((binB1 + binB2 + binB3).Replace("0", "").Length % 2 == 0);
                Boolean resultC = ((binC1 + binC2 + binC3).Replace("0", "").Length % 2 == 0);
                Boolean resultD = ((binD1 + binD2 + binD3).Replace("0", "").Length % 2 == 0);
                Boolean resultE = ((binE1 + binE2 + binE3).Replace("0", "").Length % 2 == 0);

                Int32 errorPosition = 2 ^ 4 * (resultE == true ? 0 : 1) + 2 ^ 3 * (resultD == true ? 0 : 1) + 2 ^ 2 * (resultC == true ? 0 : 1) + 2 ^ 1 * (resultB == true ? 0 : 1) + 2 ^ 0 * (resultA == true ? 0 : 1);
                hammingStatus += "Error Position: " + errorPosition.ToString();
                if (errorPosition == 3 || errorPosition == 5 || errorPosition == 6 || errorPosition == 7 || (errorPosition >= 9 && errorPosition <= 15) || (errorPosition >= 17 && errorPosition <= 23))
                {
                    hammingStatus += " - this is in a data bit.";
                }

            }



            //Convert back to MSB first.  EBU TTX document has them this way round in section 12.3.1.
            String binRev1 = (Convert.ToString(byte1, 2).PadLeft(8, Convert.ToChar("0"))).Reverse();
            String binRev2 = (Convert.ToString(byte2, 2).PadLeft(8, Convert.ToChar("0"))).Reverse();
            String binRev3 = (Convert.ToString(byte3, 2).PadLeft(8, Convert.ToChar("0"))).Reverse();

            // Or, in reverse:
            /*String binRev1 = Reverse(Convert.ToString(byte1, 2).PadLeft(8, Convert.ToChar("0")));
            String binRev2 = Reverse(Convert.ToString(byte2, 2).PadLeft(8, Convert.ToChar("0")));
            String binRev3 = Reverse(Convert.ToString(byte3, 2).PadLeft(8, Convert.ToChar("0")));*/



            String binDataBitsRev = binRev1.Substring(2, 1) + binRev1.Substring(4, 3);
            binDataBitsRev += binRev2.Substring(0, 7);
            binDataBitsRev += binRev3.Substring(0, 7);

            hr.Bits = binDataBitsRev;

            hr.Address = binDataBitsRev.Substring(0, 6).Reverse();
            hr.Mode = binDataBitsRev.Substring(6, 5).Reverse();
            hr.Data = binDataBitsRev.Substring(11, 7).Reverse();


            Int32 decAddr = Convert.ToInt32(hr.Address, 2);
            String addrType = decAddr < 40 ? "Col: " + decAddr.ToString() : "Row: " + (decAddr - 40).ToString();
            if (decAddr == 40)
                addrType = "Row 24";

            hr.RowColumn = decAddr < 40 ? decAddr : (decAddr - 40);
            if (decAddr == 40)
                hr.RowColumn = 24;


            hr.Result = "Addr: " + hr.Address + "(" + decAddr + ") " + addrType + ", Mode: " + hr.Mode + ", Data: " + hr.Data;
            hr.ErrorString = hammingStatus;
            return hr;
        }
        public HammingResults2418 HammingCheck2418a(byte byte1, byte byte2, byte byte3)
        {
            //Adapted from http://pdc.ro.nu/hamming.html

            //Convert bytes to binary strings
            //Convert bytes to binary strings
            string bin1 = Convert.ToString(byte1, 2).PadLeft(8, Convert.ToChar("0"));
            string bin2 = Convert.ToString(byte2, 2).PadLeft(8, Convert.ToChar("0"));
            string bin3 = Convert.ToString(byte3, 2).PadLeft(8, Convert.ToChar("0"));

            string hammingStatus = "OK";

            Boolean[] h = new Boolean[24];
            //Load Byte 1 into array
            for (Int32 n = 0; n < 8; n++)
            {
                h[n] = bin1.Substring(7 - n, 1) == "1" ? true : false;
            }
            //Load Byte 2 into array
            for (Int32 n = 0; n < 8; n++)
            {
                h[n + 8] = bin2.Substring(7 - n, 1) == "1" ? true : false;
            }
            //Load Byte 3 into array
            for (Int32 n = 0; n < 8; n++)
            {
                h[n + 16] = bin3.Substring(7 - n, 1) == "1" ? true : false;
            }

            //Parity Tests
            Boolean p = h[23];
            for (Int32 n = 22; n > 0; n--)
                p = p ^ h[n];

            Boolean c0 = h[0] ^ h[2] ^ h[4] ^ h[6] ^ h[8] ^ h[10] ^ h[12] ^ h[14] ^ h[16] ^ h[18] ^ h[20] ^ h[22];
            Boolean c1 = h[1] ^ h[2] ^ h[5] ^ h[6] ^ h[9] ^ h[10] ^ h[13] ^ h[14] ^ h[17] ^ h[18] ^ h[21] ^ h[22];
            Boolean c2 = h[3] ^ h[4] ^ h[5] ^ h[6] ^ h[11] ^ h[12] ^ h[13] ^ h[14] ^ h[19] ^ h[20] ^ h[21] ^ h[22];
            Boolean c3 = h[7] ^ h[8] ^ h[9] ^ h[10] ^ h[11] ^ h[12] ^ h[13] ^ h[14];
            Boolean c4 = h[15] ^ h[16] ^ h[17] ^ h[18] ^ h[19] ^ h[20] ^ h[21] ^ h[22];

            if (p && c0 && c1 && c2 && c3 && c4)
            {
                hammingStatus = "OK";
            }

            if (!p && (!c0 || !c1 || !c2 || !c3 || !c4))
            {
                hammingStatus = "Damaged beyond repair.";
            }

            if (!p && c0 && c1 && c2 && c3 && c4)
            {
                Int32 bitInError = 0;
                if (!c0)
                    bitInError += 1;
                if (!c1)
                    bitInError += 2;
                if (!c2)
                    bitInError += 4;
                if (!c3)
                    bitInError += 8;
                if (!c4)
                    bitInError += 16;

                hammingStatus = "Single bit error in bit " + bitInError.ToString() + ", corrected.  ";
                h[bitInError] = !h[bitInError];
            }

            String dataBits = "";
            dataBits += h[22] == true ? "1" : "0";
            dataBits += h[21] == true ? "1" : "0";
            dataBits += h[20] == true ? "1" : "0";
            dataBits += h[19] == true ? "1" : "0";
            dataBits += h[18] == true ? "1" : "0";
            dataBits += h[17] == true ? "1" : "0";
            dataBits += h[16] == true ? "1" : "0";
            dataBits += h[14] == true ? "1" : "0";
            dataBits += h[13] == true ? "1" : "0";
            dataBits += h[12] == true ? "1" : "0";
            dataBits += h[11] == true ? "1" : "0";
            dataBits += h[10] == true ? "1" : "0";
            dataBits += h[9] == true ? "1" : "0";
            dataBits += h[8] == true ? "1" : "0";
            dataBits += h[6] == true ? "1" : "0";
            dataBits += h[5] == true ? "1" : "0";
            dataBits += h[4] == true ? "1" : "0";
            dataBits += h[2] == true ? "1" : "0";


            HammingResults2418 hr = new HammingResults2418();
            hr.Bits = dataBits;

            hr.Data = dataBits.Substring(0, 7);

            hr.Mode = dataBits.Substring(7, 5);

            hr.Address = dataBits.Substring(12, 6);

            Int32 decAddr = Convert.ToInt32(hr.Address, 2);
            String addrType = decAddr < 40 ? "Col: " + decAddr.ToString() : "Row: " + (decAddr - 40).ToString();
            if (decAddr == 40)
                addrType = "Row 24";

            hr.RowColumn = decAddr < 40 ? decAddr : (decAddr - 40);
            if (decAddr == 40)
                hr.RowColumn = 24;

            hr.Result = "Addr: " + hr.Address + "(" + decAddr + ") " + addrType + ", Mode: " + hr.Mode + ", Data: " + hr.Data;
            hr.ErrorString = hammingStatus;

            System.Diagnostics.Debug.Print("Hamming Status: " + hammingStatus);

            return hr;
        }

    }


}
