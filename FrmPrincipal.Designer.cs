namespace Delivery
{
    partial class FrmPrincipal
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            btnImportar = new Button();
            SuspendLayout();
            // 
            // btnImportar
            // 
            btnImportar.Location = new Point(321, 193);
            btnImportar.Name = "btnImportar";
            btnImportar.Size = new Size(135, 23);
            btnImportar.TabIndex = 0;
            btnImportar.Text = "Importar pedidos.";
            btnImportar.UseVisualStyleBackColor = true;
            btnImportar.Click += btnImportar_Click;
            // 
            // FrmPrincipal
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnImportar);
            Name = "FrmPrincipal";
            Text = "FrmPrincipal";
            ResumeLayout(false);
        }

        #endregion

        private Button btnImportar;
    }
}