using System;
using System.Drawing;
using System.Windows.Forms;

namespace Delivery
{
    // Visual do aplicativo igual ao do site Villa Serena (tema escuro: verde escuro + dourado).
    // Uso: chamar Tema.Aplicar(this); no final do construtor de cada formulário.
    public static class Tema
    {
        // Cores copiadas do style.css do site
        public static readonly Color Fundo     = ColorTranslator.FromHtml("#0F2A24"); // --bg
        public static readonly Color Painel    = ColorTranslator.FromHtml("#0B211C");
        public static readonly Color Campo     = ColorTranslator.FromHtml("#143A32");
        public static readonly Color LinhaAlt  = ColorTranslator.FromHtml("#123229");
        public static readonly Color Cabecalho = ColorTranslator.FromHtml("#1B4A3F");
        public static readonly Color Borda     = ColorTranslator.FromHtml("#285247"); // --borda
        public static readonly Color Texto     = ColorTranslator.FromHtml("#F4F1E8"); // --texto
        public static readonly Color TextoMudo = ColorTranslator.FromHtml("#A3B3A9"); // --texto-mudo
        public static readonly Color Destaque  = ColorTranslator.FromHtml("#E8B765"); // --destaque (dourado)
        public static readonly Color DestaqueClaro = ColorTranslator.FromHtml("#F0C987");
        public static readonly Color TextoBotao = ColorTranslator.FromHtml("#1A1208"); // --btn-texto

        public static void Aplicar(Form f)
        {
            f.BackColor = Fundo;
            f.ForeColor = Texto;
            f.Font = new Font("Arial", 9.5f);
            Percorrer(f.Controls);
        }

        private static void Percorrer(Control.ControlCollection controles)
        {
            foreach (Control c in controles)
            {
                Estilizar(c);
                if (c.HasChildren) Percorrer(c.Controls);
            }
        }

        private static void Estilizar(Control c)
        {
            switch (c)
            {
                case DataGridView g:
                    EstilizarGrade(g);
                    break;

                case Button b:
                    EstilizarBotao(b);
                    break;

                case ComboBox cb:
                    cb.FlatStyle = FlatStyle.Flat;
                    cb.BackColor = Campo;
                    cb.ForeColor = Texto;
                    break;

                case TextBox tb:
                    tb.BorderStyle = BorderStyle.FixedSingle;
                    tb.BackColor = tb.ReadOnly ? Fundo : Campo;
                    tb.ForeColor = tb.ReadOnly ? TextoMudo : Texto;
                    break;

                case CheckBox ck:
                    ck.BackColor = Color.Transparent;
                    ck.ForeColor = Texto;
                    break;

                case Label lb:
                    lb.BackColor = Color.Transparent;
                    lb.ForeColor = TextoMudo;
                    break;

                case MenuStrip ms:
                    EstilizarMenu(ms);
                    break;

                case Panel p:
                    p.BackColor = Painel;
                    break;
            }
        }

        private static void EstilizarBotao(Button b)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.UseVisualStyleBackColor = false;
            b.Cursor = Cursors.Hand;
            b.FlatAppearance.BorderSize = 1;

            // Botões secundários ficam só com contorno; os principais ficam dourados
            bool secundario = b.Text.StartsWith("Excluir", StringComparison.OrdinalIgnoreCase)
                           || b.Text.StartsWith("Novo", StringComparison.OrdinalIgnoreCase)
                           || b.Text.StartsWith("Recarregar", StringComparison.OrdinalIgnoreCase);

            if (secundario)
            {
                b.BackColor = Fundo;
                b.ForeColor = Texto;
                b.FlatAppearance.BorderColor = Borda;
                b.FlatAppearance.MouseOverBackColor = Campo;
                b.FlatAppearance.MouseDownBackColor = Cabecalho;
            }
            else
            {
                b.BackColor = Destaque;
                b.ForeColor = TextoBotao;
                b.FlatAppearance.BorderColor = Destaque;
                b.FlatAppearance.MouseOverBackColor = DestaqueClaro;
                b.FlatAppearance.MouseDownBackColor = DestaqueClaro;
            }
        }

        private static void EstilizarGrade(DataGridView g)
        {
            g.EnableHeadersVisualStyles = false;
            g.BackgroundColor = Fundo;
            g.BorderStyle = BorderStyle.None;
            g.GridColor = Borda;
            g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            g.ColumnHeadersHeight = 34;
            g.RowTemplate.Height = 30;

            g.ColumnHeadersDefaultCellStyle.BackColor = Cabecalho;
            g.ColumnHeadersDefaultCellStyle.ForeColor = Destaque;
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Cabecalho;
            g.ColumnHeadersDefaultCellStyle.SelectionForeColor = Destaque;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 9.5f, FontStyle.Bold);
            g.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 0, 0);

            g.DefaultCellStyle.BackColor = Fundo;
            g.DefaultCellStyle.ForeColor = Texto;
            g.DefaultCellStyle.SelectionBackColor = Destaque;
            g.DefaultCellStyle.SelectionForeColor = TextoBotao;
            g.DefaultCellStyle.Padding = new Padding(6, 0, 0, 0);

            g.AlternatingRowsDefaultCellStyle.BackColor = LinhaAlt;
            g.AlternatingRowsDefaultCellStyle.ForeColor = Texto;
            g.AlternatingRowsDefaultCellStyle.SelectionBackColor = Destaque;
            g.AlternatingRowsDefaultCellStyle.SelectionForeColor = TextoBotao;
        }

        private static void EstilizarMenu(MenuStrip ms)
        {
            ms.Renderer = new ToolStripProfessionalRenderer(new CoresMenu());
            ms.BackColor = Painel;
            ms.ForeColor = Texto;
            ms.Padding = new Padding(8, 4, 8, 4);
            foreach (ToolStripItem item in ms.Items)
            {
                item.ForeColor = Texto;
                item.Padding = new Padding(10, 6, 10, 6);
            }
        }

        private class CoresMenu : ProfessionalColorTable
        {
            public override Color MenuStripGradientBegin => Painel;
            public override Color MenuStripGradientEnd => Painel;
            public override Color MenuItemSelected => Campo;
            public override Color MenuItemSelectedGradientBegin => Campo;
            public override Color MenuItemSelectedGradientEnd => Campo;
            public override Color MenuItemPressedGradientBegin => Cabecalho;
            public override Color MenuItemPressedGradientMiddle => Cabecalho;
            public override Color MenuItemPressedGradientEnd => Cabecalho;
            public override Color MenuItemBorder => Borda;
            public override Color MenuBorder => Borda;
            public override Color ToolStripDropDownBackground => Painel;
        }
    }
}
