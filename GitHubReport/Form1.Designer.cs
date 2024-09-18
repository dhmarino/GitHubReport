namespace GitHubReport
{
    partial class Form1
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.BtnObtenerRepositorios = new System.Windows.Forms.Button();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.BtnExportPdf = new System.Windows.Forms.Button();
            this.picBoxWait = new System.Windows.Forms.PictureBox();
            this.lblVersion = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picBoxWait)).BeginInit();
            this.SuspendLayout();
            // 
            // BtnObtenerRepositorios
            // 
            this.BtnObtenerRepositorios.Location = new System.Drawing.Point(142, 811);
            this.BtnObtenerRepositorios.Name = "BtnObtenerRepositorios";
            this.BtnObtenerRepositorios.Size = new System.Drawing.Size(296, 74);
            this.BtnObtenerRepositorios.TabIndex = 0;
            this.BtnObtenerRepositorios.Text = "Obtener Repositorios";
            this.BtnObtenerRepositorios.UseVisualStyleBackColor = true;
            this.BtnObtenerRepositorios.Click += new System.EventHandler(this.BtnObtenerRepositorios_Click);
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 25;
            this.listBox1.Location = new System.Drawing.Point(38, 29);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(969, 754);
            this.listBox1.TabIndex = 1;
            // 
            // BtnExportPdf
            // 
            this.BtnExportPdf.Location = new System.Drawing.Point(583, 811);
            this.BtnExportPdf.Name = "BtnExportPdf";
            this.BtnExportPdf.Size = new System.Drawing.Size(296, 74);
            this.BtnExportPdf.TabIndex = 2;
            this.BtnExportPdf.Text = "Exportar a PDF";
            this.BtnExportPdf.UseVisualStyleBackColor = true;
            this.BtnExportPdf.Click += new System.EventHandler(this.BtnExportPdf_Click);
            // 
            // picBoxWait
            // 
            this.picBoxWait.Image = global::GitHubReport.Properties.Resources._1412463378loading_gear_4;
            this.picBoxWait.Location = new System.Drawing.Point(921, 811);
            this.picBoxWait.Name = "picBoxWait";
            this.picBoxWait.Size = new System.Drawing.Size(86, 74);
            this.picBoxWait.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picBoxWait.TabIndex = 3;
            this.picBoxWait.TabStop = false;
            // 
            // lblVersion
            // 
            this.lblVersion.AutoSize = true;
            this.lblVersion.Location = new System.Drawing.Point(857, 897);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(85, 25);
            this.lblVersion.TabIndex = 4;
            this.lblVersion.Text = "Version";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1044, 931);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.picBoxWait);
            this.Controls.Add(this.BtnExportPdf);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.BtnObtenerRepositorios);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "GitHub Report";
            ((System.ComponentModel.ISupportInitialize)(this.picBoxWait)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button BtnObtenerRepositorios;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Button BtnExportPdf;
        private System.Windows.Forms.PictureBox picBoxWait;
        private System.Windows.Forms.Label lblVersion;
    }
}

