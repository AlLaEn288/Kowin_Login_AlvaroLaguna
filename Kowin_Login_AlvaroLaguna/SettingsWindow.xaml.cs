using System.Windows;
using System.Windows.Controls;

namespace Kowin_Login_AlvaroLaguna
{
    public partial class SettingsWindow : Window
    {
        private string _usuarioActual;

        public SettingsWindow(string usuario)
        {
            InitializeComponent();
            _usuarioActual = usuario;
            CargarDatos();
        }

        private void CargarDatos()
        {
            Usuario u = UserManager.ObtenerUsuario(_usuarioActual);

            if (u != null)
            {
                lblUsuarioCabecera.Text = u.NombreUsuario.ToUpper();

                sldVolumen.Value = u.Volumen;
                chkNotificaciones.IsChecked = u.Notificaciones;

                chkInicioWindows.IsChecked = u.IniciarWindows;
                chkMinimizarBandeja.IsChecked = u.MinimizarBandeja;
                chkMostrarFPS.IsChecked = u.MostrarFps;
                chkInvisible.IsChecked = u.ModoInvisible;
                sldDescarga.Value = u.LimiteDescarga;

                foreach (ComboBoxItem item in cmbIdioma.Items)
                {
                    if (item.Content.ToString() == u.Idioma)
                    {
                        cmbIdioma.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            string idiomaSeleccionado = "Español";
            if (cmbIdioma.SelectedItem is ComboBoxItem item)
            {
                idiomaSeleccionado = item.Content.ToString();
            }

            Usuario u = new Usuario
            {
                NombreUsuario = _usuarioActual,
                Volumen = (int)sldVolumen.Value,
                Notificaciones = chkNotificaciones.IsChecked == true,
                Idioma = idiomaSeleccionado,
                IniciarWindows = chkInicioWindows.IsChecked == true,
                MinimizarBandeja = chkMinimizarBandeja.IsChecked == true,
                MostrarFps = chkMostrarFPS.IsChecked == true,
                ModoInvisible = chkInvisible.IsChecked == true,
                LimiteDescarga = (int)sldDescarga.Value
            };

            // AQUÍ ESTÁ EL CAMBIO: Recibimos el error (si lo hay)
            string error = UserManager.ActualizarConfiguracion(u);

            if (error == null)
            {
                // Si error es null, es que todo fue bien
                MessageBox.Show("Ajustes guardados correctamente en la BD.", "KoWin", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            else
            {
                // SI FALLA, TE DIRÁ EXACTAMENTE POR QUÉ
                MessageBox.Show("Error al guardar en BD:\n" + error, "Error Crítico", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}