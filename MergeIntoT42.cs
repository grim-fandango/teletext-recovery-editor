using System.Data;
using System.Text;
using System.Data.SQLite;
using System.Collections;
using Microsoft.Win32;
using TeletextSharedResources;

namespace Teletext
{
    public partial class frmMergeIntoT42 : Form
    {
        String fixClockTemplate;
        String fixClockLimits;

        String[] lastSaneHH = new String[8]; 
        String[] lastSaneMM = new String[8]; 
        String[] lastSaneSS = new String[8];

        Boolean[] latchSU = new Boolean[8]; 
        Boolean[] latchST = new Boolean[8]; 
        Boolean[] latchMU = new Boolean[8]; 
        Boolean[] latchMT = new Boolean[8]; 
        Boolean[] latchHU = new Boolean[8]; 
        Boolean[] latchHT = new Boolean[8];

        String[] lastSaneDay = new string[8]; //"DAY";
        String[] lastSaneDD = new string[8]; //"DD";
        String[] lastSaneMonth = new string[8]; //"MTH";

        Int32 linePaddingPerField = 0;

        public frmMergeIntoT42()
        {
            InitializeComponent();

            // Initialise values or fetch from registry
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\UniqueCodeAndData\TeletextRecoveryEditor");
            if (key != null)
            {
                tbBinariesFolder.Text = key.GetValue("BinariesFolder") != null ? key.GetValue("BinariesFolder").ToString() : "";
                tbT42File.Text = key.GetValue("SourceT42") != null ? key.GetValue("SourceT42").ToString() : "";
                tbMergedFile.Text = key.GetValue("DestT42") != null ? key.GetValue("DestT42").ToString() : "";
                tbHeaderTemplate.Text = key.GetValue("HeaderTemplate") != null ? key.GetValue("HeaderTemplate").ToString() : "CEEFAX mpp  DAY DD MTH \\u0003";
                tbClockTemplate.Text = key.GetValue("ClockTemplate") != null ? key.GetValue("ClockTemplate").ToString() : "Hh1Mm2Ss";
                tbForceInitClockDigits.Text = key.GetValue("ForceInitialClockDigits") != null ? key.GetValue("ForceInitialClockDigits").ToString() : "xx:xx/xx";
                nudLinePadding.Text = key.GetValue("LinePadding") != null ? key.GetValue("LinePadding").ToString() : "0";
                tbMergeOnlyPage.Text = key.GetValue("MergeOnly") != null ? key.GetValue("MergeOnly").ToString() : "";

                cbThreaded.Checked = key.GetValue("Threaded") != null ? Convert.ToBoolean(key.GetValue("Threaded")) : true;
                cbClearDB.Checked = key.GetValue("ClearEntireDB") != null ? Convert.ToBoolean(key.GetValue("ClearEntireDB")) : true;
                cbSubpagesAreSeparateFiles.Checked = key.GetValue("SubpagesAreSeparateFiles") != null ? Convert.ToBoolean(key.GetValue("SubpagesAreSeparateFiles")) : false;
                cbTimeCodedSubpages.Checked = key.GetValue("TimecodedSubpages") != null ? Convert.ToBoolean(key.GetValue("TimecodedSubpages")) : true;
                cbFixHeader.Checked = key.GetValue("FixHeader") != null ? Convert.ToBoolean(key.GetValue("FixHeader")) : true;
                cbFixClock.Checked = key.GetValue("FixClock") != null ? Convert.ToBoolean(key.GetValue("FixClock")) : true;

                key.Close();
            }
        }

        private void btnMerge_Click(object sender, EventArgs e)
        {
            // Initialise
            linePaddingPerField = (Int32)nudLinePadding.Value;

            fixClockTemplate = tbClockTemplate.Text;
            fixClockLimits = "29:59/59";

            for (Int32 mag = 0; mag < 8; mag++)
            {
                lastSaneHH[mag] = tbForceInitClockDigits.Text.Substring(fixClockTemplate.IndexOf("Hh"), 2);
                lastSaneMM[mag] = tbForceInitClockDigits.Text.Substring(fixClockTemplate.IndexOf("Mm"), 2);
                lastSaneSS[mag] = tbForceInitClockDigits.Text.Substring(fixClockTemplate.IndexOf("Ss"), 2);

                latchSU[mag] = false;
                latchST[mag] = false;
                latchMU[mag] = false;
                latchMT[mag] = false;
                latchHU[mag] = false;
                latchHT[mag] = false;

                lastSaneDay[mag] = "DAY";
                lastSaneDD[mag] = "DD";
                lastSaneMonth[mag] = "MTH";
            }




            //Load T42 file as service


            //try
            //{
            //    using (File.Open(tbT42File.Text, FileMode.Open))
            //    { };
                
            //}
            //catch
            //{
            //    tbT42File.Text = tbT42File.Text.Replace("D:\\My Documents\\SkyDrive", "C:\\Users\\jasonrob\\OneDrive");
            //    tbBinariesFolder.Text = tbBinariesFolder.Text.Replace("D:\\My Documents\\SkyDrive", "C:\\Users\\jasonrob\\OneDrive");
            //    tbMergedFile.Text = tbMergedFile.Text.Replace("D:\\My Documents\\SkyDrive", "C:\\Users\\jasonrob\\OneDrive");
            //}

            Service sourceT42 = new Service();

            String result = sourceT42.OpenService(tbT42File.Text);
            sourceT42.RowLength = 42;
            Boolean usingExistingData = false;
            Int32 binsId = -1;

            // Clock stuff
            // Reset latches for fixing the clock
            //latchHU = false;
            //latchMT = false;
            //latchMU = false;
            //latchST = false;
            //latchSU = false;


            // Check to see if the binaries exist already in the DB
            //Get the bin folder name from the form

            //try
            //{

                DirectoryInfo dinfo = new DirectoryInfo(tbBinariesFolder.Text);
                String binFolder = dinfo.Parent.Name;

                using (SQLiteConnection conSQLite = OpenDatabase())
                {
                    DataSet dsExistsCheck = new DataSet();
                    SQLiteDataAdapter daSqlIte = new SQLiteDataAdapter("SELECT * FROM bins WHERE BinPath LIKE '%" + binFolder + "%'", conSQLite);
                    daSqlIte.Fill(dsExistsCheck);
                    if (dsExistsCheck.Tables[0].Rows.Count > 0)
                    {
                        stLabel.Text = "Deleting old records...";
                        DataRow d1 = dsExistsCheck.Tables[0].Rows[0];
                        binsId = Convert.ToInt32(d1["ID"]);
                        DialogResult drOverwrite = MessageBox.Show("This service already exists in the database.  Do you want to erase the DB copy and repopulate?", "Teletext Stream Creator", MessageBoxButtons.YesNo);
                   
                    if (drOverwrite == DialogResult.Yes)
                    {
                        foreach (DataRow d in dsExistsCheck.Tables[0].Rows)
                        {
                            SQLiteCommand scExecute = new SQLiteCommand("DELETE FROM binPackets WHERE ParentBinId = " + d["ID"], conSQLite);
                            scExecute.ExecuteNonQuery();
                            scExecute = new SQLiteCommand("DELETE FROM bins WHERE ID = " + d["ID"], conSQLite);
                            scExecute.ExecuteNonQuery();
                        };
                        MessageBox.Show("Services deleted from database.", "Teletext Stream Creator", MessageBoxButtons.OK);
                        usingExistingData = false;
                        binsId = -1;
                    }
                    else
                        usingExistingData = true;
                    }
                }
            //}
            //catch { }

            stLabel.Text = "Copying binaries to DB...";

            if (cbThreaded.Checked)
            {
                stProgressBar.Value = 0;

                Thread t = new Thread(delegate ()
                {
                    T42MergeThread(sourceT42, usingExistingData, binsId);

                });

                t.IsBackground = true;
                t.Start();

                do
                {
                    stProgressBar.Value = (Int32)((Double)sourceT42.Position / (Double)sourceT42.Length * 100);

                    System.Windows.Forms.Application.DoEvents();

                } while (t.IsAlive);
            }
            else
                T42MergeThread(sourceT42, usingExistingData, binsId);

            stLabel.Text = "Finished";

        }

        private void T42MergeThread(Service sourceT42, Boolean usingExistingData, Int32 binsId)
        {
            // Stage 1: load all of the bins into the DB if they aren't already there

            // Open DB
            SQLiteConnection conSQLite = OpenDatabase();

            // Loop files in the bin folder and add them
            if (!usingExistingData)
            {
                AddBinariesToDB(conSQLite);

                // Get binsID from new data
                DirectoryInfo dinfo = new DirectoryInfo(tbBinariesFolder.Text);
                String binFolder = dinfo.Parent.Name;
                DataSet dsExistsCheck = new DataSet();
                SQLiteDataAdapter daSqlIte = new SQLiteDataAdapter("SELECT * FROM bins WHERE BinPath LIKE '%" + binFolder + "%'", conSQLite);
                daSqlIte.Fill(dsExistsCheck);
                if (dsExistsCheck.Tables[0].Rows.Count > 0)
                {
                    DataRow d1 = dsExistsCheck.Tables[0].Rows[0];
                    binsId = Convert.ToInt32(d1["ID"]);
                }
            }


            // Add meta subpage timecodes if the service doesn't already use them
            if (!cbTimeCodedSubpages.Checked && !usingExistingData)
            {
                AddSubpageTimecodesToBins(sourceT42, conSQLite);
            }

            if (binsId == -1)
                System.Diagnostics.Debug.WriteLine("binsId should not be -1.  This might be because the DB data has been deleted and re-added on the same run.  Add code to look it up form the DB");


            // Stage 2: Search for squashed copies of packets in the DB
            if (!cbTimeCodedSubpages.Checked)
                MatchWithoutTimeCodes(sourceT42, conSQLite, binsId);
            else
                MatchUsingTimeCodes(sourceT42, conSQLite, binsId);

            conSQLite.Close();



        }

