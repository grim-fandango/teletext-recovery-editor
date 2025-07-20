using System.Text;
using System.Collections;
using TeletextSharedResources;

namespace TeletextStreamCreator
{
    public class Service
    {
        private BinaryReader reader = null;
        private byte b;
        private Int32 offset = 0;
        public Int32 Revolutions = 0;

        private Byte[] byteReader;

        private long localPos = 0;
        public long Position
        {
            get
            {
                return localPos;
            }
            set
            {
                //localPos = value; 
                //reader.BaseStream.Seek(0, SeekOrigin.Begin);
                this.Reset();
                //reader.ReadBytes((Int32)value);
            }
        }

        private Byte rowLength = 43;
        private Int32 frameSize = 860;

        public Byte RowLength
        {
            get
            {
                return rowLength;
            }
            set
            {
                rowLength = value;
                frameSize = rowLength * 20;
            }
        }


        public Int64 Length = 0;
        public Int32 FrameNo = 0;
        public byte FrameLine = 21;
        public Frame currentFrame = new Frame();

        private long intCheckHorizon = 5242880;
        public long CheckHorizon
        {
            get
            {
                return intCheckHorizon;
            }

            set
            {
                intCheckHorizon = value;
            }
        }

        private Int32 _linesPerFrame;
        public Int32 LinesPerFrame
        {
            get
            {
                return _linesPerFrame;
            }
        }

        private Boolean _useHamming = true;
        public Boolean UseHamming
        {
            get
            {
                return _useHamming;
            }
            set
            {
                _useHamming = value;
            }
        }


        public String FileType;
        /*{
            get
            {
                return FileType;
            }
            set
            {
                FileType = value;
            }
        }*/

