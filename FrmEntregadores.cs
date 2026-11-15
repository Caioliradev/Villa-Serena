using System;
using System.Collections.Generic;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using static Delivery.Dados;

namespace Delivery
{
    // CRUD completo da tabela entregadores.
    public class FrmEntregadores : Form
    {
        private readonly DataGridView grid = new DataGridView();
        private readonly Panel painel = new Panel { Dock = DockStyle.Bottom, Height = 130 };
        private readonly TextBox txtNome, txtTelefone, txtVeiculo, txtPlaca;
        private readonly CheckBox chkAtivo = new CheckBox { Text = "Ativo", Left = 10, Top = 78, AutoSize = true, Checked = true };
        private int idAtual = 0; // 0 = entregador novo

        public FrmEntregadores()
        {
            Text = "Entregadores";
            Width = 900;
            Height = 560;
            StartPosition = FormStartPosition.CenterParent;

            Ui.ConfigurarGrid(grid);
            Controls.Add(grid);
            Controls.Add(painel);

            txtNome = Ui.Campo(painel, "Nome *", 10, 8, 250);
            txtTelefone = Ui.Campo(painel, "Telefone *", 270, 8, 150);
            txtVeiculo = Ui.Campo(painel, "Veículo", 430, 8, 120);
            txtPlaca = Ui.Campo(painel, "Placa", 560, 8, 100);
            painel.Controls.Add(chkAtivo);

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
                    "SELECT id AS Id, nome AS Nome, telefone AS Telefone, veiculo AS Veiculo, placa AS Placa, ativo AS Ativo FROM entregadores ORDER BY nome");
                if (selecionarId > 0) Ui.SelecionarId(grid, selecionarId);
                MostrarSelecionado();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar entregadores: " + ex.Message);
            }
        }

        private void MostrarSelecionado()
        {
            if (grid.SelectedRows.Count == 0) return;
            DataGridViewRow l = grid.SelectedRows[0];
            idAtual = Convert.ToInt32(l.Cells["Id"].Value);
            txtNome.Text = Convert.ToString(l.Cells["Nome"].Value);
            txtTelefone.Text = Convert.ToString(l.Cells["Telefone"].Value);
            txtVeiculo.Text = Convert.ToString(l.Cells["Veiculo"].Value);
            txtPlaca.Text = Convert.ToString(l.Cells["Placa"].Value);
            chkAtivo.Checked = Convert.ToBoolean(l.Cells["Ativo"].Value);
        }

        private void Novo()
        {
            grid.ClearSelection();
            idAtual = 0;
            txtNome.Clear(); txtTelefone.Clear(); txtVeiculo.Clear(); txtPlaca.Clear();
            chkAtivo.Checked = true;
            txtNome.Focus();
        }

        private void Salvar()
        {
            if (txtNome.Text.Trim() == "" || txtTelefone.Text.Trim() == "")
            {
                MessageBox.Show("Nome e telefone são obrigatórios.");
                return;
            }

            List<MySqlParameter> ps = new List<MySqlParameter>
            {
                P("@n", txtNome.Text.Trim()),
                P("@t", txtTelefone.Text.Trim()),
                P("@v", OuNulo(txtVeiculo.Text)),
                P("@p", OuNulo(txtPlaca.Text)),
                P("@a", chkAtivo.Checked)
            };

            try
            {
                if (idAtual == 0)
                {
                    idAtual = (int)Inserir("INSERT INTO entregadores (nome, telefone, veiculo, placa, ativo) VALUES (@n, @t, @v, @p, @a)", ps.ToArray());
                }
                else
                {
                    ps.Add(P("@id", idAtual));
                    Executar("UPDATE entregadores SET nome = @n, telefone = @t, veiculo = @v, placa = @p, ativo = @a WHERE id = @id", ps.ToArray());
                }
                Carregar(idAtual);
                MessageBox.Show("Entregador salvo.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao salvar: " + ex.Message);
            }
        }

        private void Excluir()
        {
            if (idAtual == 0) { MessageBox.Show("Selecione um entregador."); return; }

            if (MessageBox.Show("Excluir o entregador \"" + txtNome.Text + "\"? Os pedidos dele ficarão sem entregador.",
                    "Confirmar exclusão", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                Executar("DELETE FROM entregadores WHERE id = @id", P("@id", idAtual));
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