        private void MatchWithoutTimeCodes(Service sourceT42, SQLiteConnection conSQLite, Int32 binsId)
        {
            stLabel.Text = "Matching...";

            CurrentPageInfo[] CurrentPages = new CurrentPageInfo[9];
            for (Int32 n = 0; n < 9; n++)
                CurrentPages[n] = new CurrentPageInfo();

            sourceT42.Reset();

            Line lT42;

            Boolean debugPackets = false;
            String currentPage = "";

            List <MetaLine> packets = new List<MetaLine>();

            Boolean debug = true;


            //MetaLine[] packets = new MetaLine[sourceT42.Length / sourceT42.RowLength];
            Int32 packetIndex = 0;
            Int32 linePaddingCount = 0;

            // Pass 1 - do our best to match the packets from the source with the binaries.  Lines with not much in them, or very noisy lines, are
            // susceptible to having the wrong packet associated with them.  A two-pass model means that it has an attempt at matching during Pass 1,
            // and Pass 2 checks to see which page in the bin carousel is the most popular and uses that to draw all packets for that subpage, thus
            // getting rid of any outliers.

            // Pass first 1 puts the matched-as-best-as-we-can packets into an array, along with supporting metadata. 

            do
            {
                lT42 = sourceT42.GetNextLine();
                //if (lT42.Type == LineTypes.Header)
                //    System.Diagnostics.Debug.WriteLine("BEFORE Header: {0}   Posn.: {1} ", lT42.Text, Convert.ToString(lT42.StartPos, 16));

                if (cbFixClock.Checked && lT42.Type == LineTypes.Header)
                {
                    lT42.Bytes = FixClock(lT42.Bytes);
                    lT42.Text = Encoding.Default.GetString(lT42.Bytes);

                }
                //if (lT42.Text.Substring(lT42.Text.Length - 8, 8) == "13:02/51")
                //    System.Diagnostics.Debug.WriteLine("AFTER Header: {0}   Posn.: {1} ", lT42.Text, Convert.ToString(lT42.StartPos, 16));

                if (cbFixHeader.Checked && lT42.Type == LineTypes.Header)
                {
                    lT42.Bytes = FixHeader(lT42.Bytes);
                    lT42.Text = Encoding.Default.GetString(lT42.Bytes);
                }

                if (lT42.Type == null)
                    lT42 = new Line();


                Byte bytMRAG1 = lT42.MRAG1;
                Byte bytMRAG2 = lT42.MRAG2;
                Int32 intMagazine = lT42.Magazine;

                // If the VBI line was not a teletext line, copy the T42 as is from the source file
                Boolean blnCopyAsIs = lT42.Type == LineTypes.Blank ? true : false;
                String copyAsIsReason = blnCopyAsIs == true ? "blank line" : "";

                if (lT42.Row == 0 && !blnCopyAsIs)
                {
                    // Get search terms from header packet

                    String strPage = intMagazine.ToString() + lT42.Page;
                    String strTimeCode = lT42.TimeCode;

                    // Store these so we know where we are in a parallel (interleaved) service
                    CurrentPages[intMagazine].Page = strPage;
                    CurrentPages[intMagazine].TimeCode = strTimeCode;
                    CurrentPages[intMagazine].MRAG1 = bytMRAG1;
                    CurrentPages[intMagazine].MRAG2 = bytMRAG2;

                    currentPage = strPage;
                }

                if (CurrentPages[intMagazine].Page == tbMergeOnlyPage.Text && lT42.Row == 0 && debug)
                    System.Diagnostics.Debug.WriteLine("New header for page " + tbMergeOnlyPage.Text);

                //---------------------------------------------------
                // Find packet in DB

                if (lT42.Row > 24 && !blnCopyAsIs)
                {
                    blnCopyAsIs = true;
                    copyAsIsReason = "Data packet";
                }

                //if (currentPage != tbMergeOnlyPage.Text && tbMergeOnlyPage.Text != "" && !blnCopyAsIs)
                if (CurrentPages[intMagazine].Page != tbMergeOnlyPage.Text && tbMergeOnlyPage.Text != "" && !blnCopyAsIs)
                {
                    blnCopyAsIs = true;
                    copyAsIsReason = "Only one page is being merged";
                }

                if (lT42.Type == LineTypes.Header)
                {
                    copyAsIsReason = "Header row";
                    blnCopyAsIs = true;
                }

                if (blnCopyAsIs && debug)
                    System.Diagnostics.Debug.WriteLine("Copy packet " + lT42.Row + " for page " + CurrentPages[intMagazine].Page + " as is because: " + copyAsIsReason);

                if (!blnCopyAsIs)
                {
                    if (CurrentPages[intMagazine].Page != null)
                    {
                        DataSet dsPackets = new DataSet();
                        DataTable dtPackets = new DataTable();

                        String strSQL = "";
                        Int32 bestRow = 0;

                        if (debug)
                            System.Diagnostics.Debug.WriteLine("Packet to find: Y=" + lT42.Row + " Text: " + lT42.Text);

                        // Get all the packets for this subpage



                        // fetch all examples of the row / page combination from the DB, i.e. the row from all sub pages
                        strSQL = "SELECT BinPackets.* FROM BinPackets " +
                            //"JOIN bins on bins.BinPath LIKE '" + tbBinariesFolder.Text + "%' " +
                            "JOIN bins ON bins.ID  = BinPackets.ParentBinId " +
                            "WHERE 1=1 " +
                            "AND Page = '" + CurrentPages[intMagazine].Page + "' " +
                            "AND MRAG1 = " + bytMRAG1 + " " +
                            "AND MRAG2 = " + bytMRAG2;

                        if (!cbSubpagesAreSeparateFiles.Checked)
                            strSQL += " AND ParentBinID = " + binsId;

                        using (SQLiteDataAdapter daPackets = new SQLiteDataAdapter(strSQL, conSQLite))
                        {
                            daPackets.Fill(dsPackets);
                        }

                        // Set up an array to record the match scores
                        Int32[] matchScores = new Int32[dsPackets.Tables[0].Rows.Count];

                        // Go through all the packets and score them on a byte-by-byte basis
                        Int32 rowIndex = 0;
                        foreach (DataRow d in dsPackets.Tables[0].Rows)
                        {
                            // Put this DB row into an array
                            Byte[] bytFromDb = new Byte[42];
                            Array.Copy((Byte[])d["Bytes"], 0, bytFromDb, 0, ((Byte[])d["Bytes"]).Length);

                            Int32 byteIndex = 0;
                            Int32 score = 0;
                            foreach (Byte b in lT42.Bytes)
                            {
                                //System.Diagnostics.Debug.WriteLine("comparing {0} with {1}", b, bytFromDb[byteIndex]);
                                if (b == bytFromDb[byteIndex])
                                    score++;
                                byteIndex++;
                            }
                            matchScores[rowIndex] = score;
                            //System.Diagnostics.Debug.WriteLine("Row {1} score: {0}\n", score, rowIndex);
                            rowIndex++;
                        }

                        // See which row has the highest score

                        Int32 bestScore = 0;
                        for (Int32 n = 0; n < dsPackets.Tables[0].Rows.Count; n++)
                        {
                            if (matchScores[n] > bestScore)
                            {
                                bestRow = n;
                                bestScore = matchScores[n];
                            }
                        }
                        if (matchScores.Length > 0)
                        {
                            CurrentPages[intMagazine].SubpagesUsed.Add(Convert.ToString(dsPackets.Tables[0].Rows[bestRow]["MetaTimeCode"]));
                            if (CurrentPages[intMagazine].Page == tbMergeOnlyPage.Text && false)
                                System.Diagnostics.Debug.WriteLine("Page {0}, subpage {1}, bestRow {2}, bestScore {3}", dsPackets.Tables[0].Rows[bestRow]["Page"], dsPackets.Tables[0].Rows[bestRow]["TimeCode"], bestRow, bestScore);
                        }


                        //


                        if (dsPackets.Tables[0].Rows.Count > 0)
                        {
                            DataRow drOutPacket = dsPackets.Tables[0].Rows[bestRow];

                            if (CurrentPages[intMagazine].Page == tbMergeOnlyPage.Text && debug)
                                System.Diagnostics.Debug.WriteLine("Copying:        Y=" + lT42.Row + " Text: " + drOutPacket["PacketText"]);

                            Byte[] t42Line = new Byte[42];
                            Array.Copy(lT42.Bytes, 0, t42Line, 0, 42);

                            if (lT42.Row != 0)
                                // Copy all of row
                                Array.Copy((Byte[])drOutPacket["Bytes"], 0, t42Line, 0, 42);
                            else
                            {
                                // Copy all except clock for header rows
                                //Array.Copy((Byte[])drOutPacket["Bytes"], 0, t42Line, 0, 34);
                                Array.Copy((Byte[])drOutPacket["Bytes"], 10, t42Line, 10, 24);
                                //Array.Copy((Byte[])drOutPacket["Bytes"], 1, t42Line, 0, 42);

                                Array.Copy(lT42.Bytes, 34, t42Line, 34, 8);
                            }

                            //writer.Write(t42Line);
                            MetaLine lineData = new MetaLine();
                            lineData.Index = packetIndex;
                            lineData.Page = (String)drOutPacket["Page"];
                            lineData.Bytes = t42Line;
                            lineData.TimeCode = (String)drOutPacket["TimeCode"];
                            lineData.MetaTimeCode = (String)drOutPacket["MetaTimeCode"];
                            Hashtable mrag = sourceT42.ConvertMRAG(t42Line[0], t42Line[1]);
                            lineData.Packet = Convert.ToInt32(mrag["Row"]);

                            //packets[packetIndex] = lineData;
                            packets.Add(lineData);
                            if (debug)
                                System.Diagnostics.Debug.WriteLine("Packet copied: {0} {1}", lineData.Packet, drOutPacket["PacketText"]);
                        }
                        else
                        {
                            // We can't find this line in the DB, so copy it as is to preserve timing
                            blnCopyAsIs = true;
                            copyAsIsReason = "Not found in DB (Y=" + intMagazine + ")";
                            if (debug)
                            {
                                System.Diagnostics.Debug.WriteLine(copyAsIsReason);
                                System.Diagnostics.Debug.WriteLine("SQL:" + strSQL);
                            }
                        }
                    }
                    else
                    {
                        blnCopyAsIs = true;
                        copyAsIsReason = "Page is null for magazine " + intMagazine;
                        if (CurrentPages[intMagazine].Page == tbMergeOnlyPage.Text && debug)
                            System.Diagnostics.Debug.WriteLine(copyAsIsReason);
                    }
                }

                // Output the non-teletext lines
                if (blnCopyAsIs)
                {
                    Byte[] t42Line = new Byte[42];
                    Array.Copy(lT42.Bytes, 0, t42Line, 0, 2);
                    if (debugPackets)
                        Array.Copy(Encoding.ASCII.GetBytes(copyAsIsReason), 0, t42Line, 2, copyAsIsReason.Length);


                    MetaLine lineData = new MetaLine();
                    lineData.Index = packetIndex;
                    lineData.Page = lT42.Magazine + lT42.Page;
                    lineData.Bytes = lT42.Bytes;
                    lineData.TimeCode = lT42.TimeCode;
                    lineData.MetaTimeCode = "";
                    Hashtable mrag = sourceT42.ConvertMRAG(lT42.Bytes[0], lT42.Bytes[1]);
                    lineData.Packet = Convert.ToInt32(mrag["Row"]);

                    packets.Add(lineData);
                }

                packetIndex++;


                // Do line padding stuff
                linePaddingCount++;
                if (linePaddingCount == 16 - linePaddingPerField && linePaddingPerField != 0)
                {
                    MetaLine lineData = new MetaLine();
                    lineData.Index = packetIndex;
                    lineData.Page = "0FF";
                    lineData.Bytes = new byte[42];
                    lineData.TimeCode = "";
                    lineData.MetaTimeCode = "";
                    for (Int32 n = 0; n < linePaddingPerField; n++)
                    {
                        packets.Add(lineData);
                        packetIndex++;
                    }
                    linePaddingCount = 0;
                }

                // Debug stuff
                //MetaLine l = new MetaLine();
                //l = packets.Last();
                //if (l.Packet == 0 && l.Page.Substring(1,2) != "FF" && l.Page.Substring(0,1) == "1")
                //{

                //    System.Diagnostics.Debug.WriteLine("Page: {0}, Y: {1}, Text: {2}", l.Page, l.Packet, System.Text.Encoding.ASCII.GetString(l.Bytes, 0, l.Bytes.Length));
                //}
                if (lT42.Type != LineTypes.Blank)
                    System.Diagnostics.Debug.WriteLine("");
            }


            while (sourceT42.Revolutions == 0);
            //while (sourceT42.Position < 600000);

            List<MetaLine> lstNewPackets = new List<MetaLine>();
            foreach (MetaLine m in packets)
                lstNewPackets.Add(m);


            //*****************************************************************************************************************************************************
            //*****************************************************************************************************************************************************
            //*****************************************************************************************************************************************************
            //*****************************************************************************************************************************************************


            // FOr each magazine, look at each page and check that all of the packets have come from the same binaries subpage.

            Int32 magFilter = -1;
            if (tbMergeOnlyPage.Text != "")
                magFilter = Convert.ToInt32(tbMergeOnlyPage.Text.Substring(0, 1));

            for (Int32 mag = 0; mag < 8; mag++)
            {
                if ((magFilter != -1 && magFilter == mag) || magFilter == -1)
                {
                    stLabel.Text = "Validating subpages.  Magazine: " + mag;
                    String lastPage = "";

                    List<MetaLine> lstTimecodes = new List<MetaLine>();


                    foreach (MetaLine m in packets.Where(p => (Convert.ToInt32(p.Page.Substring(0, 1)) == mag && p.Page.PadRight(3, Convert.ToChar(" ")).Substring(1,2) != "FF")))
                    {
                        if (m.Page.Length > 1)
                        {

                            if (lastPage != m.Page)
                            {
                                // Cash up the page to see what MetaTimeCodes we have
                                System.Diagnostics.Debug.WriteLine("New Page is " + m.Page);

                                /*if (lastPage == "100")
                                {
                                    System.Diagnostics.Debug.WriteLine("New Page is " + m.Page);



                                }*/

                                String topMtc = "";
                                Int32 topMtcScore = 0;

                                if (lstTimecodes.Count() > 0)
                                {
                                    // Add up the timecodes and store them in a hashtable
                                    Hashtable scores = new Hashtable();
                                    foreach (MetaLine n in lstTimecodes)
                                        if (!scores.ContainsKey(n.MetaTimeCode))
                                            scores.Add(n.MetaTimeCode, 1);
                                        else
                                            scores[n.MetaTimeCode] = (Int32)scores[n.MetaTimeCode] + 1;

                                    // See which score is the highest

                                    foreach (String key in scores.Keys)
                                    {
                                        System.Diagnostics.Debug.Print("{0} - {1}", key, scores[key]);
                                        if ((Int32)scores[key] > topMtcScore)
                                        {
                                            topMtc = key;
                                            topMtcScore = (Int32)scores[key];
                                        }
                                    }


                                    // For each row that doesn't have this most popular MTC, overwrite with the row from the reference copy with the most popular mtc

                                    foreach (MetaLine n in lstTimecodes)
                                    {
                                        if (n.MetaTimeCode != topMtc)
                                        {
                                            // Find the packet from the popular mtc

                                            String strSQL = "SELECT BinPackets.* FROM BinPackets " +
                                                "JOIN bins ON bins.ID  = BinPackets.ParentBinId " +
                                                "WHERE 1=1 " +
                                                "AND Page = '" + n.Page + "' " +
                                                "AND MRAG1 = " + n.Bytes[0] + " " +
                                                "AND MRAG2 = " + n.Bytes[1] + " " +
                                                "AND MetaTimecode = '" + topMtc + "'"
                                                ;

                                            if (!cbSubpagesAreSeparateFiles.Checked)
                                                strSQL += " AND ParentBinID = " + binsId;

                                            DataSet dsGoodPacket = new DataSet();

                                            using (SQLiteDataAdapter daPackets = new SQLiteDataAdapter(strSQL, conSQLite))
                                            {
                                                daPackets.Fill(dsGoodPacket);
                                            }

                                            Int32 index = Convert.ToInt32(n.Index);

                                            if (dsGoodPacket.Tables[0].Rows.Count > 0)
                                            {
                                                DataRow d = dsGoodPacket.Tables[0].Rows[0];

                                                MetaLine mlFromList = n;

                                                mlFromList.Bytes = (Byte[])d["Bytes"];
                                                mlFromList.MetaTimeCode = (String)d["MetaTimecode"];

 
                                                // If we fixed the clock, copy the fixed bytes back in
                                                if (mlFromList.Packet == 0)
                                                {
                                                    Array.Copy(packets[index].Bytes, 42 - fixClockTemplate.Length, mlFromList.Bytes, 42 - fixClockTemplate.Length, fixClockTemplate.Length);
                                                }

                                                // If we fixed the header, copy the fixed bytes back in
                                                if (cbFixHeader.Checked && mlFromList.Packet == 0)
                                                {
                                                    Array.Copy(packets[index].Bytes, 10, mlFromList.Bytes, 10, 42 - 10 - fixClockTemplate.Length);
                                                }

                                                lstNewPackets[index] = mlFromList;

                                            }
                                            else
                                            {
                                                // blank the line as this packet doesn't exist in the DB reference copy
                                                MetaLine blank = new MetaLine();
                                                blank.MetaTimeCode = topMtc;
                                                blank.Index = index;
                                                blank.Packet = n.Packet;
                                                blank.TimeCode = n.TimeCode;
                                                blank.Bytes = new byte[42];
                                                lstNewPackets[index] = blank;
                                            }
                                        }
                                    }





                                    //stLabel.Text = "Checking for missing packets...";

                                    // Check these packets against the DB to see if we have any missing
                                    String strCheckMissingSQL = "SELECT BinPackets.* FROM BinPackets " +
                                                    "JOIN bins ON bins.ID  = BinPackets.ParentBinId " +
                                                    "WHERE 1=1 " +
                                                    "AND Page = '" + lstTimecodes[0].Page + "' " +
                                                    "AND MetaTimecode = '" + topMtc + "'";

                                    if (!cbSubpagesAreSeparateFiles.Checked)
                                        strCheckMissingSQL += " AND ParentBinID = " + binsId;

                                    DataSet dsCompletePage = new DataSet();

                                    using (SQLiteDataAdapter daPackets = new SQLiteDataAdapter(strCheckMissingSQL, conSQLite))
                                    {
                                        daPackets.Fill(dsCompletePage);
                                    }

                                    List<MetaLine> lstMissingPackets = new List<MetaLine>();
                                    // See if there are any packets in the DB that aren't in this list
                                    // TO DO: this seems to add any data packets to lstMissingPackets - check this as data packets should have already have been
                                    // copied over as-is

                                    foreach (DataRow d in dsCompletePage.Tables[0].Rows)
                                    {
                                        MetaLine n = new MetaLine();
                                        n.Bytes = (Byte[])d["Bytes"];
                                        n.MetaTimeCode = (String)d["MetaTimecode"];
                                        n.Page = (String)d["Page"];
                                        n.TimeCode = (String)d["Timecode"];
                                        Hashtable localMRAG = sourceT42.ConvertMRAG(n.Bytes[0], n.Bytes[1]);
                                        n.Packet = (Int32)localMRAG["Row"];

                                        Boolean found = false;
                                        // Check each line in the page to see if the MRAG matches a packet in the DB
                                        foreach (MetaLine o in lstTimecodes)
                                        {
                                            if (o.Bytes[0] == n.Bytes[0] && o.Bytes[1] == n.Bytes[1])
                                            {
                                                found = true;
                                            }
                                        }

                                        if (!found)
                                            lstMissingPackets.Add(n);

                                    }

                                    // Get largest and smallest indexes in this page - this will give us a search range in the main packets list to find a spare packet
                                    Int32 min = lstTimecodes[0].Index;
                                    Int32 max = min;
                                    foreach (MetaLine n in lstTimecodes)
                                    {
                                        if (n.Index < min)
                                            min = n.Index;
                                        if (n.Index > max)
                                            max = n.Index;
                                    }


                                    Int32 lastPacket = 0;
                                    Int32 missingIndex = 0;
                                    List<MetaLine> lstMissingPacketsSorted = lstMissingPackets.OrderBy(o => o.Packet).ToList();

                                    System.Diagnostics.Debug.WriteLine("Now checking for blank lines in the t42 that are not blank in the bin file");
                                    for (Int32 i = (Int32)min; i < max + 1 && missingIndex < lstMissingPacketsSorted.Count; i++)
                                    {
                                        if ((packets[i].Bytes[0] != 255 && packets[i].Bytes[1] != 255) && (packets[i].Bytes[0] != 0 && packets[i].Bytes[1] != 0))
                                        {
                                            // Since this isn't a blank line, note the packet no. if the magazine matches the page we are trying to fix
                                            Hashtable mrag = sourceT42.ConvertMRAG(packets[i].Bytes[0], packets[i].Bytes[1]);
                                            if ((Int32)mrag["Magazine"] == Convert.ToInt32(lstTimecodes[0].Page.Substring(0, 1)))
                                            {
                                                lastPacket = (Int32)mrag["Row"];
                                                System.Diagnostics.Debug.WriteLine("Packet: {0} is present in the source t42", lastPacket);
                                                if (lstMissingPacketsSorted[0].Packet == lastPacket)
                                                {
                                                    System.Diagnostics.Debug.WriteLine("Packet: {0} has therefore been removed from the missing packets list", lstMissingPacketsSorted[0].Packet);
                                                    lstMissingPacketsSorted.RemoveAt(0);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // this packet is a blank so put a missing packet in it
                                            Hashtable nextMissing = sourceT42.ConvertMRAG(lstMissingPacketsSorted[missingIndex].Bytes[0], lstMissingPacketsSorted[missingIndex].Bytes[1]);

                                            // Copy the missing packet in
                                            lstNewPackets[i] = lstMissingPacketsSorted[missingIndex];

                                            lastPacket = lstMissingPacketsSorted[missingIndex].Packet;

                                            // Remove the copied packet from the list
                                            lstMissingPacketsSorted.RemoveAt(missingIndex);



                                            System.Diagnostics.Debug.WriteLine("Blank line replaced: Packet: {2}", packets[i].Bytes[0], packets[i].Bytes[1], nextMissing["Row"]);

                                        }
                                    }
                                }

                                lstTimecodes.Clear();
                                lastPage = m.Page;

                            }

                            if ((tbMergeOnlyPage.Text == "" || (tbMergeOnlyPage.Text == m.Page)) && lastPage != "")
                                lstTimecodes.Add(m);
                            //System.Diagnostics.Debug.WriteLine(tbMergeOnlyPage.Text);

                            Hashtable convertedMRAG = sourceT42.ConvertMRAG(m.Bytes[0], m.Bytes[1]);

                            System.Diagnostics.Debug.WriteLine(" P{0}, TC: {1}, MTC: {2}, Mag: {3}, Pkt: {4}", m.Page, m.TimeCode, m.MetaTimeCode, convertedMRAG["Magazine"], convertedMRAG["Row"]);
                        }
                    }
                }
            }

            Byte[] parityBitSetList = { 0, 3, 5, 6, 9, 10, 12, 15, 17, 18, 20, 23, 24, 27, 29, 30, 33, 34, 36, 39, 40, 43, 45, 46, 48, 51, 53, 54, 57, 58, 60, 63, 65, 66, 68, 71, 72, 75, 77, 78, 80, 83, 85, 86, 89, 90, 92, 95, 96, 99, 101, 102, 105, 106, 108, 111, 113, 114, 116, 119, 120, 123, 125, 126 };



            BinaryWriter writer = new BinaryWriter(File.Open(tbMergedFile.Text, FileMode.Create));
            foreach (MetaLine m in lstNewPackets)
            {
                // Set parity on packet
                Int32 startByte = 2;  //default for packets 1-24
                if (m.Packet == 0)
                    startByte = 10;
                if (m.Packet > 24)
                    startByte = 100;

                for (Int32 lineByteIndex = startByte; lineByteIndex < 42; lineByteIndex++)
                {
                    if (parityBitSetList.Contains<Byte>(m.Bytes[lineByteIndex]))
                        m.Bytes[lineByteIndex] = Convert.ToByte(m.Bytes[lineByteIndex] | (Byte)128);
                }

                // Write packet to disk
                writer.Write(m.Bytes);

            }
            writer.Close();


        }


        private void MatchUsingTimeCodes(Service sourceT42, SQLiteConnection conSQLite, Int32 binsId)
        {
            stLabel.Text = "Matching...";

            CurrentPageInfo[] CurrentPages = new CurrentPageInfo[9];
            for (Int32 n = 0; n < 9; n++)
                CurrentPages[n] = new CurrentPageInfo();

            BinaryWriter writer = new BinaryWriter(File.Open(tbMergedFile.Text, FileMode.Create));


            // Get next header
            Line lT42;
            Int32 ccount = 0;

            Boolean debugPackets = false;
            String currentPage = "";

            do
            {
                lT42 = sourceT42.GetNextLine();

                if (lT42.Type == null)
                    lT42 = new Line();


                Byte bytMRAG1 = lT42.MRAG1;
                Byte bytMRAG2 = lT42.MRAG2;
                Int32 intMagazine = lT42.Magazine;

                // If the VBI line was not a teletext line, copy the T42 as is from the source file
                Boolean blnCopyAsIs = lT42.Type == LineTypes.Blank ? true : false;
                String copyAsIsReason = blnCopyAsIs == true ? "blank line" : "";

                if (lT42.Row == 0 && !blnCopyAsIs)
                {
                    // Get search terms from header packet

                    String strPage = intMagazine.ToString() + lT42.Page;
                    String strTimeCode = lT42.TimeCode;


                    // Store these so we know where we are in an interleaved service
                    CurrentPages[intMagazine].Page = strPage;
                    CurrentPages[intMagazine].TimeCode = strTimeCode;
                    CurrentPages[intMagazine].MRAG1 = bytMRAG1;
                    CurrentPages[intMagazine].MRAG2 = bytMRAG2;

                    currentPage = strPage;
                }


                //---------------------------------------------------
                // Find packet in DB

                if (lT42.Row > 24 && !blnCopyAsIs)
                {
                    blnCopyAsIs = true;
                    copyAsIsReason = "Data packet";
                }

                if (currentPage != tbMergeOnlyPage.Text && tbMergeOnlyPage.Text != "" && !blnCopyAsIs)
                {
                    blnCopyAsIs = true;
                    copyAsIsReason = "Only one page is being merged";
                }

                if (!blnCopyAsIs)
                {
                    if (CurrentPages[intMagazine].Page != null)
                    {
                        DataSet dsPackets = new DataSet();
                        DataTable dtPackets = new DataTable();

                        String strSQL = "";
                        Int32 bestRow = 0;

                        // Get all the packets for this subpage

                        strSQL = "SELECT BinPackets.* FROM BinPackets " +
                                //"JOIN bins on bins.BinPath LIKE '" + tbBinariesFolder.Text + "%' " +
                                "JOIN bins ON bins.ID  = BinPackets.ParentBinId " +
                                "WHERE 1=1 " +
                                "AND ParentBinID = " + binsId + " " +
                                "AND Page = '" + CurrentPages[intMagazine].Page + "' " +
                                "AND TimeCode = '" + CurrentPages[intMagazine].TimeCode + "' " +
                                "AND MRAG1 = " + bytMRAG1 + " " +
                                "AND MRAG2 = " + bytMRAG2;

                        using (SQLiteDataAdapter daPackets = new SQLiteDataAdapter(strSQL, conSQLite))
                        {
                            daPackets.Fill(dsPackets);
                            daPackets.Fill(dtPackets);
                        }
                        
 
                        //


                        if (dsPackets.Tables[0].Rows.Count > 0)
                        {
                            DataRow drOutPacket = dsPackets.Tables[0].Rows[bestRow];

                            Byte[] t42Line = new Byte[42];
                            //Array.Copy((Byte[])drOutPacket["Bytes"], 1, t42Line, 0, 42);

                            if (lT42.Row != 0)
                                // Copy all of row
                                Array.Copy((Byte[])drOutPacket["Bytes"], 0, t42Line, 0, 42);
                            else
                            {
                                // Copy all except clock for header rows
                                Array.Copy((Byte[])drOutPacket["Bytes"], 0, t42Line, 0, 34);
                                //Array.Copy((Byte[])drOutPacket["Bytes"], 1, t42Line, 0, 42);

                                Array.Copy(lT42.Bytes, 34, t42Line, 34, 8);
                            }

                            writer.Write(t42Line);

                        }
                        else
                        {
                            // We can't find this line in the DB, so copy it as is to preserve timing
                            blnCopyAsIs = true;
                            copyAsIsReason = "Not found in DB (Y=" + intMagazine + ")";
                            System.Diagnostics.Debug.WriteLine(copyAsIsReason);
                            System.Diagnostics.Debug.WriteLine("SQL:" + strSQL);
                        }
                    }
                    else
                    {
                        blnCopyAsIs = true;
                        copyAsIsReason = "Page is null for magazine " + intMagazine;
                        System.Diagnostics.Debug.WriteLine(copyAsIsReason);
                    }
                }

                // Output the non-teletext lines
                if (blnCopyAsIs)
                {
                    Byte[] t42Line = new Byte[42];
                    //Array.Copy(lT42.Bytes, 0, t42Line, 0, 42);
                    Array.Copy(lT42.Bytes, 0, t42Line, 0, 2);
                    if (debugPackets)
                        Array.Copy(Encoding.ASCII.GetBytes(copyAsIsReason), 0, t42Line, 2, copyAsIsReason.Length);
                    //writer.Write(t42Line);
                    writer.Write(lT42.Bytes);
                }

                ccount++;
            }
            while (sourceT42.Revolutions == 0);
            writer.Close();
        }

        private void AddBinariesToDB(SQLiteConnection conSQLite)
        {
            String[] strBins = Directory.GetFiles(tbBinariesFolder.Text);

            foreach (String strBinariesFilename in strBins)
            {
                //Loop around T42 packets in each bin
                //if (strBinariesFilename.Substring(strBinariesFilename.Length - 7, 3) == tbMergeOnlyPage.Text || tbMergeOnlyPage.Text == "")

                // NEED TO CHECK TO SEE IF THIS FILE CONTAINS A PAGE IN THE MERGE ONLT PAGE FIELD

                // Load .bin as a T42

                Service bins = new Service();
                if (bins.OpenService(strBinariesFilename) == "")
                {
                    bins.RowLength = 42;
                    String strCurrentPage = "";

                    Line result = tbMergeOnlyPage.Text == "" ? bins.GetNextHeader() : bins.FindHeader(tbMergeOnlyPage.Text);
                    bins.Reset();


                    if (result.Magazine != -1)
                    {
                        Int64 binID = -1;
                        Boolean blnBinExistsInDB = false;

                        // Open the bin file whose filename corresponds to the page of the current header
                        //String strBinariesFilename = tbBinariesFolder.Text + "\\" + lT42.Magazine.ToString() + currentMagPage[lT42.Magazine] + ".bin";

                        // Check to see if file exists and doesn't already exist in the DB
                        DataSet dsExistsCheck = new DataSet();
                        String strThisBinFilename = "";

                        if (!cbSubpagesAreSeparateFiles.Checked)
                            strThisBinFilename = strBinariesFilename.Substring(0, strBinariesFilename.LastIndexOf("\\"));

                        SQLiteDataAdapter daSqlIte = new SQLiteDataAdapter("SELECT * FROM bins WHERE BinPath = '" + strThisBinFilename + "'", conSQLite);
                        daSqlIte.Fill(dsExistsCheck);
                        if (dsExistsCheck.Tables[0].Rows.Count > 0)
                        {
                            DataRow drRow = dsExistsCheck.Tables[0].Rows[0];
                            binID = (Int64)drRow["Id"];
                            blnBinExistsInDB = true;
                        }
                        else
                        {
                            //String strSQL = "INSERT INTO Bins (BinPath) VALUES ('" + strBinariesFilename + "')";
                            SQLiteCommand scExecute = new SQLiteCommand("INSERT INTO Bins (ID, BinPath) VALUES (null, '" + strThisBinFilename + "')", conSQLite);
                            scExecute.ExecuteNonQuery();

                            SQLiteCommand scGetLastInsertedBinId = new SQLiteCommand("SELECT last_insert_rowid()", conSQLite);
                            binID = (Int64)scGetLastInsertedBinId.ExecuteScalar();


                        }

                        if (!cbSubpagesAreSeparateFiles.Checked || (!blnBinExistsInDB && cbSubpagesAreSeparateFiles.Checked))
                        {
                            Int32 subpageCount = 0x4000;

                            for (Int32 n = 0; n < bins.Length / bins.RowLength; n++)
                            {
                                Line lBins = bins.GetNextLine();
                                String strMetaTimeCode = "";

                                if (lBins.Row == 0 && lBins.Type != LineTypes.Blank)
                                {
                                    strCurrentPage = lBins.Magazine.ToString() + lBins.Page;
                                }

                                // Does this page already exist in the DB? 



                                Boolean blnExists = false;
                                DataSet dsCount = new DataSet();
                                String strSQL = "SELECT ParentBinId FROM BinPackets WHERE page = '" + strCurrentPage + "'";

                                if (!cbSubpagesAreSeparateFiles.Checked)
                                    strSQL += " AND ParentBinID = " + binID;

                                using (SQLiteDataAdapter daPackets = new SQLiteDataAdapter(strSQL, conSQLite))
                                {
                                    daPackets.Fill(dsCount);


                                    if (dsCount.Tables[0].Rows.Count > 0)
                                    {
                                        blnExists = true;
                                        //if (cbSubpagesAreSeparateFiles.Checked)
                                        //{
                                        //    DataRow d = dsCount.Tables[0].Rows[0];
                                        //    binID = d["ParentBinId"];
                                        //}
                                    }
                                }


                                // If the page already exists, find the highest subpage and load into the subpageCount
                                if (blnExists && !cbTimeCodedSubpages.Checked)
                                {
                                    DataSet dsPackets = new DataSet();

                                    strSQL = "SELECT MAX(MetaTimeCode) As MetaTimeCode, ParentBinId FROM BinPackets WHERE page = '" + strCurrentPage + "'";

                                    if (!cbSubpagesAreSeparateFiles.Checked)
                                        strSQL += " AND ParentBinID = " + binID;

                                    using (SQLiteDataAdapter daPackets = new SQLiteDataAdapter(strSQL, conSQLite))
                                    {
                                        daPackets.Fill(dsPackets);
                                    }
                                    // Set the parentBinID to be the same as the highest subpage's parentBinID
                                    DataRow d = dsPackets.Tables[0].Rows[0];
                                    //var bum = d["ParentBinId"];
                                    if (d["ParentBinId"] != null)
                                    {
                                        binID = Convert.ToInt32(d["ParentBinId"]);
                                        //System.Diagnostics.Debug.Write("d[MetaTimeCode]");
                                        //System.Diagnostics.Debug.WriteLine(d["MetaTimeCode"].ToString());
                                        subpageCount = Convert.ToInt32((String)d["MetaTimeCode"].ToString().Replace(":", ""), 16);
                                    }
                                }

                                // If this is a new subpage bump the counter
                                if (lBins.Row == 0)
                                    subpageCount++;



                                if (!cbTimeCodedSubpages.Checked && lBins.Type != LineTypes.Blank)
                                {
                                    strMetaTimeCode = Convert.ToString(subpageCount, 16).PadLeft(4);
                                    strMetaTimeCode = strMetaTimeCode.Substring(0, 2) + ":" + strMetaTimeCode.Substring(2, 2);
                                    //strCurrentPage = lBins.Magazine.ToString() + lBins.Page;
                                }

                                SQLiteCommand scNewPacket = new SQLiteCommand("INSERT INTO BinPackets (Id, ParentBinId, Page, MRAG1, MRAG2, Timecode, MetaTimecode, PacketText, Bytes) VALUES (null, " + binID + ", '" + strCurrentPage + "'," + lBins.MRAG1 + ", " + lBins.MRAG2 + ", $timecode, $metatimecode, $text, $bytes)", conSQLite);
                                scNewPacket.Parameters.AddWithValue("$timecode", lBins.TimeCode);
                                scNewPacket.Parameters.AddWithValue("$metatimecode", strMetaTimeCode);
                                scNewPacket.Parameters.AddWithValue("$text", lBins.Text);
                                scNewPacket.Parameters.AddWithValue("$bytes", lBins.Bytes);
                                scNewPacket.ExecuteNonQuery();
                                //System.Diagnostics.Debug.WriteLine("Adding to DB: {0}", strCurrentPage);



                            }
                        }
                    }
                }
            }
        }

        private SQLiteConnection OpenDatabase()
        {
            // Open DB
            SQLiteConnection conSQLite = null;
            try
            {
                //conSQLite = new SQLiteConnection("Data Source = D:\\My Documents\\SkyDrive\\Documents\\Teletext\\TeletextRecoveryEditor.sqlite; Version = 3; ");
                conSQLite = new SQLiteConnection("Data Source = " + AppContext.BaseDirectory + "\\TeletextRecoveryEditor.sqlite; Version = 3; ");
                conSQLite.Open();
            }
            catch
            {
                MessageBox.Show("Unable to open database.", "Teletext Recovery Editor", MessageBoxButtons.OK);
            }

            if (cbClearDB.Checked && conSQLite != null)
            {
                SQLiteCommand scExecute = new SQLiteCommand("DELETE FROM Bins", conSQLite);
                scExecute.ExecuteNonQuery();
                SQLiteCommand scExecute1 = new SQLiteCommand("DELETE FROM BinPackets", conSQLite);
                scExecute1.ExecuteNonQuery();
                cbClearDB.Checked = false;
                MessageBox.Show("Database cleared.");
            }

            return conSQLite;
        }

        private void AddSubpageTimecodesToBins(Service sourceT42, SQLiteConnection conSQLite)
        {
            CurrentPageInfo[] CurrentPages = new CurrentPageInfo[9];
            for (Int32 n = 0; n < 9; n++)
                CurrentPages[n] = new CurrentPageInfo();

            // Get next header
            Line lT42;
            Int32 ccount = 0;

            Boolean debugPackets = false;
            String currentPage = "";

            do
            {
                lT42 = sourceT42.GetNextLine();

                if (lT42.Type == null)
                    lT42 = new Line();


                Byte bytMRAG1 = lT42.MRAG1;
                Byte bytMRAG2 = lT42.MRAG2;
                Int32 intMagazine = lT42.Magazine;

                // If the VBI line was not a teletext line, copy the T42 as is from the source file
                Boolean blnCopyAsIs = lT42.Type == LineTypes.Blank ? true : false;
                String copyAsIsReason = blnCopyAsIs == true ? "blank line" : "";

                if (lT42.Row == 0 && !blnCopyAsIs)
                {
                    // Get search terms from header packet

                    String strPage = intMagazine.ToString() + lT42.Page;
                    String strTimeCode = lT42.TimeCode;


                    // Store these so we know where we are in an interleaved service
                    CurrentPages[intMagazine].Page = strPage;
                    CurrentPages[intMagazine].TimeCode = strTimeCode;
                    CurrentPages[intMagazine].MRAG1 = bytMRAG1;
                    CurrentPages[intMagazine].MRAG2 = bytMRAG2;

                    currentPage = strPage;
                }


                //---------------------------------------------------
                // Find packet in DB

                if (lT42.Row > 24 && !blnCopyAsIs)
                {
                    blnCopyAsIs = true;
                    copyAsIsReason = "Data packet";
                }

                if (currentPage != tbMergeOnlyPage.Text && tbMergeOnlyPage.Text != "" && !blnCopyAsIs)
                {
                    blnCopyAsIs = true;
                    copyAsIsReason = "Only one page is being merged";
                }

                if (!blnCopyAsIs)
                {
                    if (CurrentPages[intMagazine].Page != null)
                    {
                        DataSet dsPackets = new DataSet();
                        DataTable dtPackets = new DataTable();

                        String strSQL = "";
                        Int32 bestRow = 0;

                        // Get all the packets for this subpage

                        // Pass 1 - do our best to match the packets from the source with the binaries.  Lines with not much in them, or very noisy lines, are
                        // susceptible to having the wrong packet associated with them.  A two-pass model means that it has an attempt at matching during Pass 1,
                        // and Pass 2 checks to see which page in the bin carousel is the most popular and uses that to draw all packets for that subpage, thus
                        // getting rid of any outliers.

                        // fetch all examples of the row / page combination from the DB, i.e. the row from all sub pages
                        strSQL = "SELECT BinPackets.* FROM BinPackets " +
                            //"JOIN bins on bins.BinPath LIKE '" + tbBinariesFolder.Text + "%' " +
                            "JOIN bins ON bins.ID  = BinPackets.ParentBinId " +
                            "WHERE 1=1 " +
                            "AND Page = '" + CurrentPages[intMagazine].Page + "' " +
                            "AND MRAG1 = " + bytMRAG1 + " " +
                            "AND MRAG2 = " + bytMRAG2;

                        using (SQLiteDataAdapter daPackets = new SQLiteDataAdapter(strSQL, conSQLite))
                        {
                            daPackets.Fill(dsPackets);
                        }

                        // Set up an array to record the match scores
                        Int32[] matchScores = new Int32[dsPackets.Tables[0].Rows.Count];

                        // Go through all the packets and score them on a byte-by-byte basis
                        Int32 rowIndex = 0;
                        foreach (DataRow d in dsPackets.Tables[0].Rows)
                        {
                            // Put this DB row into an array
                            Byte[] bytFromDb = new Byte[42];
                            Array.Copy((Byte[])d["Bytes"], 0, bytFromDb, 0, ((Byte[])d["Bytes"]).Length);

                            Int32 byteIndex = 0;
                            Int32 score = 0;
                            foreach (Byte b in lT42.Bytes)
                            {
                                //System.Diagnostics.Debug.WriteLine("comparing {0} with {1}", b, bytFromDb[byteIndex]);
                                if (b == bytFromDb[byteIndex])
                                    score++;
                                byteIndex++;
                            }
                            matchScores[rowIndex] = score;
                            //System.Diagnostics.Debug.WriteLine("Row {1} score: {0}\n", score, rowIndex);
                            rowIndex++;
                        }

                        // See which row has the highest score

                        Int32 bestScore = 0;
                        for (Int32 n = 0; n < dsPackets.Tables[0].Rows.Count; n++)
                        {
                            if (matchScores[n] > bestScore)
                            {
                                bestRow = n;
                                bestScore = matchScores[n];
                            }
                        }
                        if (matchScores.Length > 0)
                        {
                            //CurrentPages[intMagazine].SubpagesUsed.Add(Convert.ToString(dsPackets.Tables[0].Rows[bestRow]["TimeCode"]));
                            strSQL = "UPDATE binPackets SET Meta_TimeCode = '" + dsPackets.Tables[0].Rows[bestRow]["TimeCode"] + "' WHERE id = '" + dsPackets.Tables[0].Rows[bestRow]["Id"] + "'";
                            SQLiteDataAdapter daUpdate = new SQLiteDataAdapter(strSQL, conSQLite);
                            //System.Diagnostics.Debug.WriteLine("Page {0}, subpage {1}, bestRow {2}, bestScore {3}", dsPackets.Tables[0].Rows[bestRow]["Page"], dsPackets.Tables[0].Rows[bestRow]["TimeCode"], bestRow, bestScore);
                        }


                        ccount++;
                    }
                }
            }
            while (sourceT42.Revolutions == 0);
                
        }

        private Byte[] FixClock(Byte[] headerBytes)
        {
            // See which magazine this is
            Service tempService = new Service();
            Hashtable mrag = tempService.ConvertMRAG(headerBytes[0], headerBytes[1]);
            Int32 mag = Convert.ToInt32(mrag["Magazine"]);
            tempService = null;

            String clockText = "";
            String clockOut = "";

            for (Byte b = 34; b < 42; b++)
                clockText += Convert.ToChar(headerBytes[b] & 0x7f);



            // Force the separators in
            if (fixClockTemplate.Contains("1"))
                clockText = clockText.Substring(0, fixClockTemplate.IndexOf("1")) + fixClockLimits.Substring(fixClockTemplate.IndexOf("1"), 1) + clockText.Substring(fixClockTemplate.IndexOf("1") + 1);

            if (fixClockTemplate.Contains("2"))
                clockText = clockText.Substring(0, fixClockTemplate.IndexOf("2")) + fixClockLimits.Substring(fixClockTemplate.IndexOf("2"), 1) + clockText.Substring(fixClockTemplate.IndexOf("2") + 1);


            // Early clocks used letter O instead of zeroes - replace them for processing and put them back later
            Boolean zeroesAreLetterOs = false;
            if (clockText.IndexOf("O") > -1)
            {
                clockText = clockText.Replace("O", "0");
                zeroesAreLetterOs = true;
            }

            clockOut = clockText;

            



            String SU = clockText.Substring(fixClockTemplate.IndexOf("s"), 1);
            String lsSU = lastSaneSS[mag].Substring(1, 1);
            String ST = clockText.Substring(fixClockTemplate.IndexOf("S"), 1);
            String lsST = lastSaneSS[mag].Substring(0, 1);
            String MU = clockText.Substring(fixClockTemplate.IndexOf("m"), 1);
            String lsMU = lastSaneMM[mag].Substring(1, 1);
            String MT = clockText.Substring(fixClockTemplate.IndexOf("M"), 1);
            String lsMT = lastSaneMM[mag].Substring(0, 1);
            String HU = clockText.Substring(fixClockTemplate.IndexOf("h"), 1);
            String lsHU = lastSaneHH[mag].Substring(1, 1);

            //Seconds Units

            if (Char.IsDigit(Convert.ToChar(SU)) ? 
                Convert.ToInt32(clockText.Substring(fixClockTemplate.IndexOf("s"), 1)) <= Convert.ToInt32(fixClockLimits.Substring(fixClockTemplate.IndexOf("s"), 1)) 
                    && ((Char.IsDigit(Convert.ToChar(lsSU)) ? Convert.ToInt32(SU) > Convert.ToInt32(lsSU) : false)
                    || (SU == "0" && lsSU == "9") 
                    || lsSU == "x") 
                : false)
            {
                if (lsSU == "x" & SU == "9")
                    latchSU[mag] = true;

                clockOut = clockOut.Substring(0, fixClockTemplate.IndexOf("s")) + SU + clockOut.Substring(fixClockTemplate.IndexOf("s") + 1);
                lastSaneSS[mag] = lsST + SU;
                lsSU = SU;

                // Set the latch to change ST only if we are sufficiently far enough through the cycle
                if (Convert.ToInt32(SU) >= 7)
                    latchSU[mag] = true;

                //clockOut = clockOut.Substring(0, template.IndexOf("s")) + lsSU + clockOut.Substring(template.IndexOf("s") + 1);
            }
            else
                clockOut = clockOut.Substring(0, fixClockTemplate.IndexOf("s")) + lsSU + clockOut.Substring(fixClockTemplate.IndexOf("s") + 1);


            // Seconds Tens
            if ((Char.IsDigit(Convert.ToChar(ST)) ?
                    Convert.ToInt32(clockText.Substring(fixClockTemplate.IndexOf("S"), 1)) <= Convert.ToInt32(fixClockLimits.Substring(fixClockTemplate.IndexOf("S"), 1))
                    && (((Char.IsDigit(Convert.ToChar(lsST)) ? (Convert.ToInt32(ST) - Convert.ToInt32(lsST) == 1) : false)
                        || (ST == "0" && lsST == "5"))
                        && lsSU == "0"
                        && latchSU[mag]
                        )
                        || lsST == "x"
                    : false)
                )
            {
                if (lsST == "x" && ST == "5")
                    latchST[mag] = true;

                clockOut = clockOut.Substring(0, fixClockTemplate.IndexOf("S")) + ST + clockOut.Substring(fixClockTemplate.IndexOf("S") + 1);
                lastSaneSS[mag] = ST + lastSaneSS[mag].Substring(1, 1);
                if (lsST != "x")
                    latchSU[mag] = false;
                lsST = ST;

                // Set the latch to change MU only if we are sufficiently far enough through the cycle
                if (Convert.ToInt32(ST) >= 5)
                {
                    latchST[mag] = true;
                }
            }
            else
                clockOut = clockOut.Substring(0, fixClockTemplate.IndexOf("S")) + lsST + clockOut.Substring(fixClockTemplate.IndexOf("S") + 1);

        
            //Minutes Units
            if ((Char.IsDigit(Convert.ToChar(MU)) ?
                    Convert.ToInt32(clockText.Substring(fixClockTemplate.IndexOf("m"), 1)) <= Convert.ToInt32(fixClockLimits.Substring(fixClockTemplate.IndexOf("m"), 1))
                    && (((Char.IsDigit(Convert.ToChar(lsMU)) ? (Convert.ToInt32(MU) - Convert.ToInt32(lsMU) == 1) : false)
                        || (MU == "0" && lsMU == "9"))
                        && lastSaneSS[mag] == "00"
                        && latchST[mag]
                        )
                        || lsMU == "x"
                : false)
                )
            {

                if (lsMU == "x" && MU == "9")
                    latchMU[mag] = true;

                clockOut = clockOut.Substring(0, fixClockTemplate.IndexOf("m")) + clockText.Substring(fixClockTemplate.IndexOf("m"), 1) + clockOut.Substring(fixClockTemplate.IndexOf("m") + 1);
                lastSaneMM[mag] = lastSaneMM[mag].Substring(0, 1) + MU; 
                if (lsMU != "x")
                    latchST[mag] = false;
                lsMU = MU;

                // Set the latch to change MT only if we are sufficiently far enough through the cycle
                if (Convert.ToInt32(MU) >= 7)
                {
                    latchMU[mag] = true;
                }
            }
            else
                clockOut = clockOut.Substring(0, fixClockTemplate.IndexOf("m")) + lastSaneMM[mag].Substring(1, 1) + clockOut.Substring(fixClockTemplate.IndexOf("m") + 1);

            // Minutes Tens
            if (Char.IsDigit(Convert.ToChar(MT)) ?
                Convert.ToInt32(clockText.Substring(fixClockTemplate.IndexOf("M"), 1)) <= Convert.ToInt32(fixClockLimits.Substring(fixClockTemplate.IndexOf("M"), 1))
                && (((Char.IsDigit(Convert.ToChar(lsMT)) ? (Convert.ToInt32(MT) - Convert.ToInt32(lsMT) == 1) : false)
                || (MT == "0" && lsMT == "5")) 
                && lastSaneSS[mag] == "00"
                && latchMU[mag])
                || lsMT == "x"
                : false)
            {
                if (lsMT == "x" && MT == "5")
                    latchMT[mag] = true;

                clockOut = clockOut.Substring(0, fixClockTemplate.IndexOf("M")) + MT + clockOut.Substring(fixClockTemplate.IndexOf("M") + 1);
                lastSaneMM[mag] = MT + lastSaneMM[mag].Substring(1, 1);
                if (lsMT != "x")
                    latchMU[mag] = false;
                lsMT = MT;
 
                // Set the latch to change HU only if we are sufficiently far enough through the cycle
                if (Convert.ToInt32(MT) >= 5)
                {
                    latchMT[mag] = true;
                }
            }
            else
                clockOut = clockOut.Substring(0, fixClockTemplate.IndexOf("M")) + lastSaneMM[mag].Substring(0, 1) + clockOut.Substring(fixClockTemplate.IndexOf("M") + 1);

            // Hours Units
            if (char.IsDigit(Convert.ToChar(HU)) ?
                Convert.ToInt32(clockText.Substring(fixClockTemplate.IndexOf("h"), 1)) <= Convert.ToInt32(fixClockLimits.Substring(fixClockTemplate.IndexOf("h"), 1))
                && (((Char.IsDigit(Convert.ToChar(lsHU)) ? (Convert.ToInt32(HU) - Convert.ToInt32(lsHU) == 1) : false)
                || (HU == "0" && lsHU == "9"))
                && lastSaneMM[mag] == "00"
                && latchMT[mag])
                || lsHU == "x"
                : false)
            {
                clockOut = clockOut.Substring(0, fixClockTemplate.IndexOf("h")) + HU + clockOut.Substring(fixClockTemplate.IndexOf("h") + 1);
                lastSaneHH[mag] = lastSaneHH[mag].Substring(0, 1) + HU;
                if (lsHU !="x")
                    latchMT[mag] = false;
                lsHU = HU;

                // Set the latch to change HT only if we are sufficiently far enough through the cycle
                if (Convert.ToInt32(HU) >= 5)
                {
                    latchHU[mag] = true;
                }
            }
            else
                clockOut = clockOut.Substring(0, fixClockTemplate.IndexOf("h")) + lastSaneHH[mag].Substring(1, 1) + clockOut.Substring(fixClockTemplate.IndexOf("h") + 1);

            // Hours tens
            String HT = clockText.Substring(fixClockTemplate.IndexOf("H"), 1);
            String lsHT = lastSaneHH[mag].Substring(0, 1);
            if (Char.IsDigit(Convert.ToChar(HT)) ? Convert.ToInt32(clockText.Substring(fixClockTemplate.IndexOf("H"), 1)) <= Convert.ToInt32(fixClockLimits.Substring(fixClockTemplate.IndexOf("H"), 1))
                && (((Char.IsDigit(Convert.ToChar(lsHT)) ? (Convert.ToInt32(HT) - Convert.ToInt32(lastSaneHH[mag].Substring(0, 1)) == 1) : false)
                    || (HT == "0" && lsHT == "1"))
                    && lastSaneMM[mag] == "00"
                    && latchHU[mag])
                    || lsHT == "x"
                : false)
            {
                clockOut = HT + clockOut.Substring(fixClockTemplate.IndexOf("h"));
                lastSaneHH[mag] = HT + lastSaneHH[mag].Substring(1, 1);
                if (lsHT != "x")
                    latchHU[mag] = false;
                lsHT = HT;
            }
            else
                clockOut = clockOut.Substring(0, fixClockTemplate.IndexOf("H")) + lastSaneHH[mag].Substring(0, 1) + clockOut.Substring(fixClockTemplate.IndexOf("H") + 1);

            if (mag == 2)
            stLabel.Text = "Merging.... " + clockOut;

            // Put letter Os back if clock used them instead of zeroes
            if (zeroesAreLetterOs)
                clockOut = clockOut.Replace("0", "O");

            //if (mag == 1)
            //    System.Diagnostics.Debug.WriteLine("In: {0}   Out: {1}", clockText, clockOut);

            Array.Copy(Encoding.ASCII.GetBytes(clockOut), 0, headerBytes, 34, clockOut.Length);
            return headerBytes;
        }

        private byte[] FixHeader(Byte[] headerBytes)
        {

            // See which magazine this is
            Service tempService = new Service();
            Hashtable mrag = tempService.ConvertMRAG(headerBytes[0], headerBytes[1]);
            Int32 mag = Convert.ToInt32(mrag["Magazine"]);
            tempService = null;

            for (Int32 c=10; c<headerBytes.Length; c++)
            {
                headerBytes[c] = (Byte)(headerBytes[c] & 0x7f);
            }

            String headerIn = Encoding.Default.GetString(headerBytes);
            String headerOut = tbHeaderTemplate.Text;

            // Replace control strings for the actual control codes
            Int32 lastCtrlPos = 0;
            Int32 ctrlPos = headerOut.IndexOf("\\", lastCtrlPos);
            while (ctrlPos != -1)
            {
                String ctrlValue = headerOut.Substring(ctrlPos + 4, 2) ;
                if (ctrlValue.Substring(0, 1) == "0")
                    ctrlValue = ctrlValue.Substring(1, 1);

                Byte intCtrlValue = Convert.ToByte(ctrlValue);
                headerOut = headerOut.Substring(0, headerOut.IndexOf("\\u00")) + (Char)intCtrlValue + headerOut.Substring(ctrlPos + 6);

                lastCtrlPos = ctrlPos;
                ctrlPos = headerOut.IndexOf("\\", lastCtrlPos);
            }


            // Fix the mpp in the header - assuption is that the mpp is always the same as the mpp encoded in the hammed header

            // Find out where the mpp is in the template
            Int32 mppStart = headerOut.IndexOf("mpp");

            if (mppStart > 0)
            {
                //Fetch page number from hammed header fields

                Service s = new Service();
                Int32 magazine = mag;
                if (magazine == 0)
                    magazine = 8;

                String pageTensUnits = s.ConvertPageUnitsTens(headerBytes[2], headerBytes[3]);

                String mpp = magazine.ToString() + pageTensUnits;

                // Replace the mpp in the template for the one from the fixed original
                headerOut = headerOut.Substring(0, mppStart) + mpp + headerOut.Substring(mppStart + 3);

            }

            // Process day
            // Is the current value of 'day' sane?  If so, overwrite the current and put into headerOut
            Int32 dayStart = headerOut.IndexOf("DA");
            Int32 dayLength = 3;
            if (headerOut.IndexOf("DAY") == -1)
                dayLength = 2;

            String daysOfTheWeek = "Mon Tue Wed Thu Fri Sat Sun";
            String daySearchTerm = headerIn.Substring(dayStart + 10, dayLength);
            if (daysOfTheWeek.IndexOf(daySearchTerm) != -1)
            {
                lastSaneDay[mag] = daysOfTheWeek.Substring(daysOfTheWeek.IndexOf(daySearchTerm), dayLength);
            }
            headerOut = headerOut.Replace("DAY".Substring(0, dayLength), lastSaneDay[mag].Substring(0, dayLength));


            //  Process date number
            Int32 ddStart = headerOut.IndexOf("DD");

            if (ddStart != -1)
            {
                String dd = headerIn.Substring(ddStart + 10, 2);
                Char ddT = Convert.ToChar(dd.Substring(0, 1));
                if (ddT == 0x20 || (ddT >= 30 && ddT < 0x3a))
                {
                    lastSaneDD[mag] = ddT.ToString() + lastSaneDD[mag].Substring(1, 1);
                }

                Char ddU = Convert.ToChar(dd.Substring(1, 1));
                if (ddU >= 30 && ddU < 0x3a)
                    lastSaneDD[mag] = lastSaneDD[mag].Substring(0, 1) + ddU;
            }
            headerOut = headerOut.Replace("DD", lastSaneDD[mag]);

            // Process month
            // Is the current value of 'day' sane?  If so, overwrite the current and put into headerOut
            Int32 monthStart = headerOut.IndexOf("MTH");

            String monthsOfTheYear = "Jan Feb Mar Apr May Jun Jul Aug Sep Oct Nov Dec";
            String monthSearchTerm = headerIn.Substring(monthStart + 10, 3);
            if (monthsOfTheYear.IndexOf(monthSearchTerm) != -1)
            {
                lastSaneMonth[mag] = monthsOfTheYear.Substring(monthsOfTheYear.IndexOf(monthSearchTerm), 3);
            }
            headerOut = headerOut.Replace("MTH", lastSaneMonth[mag]);

            Array.Copy(Encoding.ASCII.GetBytes(headerOut), 0, headerBytes, 10, headerOut.Length);
            return headerBytes;
        }
        private void TbBinariesFolder_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void TbBinariesFolder_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            tbBinariesFolder.Text = files[0];
        }

        private void TbT42File_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void TbT42File_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            tbT42File.Text = files[0];
        }

