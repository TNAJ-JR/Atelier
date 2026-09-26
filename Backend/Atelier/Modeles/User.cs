using Atelier.Enum;

namespace Atelier.Modeles
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public Roles Role { get; set; }
    }
}
