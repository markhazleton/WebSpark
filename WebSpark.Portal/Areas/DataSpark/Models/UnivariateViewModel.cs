namespace WebSpark.Portal.Areas.DataSpark.Models
{
    public class UnivariateViewModel
    {
        public string FileName { get; set; }
        public List<string> AvailableColumns { get; set; } = new List<string>();
        public string SelectedColumn { get; set; }
        public string ImagePath { get; set; }
        public string Message { get; set; }
    }
}