        private void TbMergedFile_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void TbMergedFile_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            tbMergedFile.Text = files[0];
        }

        private void TbBinariesFolder_TextChanged(object sender, EventArgs e)
        {
            // Save to registry
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\UniqueCodeAndData\TeletextRecoveryEditor");
            key.SetValue("BinariesFolder", tbBinariesFolder.Text);
            key.Close();
        }

        private void TbClockTemplate_TextChanged(object sender, EventArgs e)
        {
            // Save to registry
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\UniqueCodeAndData\TeletextRecoveryEditor");
            key.SetValue("ClockTemplate", tbClockTemplate.Text);
            key.Close();
        }

        private void TbT42File_TextChanged(object sender, EventArgs e)
        {
            // Save to registry
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\UniqueCodeAndData\TeletextRecoveryEditor");
            key.SetValue("SourceT42", tbT42File.Text);
            key.Close();
        }

        private void TbMergedFile_TextChanged(object sender, EventArgs e)
        {
            // Save to registry
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\UniqueCodeAndData\TeletextRecoveryEditor");
            key.SetValue("DestT42", tbMergedFile.Text);
            key.Close();
        }

        private void TbHeaderTemplate_TextChanged(object sender, EventArgs e)
        {
            // Save to registry
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\UniqueCodeAndData\TeletextRecoveryEditor");
            key.SetValue("HeaderTemplate", tbHeaderTemplate.Text);
            key.Close();
        }

