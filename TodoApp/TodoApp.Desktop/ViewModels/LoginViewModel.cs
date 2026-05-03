using System.Windows.Input;

namespace TodoApp.Desktop.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private string _login = string.Empty;
    private string _password = string.Empty;

    public string Login
    {
        get => _login;
        set => SetProperty(ref _login, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public ICommand LoginCommand { get; }

    public ICommand ShowRegisterCommand { get; }

    public LoginViewModel(Action login, Action showRegister)
    {
        LoginCommand = new RelayCommand(_ => login());
        ShowRegisterCommand = new RelayCommand(_ => showRegister());
    }
}
