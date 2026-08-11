using AulaVirtualApp.Config;
using AulaVirtualApp.Services;

namespace AulaVirtualApp.Pages;

public partial class AdminDashboardPage : ContentPage
{
    public AdminDashboardPage()
    {
        InitializeComponent();
        UserLabel.Text = $"Sesion: {SessionService.Usuario} (ADMINISTRADOR) - {SessionService.InstitucionNombre}";
    }

    private Task Ir(Models.EntityConfig config) => Navigation.PushAsync(new EntityListPage(config));

    private async void OnFacultadesClicked(object sender, EventArgs e) => await Ir(EntityCatalog.Facultades());
    private async void OnCarrerasClicked(object sender, EventArgs e) => await Ir(EntityCatalog.Carreras());
    private async void OnSemestresClicked(object sender, EventArgs e) => await Ir(EntityCatalog.Semestres());
    private async void OnCarreraSemestresClicked(object sender, EventArgs e) => await Ir(EntityCatalog.CarreraSemestres());
    private async void OnCursosClicked(object sender, EventArgs e) => await Ir(EntityCatalog.Cursos());
    private async void OnPensumClicked(object sender, EventArgs e) => await Ir(EntityCatalog.Pensum());

    private async void OnDocentesClicked(object sender, EventArgs e)
    {
        var config = EntityCatalog.Docentes();
        config.CustomCreatePage = () => new CrearDocentePage();
        await Ir(config);
    }

    private async void OnEstudiantesClicked(object sender, EventArgs e)
    {
        var config = EntityCatalog.Estudiantes();
        config.CustomCreatePage = () => new CrearEstudiantePage();
        await Ir(config);
    }

    private async void OnSeccionesClicked(object sender, EventArgs e) => await Ir(EntityCatalog.Secciones());
    private async void OnEstudianteSeccionesClicked(object sender, EventArgs e) => await Ir(EntityCatalog.EstudianteSecciones());

    private async void OnCiclosClicked(object sender, EventArgs e) => await Ir(EntityCatalog.Ciclos());
    private async void OnAulasClicked(object sender, EventArgs e) => await Ir(EntityCatalog.Aulas());
    private async void OnModulosClicked(object sender, EventArgs e) => await Ir(EntityCatalog.Modulos());
    private async void OnHorariosClicked(object sender, EventArgs e) => await Ir(EntityCatalog.Horarios());
    private async void OnInstitucionesClicked(object sender, EventArgs e) => await Ir(EntityCatalog.Instituciones());

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        SessionService.Clear();
        await Navigation.PopToRootAsync();
    }
}
