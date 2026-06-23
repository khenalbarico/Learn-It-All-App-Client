using System.Text.RegularExpressions;
using LogicLib1.Services.App;
using LogicLib1.Services.AuthService;
using MauiApp1.Services;

namespace MauiApp1.ViewModels;

public enum AuthMode { Login, Register, VerifyEmail, ProfileSetup }

public class AuthViewModel(IAppAuthentication _auth, IAppService _appService, UserSession _userSession) : BaseViewModel
{
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    private AuthMode _mode          = AuthMode.Login;
    private string   _email         = string.Empty;
    private string   _password      = string.Empty;
    private string   _firstName     = string.Empty;
    private string   _lastName      = string.Empty;
    private string   _phoneNumber   = string.Empty;
    private bool     _isPasswordVisible;
    private string   _statusMessage = string.Empty;
    private string   _emailError    = string.Empty;
    private string   _passwordError = string.Empty;
    private bool     _validationEnabled;

    public AuthMode Mode
    {
        get => _mode;
        set
        {
            SetProperty(ref _mode, value);
            OnPropertyChanged(nameof(IsLoginMode));
            OnPropertyChanged(nameof(IsRegisterMode));
            OnPropertyChanged(nameof(IsVerifyEmailMode));
            OnPropertyChanged(nameof(IsProfileSetupMode));
            OnPropertyChanged(nameof(FormTitle));
            OnPropertyChanged(nameof(FormSubtitle));
            OnPropertyChanged(nameof(SubmitButtonText));
            OnPropertyChanged(nameof(ToggleModeText));
            OnPropertyChanged(nameof(ShowEmailPasswordFields));
            OnPropertyChanged(nameof(ShowToggleLink));
            OnPropertyChanged(nameof(ShowPasswordStrength));
            OnPropertyChanged(nameof(TogglePromptText));
            OnPropertyChanged(nameof(ToggleLinkText));
        }
    }

    public bool IsLoginMode        => Mode == AuthMode.Login;
    public bool IsRegisterMode     => Mode == AuthMode.Register;
    public bool IsVerifyEmailMode  => Mode == AuthMode.VerifyEmail;
    public bool IsProfileSetupMode => Mode == AuthMode.ProfileSetup;
    public bool ShowEmailPasswordFields => Mode == AuthMode.Login || Mode == AuthMode.Register;
    public bool ShowToggleLink          => Mode == AuthMode.Login || Mode == AuthMode.Register;

    public string FormTitle => Mode switch
    {
        AuthMode.Login        => "Welcome Back",
        AuthMode.Register     => "Create Account",
        AuthMode.VerifyEmail  => "Check Your Email",
        AuthMode.ProfileSetup => "Complete Your Profile",
        _                     => string.Empty
    };

    public string FormSubtitle => Mode switch
    {
        AuthMode.Login        => "Sign in to access your books",
        AuthMode.Register     => "Join and start learning today",
        AuthMode.VerifyEmail  => "One more step before you can continue",
        AuthMode.ProfileSetup => "Tell us a little about yourself",
        _                     => string.Empty
    };

    public string SubmitButtonText => Mode switch
    {
        AuthMode.Login        => "Sign In",
        AuthMode.Register     => "Create Account",
        AuthMode.VerifyEmail  => "I've Verified My Email",
        AuthMode.ProfileSetup => "Save & Continue",
        _                     => string.Empty
    };

    public string ToggleModeText => Mode == AuthMode.Login
        ? "Don't have an account? Register"
        : "Already have an account? Sign In";


    public string Email
    {
        get => _email;
        set
        {
            SetProperty(ref _email, value);
            if (_validationEnabled) ValidateEmail();
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            SetProperty(ref _password, value);
            OnPropertyChanged(nameof(PasswordStrengthText));
            OnPropertyChanged(nameof(PasswordStrengthColor));
            OnPropertyChanged(nameof(ShowPasswordStrength));
            OnPropertyChanged(nameof(StrengthSeg1Color));
            OnPropertyChanged(nameof(StrengthSeg2Color));
            OnPropertyChanged(nameof(StrengthSeg3Color));
            if (_validationEnabled) ValidatePassword();
        }
    }

    public string FirstName   { get => _firstName;   set => SetProperty(ref _firstName,   value); }
    public string LastName    { get => _lastName;    set => SetProperty(ref _lastName,    value); }
    public string PhoneNumber { get => _phoneNumber; set => SetProperty(ref _phoneNumber, value); }

    public bool IsPasswordVisible
    {
        get => _isPasswordVisible;
        set => SetProperty(ref _isPasswordVisible, value);
    }

    public string EmailError
    {
        get => _emailError;
        set
        {
            SetProperty(ref _emailError, value);
            OnPropertyChanged(nameof(HasEmailError));
        }
    }

    public bool HasEmailError => !string.IsNullOrEmpty(_emailError);

    public string PasswordError
    {
        get => _passwordError;
        set
        {
            SetProperty(ref _passwordError, value);
            OnPropertyChanged(nameof(HasPasswordError));
        }
    }

    public bool HasPasswordError => !string.IsNullOrEmpty(_passwordError);

    public string PasswordStrengthText => _password.Length switch
    {
        0           => string.Empty,
        < 6         => "Too short — at least 6 characters required",
        < 8         => "Weak — try adding numbers or symbols",
        _           => HasMixedCharacters(_password) ? "Strong password ✓" : "Good — mix in uppercase or symbols"
    };

