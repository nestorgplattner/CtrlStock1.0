using System;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;

namespace CapaPresentacionWPF.UserControls
{
    /// <summary>
    /// Lógica de interacción para ucConfiguracion.xaml
    /// </summary>
    public partial class ucConfiguracion : UserControl
    {
        private string connectionString = @"Data Source=Data\stockdb.db;Version=3;";

        public ucConfiguracion()
        {
            InitializeComponent();
            LoadConfigurationData();
        }

        private void LoadConfigurationData()
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT * FROM Configuracion WHERE Id = 1";

                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtNombreEmpresa.Text = reader["NombreEmpresa"].ToString();
                                txtDireccion.Text = reader["Direccion"].ToString();
                                txtTelefono.Text = reader["Telefono"].ToString();
                                txtWebsite.Text = reader["Website"].ToString();
                                txtInstagram.Text = reader["Instagram"].ToString();
                                txtFacebook.Text = reader["Facebook"].ToString();
                                txtTiktok.Text = reader["Tiktok"].ToString();
                                txtMpToken.Text = reader["MpToken"].ToString();
                                txtMpStoreId.Text = reader["MpStoreId"].ToString();
                                txtMpPosId.Text = reader["MpPosId"].ToString();
                                txtArcaToken.Text = reader["ArcaToken"].ToString();
                                txtArcaKey.Text = reader["ArcaKey"].ToString();

                                // Cargar los campos de comisiones de Mercado Pago por tipo de tarjeta
                                if (reader["ComisionMP_Debito"] != DBNull.Value)
                                    txtComisionMP_Debito.Text = reader["ComisionMP_Debito"].ToString();
                                if (reader["ComisionMP_Credito"] != DBNull.Value)
                                    txtComisionMP_Credito.Text = reader["ComisionMP_Credito"].ToString();
                                if (reader["ComisionMP_Prepaga"] != DBNull.Value)
                                    txtComisionMP_Prepaga.Text = reader["ComisionMP_Prepaga"].ToString();
                                if (reader["ComisionMP_QR"] != DBNull.Value)
                                    txtComisionMP_QR.Text = reader["ComisionMP_QR"].ToString();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar la configuración: {ex.Message}", "Error de Carga", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"
                UPDATE Configuracion SET 
                    NombreEmpresa = @NombreEmpresa,
                    Direccion = @Direccion,
                    Telefono = @Telefono,
                    Website = @Website,
                    Instagram = @Instagram,
                    Facebook = @Facebook,
                    Tiktok = @Tiktok,
                    MpToken = @MpToken,
                    MpStoreId = @MpStoreId,
                    MpPosId = @MpPosId,
                    ArcaToken = @ArcaToken,
                    ArcaKey = @ArcaKey,
                    ComisionMP_Debito = @ComisionMP_Debito,
                    ComisionMP_Credito = @ComisionMP_Credito,
                    ComisionMP_Prepaga = @ComisionMP_Prepaga,
                    ComisionMP_QR = @ComisionMP_QR
                WHERE Id = 1";

                    // Calcular el valor final de cada comisión
                    double comisionDebito = double.Parse(txtComisionMP_Debito.Text, CultureInfo.InvariantCulture);
                    double comisionCredito = double.Parse(txtComisionMP_Credito.Text, CultureInfo.InvariantCulture);
                    double comisionPrepaga = double.Parse(txtComisionMP_Prepaga.Text, CultureInfo.InvariantCulture);
                    double comisionQR = double.Parse(txtComisionMP_QR.Text, CultureInfo.InvariantCulture);

                    double iva = 0.21; // 21%
                    double ibb = 0.03; // 3%

                    double totalDebito = comisionDebito + (comisionDebito * iva) + ibb;
                    double totalCredito = comisionCredito + (comisionCredito * iva) + ibb;
                    double totalPrepaga = comisionPrepaga + (comisionPrepaga * iva) + ibb;
                    double totalQR = comisionQR + (comisionQR * iva) + ibb;

                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@NombreEmpresa", txtNombreEmpresa.Text);
                        cmd.Parameters.AddWithValue("@Direccion", txtDireccion.Text);
                        cmd.Parameters.AddWithValue("@Telefono", txtTelefono.Text);
                        cmd.Parameters.AddWithValue("@Website", txtWebsite.Text);
                        cmd.Parameters.AddWithValue("@Instagram", txtInstagram.Text);
                        cmd.Parameters.AddWithValue("@Facebook", txtFacebook.Text);
                        cmd.Parameters.AddWithValue("@Tiktok", txtTiktok.Text);
                        cmd.Parameters.AddWithValue("@MpToken", txtMpToken.Text);
                        cmd.Parameters.AddWithValue("@MpStoreId", txtMpStoreId.Text);
                        cmd.Parameters.AddWithValue("@MpPosId", txtMpPosId.Text);
                        cmd.Parameters.AddWithValue("@ArcaToken", txtArcaToken.Text);
                        cmd.Parameters.AddWithValue("@ArcaKey", txtArcaKey.Text);

                        // Guardar los valores calculados en la base de datos
                        cmd.Parameters.AddWithValue("@ComisionMP_Debito", totalDebito);
                        cmd.Parameters.AddWithValue("@ComisionMP_Credito", totalCredito);
                        cmd.Parameters.AddWithValue("@ComisionMP_Prepaga", totalPrepaga);
                        cmd.Parameters.AddWithValue("@ComisionMP_QR", totalQR);

                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Configuración guardada exitosamente.", "Guardado Exitoso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al guardar la configuración: {ex.Message}", "Error de Guardado", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}