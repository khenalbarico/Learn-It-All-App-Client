using LogicLib1.Services.AuthService;
using TestProject1.TestTools;
using TestProject1.TestTools.TestVariables;
using Xunit.Abstractions;

namespace TestProject1.AuthenticationFacts;

public class Authentication_EmailPass_Facts (ITestOutputHelper _ctx)
{
    [Fact] public async Task Register_Email_Pass_Provider()
    {
        var _sut     = _ctx.Get<IAppAuthentication>();

        var response = await _sut.RegisterWithEmailAsync(
            Authentication_Variables.Email,
            Authentication_Variables.Password);

        _ctx.WriteLine(
            $"Token: {response.Token}",
            $"Uid: {response.Uid}");
    }

    [Fact] public async Task Login_Email_Pass_Provider()
    {
        var _sut = _ctx.Get<IAppAuthentication>();

        var response = await _sut.SignInWithEmailAsync(
            Authentication_Variables.Email,
            Authentication_Variables.Password);

        _ctx.WriteLine(
            $"Token: {response.Token}",
            $"Uid: {response.Uid}");
    }
}
