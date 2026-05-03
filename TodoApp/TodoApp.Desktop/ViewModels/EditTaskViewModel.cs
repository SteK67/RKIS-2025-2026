using System.Windows.Input;
using TodoApp.Models;

namespace TodoApp.Desktop.ViewModels;

public class EditTaskViewModel : ViewModelBase
{
    private string _text = string.Empty;
    private TodoStatus _status;

    public int Id { get; set; }

    public Guid ProfileId { get; set; }

    public DateTime LastUpdate { get; set; }

    public TodoStatus[] Statuses { get; } = Enum.GetValues<TodoStatus>();

    public string Text
    {
        get => _text;
        set => SetProperty(ref _text, value);
    }

    public TodoStatus Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public ICommand SaveCommand { get; }

    public ICommand CancelCommand { get; }

    public EditTaskViewModel(TodoItem item, Action save, Action cancel)
    {
        Id = item.Id;
        ProfileId = item.ProfileId;
        LastUpdate = item.LastUpdate;
        Text = item.Text;
        Status = item.Status;
        SaveCommand = new RelayCommand(_ => save());
        CancelCommand = new RelayCommand(_ => cancel());
    }
}
