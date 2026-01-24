using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Kowin_Login_AlvaroLaguna
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string usuario = txtUser.Text;
            string pass = txtPass.Password;

            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(pass))
            {
                MostrarError("Introduzca datos");
                return;
            }

            ResultadoLogin resultado = UserManager.ValidarUsuario(usuario, pass);

            switch (resultado)
            {
                case ResultadoLogin.Exito:
                    // CAMBIO: Ahora SIEMPRE vamos al Home, sea Admin o Nominal.
                    // El Home ya decidirá si muestra el botón de Admin.
                    MessageBox.Show($"Bienvenido {usuario}.", "Acceso Correcto", MessageBoxButton.OK, MessageBoxImage.Information);

                    MainWindow home = new MainWindow(usuario);
                    home.Show();
                    this.Close();
                    break;

                case ResultadoLogin.UsuarioBaneado:
                    MessageBox.Show("Tu cuenta ha sido BANEADA por un administrador.\nNo puedes acceder.", "Acceso Denegado", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;

                case ResultadoLogin.UsuarioNoExiste:
                    MessageBox.Show($"[MySQL] El usuario '{usuario}' NO existe.", "Error Login", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;

                case ResultadoLogin.PasswordIncorrecto:
                    MessageBox.Show($"[MySQL] Contraseña incorrecta.", "Error Login", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;

                case ResultadoLogin.UsuarioBloqueado:
                    MessageBox.Show("[MySQL] Usuario bloqueado temporalmente.", "Acceso Denegado", MessageBoxButton.OK, MessageBoxImage.Warning);
                    break;

                case ResultadoLogin.ErrorBaseDatos:
                    MessageBox.Show("Error crítico: No se pudo conectar al servidor MySQL.", "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Hand);
                    break;
            }
        }

        private void MostrarError(string mensaje)
        {
            lblError.Text = mensaje;
            lblError.Visibility = Visibility.Visible;
        }

        private void BtnIrARegistro_Click(object sender, MouseButtonEventArgs e)
        {
            SignUpWindow registro = new SignUpWindow();
            registro.Show();
            this.Close();
        }

        private void BtnSalir_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}