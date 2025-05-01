namespace ProfApi.Models
{
    public class Follower
    {    
            public int FollowerId { get; set; }  
            public User Followero { get; set; }

            public int FollowingId { get; set; }  
            public User Following { get; set; }
        
    }
}
