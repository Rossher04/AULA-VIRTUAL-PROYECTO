using System.Collections.ObjectModel;
using System.Text.Json.Nodes;
using AulaVirtualApp.Models;
using AulaVirtualApp.Services;

namespace AulaVirtualApp.Pages;

public partial class EntityListPage : ContentPage
{
    private readonly EntityConfig _config;
    private readonly ObservableCollection<EntityRow> _rows = new();

    public EntityListPage(EntityConfig config)
    {
        InitializeComponent();
        _config = config;
        Title = _config.Title;
        ItemsView.ItemsSource = _rows;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        ErrorLabel.HideError();
        SetLoading(true);

        var result = await ApiClient.GetAsync(_config.ApiTarget, _config.Endpoint);

        SetLoading(false);
        RefreshViewControl.IsRefreshing = false;

        if (!result.Ok)
        {
            ErrorLabel.ShowError(result.ErrorMessage ?? "No se pudo cargar la informacion.");
            return;
        }

        _rows.Clear();
        foreach (var node in result.Data.AsJsonArray())
        {
            if (node is JsonObject obj)
            {
                _rows.Add(new EntityRow
                {
                    Title = UiHelpers.SafeText(() => _config.DisplayTitle(obj)),
                    Subtitle = _config.DisplaySubtitle != null ? UiHelpers.SafeText(() => _config.DisplaySubtitle(obj)) : string.Empty,
                    Data = obj
                });
            }
        }
    }

    private void OnRefreshing(object sender, EventArgs e)
    {
        _ = LoadAsync();
    }

    private async void OnAgregarClicked(object sender, EventArgs e)
    {
        if (_config.CustomCreatePage != null)
        {
            await Navigation.PushAsync(_config.CustomCreatePage());
        }
        else
        {
            await Navigation.PushAsync(new EntityFormPage(_config, null));
        }
    }

    private async void OnItemTapped(object sender, EventArgs e)
    {
        if (sender is Frame { BindingContext: EntityRow row })
        {
            await Navigation.PushAsync(new EntityFormPage(_config, row.Data));
        }
    }

    private async void OnEliminarInvoked(object sender, EventArgs e)
    {
        if (sender is not SwipeItem { BindingContext: EntityRow row })
            return;

        var confirmar = await DisplayAlert("Eliminar", $"Se eliminara: {row.Title}", "Eliminar", "Cancelar");
        if (!confirmar)
            return;

        SetLoading(true);
        var result = await ApiClient.DeleteAsync(_config.ApiTarget, $"{_config.Endpoint}{row.Id}/");
        SetLoading(false);

        if (!result.Ok)
        {
            await DisplayAlert("Error", result.ErrorMessage ?? "No se pudo eliminar el registro.", "OK");
            return;
        }

        _rows.Remove(row);
    }

    private void SetLoading(bool loading) => LoadingIndicator.SetLoading(loading);
}
