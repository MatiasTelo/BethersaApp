using BethersaApp.ViewModels;
using System.Windows.Controls;

namespace BethersaApp.Views;

public partial class ProductosView : UserControl
{
    public ProductosView()
    {
        InitializeComponent();
    }

    private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.Source is TabControl tabControl && 
            tabControl.SelectedItem is TabItem selectedTab &&
            selectedTab.Name == "TabRanking" &&
            DataContext is ProductosViewModel viewModel)
        {
            viewModel.RefrescarRankingCommand.Execute(null);
        }
    }
}