        private void TbForceInitClockDigits_TextChanged(object sender, EventArgs e)
        {
            // Save to registry
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\UniqueCodeAndData\TeletextRecoveryEditor");
            key.SetValue("ForceInitialClockDigits", tbForceInitClockDigits.Text);
            key.Close();
        }

        private void NudLinePadding_ValueChanged(object sender, EventArgs e)
        {
            // Save to registry
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\UniqueCodeAndData\TeletextRecoveryEditor");
            key.SetValue("LinePadding", nudLinePadding.Value);
            key.Close();
        }

        private void TbMergeOnlyPage_TextChanged(object sender, EventArgs e)
        {
            // Save to registry
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\UniqueCodeAndData\TeletextRecoveryEditor");
            key.SetValue("MergeOnly", tbMergeOnlyPage.Text);
            key.Close();
        }

        private void CbThreaded_CheckedChanged(object sender, EventArgs e)
        {
            // Save to registry
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\UniqueCodeAndData\TeletextRecoveryEditor");
            key.SetValue("Threaded", cbThreaded.Checked);
            key.Close();
        }

        private void CbClearDB_CheckedChanged(object sender, EventArgs e)
        {
            // Save to registry
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\UniqueCodeAndData\TeletextRecoveryEditor");
            key.SetValue("ClearEntireDB", cbClearDB.Checked);
            key.Close();
        }

