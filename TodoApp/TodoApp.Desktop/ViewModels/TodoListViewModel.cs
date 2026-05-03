using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using TodoApp.Data;
using TodoApp.Models;

namespace TodoApp.Desktop.ViewModels;

public class TodoListViewModel : ViewModelBase
{
    private readonly TodoRepository _todoRepository;
    private readonly Guid _profileId;
    private string _searchText = string.Empty;
    private string _selectedStatusFilter = "All";
    private TodoItem? _selectedTodo;

    public ObservableCollection<TodoItem> Todos { get; } = new();

    public ICollectionView FilteredTodos { get; }

    public string[] StatusFilters { get; } = new[] { "All", "NotStarted", "InProgress", "Completed", "Postponed", "Failed" };

    public TodoStatus[] Statuses { get; } = Enum.GetValues<TodoStatus>();

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                FilteredTodos.Refresh();
            }
        }
    }

    public string SelectedStatusFilter
    {
        get => _selectedStatusFilter;
        set
        {
            if (SetProperty(ref _selectedStatusFilter, value))
            {
                FilteredTodos.Refresh();
            }
        }
    }

    public TodoItem? SelectedTodo
    {
        get => _selectedTodo;
        set => SetProperty(ref _selectedTodo, value);
    }

    public ICommand AddCommand { get; }

    public ICommand EditCommand { get; }

    public ICommand DeleteCommand { get; }

    public ICommand ChangeStatusCommand { get; }

    public TodoListViewModel(TodoRepository todoRepository, Guid profileId, Action showAdd, Action<TodoItem> showEdit)
    {
        _todoRepository = todoRepository;
        _profileId = profileId;
        AddCommand = new RelayCommand(_ => showAdd());
        EditCommand = new RelayCommand(_ =>
        {
            if (SelectedTodo != null)
            {
                showEdit(SelectedTodo);
            }
        });
        DeleteCommand = new RelayCommand(_ => DeleteSelected());
        ChangeStatusCommand = new RelayCommand(status => ChangeStatus(status));
        FilteredTodos = CollectionViewSource.GetDefaultView(Todos);
        FilteredTodos.Filter = FilterTodo;
        Load();
    }

    public void Load()
    {
        Todos.Clear();
        foreach (var todo in _todoRepository.GetAll(_profileId))
        {
            Todos.Add(todo);
        }

        FilteredTodos.Refresh();
    }

    private bool FilterTodo(object item)
    {
        if (item is not TodoItem todo)
        {
            return false;
        }

        var textMatches = string.IsNullOrWhiteSpace(SearchText) ||
            todo.Text.Contains(SearchText, StringComparison.CurrentCultureIgnoreCase);
        var statusMatches = SelectedStatusFilter == "All" || todo.Status.ToString() == SelectedStatusFilter;
        return textMatches && statusMatches;
    }

    private void DeleteSelected()
    {
        if (SelectedTodo == null)
        {
            return;
        }

        _todoRepository.Delete(SelectedTodo.Id);
        Load();
    }

    private void ChangeStatus(object? status)
    {
        if (SelectedTodo == null || status is not TodoStatus todoStatus)
        {
            return;
        }

        _todoRepository.SetStatus(SelectedTodo.Id, todoStatus);
        Load();
    }
}