        public string OpenService(string filename)
        {
            string returnvalue = "";

            try
            {
            reader = new BinaryReader(File.Open(filename, FileMode.Open));
            }
            catch
            {
                returnvalue = "Error opening file.";
            };

            this.Length = reader.BaseStream.Length;

            if (Length > 0)
            {
                if (Convert.ToDouble(this.Length) / 20 != this.Length)
                {
                    this.Length = ((this.Length / frameSize) + 1) * frameSize;
                }

                byteReader = new Byte[Length];
                byte[] byteReaderTemp;

                byteReaderTemp = reader.ReadBytes(Convert.ToInt32(reader.BaseStream.Length));
                Array.Copy(byteReaderTemp, 0, byteReader, 0, byteReaderTemp.LongLength);

                Reset();

                _linesPerFrame = NumLinesPerFrame();
                Revolutions = 0;
            }
            else
            {
                returnvalue = "File is blank.";
            }

            reader.Close();
            reader = null;


            if (rowLength == 40)
            {
                // Convert to 42 byte packet

                // Byte array to copy into
                Byte[] byteReaderTemp = new Byte[Length/40*42];

                //MRAGless, so add one to each line 
                Byte row;
                if (FileType == "TTX")
                {
                    for (Int64 line = 0; line < this.Length / 40; line++)
                    {
                        Buffer.BlockCopy(byteReader, (Int32)(line * 40), byteReaderTemp, (Int32)(line * 42) + 2, 40);
                        String text = System.Text.Encoding.ASCII.GetString(byteReader, (Int32)line * 40, 40);

                        row = (Byte)(line % 24);
                        Line tempLine = CreatePacket(1, "00", "00:00", row, row, text);
                        byteReaderTemp[line * 42] = tempLine.MRAG1;
                        byteReaderTemp[(line * 42) + 1] = tempLine.MRAG2;
                    }
                }

                if (FileType == "PRG")
                {
                    for (Int64 line = 0; line < 24; line++)
                    {
                        Buffer.BlockCopy(byteReader, (Int32)(line * 40), byteReaderTemp, (Int32)(line * 42), 40);
                        String text = System.Text.Encoding.ASCII.GetString(byteReader, (Int32)line * 40, 40);

                        row = (Byte)(line);
                        Line tempLine = CreatePacket(1, "00", "00:00", row, row, text);
                        byteReaderTemp[line * 42] = tempLine.MRAG1;
                        byteReaderTemp[(line * 42) + 1] = tempLine.MRAG2;
                    }
                }

                if (FileType == "VTP")
                {
                    Int32 headerLength = 0x76;
                    Int32 counter = headerLength;
                    Int32 line = 0;

                    do
                    {
                        // Read 24x40 Lines
                        for (row = 0; row < 24; row++)
                        {
                            if (counter < this.Length -42)
                            {
                                Buffer.BlockCopy(byteReader, counter, byteReaderTemp, (line * 42) + 2, 40);
                                String text = System.Text.Encoding.ASCII.GetString(byteReaderTemp, (Int32)line * 40, 40);

                                Line tempLine = CreatePacket(1, "00", "00:00", row, row, text);
                                byteReaderTemp[line * 42] = tempLine.MRAG1;
                                byteReaderTemp[(line * 42) + 1] = tempLine.MRAG2;

                                counter += 40;
                                line++;
                            }
                        }
                        counter += 10;
                    } while (counter < this.Length);

                }

                if (FileType == "EP1")
                {
                    for (Int64 line = 0; line < (this.Length - 6) / 40; line++)
                    {
                        Buffer.BlockCopy(byteReader, (Int32)(line * 40) + 6, byteReaderTemp, (Int32)(line * 42) + 2, 40);
                        String text = System.Text.Encoding.ASCII.GetString(byteReader, (Int32)line * 40, 40);

                        row = (Byte)(line % 24);
                        Line tempLine = CreatePacket(1, "00", "00:00", row, row, text);
                        byteReaderTemp[line * 42] = tempLine.MRAG1;
                        byteReaderTemp[(line * 42) + 1] = tempLine.MRAG2;
                    }
                }

                if (FileType == "TTI")
                {
                    Int32 n = 0;
                    foreach (Byte b in byteReader)
                    {
                        if (b > 0x7f && b != 0x8A && b != 0x8D)
                            byteReader[n] = (Byte)((Int32)b & 0x7f);
                        n++;
                    }
                    
                    Char[] chrBytereader = new Char[byteReader.Length];
                    byteReader.CopyTo(chrBytereader, 0);
                    String ttiFile = new String(chrBytereader);

                    ttiFile.Replace(Convert.ToString((Char)13), "");
                    String[] ttiLines = ttiFile.Split((Char)10);

                    Byte magazine = 1;
                    String page = "00";
                    String subpage = "00";
                    n = 0;
                    foreach(String s in ttiLines)
                    {
                        s.Replace((Char)0x8A, (Char)0x0A);
                        s.Replace((Char)0x8D, (Char)0x0D);

                        String ttiLineType = s.Substring(0, 2);

                        String headerBits = "1100000000000000";

                        switch (ttiLineType)
                        {
                            case "PN":
                                String value = s.Substring(s.IndexOf(",") + 1);
                                magazine = Convert.ToByte(value.Substring(0, 1));
                                page = value.Substring(1, 2);
                                subpage = "00:" + value.Substring(3, 2);

                                //String headerText = "";
                                //Line tempHeader = CreatePacket(1, "00", 0, 0, headerText);
                                //byteReaderTemp[n * 42] = tempHeader.MRAG1;
                                //byteReaderTemp[(n * 42) + 1] = tempHeader.MRAG2;
                                //for (Int32 i = 0; i < 40; i++)
                                //{
                                //    if (i < headerText.Length)
                                //    {
                                //        Byte b = Convert.ToByte(Convert.ToChar(headerText.Substring(i, 1)));
                                //        if (b > 0x80)
                                //        {
                                //            b = Convert.ToByte((Int32)b - 0x80);
                                //        }
                                //        byteReaderTemp[n * 42 + i + 2] = b;
                                //    }
                                //    else
                                //        byteReaderTemp[n * 42 + i + 2] = 0x20;
                                //}
                                //n++;
                                break;
                            case "OL":
                                Int32 firstCommaPos = s.IndexOf(",", 0);
                                Int32 secondCommaPos = s.IndexOf(",", firstCommaPos + 1);

                                String rowValue = s.Substring(firstCommaPos + 1, secondCommaPos - firstCommaPos - 1);
                                String rowText = s.Substring(secondCommaPos + 1);


                                Line tempLine = CreatePacket(magazine, page, subpage, Convert.ToByte(rowValue), Convert.ToByte(rowValue), rowText, headerBits);
                                byteReaderTemp[n * 42] = tempLine.MRAG1;
                                byteReaderTemp[(n * 42) + 1] = tempLine.MRAG2;
                                if (tempLine.Type == LineTypes.Header)
                                {
                                    byteReaderTemp[(n * 42) + 2] = tempLine.PU;
                                    byteReaderTemp[(n * 42) + 3] = tempLine.PT;

                                    byteReaderTemp[(n * 42) + 4] = tempLine.MU;
                                    byteReaderTemp[(n * 42) + 5] = tempLine.MT;

                                    byteReaderTemp[(n * 42) + 6] = tempLine.HU;
                                    byteReaderTemp[(n * 42) + 7] = tempLine.HT;
                                }

                                switch (rowValue)
                                {
                                    case "26":
                                        Byte[] bytes = Encoding.ASCII.GetBytes(rowText);
                                        Byte DC = Convert.ToByte(bytes[0] & 0x0F);
                                        for (Int32 t = 0; t <= 12; t++)
                                        {
                                            
                                            Int32 triplet;
                                            triplet = bytes[t * 3 + 1] & 0x3f;
                                            triplet |= (bytes[(t * 3) + 2] & 0x3F) << 6;
                                            triplet |= (bytes[(t * 3) + 3] & 0x3F) << 12;

                                            //triplet = bytes[t * 3 + 3] & 0x3F;
                                            //triplet |= ((bytes[t * 3 + 4]) & 0x3F) << 6;
                                            //triplet |= ((bytes[t * 3 + 5]) & 0x3F) << 12;

                                            System.Diagnostics.Debug.WriteLine("Original TTI Text: " + rowText);
                                            System.Diagnostics.Debug.WriteLine("Triplet content  : " + Convert.ToString(triplet, 2).PadLeft(18, Convert.ToChar("0")));


                                        }
                                        break;

                                    default:
                                        Int32 sourceCharPosn = (tempLine.Type == LineTypes.Header ? 8 : 0);
                                        Int32 destCharPosn = (tempLine.Type == LineTypes.Header ? 10 : 2);
                                        do
                                        {

                                            Byte b = Convert.ToByte(Convert.ToChar(rowText.Substring(sourceCharPosn, 1)));
                                            if (b > 0x80)
                                            {
                                                b = Convert.ToByte((Int32)b - 0x80);
                                            }

                                            if (b == 27)
                                            {
                                                sourceCharPosn++;
                                                b = Convert.ToByte(Convert.ToChar(rowText.Substring(sourceCharPosn, 1)));
                                                b = Convert.ToByte((Int32)b - 0x40);
                                            }
                                            // else
                                            byteReaderTemp[n * 42 + destCharPosn] = b;

                                            sourceCharPosn++;
                                            destCharPosn++;

                                        } while (sourceCharPosn < rowText.Length);

                                        n++;

                                        break;
                                }
                                break;
                            case "PS":
                                firstCommaPos = s.IndexOf(",", 0);

                                headerBits = Convert.ToString(Convert.ToInt32(s.Substring(firstCommaPos + 1), 16), 2);

                                break;
                        }
                        
                    }


                }

                byteReader = byteReaderTemp;
                this.Length = byteReaderTemp.Length;

                Reset();

                _linesPerFrame = NumLinesPerFrame();
                Revolutions = 0;
                RowLength = 42;

                //For debugging
                BinaryWriter writer = new BinaryWriter(new FileStream("conv.bin", FileMode.Create));
                writer.Write(byteReader);
                writer.Close();
                writer = null;
            }

            return returnvalue;

        }

