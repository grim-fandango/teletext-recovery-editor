using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TeletextRecoveryEditor.Models
{
    public partial class STL
    {
        public GeneralSubtitleInformation GeneralSubtitleInformation { get; set; }
        public bool AddSubtitle(TextAndTimingInformation tti)
        {
            _TextAndTimingInformation.Add(tti);
            return true;
        }
        public void Save(string Filename)
        {
            using (var writer = new BinaryWriter(File.OpenWrite(Filename)))
            {
                // Output Header

                writer.Write(GeneralSubtitleInformation.CodePageNumber);
                writer.Write(GeneralSubtitleInformation.DiskFormatCode);
                writer.Write(GeneralSubtitleInformation.DisplayStandardCode);
                writer.Write(GeneralSubtitleInformation.CharacterCodeTable);
                writer.Write(GeneralSubtitleInformation.LanguageCode);
                writer.Write(GeneralSubtitleInformation.OriginalProgrammeTitle);
                writer.Write(GeneralSubtitleInformation.OriginalEpisodeTitle);
                writer.Write(GeneralSubtitleInformation.TranslatedProgrammeTitle);
                writer.Write(GeneralSubtitleInformation.TranslatedEpisodeTitle);
                writer.Write(GeneralSubtitleInformation.TranslatorsName);
                writer.Write(GeneralSubtitleInformation.TranslatorsContactDetails);
                writer.Write(GeneralSubtitleInformation.SubtitleListReferenceCode);
                writer.Write(GeneralSubtitleInformation.CreationDate);
                writer.Write(GeneralSubtitleInformation.RevisionDate);
                writer.Write(GeneralSubtitleInformation.RevisionNumber);
                writer.Write(GeneralSubtitleInformation.TotalNumberOfTtiBlocks);
                writer.Write(GeneralSubtitleInformation.TotalNumberOfSubtitleGroups);
                writer.Write(GeneralSubtitleInformation.MaximumNumberOfDisplayableCharactersPerRow);
                writer.Write(GeneralSubtitleInformation.MaximumNumberOfDisplayableRows);
                writer.Write(GeneralSubtitleInformation.TimeCodeStatus);
                writer.Write(GeneralSubtitleInformation.TimeCodeStartOfProgramme);
                writer.Write(GeneralSubtitleInformation.TimeCodeFirstInCue);
                writer.Write(GeneralSubtitleInformation.TotalNumberOfDisks);
                writer.Write(GeneralSubtitleInformation.DiskSequenceNumber);
                writer.Write(GeneralSubtitleInformation.CountryOfOrigin);
                writer.Write(GeneralSubtitleInformation.Publisher);
                writer.Write(GeneralSubtitleInformation.EditorsName);
                writer.Write(GeneralSubtitleInformation.EditorsContactDetails);
                writer.Write(GeneralSubtitleInformation.UserDefinedArea);
                writer.Write(GeneralSubtitleInformation.Reserved);

                // Loop list and output subtitles
                foreach (TextAndTimingInformation subtitle in _TextAndTimingInformation)
                {

                }
            }
        }

        private List<TextAndTimingInformation> _TextAndTimingInformation;
    }

    public partial class GeneralSubtitleInformation
    {
        public GeneralSubtitleInformation() {
            CodePageNumber = Encoding.ASCII.GetBytes("437");
            DiskFormatCode = Encoding.ASCII.GetBytes("STL25.01");
            DisplayStandardCode = Encoding.ASCII.GetBytes("1");
            CharacterCodeTable = Encoding.ASCII.GetBytes("00");
            LanguageCode = Encoding.ASCII.GetBytes("09");
            OriginalProgrammeTitle = Encoding.ASCII.GetBytes("".PadRight(32));
            OriginalEpisodeTitle = Encoding.ASCII.GetBytes("".PadRight(32));
            TranslatedProgrammeTitle = Encoding.ASCII.GetBytes("".PadRight(32));
            TranslatedEpisodeTitle = Encoding.ASCII.GetBytes("".PadRight(32));
            TranslatorsName = Encoding.ASCII.GetBytes("".PadRight(32));
            TranslatorsContactDetails = Encoding.ASCII.GetBytes("".PadRight(32));
            SubtitleListReferenceCode = Encoding.ASCII.GetBytes("".PadRight(16));
            CreationDate = Encoding.ASCII.GetBytes(DateTime.Now.ToString("yyMMdd"));
            RevisionDate = Encoding.ASCII.GetBytes(DateTime.Now.ToString("yyMMdd"));
            RevisionNumber = Encoding.ASCII.GetBytes("00");
            TotalNumberOfTtiBlocks = Encoding.ASCII.GetBytes("".PadRight(5));
            TotalNumberOfSubtitleGroups = Encoding.ASCII.GetBytes("".PadRight(3));
            MaximumNumberOfDisplayableCharactersPerRow = Encoding.ASCII.GetBytes("".PadRight(2));
            MaximumNumberOfDisplayableRows = Encoding.ASCII.GetBytes("".PadRight(2));
            TimeCodeStatus = Encoding.ASCII.GetBytes("0");
            TimeCodeStartOfProgramme = Encoding.ASCII.GetBytes("".PadRight(TimeCodeStartOfProgramme.Length));
            TimeCodeFirstInCue = Encoding.ASCII.GetBytes("".PadRight(TimeCodeFirstInCue.Length));
            TotalNumberOfDisks = Encoding.ASCII.GetBytes("".PadRight(TotalNumberOfDisks.Length));
            DiskSequenceNumber = Encoding.ASCII.GetBytes("".PadRight(DiskSequenceNumber.Length));
            CountryOfOrigin = Encoding.ASCII.GetBytes("GBR");
            Publisher = Encoding.ASCII.GetBytes("Teletext Recovery Editor".PadRight(Publisher.Length));
            EditorsName = Encoding.ASCII.GetBytes("Teletext Recovery Editor".PadRight(EditorsName.Length));
            EditorsContactDetails = Encoding.ASCII.GetBytes("Teletext Recovery Editor".PadRight(EditorsContactDetails.Length));
            UserDefinedArea = Encoding.ASCII.GetBytes("File created by Teletext Recovery Editor - see teletextarchaeologist.org for details.".PadRight(576));
            Reserved = Encoding.ASCII.GetBytes("Teletext Recovery Editor".PadRight(Reserved.Length));

        }
        public byte[] CodePageNumber { get; set; } = new byte[3]; 
        public byte[] DiskFormatCode { get; set; } = new byte[8];
        public byte[] DisplayStandardCode { get; set; } = new byte[1];
        public byte[] CharacterCodeTable { get; set; } = new byte[2];
        public byte[] LanguageCode { get; set; } = new byte[2];
        public byte[] OriginalProgrammeTitle { get; set; } = new byte[32];
        public byte[] OriginalEpisodeTitle { get; set; } = new byte[32];
        public byte[] TranslatedProgrammeTitle { get; set; } = new byte[32];
        public byte[] TranslatedEpisodeTitle { get; set; } = new byte[32];
        public byte[] TranslatorsName { get; set; } = new byte[32];
        public byte[] TranslatorsContactDetails { get; set; } = new byte[32];
        public byte[] SubtitleListReferenceCode { get; set; } = new byte[16];
        public byte[] CreationDate { get; set; } = new byte[6];
        public byte[] RevisionDate { get; set; } = new byte[6];
        public byte[] RevisionNumber { get; set; } = new byte[2];
        public byte[] TotalNumberOfTtiBlocks { get; set; } = new byte[5];
        public byte[] TotalNumberOfSubtitleGroups { get; set; } = new byte[3];
        public byte[] MaximumNumberOfDisplayableCharactersPerRow { get; set; } = new byte[2];
        public byte[] MaximumNumberOfDisplayableRows { get; set; } = new byte[2];
        public byte[] TimeCodeStatus { get; set; } = new byte[1];
        public byte[] TimeCodeStartOfProgramme { get; set; } = new byte[8];
        public byte[] TimeCodeFirstInCue { get; set; } = new byte[8];
        public byte[] TotalNumberOfDisks { get; set; } = new byte[1];
        public byte[] DiskSequenceNumber { get; set; } = new byte[1];
        public byte[] CountryOfOrigin { get; set; } = new byte[3];
        public byte[] Publisher { get; set; } = new byte[32];
        public byte[] EditorsName { get; set; } = new byte[32];
        public byte[] EditorsContactDetails { get; set; } = new byte[32];
        public byte[] UserDefinedArea { get; set; } = new byte[576];
        public byte[] Reserved { get; set; } = new byte[75];
    }

    public partial class TextAndTimingInformation
    {
        public TextAndTimingInformation()
        {
            SubtitleGroupNumber = 0;
            SubtitleNumber = 0;
            CumulativeStatus = 0;
            TimeCodeIn = new byte[] { 0, 0, 0, 0 };
            TimeCodeOut = new byte[] { 0, 0, 0, 0 };
            VerticalPosition = 20;
            JustificationCode = 2;
            CommentFlag = 0;
            TextField = "";
        }

        public TextAndTimingInformation(byte subtitleGroupNumber, int subtitleNumber, byte cumulativeStatus, 
            byte[] timeCodeIn, byte[] timeCodeOut, byte verticalPosition, string textField, byte justificationCode = 2, byte commentFlag = 0, byte extensionBlockNumber = 0)
        {
            SubtitleGroupNumber = subtitleGroupNumber;
            SubtitleNumber = subtitleNumber;
            ExtensionBlockNumber = extensionBlockNumber;
            CumulativeStatus = cumulativeStatus;
            TimeCodeIn = timeCodeIn;
            TimeCodeOut = timeCodeOut;
            VerticalPosition = verticalPosition;
            JustificationCode = justificationCode;
            CommentFlag = commentFlag;
            TextField = textField;
        }

        public byte SubtitleGroupNumber { get; set; } = new byte();
        public int SubtitleNumber 
        {
            get { return this.SubtitleNumber; }
            set 
            {
                this.SubtitleNumber = value;
                double hi = Math.Floor(Convert.ToDouble(value) / 256);
                double lo = Math.Floor(Convert.ToDouble(value) % 256);
                if (hi > 0xff) hi = 0xff;
                this._SubtitleNumber = new byte[] { Convert.ToByte(hi), Convert.ToByte(lo) };
            } 
        }
        private byte[] _SubtitleNumber { get; set; } = new byte[2];
        public byte ExtensionBlockNumber { get; set; } = new byte();
        public byte CumulativeStatus { get; set; } = new byte();
        public byte[] TimeCodeIn { get; set; } = new byte[4];
        public byte[] TimeCodeOut { get; set; } = new byte[4];
        public byte VerticalPosition { get; set; } = new byte();
        public byte JustificationCode { get; set; } = new byte();
        public byte CommentFlag { get; set; } = new byte();
        public string TextField 
        { 
            get 
            { 
                return this.TextField; 
            } 
            set 
            { 
                this.TextField = value; 
                this._TextField = Encoding.ASCII.GetBytes(value.Substring(0, 112).PadRight(_TextField.Length, (char)0x8f));
            } 
        }
        public byte[] _TextField { get; set; } = new byte[112];
    }
}
