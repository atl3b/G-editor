using System.Windows;
using GEditor.App.ViewModels;

namespace GEditor.App;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
