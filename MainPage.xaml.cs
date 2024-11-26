using System.Text.Json;
using ScientistManager;

namespace JsonManager;

public partial class MainPage : ContentPage
{
    private List<Scientist> _scientists;
    private string _filePath;
    private Scientist? _selectedScientist;

    public MainPage()
    {
        InitializeComponent();
        _scientists = new List<Scientist>();
    }

    private async void OnOpenJsonFileClicked(object sender, EventArgs e)
    {
        try
        {
            var file = await FileSystem.OpenAppPackageFileAsync("scientist.json");
            using var reader = new StreamReader(file);
            string json = await reader.ReadToEndAsync();
            
            _scientists = JsonSerializer.Deserialize<List<Scientist>>(json) ?? new List<Scientist>();
            _filePath = Path.Combine(FileSystem.AppDataDirectory, "scientist.json");
            
            File.WriteAllText(_filePath, json);

            UpdateScientistsGrid();

            await DisplayAlert("Успіх", $"Файл JSON успішно відкрито і скопійовано до {_filePath}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка", $"Не вдалося відкрити файл: {ex.Message}", "OK");
        }
    }

    private async void OnSaveJsonClicked(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(_filePath))
            {
                await DisplayAlert("Помилка", "Файл для збереження не обрано!", "OK");
                return;
            }

            string json = JsonSerializer.Serialize(_scientists, new JsonSerializerOptions { WriteIndented = true });
            
            File.WriteAllText(_filePath, json);

            await DisplayAlert("Успіх", "Зміни успішно збережено в тому ж файлі!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка", $"Не вдалося зберегти файл: {ex.Message}", "OK");
        }
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        string query = e.NewTextValue?.ToLower();
        if (string.IsNullOrWhiteSpace(query))
        {
            UpdateScientistsGrid();
            return;
        }

        var filtered = _scientists.Where(s =>
            s.FullName.ToLower().Contains(query) ||
            s.Faculty.ToLower().Contains(query) ||
            s.Rank.ToLower().Contains(query)).ToList();

        UpdateScientistsGrid(filtered);
    }

    private void UpdateScientistsGrid(IEnumerable<Scientist>? filtered = null)
    {
        var listToDisplay = filtered ?? _scientists;

        ScientistsGrid.Children.Clear();
        ScientistsGrid.RowDefinitions.Clear();
        ScientistsGrid.ColumnDefinitions.Clear();

        for (int i = 0; i < 6; i++)
        {
            ScientistsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        }

        ScientistsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        AddGridHeader("ПІБ", 0, 0);
        AddGridHeader("Факультет", 0, 1);
        AddGridHeader("Кафедра", 0, 2);
        AddGridHeader("Ступінь", 0, 3);
        AddGridHeader("Звання", 0, 4);
        AddGridHeader("Дата звання", 0, 5);

        int row = 1;
        foreach (var scientist in listToDisplay)
        {
            ScientistsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            AddGridCell(scientist.FullName, row, 0, scientist);
            AddGridCell(scientist.Faculty, row, 1, scientist);
            AddGridCell(scientist.Department, row, 2, scientist);
            AddGridCell(scientist.Degree, row, 3, scientist);
            AddGridCell(scientist.Rank, row, 4, scientist);
            AddGridCell(scientist.RankDate.ToString("yyyy-MM-dd"), row, 5, scientist);

            row++;
        }
    }

    private void AddGridHeader(string text, int row, int column)
    {
        var label = new Label
        {
            Text = text,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            BackgroundColor = Colors.LightGray,
            Padding = new Thickness(5)
        };
        Grid.SetRow(label, row);
        Grid.SetColumn(label, column);
        ScientistsGrid.Children.Add(label);
    }

    private void AddGridCell(string text, int row, int column, Scientist scientist)
    {
        var label = new Label
        {
            Text = text,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center,
            Padding = new Thickness(5)
        };

        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += (s, e) => OnRowTapped(scientist);
        label.GestureRecognizers.Add(tapGesture);

        Grid.SetRow(label, row);
        Grid.SetColumn(label, column);
        ScientistsGrid.Children.Add(label);
    }

    private void OnRowTapped(Scientist scientist)
    {
        _selectedScientist = scientist;
        DisplayAlert("Запис вибрано", $"Ви вибрали: {scientist.FullName}", "OK");
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        var form = new ScientistForm();
        await Navigation.PushModalAsync(form);
        form.Disappearing += (s, e) =>
        {
            if (form.Scientist != null)
            {
                _scientists.Add(form.Scientist);
                UpdateScientistsGrid();
                SaveScientistsToFile(); 
            }
        };
    }

    private async void OnEditClicked(object sender, EventArgs e)
    {
        if (_selectedScientist == null)
        {
            await DisplayAlert("Помилка", "Спочатку виберіть запис для редагування!", "OK");
            return;
        }

        var form = new ScientistForm(_selectedScientist);
        await Navigation.PushModalAsync(form);
        form.Disappearing += (s, e) =>
        {
            UpdateScientistsGrid();
            SaveScientistsToFile(); 
        };
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (_selectedScientist == null)
        {
            await DisplayAlert("Помилка", "Спочатку виберіть запис для видалення!", "OK");
            return;
        }

        bool confirm = await DisplayAlert("Підтвердження", $"Видалити запис: {_selectedScientist.FullName}?", "Так", "Ні");
        if (confirm)
        {
            _scientists.Remove(_selectedScientist);
            
            _selectedScientist = null;
            
            await SaveScientistsToFile();
            
            UpdateScientistsGrid();
        }
    }

    private async void OnAboutClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Про програму", "Автор: Лисенко Катерина\nКурс: 2\nГрупа: К-14\nОпис: Лабораторна робота №3 про дані науковців навчального закладу", "OK");
    }

    private async void OnExitClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Підтвердження", "Ви впевнені, що хочете вийти?", "Так", "Ні");
        if (confirm)
        {
            Environment.Exit(0);
        }
    }
    
    private async Task SaveScientistsToFile()
    {
        try
        {
            string json = JsonSerializer.Serialize(_scientists, new JsonSerializerOptions { WriteIndented = true });
            
            File.WriteAllText(_filePath, json);
            
            await DisplayAlert("Успіх", "Зміни успішно збережено в тому ж файлі!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка", $"Не вдалося зберегти файл: {ex.Message}", "OK");
        }
    }
}
