namespace Delivery
{
    partial class FrmLogin
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            txtEmail = new TextBox();
            txtSenha = new TextBox();
            btnEntrar = new Button();
            Email = new Label();
            label1 = new Label();
            SuspendLayout();
            // 
            // txtEmail
            // 
            txtEmail.Location = new Point(318, 161);
            txtEmail.Name = "txtEmail";
            txtEmail.Size = new Size(152, 23);
            txtEmail.TabIndex = 0;
            // 
            // txtSenha
            // 
            txtSenha.Location = new Point(318, 199);
            txtSenha.Name = "txtSenha";
            txtSenha.Size = new Size(152, 23);
            txtSenha.TabIndex = 1;
            txtSenha.UseSystemPasswordChar = true;
            // 
            // btnEntrar
            // 
            btnEntrar.Location = new Point(353, 228);
            btnEntrar.Name = "btnEntrar";
            btnEntrar.Size = new Size(81, 23);
            btnEntrar.TabIndex = 2;
            btnEntrar.Text = "Entrar";
            btnEntrar.UseCompatibleTextRendering = true;
            btnEntrar.UseVisualStyleBackColor = false;
            btnEntrar.Click += btnEntrar_Click_1;
            // 
            // Email
            // 
            Email.AutoSize = true;
            Email.Location = new Point(274, 164);
            Email.Name = "Email";
            Email.Size = new Size(36, 15);
            Email.TabIndex = 3;
            Email.Text = "Email";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(274, 202);
            label1.Name = "label1";
            label1.Size = new Size(39, 15);
            label1.TabIndex = 4;
            label1.Text = "Senha";
            label1.Click += label1_Click;
            // 
            // FrmLogin
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(label1);
            Controls.Add(Email);
            Controls.Add(btnEntrar);
            Controls.Add(txtSenha);
            Controls.Add(txtEmail);
            Name = "FrmLogin";
            Text = "FrmLogin";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtEmail;
        private TextBox txtSenha;
        private Button btnEntrar;
        private Label Email;
        private Label label1;
    }
}