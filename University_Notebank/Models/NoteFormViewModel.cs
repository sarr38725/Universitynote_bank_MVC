using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace University_Notebank.Models
{
    public class NoteFormViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int MajorId { get; set; }
        public int TermId { get; set; }
        public int? Batch { get; set; }
        public IFormFile CoverImage { get; set; }
        public IFormFile NoteFile { get; set; }
        public List<SelectListItem> Majors { get; set; }
        public List<SelectListItem> Terms { get; set; }
        public List<SelectListItem> Batches { get; set; }
    }
}