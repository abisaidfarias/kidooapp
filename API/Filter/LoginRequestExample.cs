using Application.DTOs.Request.Account;
using Swashbuckle.AspNetCore.Filters;

public class LoginRequestExample : IExamplesProvider<LoginDTO>
{
    public LoginDTO GetExamples()
    {
        return new LoginDTO
        {
            Email = "abisaidfarias@gmail.com",
            Password = "@Admin1234"
        };
    }
}