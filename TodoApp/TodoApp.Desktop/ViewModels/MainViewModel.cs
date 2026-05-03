using TodoApp.Data;
using TodoApp.Models;

namespace TodoApp.Desktop.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly ProfileRepository _profileRepository = new();
    private readonly TodoRepository _todoRepository = new();
    private ViewModelBase _currentViewModel;
    private string _message = string.Empty;
    private Profile? _currentProfile;

    public LoginViewModel LoginViewModel { get; }

    public RegisterViewModel RegisterViewModel { get; }

    public TodoListViewModel? TodoListViewModel { get; private set; }

    public AddTaskViewModel? AddTaskViewModel { get; private set; }

    public EditTaskViewModel? EditTaskViewModel { get; private set; }

    public ViewModelBase CurrentViewModel
    {
        get => _currentViewModel;
        set => SetProperty(ref _currentViewModel, value);
    }

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public string CurrentProfileName => _currentProfile == null
        ? string.Empty
        : $"{_currentProfile.FirstName} {_currentProfile.LastName}";

    public MainViewModel()
    {
        LoginViewModel = new LoginViewModel(Login, ShowRegister);
        RegisterViewModel = new RegisterViewModel(Register, ShowLogin);
        _currentViewModel = LoginViewModel;
    }

    private void ShowLogin()
    {
        Message = string.Empty;
        CurrentViewModel = LoginViewModel;
    }

    private void ShowRegister()
    {
        Message = string.Empty;
        CurrentViewModel = RegisterViewModel;
    }

    private void ShowTodoList()
    {
        try
        {
            if (_currentProfile == null)
            {
                return;
            }

            TodoListViewModel = new TodoListViewModel(_todoRepository, _currentProfile.Id, ShowAddTask, ShowEditTask);
            OnPropertyChanged(nameof(TodoListViewModel));
            OnPropertyChanged(nameof(CurrentProfileName));
            CurrentViewModel = TodoListViewModel;
        }
        catch (Exception ex)
        {
            Message = $"Ошибка загрузки задач: {ex.Message}";
        }
    }

    private void ShowAddTask()
    {
        AddTaskViewModel = new AddTaskViewModel(AddTask, ShowTodoList);
        OnPropertyChanged(nameof(AddTaskViewModel));
        CurrentViewModel = AddTaskViewModel;
    }

    private void ShowEditTask(TodoItem item)
    {
        EditTaskViewModel = new EditTaskViewModel(item, EditTask, ShowTodoList);
        OnPropertyChanged(nameof(EditTaskViewModel));
        CurrentViewModel = EditTaskViewModel;
    }

    private void Login()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(LoginViewModel.Login) || string.IsNullOrWhiteSpace(LoginViewModel.Password))
            {
                Message = "Введите логин и пароль.";
                return;
            }

            var profile = _profileRepository.GetByCredentials(LoginViewModel.Login.Trim(), LoginViewModel.Password);
            if (profile == null)
            {
                Message = "Профиль не найден.";
                return;
            }

            _currentProfile = profile;
            Message = string.Empty;
            ShowTodoList();
        }
        catch (Exception ex)
        {
            Message = $"Ошибка входа: {ex.Message}";
        }
    }

    private void Register()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(RegisterViewModel.Login) ||
                string.IsNullOrWhiteSpace(RegisterViewModel.Password) ||
                string.IsNullOrWhiteSpace(RegisterViewModel.FirstName) ||
                string.IsNullOrWhiteSpace(RegisterViewModel.LastName))
            {
                Message = "Заполните все поля.";
                return;
            }

            if (!int.TryParse(RegisterViewModel.BirthYear, out var birthYear) || birthYear < 1900 || birthYear > DateTime.Now.Year)
            {
                Message = "Некорректный год рождения.";
                return;
            }

            var login = RegisterViewModel.Login.Trim();
            if (_profileRepository.ExistsByLogin(login))
            {
                Message = "Такой логин уже существует.";
                return;
            }

            _currentProfile = new Profile(login, RegisterViewModel.Password, RegisterViewModel.FirstName.Trim(), RegisterViewModel.LastName.Trim(), birthYear);
            _profileRepository.Add(_currentProfile);
            Message = string.Empty;
            ShowTodoList();
        }
        catch (Exception ex)
        {
            Message = $"Ошибка регистрации: {ex.Message}";
        }
    }

    private void AddTask()
    {
        try
        {
            if (_currentProfile == null || AddTaskViewModel == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(AddTaskViewModel.Text))
            {
                Message = "Введите текст задачи.";
                return;
            }

            _todoRepository.Add(new TodoItem(AddTaskViewModel.Text.Trim(), _currentProfile.Id));
            Message = string.Empty;
            ShowTodoList();
        }
        catch (Exception ex)
        {
            Message = $"Ошибка добавления задачи: {ex.Message}";
        }
    }

    private void EditTask()
    {
        try
        {
            if (EditTaskViewModel == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(EditTaskViewModel.Text))
            {
                Message = "Введите текст задачи.";
                return;
            }

            _todoRepository.Update(new TodoItem
            {
                Id = EditTaskViewModel.Id,
                ProfileId = EditTaskViewModel.ProfileId,
                Text = EditTaskViewModel.Text.Trim(),
                Status = EditTaskViewModel.Status,
                LastUpdate = EditTaskViewModel.LastUpdate
            });
            Message = string.Empty;
            ShowTodoList();
        }
        catch (Exception ex)
        {
            Message = $"Ошибка редактирования задачи: {ex.Message}";
        }
    }
}
