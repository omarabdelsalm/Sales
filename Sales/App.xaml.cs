using Microsoft.EntityFrameworkCore;

using Sales.Shared.Data;

namespace Sales;

public partial class App : Application
{
    public App(MainPage mainPage)
    {
        InitializeComponent();
        MainPage = mainPage;
    }
}