        private Int32 NumLinesPerFrame()
        {
            // Get line to force the population of currentFrame object
            Line dummy = GetNextLine();
            Int32 linesPerFrame = 0;

            // Count the rows that don't start with 00 00
            for (Int32 n = 1; n < 21; n++)
            {
                Byte b0 = currentFrame.LineOriginal[n, 0];
                Byte b1 = currentFrame.LineOriginal[n, 1];

                if (b0 != 0 || b1 != 0)
                    linesPerFrame++;
            }

            // Reset Service to beginning
            this.Reset();

            return linesPerFrame;
        }

        public Line GetNextLine()
        {
            return internalGetNextLine(-1);
        }

        public Line GetNextLine(String longPage)
        {
            Int32 magazine = Convert.ToInt32(longPage.Substring(0, 1));
            String page = longPage.Substring(1, 2);
            return internalGetNextLine(magazine);
        }

        private Line internalGetNextLine(int magazine)
        {
            Boolean escape = false;
            Line workingLine = new Line();

            while (!escape)
            {
                Header head = new Header();

                workingLine.Clear();

                // Always return the first line if MRAG not filtered
                if (magazine == -1)
                    escape = true;

                //At end of file?  Reset pointer and let's go round again!
                //if (reader.BaseStream.Position >= reader.BaseStream.Length && FrameLine >= 20)
                if (localPos >= byteReader.LongLength && FrameLine >= 20)
                {
                    Revolutions++;
                    Reset();
                }

                FrameLine++;


                //At end of frame?
                Boolean keepLooking = true;
                if (FrameLine > 20 && keepLooking)
                {
                    //Read next Frame
                    for (byte line = 1; line <= 20 & keepLooking; line++)
                    {
                        currentFrame.LineConverted[line] = "";
                        for (byte byt = 1; byt <= rowLength & keepLooking; byt++)
                        {
                            //b = reader.ReadByte();
                            try
                            {
                                b = byteReader[localPos];
                                localPos++;
                                currentFrame.LineOriginal[line, byt] = b;
                                currentFrame.LineConverted[line] += DecodeChar(b);
                            }
                            catch 
                            {
                                //System.Diagnostics.Debug.Print("byteReader.length: " + byteReader.Length.ToString() + " localPos: " + localPos.ToString() + " byt: " + byt.ToString() + " line:" + line.ToString());
                                keepLooking = false;
                            }
                        }
                    }
                    FrameLine = 1;
                }

                //Set line and frame properties
                workingLine.LineNo = FrameLine;
                workingLine.Frame = Convert.ToInt32(localPos / frameSize);

                //Mark the start of the line in the Service file
                workingLine.StartPos = localPos - frameSize + ((FrameLine - 1) * rowLength);
                workingLine.EndPos = localPos - frameSize + (FrameLine * rowLength) - 1;

                if (workingLine.StartPos < 0)
                    workingLine.StartPos += frameSize;

                if (workingLine.EndPos < 0)
                    workingLine.EndPos += frameSize;

                //Get MRAG


                workingLine.MRAG1 = currentFrame.LineOriginal[FrameLine, 1];
                workingLine.MRAG2 = currentFrame.LineOriginal[FrameLine, 2];
                workingLine.PU = currentFrame.LineOriginal[FrameLine, 3];
                workingLine.PT = currentFrame.LineOriginal[FrameLine, 4];
                workingLine.MU = currentFrame.LineOriginal[FrameLine, 5];
                workingLine.MT = currentFrame.LineOriginal[FrameLine, 6];
                workingLine.HU = currentFrame.LineOriginal[FrameLine, 7];
                workingLine.HT = currentFrame.LineOriginal[FrameLine, 8];
                workingLine.CA = currentFrame.LineOriginal[FrameLine, 9];
                workingLine.CB = currentFrame.LineOriginal[FrameLine, 10];
                head.mrag1 = workingLine.MRAG1;
                head.mrag2 = workingLine.MRAG2;


                workingLine.Magazine = GetMagazine(head.mrag1);
                workingLine.Row = GetRow(head.mrag1, head.mrag2);            

                //create array of the packet bytes
                Byte[] byteArray = new Byte[44];
                for (int i = 0; i < 44; i++ )
                {
                    byteArray[i] = currentFrame.LineOriginal[FrameLine, i];
                }
                workingLine.Bytes = byteArray;

                /*
                _useHamming = false;
                Int32 mnh = GetMagazine(head.mrag1);
                _useHamming = true;
                Int32 mh = GetMagazine(head.mrag1);
                if (mh != mnh)
                    System.Diagnostics.Debug.WriteLine(mh + "  " + mnh);
                 * */

                // Check to see if the Magazine matches the filter
                if (!escape && magazine == workingLine.Magazine)
                    escape = true;

                //If row is out of bounds get another row
                if (workingLine.Row > 31)
                    escape = false;

                // If row is a non-data row get the next row
                if (currentFrame.LineOriginal[FrameLine, 1] == 0 && currentFrame.LineOriginal[FrameLine, 2] == 0) 
                    escape = false;

                if (escape)
                {
                    Byte lineLength = 40;

                    switch (workingLine.Row)
                    {
                        case 24:
                            //Fastext line
                            workingLine.Type = LineTypes.FastextDisplay;
                            break;

                        case 0:
                            //Header line
                            workingLine.Type = LineTypes.Header;
                            // Read rest of header data
                            head.pu = currentFrame.LineOriginal[FrameLine, 3];
                            head.pt = currentFrame.LineOriginal[FrameLine, 4];
                            head.mu = currentFrame.LineOriginal[FrameLine, 5];
                            head.mt = currentFrame.LineOriginal[FrameLine, 6];
                            head.hu = currentFrame.LineOriginal[FrameLine, 7];
                            head.ht = currentFrame.LineOriginal[FrameLine, 8];
                            head.ca = currentFrame.LineOriginal[FrameLine, 9];
                            head.cb = currentFrame.LineOriginal[FrameLine, 10];
                            offset += 8;

                            workingLine.Page = GetPageNumber(head.pu, head.pt);
                            workingLine.TimeCode = GetTimeCode(head.mu, head.mt, head.hu, head.ht);
                            workingLine.Flags.C4_Erase = GetBit(8, head.mt);
                            workingLine.Flags.C5_Newsflash = GetBit(6, head.ht);
                            workingLine.Flags.C6_Subtitle = GetBit(8, head.ht);
                            workingLine.Flags.C7_SuppressHeader = GetBit(2, head.ca);
                            workingLine.Flags.C8_Update = GetBit(4, head.ca);
                            workingLine.Flags.C9_InterruptedSequence = GetBit(6, head.ca);
                            workingLine.Flags.C10_InhibitDisplay = GetBit(8, head.ca);
                            workingLine.Flags.C11_MagazineSerial = GetBit(2, head.cb);
                            workingLine.Flags.C12 = GetBit(4, head.cb);
                            workingLine.Flags.C13 = GetBit(6, head.cb);
                            workingLine.Flags.C14 = GetBit(8, head.cb);

                            break;

                        default:
                            // If not header or fastext, must be row
                            // That said, some rows are >24, what are these?
                            workingLine.Type = LineTypes.Line;
                            break;
                    }

                    try
                    {
                        if (workingLine.Type == LineTypes.Header)
                        {
                            workingLine.Text = currentFrame.LineConverted[FrameLine].Substring(10, lineLength - 8);
                        }
                        else
                        {
                            workingLine.Text = currentFrame.LineConverted[FrameLine].Substring(2, lineLength);
                        }
                    }
                    catch { }
                }

                //localPos = reader.BaseStream.Position;
            }
            return workingLine;
        }