        private void CbSubpagesAreSeparateFiles_CheckedChanged(object sender, EventArgs e)
        {
            // Save to registry
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\UniqueCodeAndData\TeletextRecoveryEditor");
            key.SetValue("SubpagesAreSeparateFiles", cbSubpagesAreSeparateFiles.Checked);
            key.Close();
        }

        private void CbTimeCodedSubpages_CheckedChanged(object sender, EventArgs e)
        {
            // Save to registry
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\UniqueCodeAndData\TeletextRecoveryEditor");
            key.SetValue("TimecodedSubpages", cbTimeCodedSubpages.Checked);
            key.Close();
        }

        private void CbFixHeader_CheckedChanged(object sender, EventArgs e)
        {
            // Save to registry
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\UniqueCodeAndData\TeletextRecoveryEditor");
            key.SetValue("FixHeader", cbFixHeader.Checked);
            key.Close();
        }

        private void CbFixClock_CheckedChanged(object sender, EventArgs e)
        {
            // Save to registry
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\UniqueCodeAndData\TeletextRecoveryEditor");
            key.SetValue("FixClock", cbFixClock.Checked);
            key.Close();
        }
    }

    public class CurrentPageInfo
    {
        public String Page;
        public String TimeCode;
        public Byte MRAG1;
        public Byte MRAG2;
        public List<String> SubpagesUsed;

        public CurrentPageInfo()
        {
            this.SubpagesUsed = new List<String>();
            this.Page = "";
            this.TimeCode = "";
            this.MRAG1 = 0;
            this.MRAG2 = 0;
        }

        public String MostFrequentSubpage()
        {
            return SubpagesUsed.GroupBy(i => i).OrderByDescending(grp => grp.Count()).Select(grp => grp.Key).First();
        }
    }

    public struct MetaLine
    {
        public Int32 Index;
        public String Page;
        public Int32 Packet;
        public Byte[] Bytes;
        public String TimeCode;
        public String MetaTimeCode;
    }
}


