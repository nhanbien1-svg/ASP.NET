namespace CMS.DATA.Entities
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedData { get; set; } = DateTime.Now;
        // Khóa ngoại kết nối với Category

        public int CategoryID { get; set; }
        public virtual Category Category { get; set; }


    }
}