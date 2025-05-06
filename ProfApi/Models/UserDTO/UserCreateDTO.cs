namespace ProfApi.Models.UserDTO
{
    public class UserCreateDTO
    {
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public IFormFile profilePicture { get; set; }
    }
}