        public string ExtractValue(byte field)
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
            if (_useHamming)
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
                    hammingStatus = "Error in P4";

                if ((!parityA || !parityB || !parityC) && parityD)
                    hammingStatus = "Double Error";

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

            return binRev;
        }

        private static char DecodeChar(byte b)
        {
            // Returns a decoded char for a received byte
            char c;

            c = Convert.ToChar(b);

            Int32 bAnded = (Convert.ToInt32(b) & 127);

            char converted = Convert.ToChar(bAnded);

            return converted;
        }

        private Int32 GetMagazine(byte mrag1)
        {
            string binRev = ExtractValue(mrag1);

            /*_useHamming = false;
            String mnh = ExtractValue(mrag1);
            _useHamming = true;
            String mh = ExtractValue(mrag1);
            if (mh != mnh)
                System.Diagnostics.Debug.WriteLine(mh + "  " + mnh);
            */
            
            //Construct return binary as MSB first
            string magBin = binRev.Substring(5, 1) + binRev.Substring(3, 1) + binRev.Substring(1, 1);

            byte magazine = Convert.ToByte(magBin, 2);

            if (magazine == 0)
                magazine = 8;

            return Convert.ToInt32(magazine);
        }

        private Int32 GetRow(byte mrag1, byte mrag2)
        {
            //Convert to binary string
            string binRev1 = ExtractValue(mrag1);
            string binRev2 = ExtractValue(mrag2);

            // Construct return binary as MSB first
            string rowBin = binRev2.Substring(7, 1) + binRev2.Substring(5, 1) + binRev2.Substring(3, 1) + binRev2.Substring(1, 1) + binRev1.Substring(7, 1);

            byte row = Convert.ToByte(rowBin, 2);
            return row;
        }

