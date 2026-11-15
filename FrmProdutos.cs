using System;
using System.Data;
using System.Globalization;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using static Delivery.Dados;

namespace Delivery
{
    // Visualizar, ALTERAR e EXCLUIR produtos. Não existe "novo produto":
    // os produtos são cadastrados manualmente pelo script SQL.
    public class FrmProdutos : Form
    {
        private readonly DataGridView grid = new DataGridView();
        private readonly Panel painel = new Panel { Dock = DockStyle.Bottom, Height = 150 };
        private readonly TextBox txtCodigo, txtNome, txtDescricao, txtPreco, txtImagem;
        private readonly ComboBox cboCategoria = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Left = 170, Top = 26, Width = 160 };
        private readonly CheckBox chkAtivo = new CheckBox { Text = "Ativo", Left = 10, Top = 108, AutoSize = true };
        private int idAtual = 0;

        public FrmProdutos()
        {
            Text = "Produtos";
            Width = 1000;
            Height = 650;
            StartPosition = FormStartPosition.CenterParent;

            Ui.ConfigurarGrid(grid);
            Controls.Add(grid);
            Controls.Add(painel);

            txtCodigo = Ui.Campo(painel, "Código (site)", 10, 8, 150);
            txtCodigo.ReadOnly = true; // o código liga o produto ao JSON do site, não deve mudar
            Ui.Rotulo(painel, "Categoria", 170, 8);
            painel.Controls.Add(cboCategoria);
            txtNome = Ui.Campo(painel, "Nome", 340, 8, 250);
            txtPreco = Ui.Campo(painel, "Preço", 600, 8, 90);
            txtDescricao = Ui.Campo(painel, "Descrição", 10, 56, 410);
            txtImagem = Ui.Campo(painel, "Imagem", 430, 56, 260);
            painel.Controls.Add(chkAtivo);

            Button btnSalvar = Ui.Botao(painel, "Salvar alteração", 400, 104);
            Button btnExcluir = Ui.Botao(painel, "Excluir", 510, 104);
            btnSalvar.Width = 105;
            btnSalvar.Click += (s, e) => Salvar();
            btnExcluir.Click += (s, e) => Excluir();

            grid.SelectionChanged += (s, e) => MostrarSelecionado();
            Load += (s, e) => { CarregarCategorias(); Carregar(0); };
        }

        private void CarregarCategorias()
        {
            cboCategoria.DisplayMember = "nome";
            cboCategoria.ValueMember = "id";
            cboCategoria.DataSource = Consultar("SELECT CAST(id AS SIGNED) AS id, nome FROM categorias ORDER BY id");
        }

        private void Carregar(int selecionarId)
        {
            try
            {
                grid.DataSource = Consultar(
                    @"SELECT p.id AS Id, c.nome AS Categoria, p.codigo AS Codigo, p.nome AS Nome,
                             p.descricao AS Descricao, p.preco AS Preco, p.imagem AS Imagem,
                             p.ativo AS Ativo, p.categoria_id AS CategoriaId
                      FROM produtos p
                      JOIN categorias c ON c.id = p.categoria_id
                      ORDER BY c.id, p.nome");
                grid.Columns["CategoriaId"].Visible = false;
                grid.Columns["Imagem"].Visible = false;
                grid.Columns["Preco"].DefaultCellStyle.Format = "C2";
                if (selecionarId > 0) Ui.SelecionarId(grid, selecionarId);
                MostrarSelecionado();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar produtos: " + ex.Message);
            }
        }

        private void MostrarSelecionado()
        {
            if (grid.SelectedRows.Count == 0) { idAtual = 0; return; }

            DataGridViewRow l = grid.SelectedRows[0];
            idAtual = Convert.ToInt32(l.Cells["Id"].Value);
            txtCodigo.Text = Convert.ToString(l.Cells["Codigo"].Value);
            txtNome.Text = Convert.ToString(l.Cells["Nome"].Value);
            txtDescricao.Text = Convert.ToString(l.Cells["Descricao"].Value);
            txtImagem.Text = Convert.ToString(l.Cells["Imagem"].Value);
            txtPreco.Text = Convert.ToDecimal(l.Cells["Preco"].Value).ToString("0.00");
            chkAtivo.Checked = Convert.ToBoolean(l.Cells["Ativo"].Value);
            cboCategoria.SelectedValue = Convert.ToInt64(l.Cells["CategoriaId"].Value);
        }

        private bool LerPreco(out decimal preco)
        {
            string t = txtPreco.Text.Trim();
            return (decimal.TryParse(t, NumberStyles.Number, CultureInfo.CurrentCulture, out preco)
                 || decimal.TryParse(t, NumberStyles.Number, CultureInfo.InvariantCulture, out preco))
                 && preco >= 0;
        }

        private void Salvar()
        {
            if (idAtual == 0) { MessageBox.Show("Selecione um produto."); return; }
            if (txtNome.Text.Trim() == "" || txtDescricao.Text.Trim() == "")
            {
                MessageBox.Show("Nome e descrição são obrigatórios.");
                return;
            }
            if (!LerPreco(out decimal preco))
            {
                MessageBox.Show("Preço inválido.");
                return;
            }

            try
            {
                Executar(@"UPDATE produtos
                           SET categoria_id = @c, nome = @n, descricao = @d,
                               preco = @p, imagem = @i, ativo = @a
                           WHERE id = @id",
                    P("@c", Convert.ToInt64(cboCategoria.SelectedValue)),
                    P("@n", txtNome.Text.Trim()),
                    P("@d", txtDescricao.Text.Trim()),
                    P("@p", preco),
                    P("@i", OuNulo(txtImagem.Text)),
                    P("@a", chkAtivo.Checked),
                    P("@id", idAtual));
                Carregar(idAtual);
                MessageBox.Show("Produto atualizado.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao salvar: " + ex.Message);
            }
        }

        private void Excluir()
        {
            if (idAtual == 0) { MessageBox.Show("Selecione um produto."); return; }

            if (MessageBox.Show("Excluir o produto \"" + txtNome.Text + "\"?", "Confirmar exclusão",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                Executar("DELETE FROM produtos WHERE id = @id", P("@id", idAtual));
                Carregar(0);
            }
            catch (MySqlException ex) when (ex.Number == 1451)
            {
                // Produto já foi vendido: o banco não deixa apagar (integridade referencial).
                if (MessageBox.Show("Este produto já aparece em pedidos e não pode ser excluído.\nDeseja apenas desativá-lo?",
                        "Produto em uso", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Executar("UPDATE produtos SET ativo = 0 WHERE id = @id", P("@id", idAtual));
                    Carregar(idAtual);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao excluir: " + ex.Message);
            }
        }
    }
}
