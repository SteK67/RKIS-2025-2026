using System.Windows.Input;

namespace TodoApp.Desktop.ViewModels;

public class AddTaskViewModel : ViewModelBase
{
    private string _text = string.Empty;

    public string Text
    {
        get => _text;
        set => SetProperty(ref _text, value);
    }

    public ICommand SaveCommand { get; }

    public ICommand CancelCommand { get; }

    public AddTaskViewModel(Action save, Action cancel)
    {
        SaveCommand = new RelayCommand(_ => save());
        CancelCommand = new RelayCommand(_ => cancel());
    }
}
