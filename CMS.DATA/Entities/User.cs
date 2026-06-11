namespace CMS.DATA.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }

        // Thay thế 'Password' bằng 'PasswordHash'
        public string PasswordHash { get; set; }
    }
}