        private string GetPageNumber(byte pu, byte pt)
        {
            //Removing hamming and convert to LSB first
            string binRevUnits = ExtractValue(pu);
            string binRevTens = ExtractValue(pt);

            // Construct return binary as MSB first
            string binUnits = binRevUnits.Substring(7, 1) + binRevUnits.Substring(5, 1) + binRevUnits.Substring(3, 1) + binRevUnits.Substring(1, 1);
            string binTens = binRevTens.Substring(7, 1) + binRevTens.Substring(5, 1) + binRevTens.Substring(3, 1) + binRevTens.Substring(1, 1);

            //Get page units and tens
            int pageUnits = Convert.ToInt32(binUnits, 2);
            int pageTens = Convert.ToInt32(binTens, 2);

            return Convert.ToString(pageTens, 16).ToUpper() + Convert.ToString(pageUnits, 16).ToUpper();
        }

        private string GetTimeCode(byte mu, byte mt, byte hu, byte ht)
        {
            //Removing hamming and convert to LSB first
            string binRevMinUnits = ExtractValue(mu);
            string binRevMinTens = ExtractValue(mt);
            string binRevHourUnits = ExtractValue(hu);
            string binRevHourTens = ExtractValue(ht);

            // Construct return binary as MSB first
            string binMinUnits = binRevMinUnits.Substring(7, 1) + binRevMinUnits.Substring(5, 1) + binRevMinUnits.Substring(3, 1) + binRevMinUnits.Substring(1, 1);
            string binMinTens = binRevMinTens.Substring(5, 1) + binRevMinTens.Substring(3, 1) + binRevMinTens.Substring(1, 1);
            string binHourUnits = binRevHourUnits.Substring(7, 1) + binRevHourUnits.Substring(5, 1) + binRevHourUnits.Substring(3, 1) + binRevHourUnits.Substring(1, 1);
            string binHourTens = binRevHourTens.Substring(3, 1) + binRevHourTens.Substring(1, 1);

            int intMins = (Convert.ToInt32(binMinTens, 2) * 10) + Convert.ToInt32(binMinUnits, 2);
            int intHour = (Convert.ToInt32(binHourTens, 2) * 10) + Convert.ToInt32(binHourUnits, 2);
            //Console.Write("hours:{0}, mins{1}", intHour.ToString(), intMins.ToString());

            string strMins = "00".Substring(0, 2 - intMins.ToString().Length) + intMins.ToString();
            string strHour = "00".Substring(0, 2 - intHour.ToString().Length) + intHour.ToString();

            return strHour + ":" + strMins;
        }

