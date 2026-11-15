using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace Delivery
{
    public partial class FrmLogin : Form
    {
        public FrmLogin()
        {
            InitializeComponent();
            Tema.Aplicar(this);
            btnEntrar.Click += btnEntrar_Click;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnEntrar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEmail.Text) || string.IsNullOrWhiteSpace(txtSenha.Text))
            {
                MessageBox.Show("Preencha o e-mail e a senha.");
                return;
            }

            string sql = "SELECT id, nome FROM usuarios WHERE email = @email AND senha_hash = SHA2(@senha, 256)";

            try
            {
                using (var conn = Conexao.ObterConexao())
                {
                    conn.Open();

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@email", txtEmail.Text.Trim());
                        cmd.Parameters.AddWithValue("@senha", txtSenha.Text);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var principal = new FrmPrincipal();
                                principal.FormClosed += (s, args) => Application.Exit();
                                principal.Show();
                                this.Hide();
                            }
                            else
                            {
                                MessageBox.Show("E-mail ou senha inválidos.");
                            }
                        }
                    }
                }
            }
            catch (MySqlException)
            {
                MessageBox.Show("Não foi possível conectar ao banco de dados.");
            }
        }

        
    }
}