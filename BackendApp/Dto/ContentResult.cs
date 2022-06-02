namespace BackendApp.Dto
{
    public class ContentResult<T>
    {
        public int CurrentPage { get; set; }

        public int TotalRecord { get; set; }

        public int TotalPage { get; set; }

        public T Result { get; set; }
    }
}