using ProfApi.Models.UserDTO;

namespace ProfApi.Models
{
    public class User : UserDetailDTO
    {
        public ICollection<Follower> Followers { get; set; }
        public ICollection<Follower> Following { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