        private Boolean GetBit(Int32 bit, byte inByte)
        {
            // Note: this uses the teletext spec bit notation method, ie LSB first.

            // Check bit is in range
            if (bit < 1 || bit > 8)
                throw new ArgumentException("GetBit: Bit reference is out of range.");

            String bin = ExtractValue(inByte);

            if (bin.Substring(bit - 1, 1) == "1")
                return true;
            else
                return false;

        }

        public void Reset()
        {
            //reader.BaseStream.Seek(0, SeekOrigin.Begin);
            localPos = 0;
            //FrameLine set too high so that the Frame is read again from the Service's new position
            FrameLine = 99;
            //Revolutions = 0;
        }

        public Line FindHeader(string magpage)
        {
            string page = magpage.Substring(1, 2);
            int magazine = Convert.ToInt32(magpage.Substring(0, 1));
            Line line = new Line();
            Boolean escape = false;
            long initPosition = localPos;
            Int32 revs = Revolutions;

            while (!escape)
            {
                line = GetNextLine();
                if (line.Page == page && line.Magazine == magazine)
                    escape = true;

                if (localPos > initPosition + intCheckHorizon)
                {
                    escape = true;
                    line.Text = "Line not found (searched past CheckHorizon).";
                    line.Magazine = -1;
                }

                if (Revolutions > revs + 1)
                {
                    escape = true;
                    line.Text = "Line not found (too many Revolutions).";
                    line.Magazine = -1;
                }
            }
            
            return line;
        }

