namespace Domain.Entities;
public class User
{
    public int Id { get; private set; }
    public string FirstName { get; private set; } = string.Empty;   
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
}

