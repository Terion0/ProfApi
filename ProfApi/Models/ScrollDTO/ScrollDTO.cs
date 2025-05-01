namespace ProfApi.Models.ScrollDTO
{
    public class ScrollDTO<T>
    {
        public List<T> Data { get; set; }
        public int TotalRecords { get; set; }
        public int LastId { get; set; }
        public bool HasMore { get; set; }
    }
}
