namespace Luncher.Forms.MainForm
{
    partial class MainForm
    {
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.Log = new System.Windows.Forms.RichTextBox();
            this.visualStudio2012DarkTheme1 = new Telerik.WinControls.Themes.VisualStudio2012DarkTheme();
            this.CrashPanel = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.CrashPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // Log
            // 
            this.Log.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.Log.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Log.Location = new System.Drawing.Point(0, 0);
            this.Log.Name = "Log";
            this.Log.ReadOnly = true;
            this.Log.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.Log.Size = new System.Drawing.Size(793, 380);
            this.Log.TabIndex = 0;
            this.Log.Text = "";
            // 
            // CrashPanel
            // 
            this.CrashPanel.BackColor = System.Drawing.Color.Gainsboro;
            this.CrashPanel.Controls.Add(this.label1);
            this.CrashPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.CrashPanel.Location = new System.Drawing.Point(0, 380);
            this.CrashPanel.Name = "CrashPanel";
            this.CrashPanel.Size = new System.Drawing.Size(793, 27);
            this.CrashPanel.TabIndex = 1;
            this.CrashPanel.Visible = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.Red;
            this.label1.Location = new System.Drawing.Point(6, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(206, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Запуск лаунчера завершился неудачей";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(793, 407);
            this.Controls.Add(this.Log);
            this.Controls.Add(this.CrashPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "Luncher";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.CrashPanel.ResumeLayout(false);
            this.CrashPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.RichTextBox Log;
        private Telerik.WinControls.Themes.VisualStudio2012DarkTheme visualStudio2012DarkTheme1;
        private System.Windows.Forms.Panel CrashPanel;
        private System.Windows.Forms.Label label1;

    }
}

