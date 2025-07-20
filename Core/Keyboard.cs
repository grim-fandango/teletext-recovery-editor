using System.Text;
using System.Text.RegularExpressions;
using TeletextSharedResources;

namespace Teletext
{
    public partial class TeletextRecoveryEditor : Form
    {
        private void TeletextRecoveryEditor_KeyDown(object sender, KeyEventArgs e)
        {
            //Page beforeChangesPage = new Page(formPage.Lines, formPage.modeMap, formPage.modeMapL2, formPage.Subpage);
            Page beforeChangesPage = new Page();
            if (formPage != null)
                if (formPage.Lines[0].Bytes != null)
                    beforeChangesPage = new Page(formPage.Lines, formPage.modeMap, formPage.modeMapL2, formPage.Subpage);

            bool codeAdded = false;

            if (!tbCursorHex.Focused && !tbTimeCode.Focused && !tbPage.Focused && !tbPosition.Focused && e.KeyCode.ToString() != "ShiftKey" && e.KeyCode.ToString() != "ControlKey" && formPage != null)
            {
                e.Handled = true;
                System.Diagnostics.Debug.Print("KeyEventArgs: KeyCode=" + e.KeyCode.ToString() + " KeyData=" + e.KeyData + " KeyValue=0x" + e.KeyValue.ToString("X") + " Modifiers=" + e.Modifiers);

                Int32 ascii = KeyCodeToASCII(e.KeyCode.ToString(), (Int32)e.KeyValue, e.Modifiers.ToString());

                if (ascii >= 0x20 && ascii <= 0x7f && formPage.Lines[0].Type != LineTypes.Blank)
                    //if (ascii >= 0x20 && ascii <= 0x7f && e.Modifiers.ToString() != "Control" && formPage.Lines[0].Type != LineTypes.Blank)
                    // Set the undo page
                    undoPage = beforeChangesPage.Clone();

                // get coordinates of cursor
                Byte bytPktPosnInArray = 255;// = charMap.cursorY;
                Byte bytColumn = charMap.cursorX;

                Line lineEdited = new Line();

                Boolean found = false;

                // Run through all lines in the page and see if any of them match the row we want to paste the current line into.
                // If so, great, we can overwrite that one.
                for (Int32 i = 0; (i < 256 && !found); i++)
                {
                    Line l = formPage.Lines[i];
                    if (l.Row == charMap.cursorY && !found)
                    {
                        bytPktPosnInArray = (Byte)i;
                        found = true;
                        lineEdited = l;
                    }
                }

                if (!found)
                {
                    //Check to see if the packet that is the same as the row number is free, if so create packet there.  If not find the next available space
                    if (formPage.Lines[charMap.cursorY].Type == LineTypes.Blank)
                    {
                        createPacket((Byte)(charMap.cursorY), (Byte)(charMap.cursorY), "".PadRight(charMap.CharWidth, (char)0x20));
                        bytPktPosnInArray = (Byte)(charMap.cursorY);
                        found = true;
                        lineEdited = formPage.Lines[bytPktPosnInArray];
                        UpdateHexUnderCursor();
                    }
                    else
                    {

                        int freeLine = -1;
                        for (Int32 i = 0; i < 256 && freeLine == -1; i++)
                        {
                            Line l2 = formPage.Lines[i];
                            if (l2.Type == LineTypes.Blank)
                            {
                                freeLine = i;
                            }
                        }
                        // Create a free line, but not if we are within a T42 service
                        if (freeLine >= 0)
                        {
                            if (service.ServiceType != ServiceType.Service)
                            {
                                createPacket((Byte)(freeLine), (Byte)(charMap.cursorY), "".PadRight(charMap.CharWidth, (char)0x20));
                                bytPktPosnInArray = (byte)freeLine;
                                lineEdited = formPage.Lines[freeLine];
                                found = true;
                            }
                            else
                            {
                                MessageBox.Show("Unable to insert a new line into a T42 service as this would change the file length and change the timing.", "Teletext Recovery Editor", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                            }
                        }
                    }
                }

                // create packet if one doesn't exist
                //strLine = formPage.Lines[bytPacket].Text;
                //String strLine = null;
                /*if (lineEdited.MRAG1 == 0)
                {
                    Boolean result = createPacket(charMap.cursorY, charMap.cursorY,"");
                    if (!result)
                        MessageBox.Show("There are no free elements in which to insert a new packet.", "Teletext Stream Editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //lineEdited.Row = charMap.cursorY;
                    System.Diagnostics.Debug.Print("");
                }*/



                switch (e.KeyValue)
                {
                    case 8:
                        // Backspace
                        if (bytColumn > 0)
                        {
                            formPage.Lines[bytPktPosnInArray].Text = lineEdited.Text.Substring(0, bytColumn - 1) + " " + lineEdited.Text.Substring(bytColumn);
                            //System.Diagnostics.Debug.Print(formPage.Lines[bytPacket].Text);
                            charMap.cursorX--;
                        }
                        changesMade = true; redrawThumbnails = true;
                        break;
                    case 46:
                        // Delete key
                        formPage.Lines[bytPktPosnInArray].Text = lineEdited.Text.Substring(0, bytColumn) + lineEdited.Text.Substring(bytColumn + 1) + " ";
                        Array.Copy(Encoding.ASCII.GetBytes(formPage.Lines[bytPktPosnInArray].Text), 0, formPage.Lines[bytPktPosnInArray].Bytes, 2, formPage.Lines[bytPktPosnInArray].Text.Length);
                        changesMade = true; redrawThumbnails = true;
                        ascii = -1;
                        break;
                    case 45:
                        // Insert key
                        formPage.Lines[bytPktPosnInArray].Text = (lineEdited.Text.Substring(0, bytColumn) + " " + lineEdited.Text.Substring(bytColumn)).Substring(0, 40);
                        Array.Copy(Encoding.ASCII.GetBytes(formPage.Lines[bytPktPosnInArray].Text), 0, formPage.Lines[bytPktPosnInArray].Bytes, 2, formPage.Lines[bytPktPosnInArray].Text.Length);
                        changesMade = true; redrawThumbnails = true;
                        ascii = -1;
                        break;
                    default:
                        break;
                }

                // Normal Characters to print
                if (ascii >= 0x20 && ascii <= 0x7f && e.Modifiers.ToString() != "Control" &&
                    lineEdited.Type != LineTypes.Unknown && lineEdited.Text != "")
                {
                    if (Control.IsKeyLocked(Keys.CapsLock) && ascii >= 97 && ascii <= 122)
                    {
                        ascii -= 32;
                    }

                    // place character in the page object
                    if (lineEdited.Text.Length < 40)
                        lineEdited.Text = lineEdited.Text.PadRight(40, Convert.ToChar(" "));

                    formPage.Lines[bytPktPosnInArray].Text = lineEdited.Text.Substring(0, bytColumn) + (Char)ascii + lineEdited.Text.Substring(bytColumn + 1);
                    TeletextTools tools = new TeletextTools();
                    formPage.Lines[bytPktPosnInArray].Bytes[bytColumn + 2] = tools.CalcParity((Byte)ascii);
                    changesMade = true; redrawThumbnails = true;

                    // Move cursor if not graphics.  if graphics, just redraw
                    if (!(charMap.GraphicsCursor && e.KeyCode.ToString().Contains("NumPad")))
                    {
                        if (charMap.cursorY < 24 && charMap.cursorX > 38)
                        {
                            charMap.cursorX = 0;
                            if (charMap.cursorY < 24)
                                charMap.cursorY++;
                        }
                        else
                            if (charMap.cursorY < 25 && charMap.cursorX < 39)
                            charMap.cursorX++;
                    }
                    else
                        charMap.MoveCursor();
                }
                else
                    System.Diagnostics.Debug.Print("Not printed: " + ascii + " " + (Char)ascii);



                //Deal with the control key functions
                if (e.KeyValue >= 0x20 && e.KeyValue <= 0x7f && e.Modifiers.ToString().Contains("Control"))
                {
                    String clipText = "";
                    switch (e.KeyValue)
                    {
                        case 0x4c:
                            // CTRL-L
                            if (bytPktPosnInArray != 255)
                            {
                                Clipboard.SetText(formPage.Lines[bytPktPosnInArray].Text.Replace((char)0x00, (char)0x20));
                            }
                            break;
                        case 0x5a:
                            //CTRL-Z
                            //formPage = new Page();
                            formPage = new Page(undoPage.Lines, undoPage.modeMap, undoPage.modeMapL2, undoPage.Subpage);

                            break;
                        case 0x56:
                            //CTRL-V
                            undoPage = beforeChangesPage.Clone();
                            clipText = Clipboard.GetText();
                            if (clipText.Contains("\r\n") || true)
                            //Paste box
                            {
                                //char[] newlineSeparator = System.Environment.NewLine.ToCharArray();
                                //clipText = clipText.Replace("\r\n", "\uffff");
                                //String[] clipLines = clipText.Split(newlineSeparator);
                                String[] clipLines = Regex.Split(clipText, "\r\n");
                                Byte yPos = 0;

                                Byte bytPktPosnInArray_copy = bytPktPosnInArray;
                                foreach (String l in clipLines)
                                {
                                    if (l != "")
                                    {

                                        found = false;

                                        // Run through all lines in the page and see if any of them match the row we want to paste the current line into.
                                        // If so, great, we can overwrite that one.
                                        for (Int32 i = 0; (i < 255 && !found); i++)
                                        {
                                            Line l2 = formPage.Lines[i];
                                            if (l2.Row == charMap.cursorY + yPos && !found)
                                            {
                                                bytPktPosnInArray = (Byte)i;
                                                found = true;

                                            }
                                        }

                                        if (!found)
                                        {

                                            //Check to see if the packet that is the same as the row number is free, if so create packet there.  If not find the next available space
                                            if (formPage.Lines[charMap.cursorY + yPos].Type == LineTypes.Blank)
                                            {
                                                bytPktPosnInArray = (Byte)(charMap.cursorY + yPos);
                                                createPacket(bytPktPosnInArray, (Byte)(charMap.cursorY + yPos), l.PadRight(charMap.CharWidth, (char)0x20));
                                                found = true;
                                            }
                                            else
                                            {

                                                int freeLine = -1;
                                                for (Int32 i = 0; i < 256 && i != -1; i++)
                                                {
                                                    Line l2 = formPage.Lines[i];
                                                    if (l2.Type == LineTypes.Blank)
                                                    {
                                                        freeLine = i;
                                                        i = -2;
                                                    }
                                                }
                                                if (freeLine >= 0)
                                                {
                                                    createPacket((Byte)(freeLine), (Byte)(charMap.cursorY + yPos), "".PadRight(charMap.CharWidth, (char)0x20));
                                                    bytPktPosnInArray = (byte)freeLine;
                                                    found = true;
                                                }
                                                else
                                                {
                                                    System.Diagnostics.Debug.Print("");
                                                }
                                            }
                                            //bytPktPosnInArray = createPacket(bytPktPosnInArray, (Byte)(charMap.cursorY + yPos), "".PadRight(charMap.CharWidth, (char)0x20));

                                        }

                                        String newLine = "";
                                        /*if (!found)
                                        {
                                            createPacket((Byte)(bytPktPosnInArray + 1), (Byte)(charMap.cursorY + yPos), "".PadRight(charMap.CharWidth, (char)0x20));
                                        }*/

                                        if (formPage.Lines[bytPktPosnInArray].Text == "")
                                            formPage.Lines[bytPktPosnInArray].Text = "".PadRight(charMap.CharWidth, (char)0x20);

                                        newLine = formPage.Lines[bytPktPosnInArray].Text.Substring(0, charMap.cursorX) + l;

                                        if (newLine.Length < charMap.CharWidth && l.Length > 0)
                                            newLine += formPage.Lines[bytPktPosnInArray].Text.Substring(charMap.cursorX + l.Length, charMap.CharWidth - (charMap.cursorX + l.Length));

                                        formPage.Lines[bytPktPosnInArray].Text = newLine.Length > charMap.CharWidth ? newLine.Substring(charMap.CharWidth) : newLine;
                                        Array.Copy(Encoding.ASCII.GetBytes(formPage.Lines[bytPktPosnInArray].Text), 0, formPage.Lines[bytPktPosnInArray].Bytes, 2, formPage.Lines[bytPktPosnInArray].Text.Length);
                                        formPage.Lines[bytPktPosnInArray].CalcHammingCodes();

                                        //bytPktPosnInArray = bytPktPosnInArray_copy;
                                        //bytPktPosnInArray = (Byte)(charMap.cursorY + yPos);
                                    }

                                    yPos++;

                                }
                            }
                            else
                            {
                                if (bytPktPosnInArray == 255)
                                {
                                    Byte result = createPacket(charMap.cursorY, charMap.cursorY, clipText);
                                    if (result == 255)
                                        MessageBox.Show("There are no free elements in which to insert a new packet.", "Teletext Stream Editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    //formPage.Lines[bytPktPosnInArray].Row = charMap.cursorY;
                                }
                                else
                                {
                                    formPage.Lines[bytPktPosnInArray].Text = clipText.PadRight(40, (Char)0x00);
                                    Array.Copy(Encoding.ASCII.GetBytes(formPage.Lines[bytPktPosnInArray].Text), 0, formPage.Lines[bytPktPosnInArray].Bytes, 2, formPage.Lines[bytPktPosnInArray].Text.Length);
                                    formPage.Lines[bytPktPosnInArray].CalcHammingCodes();
                                }
                            }
                            changesMade = true; redrawThumbnails = true;
                            break;
                        case 0x43:
                            //CTRL-C
                            //String clipText = (selectedBoxEndXChar - boxStartXChar).ToString("D2") + (selectedBoxEndYChar - boxStartYChar).ToString("D2");

                            if (selectedBoxEndXChar > charMap.CharWidth)
                                selectedBoxEndXChar = charMap.CharWidth;
                            if (selectedBoxEndYChar > charMap.CharHeight)
                                selectedBoxEndYChar = charMap.CharHeight;

                            for (Int32 y = boxStartYChar; y < selectedBoxEndYChar; y++)
                            {
                                found = false;
                                /* //Convert y coordinate (i.e. packet no) into an array index no.
                                for (Int32 i = 0; i < 256 && !found; i++)
                                {
                                    Line l = formPage.Lines[i];
                                    if (l.Row == y)
                                    {
                                        lineEdited = l;
                                        bytPktPosnInArray = (Byte)i;
                                        found = true;
                                    }
                                }*/

                                Line l = formPage.Lines[formPage.GetPacketIndex(y)];


                                if (l.Text != null && l.Text != "")
                                {
                                    //if (formPage.Lines[bytPktPosnInArray].Text != null && formPage.Lines[bytPktPosnInArray].Text.Length > 0 && found)
                                    if (boxStartXChar <= selectedBoxEndXChar)
                                        clipText += l.Text.Substring(boxStartXChar, selectedBoxEndXChar - boxStartXChar) + System.Environment.NewLine;
                                }
                                else
                                    clipText += System.Environment.NewLine;
                            }

                            // If no area was selected, copy the value under the cursor
                            if (clipText == "")
                            {
                                var hex = GetHex(bytPktPosnInArray, charMap.cursorX);
                                clipText = Convert.ToString(Convert.ToChar(Convert.ToInt32(GetHex(bytPktPosnInArray, charMap.cursorX).Substring(2, 2), 16))) + "\r\n";
                            }

                            try
                            {
                                Clipboard.SetData(DataFormats.Text, clipText.Replace((Char)0x00, (Char)0x20));
                            }
                            catch
                            {
                                MessageBox.Show("The clipboard did not accept the content.", "Teletext Recovery Editor", MessageBoxButtons.OK);
                            }

                            break;

                        // Colours

                        case 0x4b:
                            // Black
                            codeAdded = true;
                            ascii = 0x00;
                            break;
                        case 0x52:
                            // Red
                            codeAdded = true;
                            ascii = 0x01;
                            break;
                        case 0x47:
                            // Green
                            codeAdded = true;
                            ascii = 0x02;
                            break;
                        case 0x59:
                            // Yellow
                            codeAdded = true;
                            ascii = 0x03;
                            break;
                        case 0x42:
                            // Blue
                            codeAdded = true;
                            ascii = 0x04;
                            break;
                        case 0x4d:
                            // Magenta
                            codeAdded = true;
                            ascii = 0x05;
                            break;
                        case 0x53:
                            // Cyan
                            codeAdded = true;
                            ascii = 0x06;
                            break;
                        case 0x57:
                            // White
                            codeAdded = true;
                            ascii = 0x07;
                            break;
                        case 0x44:
                            // CTRL-D, double height
                            codeAdded = true;
                            ascii = 0x0d;
                            break;
                        case 0x4e:
                            // CTRL-N, new background
                            codeAdded = true;
                            ascii = 0x1d;
                            break;

                        case 0x26:
                            // CTRL + CRSR UP

                            if (e.Modifiers.ToString().Contains("Shift"))
                            {
                                // Move to the previous item in lvThumbnails
                                var selectedItem = lvThumbnails.SelectedItems[0].Index;
                                if (selectedItem > 1)
                                {
                                    lvThumbnails.SelectedItems[0].Selected = false;
                                    lvThumbnails.Items[selectedItem - 1].Selected = true;
                                    lvThumbnails_SelectedIndexChanged(null, null);
                                }
                            }
                            else
                            {
                                // Move selected carousel to the one above
                                if (recoveredPages.SelectedIndex > 0)
                                    recoveredPages.SelectedIndex--;
                            }
                            break;
                        case 0x28:
                            // CTRL + CRSR Down
                            if (lvThumbnails.SelectedItems.Count > 0)
                            {
                                if (e.Modifiers.ToString().Contains("Shift"))
                                {
                                    // Move to the previous item in lvThumbnails
                                    var selectedItem = lvThumbnails.SelectedItems[0].Index;
                                    if (selectedItem < lvThumbnails.Items.Count - 2)
                                    {
                                        lvThumbnails.SelectedItems[0].Selected = false;
                                        lvThumbnails.Items[selectedItem + 1].Selected = true;
                                        lvThumbnails_SelectedIndexChanged(null, null);
                                    }
                                    
                                }
                                else
                                {
                                    // Move selected carousel to the one below
                                    if (recoveredPages.SelectedIndex < recoveredPages.Items.Count - 1)
                                        recoveredPages.SelectedIndex++;
                                }
                            }
                            break;

                        default:
                            break;
                    }
                }

                if (codeAdded)
                {
                    var modifiers = e.Modifiers.ToString();
                    if (modifiers.Contains("Shift"))
                        ascii += 0x10;
                    // place character in the page object
                    formPage.Lines[bytPktPosnInArray].Text = lineEdited.Text.Substring(0, bytColumn) + (Char)ascii + lineEdited.Text.Substring(bytColumn + 1);
                    TeletextTools tools = new TeletextTools();
                    formPage.Lines[bytPktPosnInArray].Bytes[bytColumn + 2] = tools.CalcParity((Byte)ascii);
                    //System.Diagnostics.Debug.Print(formPage.Lines[bytPacket].Text + " " + (Char)ascii);
                    changesMade = true; redrawThumbnails = true;
                }


                // Get the new position of the cursor in case it has moved as a result of the keypress

                bytPktPosnInArray = formPage.GetPacketIndex(charMap.cursorY);

                //Change cursor size
                if (formPage.Lines[bytPktPosnInArray].Text != null)
                {
                    if (formPage.Lines[bytPktPosnInArray].Text.Contains((char)13) && renderer.PresentationLevel >= 1)
                        charMap.CursorHeightMultiplier = 2;
                    else
                        charMap.CursorHeightMultiplier = 1;
                }

                ResolveCursorType(bytPktPosnInArray);

                //Update x/y position display
                lblXpos.Text = "X: " + charMap.cursorX;
                lblYPos.Text = "Y: " + charMap.cursorY;

                // Get hex value of character under the cursor

                UpdateHexUnderCursor(bytPktPosnInArray);


                //Update page render if changes have been made
                if (e.KeyValue < 37 || e.KeyValue > 40 || codeAdded)
                {
                    if (!(e.KeyValue == 0x43 && e.Modifiers.ToString().Contains("Control")))
                    {
                        RenderItemToCharMap();

                        if (e.KeyValue != 13)
                            changesMade = true;
                    }
                }
            }
        }

        protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, System.Windows.Forms.Keys keyData)
        {

            if (keyData == Keys.Right || keyData == Keys.Left || keyData == Keys.Up || keyData == Keys.Down)
            {
                // Twiddling to make sure that the cursor is the right size on subsequent double height lines
                if (charMap.CursorHeightMultiplier == 2 && charMap.cursorX == 39 && keyData == Keys.Right)
                    charMap.cursorY++;
                if (charMap.CursorHeightMultiplier == 2 && charMap.cursorX == 0 && keyData == Keys.Left)
                    charMap.cursorY--;

                int p = formPage.GetPacketIndex(keyData == Keys.Up ? charMap.cursorY - 1 : charMap.cursorY);


                if (p != 255)
                    if (formPage.Lines[p].Text != null)
                        if (formPage.Lines[p].Text.Contains("\u000d"))
                            charMap.CursorHeightMultiplier = 2;
                        else
                            charMap.CursorHeightMultiplier = 1;

            }

            //Pass key to charmap
            charMap.Key(keyData);

            if (!tbCursorHex.Focused)
                UpdateHexUnderCursor();

            return base.ProcessCmdKey(ref msg, keyData);

        }
        private void TeletextRecoveryEditor_KeyPress(object sender, KeyPressEventArgs e)
        {
            //handling this event stops the form from going 'bong'
            if (!tbCursorHex.Focused && !tbPage.Focused && !tbTimeCode.Focused && !tbPosition.Focused)
                e.Handled = true;
        }

    }
}
