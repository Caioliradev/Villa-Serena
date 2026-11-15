using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Delivery
{
    public partial class FrmPrincipal : Form
    {
        public FrmPrincipal()
        {
            InitializeComponent();

            var menu = new MenuStrip();
            menu.Items.Add("Pedidos", null, (s, e) => Abrir(new FrmPedidos()));
            menu.Items.Add("Produtos", null, (s, e) => Abrir(new FrmProdutos()));
            menu.Items.Add("Clientes", null, (s, e) => Abrir(new FrmClientes()));
            menu.Items.Add("Entregadores", null, (s, e) => Abrir(new FrmEntregadores()));
            MainMenuStrip = menu;
            Controls.Add(menu);
            Tema.Aplicar(this);
        }

        private void Abrir(Form f)
        {
            using (f) { f.ShowDialog(this); }
        }

        private void btnImportar_Click(object sender, EventArgs e)
        {
            using (var dialogo = new OpenFileDialog())
            {
                dialogo.Filter = "Arquivos JSON|*.json";
                if (dialogo.ShowDialog() != DialogResult.OK) return;

                try
                {
                    var (importados, ignorados) = ImportadorPedidos.Importar(dialogo.FileName);
                    MessageBox.Show($"{importados} importados, {ignorados} ignorados.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro ao importar: " + ex.Message);
                }
            }
        }
    }
}
