using MediatR;

namespace Application.Users.Commands
{
    public record CreateUserCommand(
        string FirstName,
        string LastName,
        string Email,
        string PhoneNumber
    ) : IRequest<int>; 
}
