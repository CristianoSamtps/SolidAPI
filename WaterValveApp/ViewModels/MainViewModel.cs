
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WaterValveApp.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private string humidityThreshold = "40";

    [ObservableProperty]
    private string savedThreshold = "40";

    [ObservableProperty]
    private string statusMessage = "Limite ainda não alterado.";

    [RelayCommand]
    private void SaveThreshold()
    {
        if (!double.TryParse(HumidityThreshold, out double value) ||
            value < 0 || value > 100)
        {
            StatusMessage = "Introduz um valor entre 0 e 100.";
            return;
        }

        SavedThreshold = value.ToString("0.##");
        HumidityThreshold = SavedThreshold;
        StatusMessage = $"Limite guardado: {SavedThreshold}%";
    }
}
