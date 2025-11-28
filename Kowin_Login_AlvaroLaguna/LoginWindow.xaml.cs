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

            // TC 04: Campos vacíos
            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(pass))
            {
                MostrarError("Introduzca datos");
                return;
            }

            // Llamamos al validador nuevo
            ResultadoLogin resultado = UserManager.ValidarUsuario(usuario, pass);

            switch (resultado)
            {
                case ResultadoLogin.Exito:
                    // TC 01: Login Exitoso
                    MainWindow home = new MainWindow(usuario);
                    home.Show();
                    this.Close();
                    break;

                case ResultadoLogin.UsuarioNoExiste:
                    // TC 03: Usuario no existe
                    MostrarError("El usuario introducido no existe");
                    break;

                case ResultadoLogin.PasswordIncorrecto:
                    // TC 02: Contraseña mal
                    MostrarError("La contraseña introducida no es correcta");
                    break;

                case ResultadoLogin.UsuarioBloqueado:
                    // TC 07: Bloqueo
                    MostrarError("Usuario bloqueado temporalmente (1 min)");
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
            // TC 06: Salir de la aplicación
            Application.Current.Shutdown();
        }
    }
}