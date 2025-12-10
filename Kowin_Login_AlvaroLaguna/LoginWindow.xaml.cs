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

            // Llamamos al validador de MySQL
            ResultadoLogin resultado = UserManager.ValidarUsuario(usuario, pass);

            switch (resultado)
            {
                case ResultadoLogin.Exito:
                    // === EVIDENCIA 1: LOGIN EXITOSO ===
                    // Mostramos un mensaje explícito confirmando la conexión a MySQL
                    MessageBox.Show($"[MySQL] Conexión Exitosa.\nSe ha verificado el usuario '{usuario}' en la base de datos 'kowin_db'.\nAcceso Concedido.",
                                    "Verificación MySQL - Éxito",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);

                    // Abrimos el Home
                    MainWindow home = new MainWindow(usuario);
                    home.Show();
                    this.Close();
                    break;

                case ResultadoLogin.UsuarioNoExiste:
                    // === EVIDENCIA 2: USUARIO NO ENCONTRADO ===
                    MessageBox.Show($"[MySQL] Consulta Finalizada.\nEl usuario '{usuario}' NO existe en la tabla 'usuarios'.",
                                    "Verificación MySQL - Fallo",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                    break;

                case ResultadoLogin.PasswordIncorrecto:
                    // === EVIDENCIA 2: CONTRASEÑA INCORRECTA ===
                    MessageBox.Show($"[MySQL] Consulta Finalizada.\nEl usuario existe, pero la contraseña no coincide con el registro en la base de datos.",
                                    "Verificación MySQL - Fallo",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                    break;

                case ResultadoLogin.UsuarioBloqueado:
                    // === EVIDENCIA: BLOQUEO ===
                    MessageBox.Show("[MySQL] Acceso Denegado.\nEl campo 'fecha_fin_bloqueo' indica que este usuario está temporalmente bloqueado.",
                                    "Verificación MySQL - Bloqueo",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
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