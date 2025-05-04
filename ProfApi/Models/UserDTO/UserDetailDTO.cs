namespace ProfApi.Models.UserDTO
{
    public class UserDetailDTO 
    {
        public string ProfilePicture { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Description { get; set; }
        public string Adress { get; set; }
        public int UserId { get; set; }
        public int CountFollowers { get; set; }
        public int CountFollowing { get; set; }
        public UserType Type { get; set; }


    }
}
