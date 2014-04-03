namespace Luncher
{
    partial class UserProfile
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
            this.visualStudio2012DarkTheme1 = new Telerik.WinControls.Themes.VisualStudio2012DarkTheme();
            this.Nickname = new Telerik.WinControls.UI.RadTextBox();
            this.Password = new Telerik.WinControls.UI.RadTextBox();
            this.CancelButton = new Telerik.WinControls.UI.RadButton();
            this.SaveProfile = new Telerik.WinControls.UI.RadButton();
            this.DelProfile = new Telerik.WinControls.UI.RadButton();
            this.radRadioButton1 = new Telerik.WinControls.UI.RadRadioButton();
            this.radRadioButton2 = new Telerik.WinControls.UI.RadRadioButton();
            this.UsePassword = new Telerik.WinControls.UI.RadCheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.Nickname)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Password)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CancelButton)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.SaveProfile)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DelProfile)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radRadioButton1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radRadioButton2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.UsePassword)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            this.SuspendLayout();
            // 
            // Nickname
            // 
            this.Nickname.Location = new System.Drawing.Point(102, 42);
            this.Nickname.Name = "Nickname";
            this.Nickname.NullText = "Ник";
            this.Nickname.Size = new System.Drawing.Size(195, 21);
            this.Nickname.TabIndex = 0;
            this.Nickname.ThemeName = "VisualStudio2012Dark";
            // 
            // Password
            // 
            this.Password.Location = new System.Drawing.Point(102, 69);
            this.Password.Name = "Password";
            this.Password.NullText = "Пароль";
            this.Password.Size = new System.Drawing.Size(195, 21);
            this.Password.TabIndex = 1;
            this.Password.ThemeName = "VisualStudio2012Dark";
            // 
            // CancelButton
            // 
            this.CancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.CancelButton.Location = new System.Drawing.Point(152, 119);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(110, 24);
            this.CancelButton.TabIndex = 2;
            this.CancelButton.Text = "Отмена";
            this.CancelButton.ThemeName = "VisualStudio2012Dark";
            // 
            // SaveProfile
            // 
            this.SaveProfile.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.SaveProfile.Location = new System.Drawing.Point(268, 119);
            this.SaveProfile.Name = "SaveProfile";
            this.SaveProfile.Size = new System.Drawing.Size(110, 24);
            this.SaveProfile.TabIndex = 3;
            this.SaveProfile.Text = "Готово";
            this.SaveProfile.ThemeName = "VisualStudio2012Dark";
            // 
            // DelProfile
            // 
            this.DelProfile.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.DelProfile.Location = new System.Drawing.Point(36, 119);
            this.DelProfile.Name = "DelProfile";
            this.DelProfile.Size = new System.Drawing.Size(110, 24);
            this.DelProfile.TabIndex = 4;
            this.DelProfile.Text = "Удалить";
            this.DelProfile.ThemeName = "VisualStudio2012Dark";
            // 
            // radRadioButton1
            // 
            this.radRadioButton1.Location = new System.Drawing.Point(70, 9);
            this.radRadioButton1.Name = "radRadioButton1";
            this.radRadioButton1.Size = new System.Drawing.Size(74, 18);
            this.radRadioButton1.TabIndex = 5;
            this.radRadioButton1.Text = "Лицензия";
            this.radRadioButton1.ThemeName = "VisualStudio2012Dark";
            this.radRadioButton1.ToggleStateChanged += new Telerik.WinControls.UI.StateChangedEventHandler(this.radRadioButton1_ToggleStateChanged);
            // 
            // radRadioButton2
            // 
            this.radRadioButton2.CheckState = System.Windows.Forms.CheckState.Checked;
            this.radRadioButton2.Location = new System.Drawing.Point(191, 9);
            this.radRadioButton2.Name = "radRadioButton2";
            this.radRadioButton2.Size = new System.Drawing.Size(153, 18);
            this.radRadioButton2.TabIndex = 6;
            this.radRadioButton2.TabStop = true;
            this.radRadioButton2.Text = "Без авторзации(пиратка)";
            this.radRadioButton2.ThemeName = "VisualStudio2012Dark";
            this.radRadioButton2.ToggleState = Telerik.WinControls.Enumerations.ToggleState.On;
            this.radRadioButton2.ToggleStateChanged += new Telerik.WinControls.UI.StateChangedEventHandler(this.radRadioButton2_ToggleStateChanged);
            // 
            // UsePassword
            // 
            this.UsePassword.Location = new System.Drawing.Point(94, 96);
            this.UsePassword.Name = "UsePassword";
            this.UsePassword.Size = new System.Drawing.Size(227, 18);
            this.UsePassword.TabIndex = 7;
            this.UsePassword.Text = "Запрашивать пароль при запуске игры";
            this.UsePassword.ThemeName = "VisualStudio2012Dark";
            // 
            // UserProfile
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(414, 146);
            this.Controls.Add(this.UsePassword);
            this.Controls.Add(this.radRadioButton2);
            this.Controls.Add(this.radRadioButton1);
            this.Controls.Add(this.DelProfile);
            this.Controls.Add(this.SaveProfile);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.Password);
            this.Controls.Add(this.Nickname);
            this.Name = "UserProfile";
            // 
            // 
            // 
            this.RootElement.ApplyShapeToControl = true;
            this.Text = "UserProfile";
            this.ThemeName = "VisualStudio2012Dark";
            ((System.ComponentModel.ISupportInitialize)(this.Nickname)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Password)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CancelButton)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.SaveProfile)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DelProfile)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radRadioButton1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radRadioButton2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.UsePassword)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Telerik.WinControls.Themes.VisualStudio2012DarkTheme visualStudio2012DarkTheme1;
        private Telerik.WinControls.UI.RadTextBox Nickname;
        private Telerik.WinControls.UI.RadTextBox Password;
        private Telerik.WinControls.UI.RadButton CancelButton;
        private Telerik.WinControls.UI.RadButton SaveProfile;
        private Telerik.WinControls.UI.RadButton DelProfile;
        private Telerik.WinControls.UI.RadRadioButton radRadioButton1;
        private Telerik.WinControls.UI.RadRadioButton radRadioButton2;
        private Telerik.WinControls.UI.RadCheckBox UsePassword;
    }
}