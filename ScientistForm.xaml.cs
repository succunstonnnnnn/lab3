using ScientistManager;

namespace JsonManager;

public partial class ScientistForm : ContentPage
{
    public Scientist Scientist { get; private set; }

    public ScientistForm(Scientist scientist = null)
    {
        InitializeComponent();
        Scientist = scientist ?? new Scientist();

        if (scientist != null)
        {
            FullNameEntry.Text = scientist.FullName;
            FacultyEntry.Text = scientist.Faculty;
            ChairEntry.Text = scientist.Department;
            DegreeEntry.Text = scientist.Degree;
            RankEntry.Text = scientist.Rank;
            TitleDatePicker.Date = scientist.RankDate;
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        Scientist.FullName = FullNameEntry.Text;
        Scientist.Faculty = FacultyEntry.Text;
        Scientist.Department = ChairEntry.Text;
        Scientist.Degree = DegreeEntry.Text;
        Scientist.Rank = RankEntry.Text;
        Scientist.RankDate = TitleDatePicker.Date;

        await Navigation.PopModalAsync();
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}