        public Line GetNextHeader()
        {
            Line localline = new Line();
            Boolean escape = false;

            while (!escape)
            {
                localline = GetNextLine();
                if (localline.Row == 0)
                    escape = true;
            }
            return localline;
        }

        public Page GetPage(string magPage)
        {
            Page localPage = new Page();
            Line nextLine = new Line();

            // Move Service to the header for required page
            Line header = FindHeader(magPage);

            localPage.Lines[0].Text = header.Text;
            localPage.Lines[0].LineNo = header.LineNo;
            localPage.Lines[0].Frame = header.Frame;
            localPage.Lines[0].EndPos = header.EndPos;
            localPage.Lines[0].Magazine = header.Magazine;
            localPage.Lines[0].Page = header.Page;
            localPage.Lines[0].Row = header.Row;
            localPage.Lines[0].StartPos = header.StartPos;
            localPage.Lines[0].TimeCode = header.TimeCode;
            localPage.Lines[0].Type = header.Type;
            localPage.Lines[0].Flags = header.Flags;
            localPage.Lines[0].MRAG1 = header.MRAG1;
            localPage.Lines[0].MRAG2 = header.MRAG2;
            localPage.Lines[0].PU = header.PU;
            localPage.Lines[0].PT = header.PT;
            localPage.Lines[0].MU = header.MU;
            localPage.Lines[0].MT = header.MT;
            localPage.Lines[0].HU = header.HU;
            localPage.Lines[0].HT = header.HT;
            localPage.Lines[0].CA = header.CA;
            localPage.Lines[0].CB = header.CB;
            localPage.Lines[0].Bytes = header.Bytes;

            if (header.Magazine != -1)
            {
                Int32 row = 1;

                // Get next line
                nextLine = GetNextLine(magPage);
                localPage.Lines[row].Text = nextLine.Text;

                while (nextLine.Row != 0 && row < 256)
                {
                    localPage.Lines[row].Text = nextLine.Text;
                    localPage.Lines[row].LineNo = nextLine.LineNo;
                    localPage.Lines[row].Frame = nextLine.Frame;
                    localPage.Lines[row].EndPos = nextLine.EndPos;
                    localPage.Lines[row].Magazine = nextLine.Magazine;
                    localPage.Lines[row].Page = nextLine.Page;
                    localPage.Lines[row].Row = nextLine.Row;
                    localPage.Lines[row].StartPos = nextLine.StartPos;
                    localPage.Lines[row].TimeCode = nextLine.TimeCode;
                    localPage.Lines[row].Type = nextLine.Type;
                    localPage.Lines[row].MRAG1 = nextLine.MRAG1;
                    localPage.Lines[row].MRAG2 = nextLine.MRAG2;
                    localPage.Lines[row].Bytes = nextLine.Bytes;
                    nextLine = GetNextLine(magPage);

                    row++;
                }
            }
            else
            {
                localPage.Lines[0].Magazine = -1;
            }

            // Since the end of the page is governed by a header row of the same magazine, decrement the frame line pointer
            // so that the header row is read next time (or entire pages are skipped)
            FrameLine--;

            return localPage;
        }

