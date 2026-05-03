using System.Windows.Input;

namespace TodoApp.Desktop.ViewModels;

public class RegisterViewModel : ViewModelBase
{
    private string _login = string.Empty;
    private string _password = string.Empty;
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private string _birthYear = string.Empty;

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

    public string FirstName
    {
        get => _firstName;
        set => SetProperty(ref _firstName, value);
    }

    public string LastName
    {
        get => _lastName;
        set => SetProperty(ref _lastName, value);
    }

    public string BirthYear
    {
        get => _birthYear;
        set => SetProperty(ref _birthYear, value);
    }

    public ICommand RegisterCommand { get; }

    public ICommand ShowLoginCommand { get; }

    public RegisterViewModel(Action register, Action showLogin)
    {
        RegisterCommand = new RelayCommand(_ => register());
        ShowLoginCommand = new RelayCommand(_ => showLogin());
    }
}
