using System.Collections;
using TeletextSharedResources;
using LineTypes = TeletextSharedResources.LineTypes;

namespace Teletext
{
    class TrimCarousel
    {
        public Hashtable Trim(Hashtable thisCarousel)
        {
            List<SubPageMap> subpageMap = new List<SubPageMap>();

            // Loop all subpages and assign a subpage code to them
            foreach (DictionaryEntry d in thisCarousel)
            {
                System.Diagnostics.Debug.WriteLine("Subpage key: " + d.Key);
                Page thisPage = (Page)thisCarousel[d.Key];
                // Go through each packet and see if it compares to the same row on other pages
                for (Int32 row = 1; row < 25; row++)
                {
                    Line thisLine = thisPage.GetRow(row);
                    if (thisLine.Bytes != null)
                    {
                        foreach (DictionaryEntry candidateEntry in thisCarousel)
                        {
                            if (candidateEntry.Key != d.Key)
                            {
                                Page candidatePage = (Page)thisCarousel[candidateEntry.Key];
                                Line candidateLine = candidatePage.GetRow(row);

                                // Check to see if this line matches the candidate - score it and see if it passes a threshold
                                if (candidateLine.Bytes != null)
                                {
                                    Int32 byteIndex = 0;
                                    Int32 score = 0;
                                    foreach (Byte b in thisLine.Bytes)
                                    {
                                        //System.Diagnostics.Debug.WriteLine("comparing {0} with {1}", b, bytFromDb[byteIndex]);
                                        if (b == candidateLine.Bytes[byteIndex])
                                            score++;
                                        byteIndex++;
                                    }
                                    if (score > 35)
                                    {
                                        subpageMap.Add(new SubPageMap((Int32)d.Key, row, (Int32)candidateEntry.Key, 0));
                                        System.Diagnostics.Debug.WriteLine("This subpage: {0}, row: {1} has a candidate matching row on subpage: {2}", (Int32)d.Key, row, (Int32)candidateEntry.Key, 0);
                                        System.Diagnostics.Debug.Write("");
                                    }

                                }
                            }
                        }
                    }
                }
            }

            Int32 subPageIndex = 1;
            Hashtable recoveriesSubPages = (Hashtable)thisCarousel.Clone();

            foreach (DictionaryEntry pageEntry in thisCarousel)
            {
                var subPage = subpageMap.Where(i => i.Key == (Int32)pageEntry.Key).Select(s => s);
                Page p = (Page)recoveriesSubPages[pageEntry.Key];
                if (p.Subpage == 0 || true)
                {

                    var subPageGroups = subPage.GroupBy(i => i.AlsoInKey)
                       .Select(grp => new subPageMeta
                       {
                           Key = grp.Key,
                           Count = grp.Count(),
                           Percentage = 0
                       }).ToArray();


                    // Go through each page and compare the number of rows that match with the number of rows in the subpage
                    foreach (DictionaryEntry d in thisCarousel)
                    {
                        Page r = (Page)thisCarousel[d.Key];

                        Int32 numRows = 0;
                        for (Int32 row = 1; row < 25; row++)
                        {
                            Line thisLine = r.GetRow(row);
                            if (thisLine.Bytes != null)
                                numRows++;
                        }

                        // find the key in the groups array and add the %age match rate
                        for (Int32 n = 0; n < subPageGroups.Length; n++)
                        {
                            var element = subPageGroups[n];
                            if (element.Key == (Int32)d.Key)
                                subPageGroups[n].Percentage = (Int32)((Double)element.Count / (Double)numRows * 100);
                        }
                    }

                    // Give this page a subpage ID
                    Page thisPage = (Page)recoveriesSubPages[pageEntry.Key];
                    thisPage.Subpage = subPageIndex;
                    recoveriesSubPages[pageEntry.Key] = thisPage;

                    System.Diagnostics.Debug.WriteLine("\n\nWhich subpages match this subpage (" + pageEntry.Key + ")?:");
                    // See which subpages are the same as this one
                    foreach (subPageMeta s in subPageGroups)
                    {
                        if (s.Percentage > 92)
                        {
                            Page q = (Page)recoveriesSubPages[s.Key];
                            q.Subpage = subPageIndex;
                            recoveriesSubPages[s.Key] = q;
                            System.Diagnostics.Debug.WriteLine("Match at {0}%: {1}", s.Percentage, s.Key);
                        }
                        else
                            System.Diagnostics.Debug.WriteLine("No Match at {0}%: {1}", s.Percentage, s.Key );

                    }




                    subPageIndex++;


                }
            }

                    // Debug stuff
                    //System.Diagnostics.Debug.WriteLine("This key: {0}", pageEntry.Key);
                    foreach (DictionaryEntry x in recoveriesSubPages)
                    {
                        Page y = (Page)recoveriesSubPages[x.Key];
                        System.Diagnostics.Debug.WriteLine("Key: {0}, subpage {1}", x.Key, y.Subpage);
                    }

            thisCarousel = recoveriesSubPages;

            Hashtable newRecoveries = new Hashtable();
            Int32 newKey = 0;

            // For each subpage ID, see which page that has been allocated this ID has the highest number of packets
            for (Int32 subpage = 0; subpage < subPageIndex; subpage++)
            {
                Int32 bigPacket = 0;
                foreach (DictionaryEntry d in thisCarousel)
                {
                    Page p = (Page)thisCarousel[d.Key];
                    if (p.Subpage == subpage)
                    {
                        Int32 numPackets = 0;
                        for (Int32 n = 0; n < 255; n++)
                        {
                            if (p.Lines[n].Type != LineTypes.Blank && p.Lines[n].Bytes != null)
                            {
                                if (p.Lines[n].Row < 25)
                                {
                                    numPackets++;
                                }
                            }
                        }
                        if (numPackets > bigPacket)
                            bigPacket = numPackets;
                    }
                }

                System.Diagnostics.Debug.WriteLine("Subpage {0} - one or more of the pages has the largest packet count, which is {1}\n", subpage, bigPacket);


                // Repeat the loop and copy only those pages which have the same number of packets as bigPacket
                foreach (DictionaryEntry d in thisCarousel)
                {
                    Page p = (Page)thisCarousel[d.Key];
                    if (p.Subpage == subpage)
                    {
                        Int32 numPackets = 0;
                        for (Int32 n = 0; n < 255; n++)
                        {
                            if (p.Lines[n].Type != LineTypes.Blank && p.Lines[n].Bytes != null)
                            {
                                if (p.Lines[n].Row < 25)
                                {
                                    numPackets++;
                                }
                            }
                        }

                        System.Diagnostics.Debug.WriteLine("Subpage {0} with key {1} contained {2} packets.", subpage, d.Key, numPackets); 

                        if (numPackets == bigPacket)
                        {
                            newRecoveries.Add(newKey, thisCarousel[d.Key]);
                            System.Diagnostics.Debug.WriteLine("Added subpage with key {0} which contains {1} packets.", d.Key, numPackets);
                            newKey++;
                        }
                    }
                }






                /*
                            // Copy recoveries array to another array, omitting the key if in the selecteditems list

                            foreach (ListViewItem l in lvThumbnails.Items)
                            {
                                // is this item in the selected list?
                                Boolean selected = false;
                                foreach (ListViewItem m in lvThumbnails.SelectedItems)
                                {
                                    if (l.ImageKey == m.ImageKey)
                                        selected = true;


                                    // if not, copy to newRecoveries
                                    if (selected)
                                    {
                                        //newRecoveries.Add(l.Index, (Page)recoveries[l]);
                                        newRecoveries.Remove(Convert.ToInt32(l.ImageKey));
                                    }
                                }
                            }
                            */

            }

            return newRecoveries;
        }


    }
}
