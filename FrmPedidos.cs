using System;
using System.Data;
using System.Windows.Forms;
using static Delivery.Dados;

namespace Delivery
{
    public class FrmPedidos : Form
    {
        private readonly DataGridView grid = new DataGridView();
        private readonly DataGridView gridItens = new DataGridView();
        private readonly Panel painel = new Panel { Dock = DockStyle.Bottom, Height = 60 };
        private readonly ComboBox cboStatus = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Left = 10, Top = 26, Width = 180 };
        private readonly ComboBox cboEntregador = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Left = 200, Top = 26, Width = 220 };
        private int idAtual = 0;

        public FrmPedidos()
        {
            Text = "Pedidos";
            Width = 1050;
            Height = 650;
            StartPosition = FormStartPosition.CenterParent;

            Ui.ConfigurarGrid(grid);
            Ui.ConfigurarGrid(gridItens);
            gridItens.Dock = DockStyle.Bottom;
            gridItens.Height = 150;

            // O controle com Dock=Fill deve ser adicionado primeiro.
            Controls.Add(grid);
            Controls.Add(gridItens);
            Controls.Add(painel);

            cboStatus.Items.AddRange(new object[] { "pendente", "preparando", "saiu_para_entrega", "entregue", "cancelado" });
            Ui.Rotulo(painel, "Status", 10, 8);
            Ui.Rotulo(painel, "Entregador", 200, 8);
            painel.Controls.Add(cboStatus);
            painel.Controls.Add(cboEntregador);

            Button btnSalvar = Ui.Botao(painel, "Salvar", 440, 18);
            Button btnExcluir = Ui.Botao(painel, "Excluir pedido", 550, 18);
            Button btnRecarregar = Ui.Botao(painel, "Recarregar", 660, 18);
            btnSalvar.Click += (s, e) => Salvar();
            btnExcluir.Click += (s, e) => Excluir();
            btnRecarregar.Click += (s, e) => Carregar(idAtual);

            grid.SelectionChanged += (s, e) => MostrarSelecionado();
            Load += (s, e) => { CarregarEntregadores(); Carregar(0); };
            Tema.Aplicar(this);
        }

        private void CarregarEntregadores()
        {
            DataTable dt = Consultar("SELECT CAST(id AS SIGNED) AS id, nome FROM entregadores WHERE ativo = 1 ORDER BY nome");
            DataRow vazio = dt.NewRow();
            vazio["id"] = 0;
            vazio["nome"] = "(sem entregador)";
            dt.Rows.InsertAt(vazio, 0);
            cboEntregador.DisplayMember = "nome";
            cboEntregador.ValueMember = "id";
            cboEntregador.DataSource = dt;
        }

        private void Carregar(int selecionarId)
        {
            try
            {
                grid.DataSource = Consultar(
                    @"SELECT p.id AS Id, p.id_externo AS Pedido, p.nome_cliente AS Cliente,
                             p.telefone AS Telefone, p.endereco_entrega AS Endereco,
                             p.status AS Status, e.nome AS Entregador, p.total AS Total,
                             p.criado_em AS Data,
                             CAST(COALESCE(p.entregador_id, 0) AS SIGNED) AS EntregadorId
                      FROM pedidos p
                      LEFT JOIN entregadores e ON e.id = p.entregador_id
                      ORDER BY p.criado_em DESC");
                grid.Columns["EntregadorId"].Visible = false;
                grid.Columns["Total"].DefaultCellStyle.Format = "C2";
                grid.Columns["Data"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                if (selecionarId > 0) Ui.SelecionarId(grid, selecionarId);
                MostrarSelecionado();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar pedidos: " + ex.Message);
            }
        }

        private void MostrarSelecionado()
        {
            if (grid.SelectedRows.Count == 0)
            {
                idAtual = 0;
                gridItens.DataSource = null;
                return;
            }

            DataGridViewRow linha = grid.SelectedRows[0];
            idAtual = Convert.ToInt32(linha.Cells["Id"].Value);
            cboStatus.SelectedItem = Convert.ToString(linha.Cells["Status"].Value);
            cboEntregador.SelectedValue = Convert.ToInt64(linha.Cells["EntregadorId"].Value);

            gridItens.DataSource = Consultar(
                @"SELECT pr.nome AS Produto, i.quantidade AS Qtd,
                         i.preco_unitario AS Preco, i.quantidade * i.preco_unitario AS Subtotal
                  FROM itens_pedido i
                  JOIN produtos pr ON pr.id = i.produto_id
                  WHERE i.pedido_id = @id", P("@id", idAtual));
            gridItens.Columns["Preco"].DefaultCellStyle.Format = "C2";
            gridItens.Columns["Subtotal"].DefaultCellStyle.Format = "C2";
        }

        private void Salvar()
        {
            if (idAtual == 0 || cboStatus.SelectedItem == null)
            {
                MessageBox.Show("Selecione um pedido.");
                return;
            }

            long entregador = Convert.ToInt64(cboEntregador.SelectedValue);
            try
            {
                Executar("UPDATE pedidos SET status = @s, entregador_id = @e WHERE id = @id",
                    P("@s", cboStatus.SelectedItem.ToString()),
                    P("@e", entregador == 0 ? (object)DBNull.Value : entregador),
                    P("@id", idAtual));
                Carregar(idAtual);
                MessageBox.Show("Pedido atualizado.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao salvar: " + ex.Message);
            }
        }

        private void InitializeComponent()
        {

        }

        private void Excluir()
        {
            if (idAtual == 0)
            {
                MessageBox.Show("Selecione um pedido.");
                return;
            }

            string nome = Convert.ToString(grid.SelectedRows[0].Cells["Pedido"].Value);
            if (MessageBox.Show("Excluir o pedido " + nome + "? Os itens e pagamentos dele também serão removidos.",
                    "Confirmar exclusão", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                Executar("DELETE FROM pedidos WHERE id = @id", P("@id", idAtual));
                Carregar(0);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao excluir: " + ex.Message);
            }
        }
    }
}
