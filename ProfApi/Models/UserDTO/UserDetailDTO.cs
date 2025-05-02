namespace ProfApi.Models.UserDTO
{
    public class UserDetailDTO : UserCreateDTO
    {
        public int UserId { get; set; }
        public int CountFollowers { get; set; }
        public int CountFollowing { get; set; }
        public UserType Type { get; set; }


    }
}
