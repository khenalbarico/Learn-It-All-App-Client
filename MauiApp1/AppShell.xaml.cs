using MauiApp1.Views;

namespace MauiApp1;

public partial class AppShell : Shell
{
    public AppShell(HomePage homePage, LibraryPage libraryPage, MyLibraryPage myLibraryPage, AccountPage accountPage)
    {
        InitializeComponent();

        HomeTab.Content      = homePage;
        LibraryTab.Content   = libraryPage;
        MyLibraryTab.Content = myLibraryPage;
        AccountTab.Content   = accountPage;

        Routing.RegisterRoute(nameof(AuthPage), typeof(AuthPage));
    }
}
