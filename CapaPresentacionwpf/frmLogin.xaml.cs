using CapaPresentacionWPF.Data; // o la ruta donde esté tu clase de acceso a datos
using System.Data.SqlClient;
using System.Windows;

namespace CapaPresentacionWPF
{
    public partial class frmLogin : Window
    {
        private UsuarioData _usuarioData = new UsuarioData();

        public frmLogin()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string usuario = txtUsuario.Text.Trim();
            string clave = txtClave.Password;

            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(clave))
            {
                MostrarError("Debe ingresar usuario y contraseña.");
                return;
            }

            var usuarioValido = _usuarioData.Login(usuario, clave);

            if (usuarioValido != null)
            {
                DialogResult = true; // Solo funciona si fue abierto con ShowDialog()
                Close();
            }
            else
            {
                MostrarError("Usuario o contraseña incorrectos.");
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MostrarError(string mensaje)
        {
            lblError.Text = mensaje;
            lblError.Visibility = Visibility.Visible;
        }
    }
}