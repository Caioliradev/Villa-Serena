using System;
using System.Collections.Generic;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using static Delivery.Dados;

namespace Delivery
{
    // CRUD completo da tabela clientes.
    public class FrmClientes : Form
    {
        private readonly DataGridView grid = new DataGridView();
        private readonly Panel painel = new Panel { Dock = DockStyle.Bottom, Height = 130 };
        private readonly TextBox txtNome, txtEmail, txtTelefone, txtEndereco;
        private int idAtual = 0; // 0 = cliente novo

        public FrmClientes()
        {
            Text = "Clientes";
            Width = 900;
            Height = 560;
            StartPosition = FormStartPosition.CenterParent;

            Ui.ConfigurarGrid(grid);
            Controls.Add(grid);
            Controls.Add(painel);

            txtNome = Ui.Campo(painel, "Nome *", 10, 8, 250);
            txtEmail = Ui.Campo(painel, "E-mail", 270, 8, 250);
            txtTelefone = Ui.Campo(painel, "Telefone", 530, 8, 150);
            txtEndereco = Ui.Campo(painel, "Endereço", 10, 56, 510);

            Button btnNovo = Ui.Botao(painel, "Novo", 530, 72);
            Button btnSalvar = Ui.Botao(painel, "Salvar", 635, 72);
            Button btnExcluir = Ui.Botao(painel, "Excluir", 740, 72);
            btnNovo.Click += (s, e) => Novo();
            btnSalvar.Click += (s, e) => Salvar();
            btnExcluir.Click += (s, e) => Excluir();

            grid.SelectionChanged += (s, e) => MostrarSelecionado();
            Load += (s, e) => Carregar(0);
        }

        private void Carregar(int selecionarId)
        {
            try
            {
                grid.DataSource = Consultar(
                    "SELECT id AS Id, nome AS Nome, email AS Email, telefone AS Telefone, endereco AS Endereco FROM clientes ORDER BY nome");
                if (selecionarId > 0) Ui.SelecionarId(grid, selecionarId);
                MostrarSelecionado();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar clientes: " + ex.Message);
            }
        }

        private void MostrarSelecionado()
        {
            if (grid.SelectedRows.Count == 0) return;
            DataGridViewRow l = grid.SelectedRows[0];
            idAtual = Convert.ToInt32(l.Cells["Id"].Value);
            txtNome.Text = Convert.ToString(l.Cells["Nome"].Value);
            txtEmail.Text = Convert.ToString(l.Cells["Email"].Value);
            txtTelefone.Text = Convert.ToString(l.Cells["Telefone"].Value);
            txtEndereco.Text = Convert.ToString(l.Cells["Endereco"].Value);
        }

        private void Novo()
        {
            grid.ClearSelection();
            idAtual = 0;
            txtNome.Clear(); txtEmail.Clear(); txtTelefone.Clear(); txtEndereco.Clear();
            txtNome.Focus();
        }

        private void Salvar()
        {
            if (txtNome.Text.Trim() == "")
            {
                MessageBox.Show("Informe o nome do cliente.");
                return;
            }

            List<MySqlParameter> ps = new List<MySqlParameter>
            {
                P("@n", txtNome.Text.Trim()),
                P("@e", OuNulo(txtEmail.Text)),
                P("@t", OuNulo(txtTelefone.Text)),
                P("@d", OuNulo(txtEndereco.Text))
            };

            try
            {
                if (idAtual == 0)
                {
                    idAtual = (int)Inserir("INSERT INTO clientes (nome, email, telefone, endereco) VALUES (@n, @e, @t, @d)", ps.ToArray());
                }
                else
                {
                    ps.Add(P("@id", idAtual));
                    Executar("UPDATE clientes SET nome = @n, email = @e, telefone = @t, endereco = @d WHERE id = @id", ps.ToArray());
                }
                Carregar(idAtual);
                MessageBox.Show("Cliente salvo.");
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                MessageBox.Show("Já existe um cliente com esse e-mail.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao salvar: " + ex.Message);
            }
        }

        private void Excluir()
        {
            if (idAtual == 0) { MessageBox.Show("Selecione um cliente."); return; }

            if (MessageBox.Show("Excluir o cliente \"" + txtNome.Text + "\"?", "Confirmar exclusão",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                Executar("DELETE FROM clientes WHERE id = @id", P("@id", idAtual));
                idAtual = 0;
                Carregar(0);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao excluir: " + ex.Message);
            }
        }
    }
}
