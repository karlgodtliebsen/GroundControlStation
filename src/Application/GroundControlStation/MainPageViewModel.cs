namespace GroundControlStationApp;

/// <summary>
/// ViewModel for the MainPage.
/// </summary>
public class MainPageViewModel
{
    /// <summary>
    /// Gets or sets the list of Person objects.
    /// </summary>	
    public List<Person> Data { get; set; }

    /// <summary>
    /// Initializes a new instance of the ViewModel class with sample data.
    /// </summary>
    public MainPageViewModel()
    {
        // Initialize the Data property with a list of Person objects
        Data =
        [
            new Person { Name = "David", Height = 170 },
            new Person { Name = "Michael", Height = 96 },
            new Person { Name = "Steve", Height = 65 },
            new Person { Name = "Joel", Height = 182 },
            new Person { Name = "Bob", Height = 134 }
        ];
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