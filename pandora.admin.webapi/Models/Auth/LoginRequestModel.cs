namespace Pandora.Admin.WebAPI.Models.Auth;

public class LoginRequestModel
{
    public string username { get; set; }
    public string password { get; set; }
    public string action { get; set; }
}