        public Hashtable ConvertMRAG(Byte mrag1, Byte mrag2)
        {
            Hashtable convertedMRAG = new Hashtable();
            convertedMRAG["Magazine"] = GetMagazine(mrag1);
            convertedMRAG["Row"] = GetRow(mrag1, mrag2);

            return convertedMRAG;
        }

        public String ConvertPageUnitsTens(Byte pu, Byte pt)
        {
            return GetPageNumber(pu, pt);
        }

        public Line CreatePacket(Int32 Magazine, String Page, String Subpage, byte PacketNo, byte RowNo, string Text, String flags = "10000000000")
        {
            String textForByte = Text.PadRight(40, (Char)0x00);
            flags = flags.PadLeft(16);

            Line newline = new Line();
            newline.Clear();
            newline.EndPos = 0;
            newline.Flags = null;
            newline.Frame = 0;
            newline.LineNo = PacketNo;
            newline.Magazine = Magazine;
            newline.Page = Page;
            newline.Row = RowNo;
            newline.StartPos = 0;
            newline.Text = Text;
            
            Array.Copy(Encoding.ASCII.GetBytes(textForByte), 0, newline.Bytes, 3, textForByte.Length);
            newline.TimeCode = Subpage;
            newline.Flags = new ControlFlags();
            newline.Flags.C4_Erase = (flags.Substring(1, 1) == "1" ? true : false);
            newline.Flags.C5_Newsflash = (flags.Substring(2, 1) == "1" ? true : false);
            newline.Flags.C6_Subtitle = (flags.Substring(3, 1) == "1" ? true : false);
            newline.Flags.C7_SuppressHeader = (flags.Substring(4, 1) == "1" ? true : false);
            newline.Flags.C8_Update = (flags.Substring(5, 1) == "1" ? true : false);
            newline.Flags.C9_InterruptedSequence = (flags.Substring(6, 1) == "1" ? true : false);
            newline.Flags.C10_InhibitDisplay = (flags.Substring(7, 1) == "1" ? true : false);
            newline.Flags.C11_MagazineSerial = (flags.Substring(8, 1) == "1" ? true : false);
            newline.Flags.C12 = (flags.Substring(9, 1) == "1" ? true : false);
            newline.Flags.C13 = (flags.Substring(10, 1) == "1" ? true : false);
            newline.Flags.C14 = (flags.Substring(11, 1) == "1" ? true : false);

            switch (PacketNo)
            {
                case 0:
                    newline.Type = LineTypes.Header;
                    break;
                case 25:
                    newline.Type = LineTypes.FastextDisplay;
                    break;
                default:
                    newline.Type = LineTypes.Line;
                    break;
            }

            newline.CalcHammingCodes();
            newline.Bytes[1] = newline.MRAG1;
            newline.Bytes[2] = newline.MRAG2;
            return newline;
        }

    }



}
