namespace University_Notebank.Models
{
    public class NoteAttachment
    {
        public int Id { get; set; }
        public int NoteId { get; set; }
        public Note Note { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public long FileSize { get; set; }
    }
}