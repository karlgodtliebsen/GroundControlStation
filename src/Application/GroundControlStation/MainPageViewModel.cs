using System.Collections.ObjectModel;
using System.Windows.Input;

using UraniumUI;

namespace GroundControlStationApp;

/// <summary>
/// 
/// </summary>
public class MainPageViewModel : BindableObject
{
    public ObservableCollection<TodoItem> Items { get; protected set; } = [];

    public ObservableCollection<TodoItem> SelectedItems { get; set; } = [];

    private TodoItem newItem = new();

    public TodoItem NewItem
    {
        get => newItem;
        set
        {
            newItem = value;
            OnPropertyChanged();
        }
    }

    public ICommand AddNewItemCommand { get; protected set; }

    public ICommand RemoveSelectedItemsCommand { get; protected set; }

    public MainPageViewModel()
    {
        if (Items.Count == 0)
        {
            Items.Add(new TodoItem { Content = "Throw away the rubbish", Type = TodoItem.TodoItemType.Personal });
            Items.Add(new TodoItem { Content = "Attend the meeting today\n11:00AM", Type = TodoItem.TodoItemType.Work });
            Items.Add(new TodoItem { Content = "Prepare presentation for new project", Type = TodoItem.TodoItemType.Work });
            Items.Add(new TodoItem { Content = "Spend time with family", Type = TodoItem.TodoItemType.Family });
            Items.Add(new TodoItem { Content = "Complete the puzzle", Type = TodoItem.TodoItemType.Hobby });
            Items.Add(new TodoItem { Content = "Don't forget to call dad", Type = TodoItem.TodoItemType.Family });
        }

        AddNewItemCommand = new Command(() =>
        {
            Items.Insert(0, NewItem);
            NewItem = new TodoItem();
        });

        RemoveSelectedItemsCommand = new Command(() =>
        {
            foreach (var item in SelectedItems)
            {
                Items.Remove(item);
            }
        });
    }
}

public class TodoItem : UraniumBindableObject
{
    public string Content { get; set; } = null!;

    private bool isDone;

    public bool IsDone
    {
        get => isDone;
        set => SetProperty(ref isDone, value);
    }

    public TodoItemType Type { get; set; }

    public static TodoItemType[]? AvailableTypes => Enum.GetValues(typeof(TodoItemType)) as TodoItemType[];

    public enum TodoItemType
    {
        Personal,
        Work,
        Hobby,
        Family
    }
}

/// <summary>
/// Represents a person with a name and height.
/// </summary>
public class Person
{
    /// <summary>
    /// Gets or sets the name of the person.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the height of the person.
    /// </summary>	
    public double Height { get; set; }
}
