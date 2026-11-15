using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace Delivery
{
    // Único ponto de acesso ao banco. Todos os formulários usam esta classe.
    // Se a sua classe Conexao for diferente, ajuste SOMENTE o método Abrir().
    public static class Dados
    {
        private static MySqlConnection Abrir()
        {
            MySqlConnection cn = Conexao.ObterConexao();
            if (cn.State != ConnectionState.Open) cn.Open();
            return cn;
        }

        public static MySqlParameter P(string nome, object valor)
        {
            return new MySqlParameter(nome, valor ?? DBNull.Value);
        }

        // Texto vazio vira NULL no banco (necessário para colunas UNIQUE opcionais, como e-mail).
        public static object OuNulo(string texto)
        {
            return string.IsNullOrWhiteSpace(texto) ? (object)DBNull.Value : texto.Trim();
        }

        public static DataTable Consultar(string sql, params MySqlParameter[] parametros)
        {
            using (MySqlConnection cn = Abrir())
            using (MySqlCommand cmd = new MySqlCommand(sql, cn))
            {
                cmd.Parameters.AddRange(parametros);
                DataTable tabela = new DataTable();
                using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                {
                    da.Fill(tabela);
                }
                return tabela;
            }
        }

        public static int Executar(string sql, params MySqlParameter[] parametros)
        {
            using (MySqlConnection cn = Abrir())
            using (MySqlCommand cmd = new MySqlCommand(sql, cn))
            {
                cmd.Parameters.AddRange(parametros);
                return cmd.ExecuteNonQuery();
            }
        }

        // Executa um INSERT e devolve o id gerado.
        public static long Inserir(string sql, params MySqlParameter[] parametros)
        {
            using (MySqlConnection cn = Abrir())
            using (MySqlCommand cmd = new MySqlCommand(sql, cn))
            {
                cmd.Parameters.AddRange(parametros);
                cmd.ExecuteNonQuery();
                return cmd.LastInsertedId;
            }
        }
    }

    // Pequenos auxiliares para montar as telas por código (sem precisar do Designer).
    public static class Ui
    {
        public static void ConfigurarGrid(DataGridView g)
        {
            g.Dock = DockStyle.Fill;
            g.ReadOnly = true;
            g.AllowUserToAddRows = false;
            g.AllowUserToDeleteRows = false;
            g.MultiSelect = false;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            g.RowHeadersVisible = false;
            g.BackgroundColor = SystemColors.Window;
        }

        public static Label Rotulo(Control pai, string texto, int x, int y)
        {
            Label lbl = new Label { Text = texto, Left = x, Top = y, AutoSize = true };
            pai.Controls.Add(lbl);
            return lbl;
        }

        public static TextBox Campo(Control pai, string rotulo, int x, int y, int largura)
        {
            Rotulo(pai, rotulo, x, y);
            TextBox txt = new TextBox { Left = x, Top = y + 18, Width = largura };
            pai.Controls.Add(txt);
            return txt;
        }

        public static Button Botao(Control pai, string texto, int x, int y)
        {
            Button b = new Button { Text = texto, Left = x, Top = y, Width = 100, Height = 30 };
            pai.Controls.Add(b);
            return b;
        }

        public static void SelecionarId(DataGridView g, int id)
        {
            foreach (DataGridViewRow r in g.Rows)
            {
                if (Convert.ToInt32(r.Cells["Id"].Value) == id)
                {
                    g.CurrentCell = r.Cells["Id"];
                    r.Selected = true;
                    return;
                }
            }
        }
    }
}