    public Color PasswordStrengthColor => _password.Length switch
    {
        0           => Colors.Transparent,
        < 6         => Color.FromArgb("#E53E3E"),
        < 8         => Color.FromArgb("#D97706"),
        _           => HasMixedCharacters(_password) ? Color.FromArgb("#38A169") : Color.FromArgb("#2E7BC4")
    };

    public bool ShowPasswordStrength => IsRegisterMode && _password.Length > 0;

    private static readonly Color _strengthEmpty = Color.FromArgb("#E5EAF2");

    public Color StrengthSeg1Color => _password.Length > 0
        ? PasswordStrengthColor : _strengthEmpty;

    public Color StrengthSeg2Color => _password.Length >= 6
        ? PasswordStrengthColor : _strengthEmpty;

    public Color StrengthSeg3Color => _password.Length >= 8 && HasMixedCharacters(_password)
        ? PasswordStrengthColor : _strengthEmpty;

    public string TogglePromptText => Mode == AuthMode.Login
        ? "Don't have an account?"
        : "Already have an account?";

    public string ToggleLinkText => Mode == AuthMode.Login ? "Register" : "Sign In";

    private static bool HasMixedCharacters(string s)
        => s.Any(char.IsUpper) && (s.Any(char.IsDigit) || s.Any(c => !char.IsLetterOrDigit(c)));

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            SetProperty(ref _statusMessage, value);
            OnPropertyChanged(nameof(HasStatus));
        }
    }

    public bool HasStatus => !string.IsNullOrEmpty(StatusMessage);


    public Func<Task>? AuthCompleted { get; set; }

    public Command ToggleModeCommand => new(() =>
    {
        ErrorMessage  = string.Empty;
        EmailError    = string.Empty;
        PasswordError = string.Empty;
        _validationEnabled = false;
        Mode = Mode == AuthMode.Login ? AuthMode.Register : AuthMode.Login;
    });

    public Command TogglePasswordVisibilityCommand => new(() =>
        IsPasswordVisible = !IsPasswordVisible);

    public Command SubmitCommand => new(async () => await OnSubmitAsync(), () => !IsBusy);

    public Command ResendVerificationCommand => new(async () => await OnResendVerificationAsync(), () => !IsBusy);


    private async Task OnSubmitAsync()
    {
        ErrorMessage  = string.Empty;
        StatusMessage = string.Empty;
        IsBusy = true;
        try
        {
            if (Mode == AuthMode.Login || Mode == AuthMode.Register)
            {
                if (!ValidateEmailPasswordFields()) return;
            }

            if (Mode == AuthMode.Login)
            {
                await _auth.SignInWithEmailAsync(Email, Password);
                var existing = await _appService.TryGetUserInfo();
                if (existing is null)
                    Mode = AuthMode.ProfileSetup;
                else
                {
                    _userSession.UserInfo = existing;
                    await CompleteAuthAsync();
                }
            }
            else if (Mode == AuthMode.Register)
            {
                await _auth.RegisterWithEmailAsync(Email, Password);
                Mode = AuthMode.VerifyEmail;
            }
            else if (Mode == AuthMode.VerifyEmail)
            {
                await _auth.CheckEmailVerifiedAsync();
                var existing = await _appService.TryGetUserInfo();
                if (existing is null)
                    Mode = AuthMode.ProfileSetup;
                else
                {
                    _userSession.UserInfo = existing;
                    await CompleteAuthAsync();
                }
            }
            else if (Mode == AuthMode.ProfileSetup)
            {
                if (!ValidateProfileFields()) return;
                await _appService.CreateUser(_firstName, _lastName, _phoneNumber);
                _userSession.UserInfo = await _appService.TryGetUserInfo();
                await CompleteAuthAsync();
            }
        }
        catch (EmailNotVerifiedException)
        {
            Mode = AuthMode.VerifyEmail;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task OnResendVerificationAsync()
    {
        ErrorMessage  = string.Empty;
        StatusMessage = string.Empty;
        IsBusy = true;
        try
        {
            await _auth.SendEmailVerificationAsync();
            StatusMessage = "Verification email resent. Check your inbox.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool ValidateEmailPasswordFields()
    {
        _validationEnabled = true;
        ValidateEmail();
        ValidatePassword();
        return !HasEmailError && !HasPasswordError;
    }

    private void ValidateEmail()
    {
        if (string.IsNullOrWhiteSpace(_email))
        {
            EmailError = "Email address is required.";
            return;
        }
        EmailError = EmailRegex.IsMatch(_email) ? string.Empty : "Please enter a valid email address.";
    }

    private void ValidatePassword()
    {
        if (string.IsNullOrEmpty(_password))
        {
            PasswordError = "Password is required.";
            return;
        }
        PasswordError = _password.Length < 6 ? "Password must be at least 6 characters." : string.Empty;
    }

    private bool ValidateProfileFields()
    {
        if (string.IsNullOrWhiteSpace(_firstName) ||
            string.IsNullOrWhiteSpace(_lastName)  ||
            string.IsNullOrWhiteSpace(_phoneNumber))
        {
            ErrorMessage = "Please fill in all fields.";
            return false;
        }
        return true;
    }

    private async Task CompleteAuthAsync()
        => await (AuthCompleted?.Invoke() ?? Task.CompletedTask);
}
