namespace Luncher.Forms
{
    partial class ProfileForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProfileForm));
            Telerik.WinControls.UI.RadListDataItem radListDataItem1 = new Telerik.WinControls.UI.RadListDataItem();
            Telerik.WinControls.UI.RadListDataItem radListDataItem2 = new Telerik.WinControls.UI.RadListDataItem();
            Telerik.WinControls.UI.RadListDataItem radListDataItem3 = new Telerik.WinControls.UI.RadListDataItem();
            this.ProfileName = new Telerik.WinControls.UI.RadTextBox();
            this.radLabel1 = new Telerik.WinControls.UI.RadLabel();
            this.radGroupBox1 = new Telerik.WinControls.UI.RadGroupBox();
            this.radLabel4 = new Telerik.WinControls.UI.RadLabel();
            this.radLabel3 = new Telerik.WinControls.UI.RadLabel();
            this.ResY = new Telerik.WinControls.UI.RadTextBox();
            this.ResX = new Telerik.WinControls.UI.RadTextBox();
            this.radLabel2 = new Telerik.WinControls.UI.RadLabel();
            this.LState = new Telerik.WinControls.UI.RadDropDownList();
            this.Gamedir = new Telerik.WinControls.UI.RadTextBox();
            this.UseDirectory = new Telerik.WinControls.UI.RadCheckBox();
            this.radGroupBox2 = new Telerik.WinControls.UI.RadGroupBox();
            this.EnableOther = new Telerik.WinControls.UI.RadCheckBox();
            this.Versions = new Telerik.WinControls.UI.RadDropDownList();
            this.EnableAlpha = new Telerik.WinControls.UI.RadCheckBox();
            this.EnableBeta = new Telerik.WinControls.UI.RadCheckBox();
            this.EnableExp = new Telerik.WinControls.UI.RadCheckBox();
            this.radGroupBox3 = new Telerik.WinControls.UI.RadGroupBox();
            this.ExecJava = new Telerik.WinControls.UI.RadTextBox();
            this.UseExec = new Telerik.WinControls.UI.RadCheckBox();
            this.Args = new Telerik.WinControls.UI.RadTextBox();
            this.UseArgs = new Telerik.WinControls.UI.RadCheckBox();
            this.radButton1 = new Telerik.WinControls.UI.RadButton();
            this.radButton2 = new Telerik.WinControls.UI.RadButton();
            this.radButton3 = new Telerik.WinControls.UI.RadButton();
            this.radButton4 = new Telerik.WinControls.UI.RadButton();
            ((System.ComponentModel.ISupportInitialize)(this.ProfileName)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radLabel1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radGroupBox1)).BeginInit();
            this.radGroupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.radLabel4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radLabel3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ResY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ResX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radLabel2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LState)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Gamedir)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.UseDirectory)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radGroupBox2)).BeginInit();
            this.radGroupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.EnableOther)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Versions)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.EnableAlpha)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.EnableBeta)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.EnableExp)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radGroupBox3)).BeginInit();
            this.radGroupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ExecJava)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.UseExec)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Args)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.UseArgs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radButton1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radButton2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radButton3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.radButton4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            this.SuspendLayout();
            // 
            // ProfileName
            // 
            resources.ApplyResources(this.ProfileName, "ProfileName");
            this.ProfileName.Name = "ProfileName";
            this.ProfileName.ThemeName = "VisualStudio2012Dark";
            this.ProfileName.TextChanged += new System.EventHandler(this.ProfileName_TextChanged);
            // 
            // radLabel1
            // 
            this.radLabel1.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.radLabel1, "radLabel1");
            this.radLabel1.Name = "radLabel1";
            this.radLabel1.ThemeName = "VisualStudio2012Dark";
            ((Telerik.WinControls.UI.RadLabelElement)(this.radLabel1.GetChildAt(0))).Text = resources.GetString("resource.Text");
            ((Telerik.WinControls.Primitives.FillPrimitive)(this.radLabel1.GetChildAt(0).GetChildAt(0))).Visibility = Telerik.WinControls.ElementVisibility.Collapsed;
            // 
            // radGroupBox1
            // 
            this.radGroupBox1.AccessibleRole = System.Windows.Forms.AccessibleRole.Grouping;
            this.radGroupBox1.Controls.Add(this.radLabel4);
            this.radGroupBox1.Controls.Add(this.radLabel3);
            this.radGroupBox1.Controls.Add(this.ResY);
            this.radGroupBox1.Controls.Add(this.ResX);
            this.radGroupBox1.Controls.Add(this.radLabel2);
            this.radGroupBox1.Controls.Add(this.LState);
            this.radGroupBox1.Controls.Add(this.Gamedir);
            this.radGroupBox1.Controls.Add(this.UseDirectory);
            this.radGroupBox1.Controls.Add(this.radLabel1);
            this.radGroupBox1.Controls.Add(this.ProfileName);
            resources.ApplyResources(this.radGroupBox1, "radGroupBox1");
            this.radGroupBox1.Name = "radGroupBox1";
            this.radGroupBox1.ThemeName = "VisualStudio2012Dark";
            // 
            // radLabel4
            // 
            this.radLabel4.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.radLabel4, "radLabel4");
            this.radLabel4.Name = "radLabel4";
            this.radLabel4.ThemeName = "VisualStudio2012Dark";
            ((Telerik.WinControls.UI.RadLabelElement)(this.radLabel4.GetChildAt(0))).Text = resources.GetString("resource.Text1");
            ((Telerik.WinControls.Primitives.FillPrimitive)(this.radLabel4.GetChildAt(0).GetChildAt(0))).Visibility = Telerik.WinControls.ElementVisibility.Collapsed;
            // 
            // radLabel3
            // 
            this.radLabel3.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.radLabel3, "radLabel3");
            this.radLabel3.Name = "radLabel3";
            this.radLabel3.ThemeName = "VisualStudio2012Dark";
            ((Telerik.WinControls.UI.RadLabelElement)(this.radLabel3.GetChildAt(0))).Text = resources.GetString("resource.Text2");
            ((Telerik.WinControls.Primitives.FillPrimitive)(this.radLabel3.GetChildAt(0).GetChildAt(0))).Visibility = Telerik.WinControls.ElementVisibility.Collapsed;
            // 
            // ResY
            // 
            resources.ApplyResources(this.ResY, "ResY");
            this.ResY.Name = "ResY";
            this.ResY.ThemeName = "VisualStudio2012Dark";
            // 
            // ResX
            // 
            resources.ApplyResources(this.ResX, "ResX");
            this.ResX.Name = "ResX";
            this.ResX.ThemeName = "VisualStudio2012Dark";
            // 
            // radLabel2
            // 
            this.radLabel2.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.radLabel2, "radLabel2");
            this.radLabel2.Name = "radLabel2";
            this.radLabel2.ThemeName = "VisualStudio2012Dark";
            ((Telerik.WinControls.UI.RadLabelElement)(this.radLabel2.GetChildAt(0))).Text = resources.GetString("resource.Text3");
            ((Telerik.WinControls.Primitives.FillPrimitive)(this.radLabel2.GetChildAt(0).GetChildAt(0))).Visibility = Telerik.WinControls.ElementVisibility.Collapsed;
            // 
            // LState
            // 
            this.LState.DropDownStyle = Telerik.WinControls.RadDropDownStyle.DropDownList;
            radListDataItem1.Tag = "keep the launcher open";
            radListDataItem1.Text = _locRm.GetString("data.keepopen");
            resources.ApplyResources(radListDataItem1, "radListDataItem1");
            radListDataItem2.Tag = "hide launcher and re-open when game closes";
            radListDataItem2.Text = _locRm.GetString("data.minsize");
            resources.ApplyResources(radListDataItem2, "radListDataItem2");
            radListDataItem3.Tag = "close launcher when game starts";
            radListDataItem3.Text = _locRm.GetString("data.close");
            resources.ApplyResources(radListDataItem3, "radListDataItem3");
            this.LState.Items.Add(radListDataItem1);
            this.LState.Items.Add(radListDataItem2);
            this.LState.Items.Add(radListDataItem3);
            resources.ApplyResources(this.LState, "LState");
            this.LState.Name = "LState";
            this.LState.ThemeName = "VisualStudio2012Dark";
            // 
            // Gamedir
            // 
            resources.ApplyResources(this.Gamedir, "Gamedir");
            this.Gamedir.Name = "Gamedir";
            this.Gamedir.ThemeName = "VisualStudio2012Dark";
            // 
            // UseDirectory
            // 
            resources.ApplyResources(this.UseDirectory, "UseDirectory");
            this.UseDirectory.Name = "UseDirectory";
            this.UseDirectory.ThemeName = "VisualStudio2012Dark";
            this.UseDirectory.ToggleStateChanged += new Telerik.WinControls.UI.StateChangedEventHandler(this.UseDirectory_ToggleStateChanged);
            ((Telerik.WinControls.UI.RadCheckBoxElement)(this.UseDirectory.GetChildAt(0))).Text = resources.GetString("resource.Text4");
            ((Telerik.WinControls.Primitives.FillPrimitive)(this.UseDirectory.GetChildAt(0).GetChildAt(0))).Visibility = Telerik.WinControls.ElementVisibility.Collapsed;
            // 
            // radGroupBox2
            // 
            this.radGroupBox2.AccessibleRole = System.Windows.Forms.AccessibleRole.Grouping;
            this.radGroupBox2.Controls.Add(this.EnableOther);
            this.radGroupBox2.Controls.Add(this.Versions);
            this.radGroupBox2.Controls.Add(this.EnableAlpha);
            this.radGroupBox2.Controls.Add(this.EnableBeta);
            this.radGroupBox2.Controls.Add(this.EnableExp);
            resources.ApplyResources(this.radGroupBox2, "radGroupBox2");
            this.radGroupBox2.Name = "radGroupBox2";
            this.radGroupBox2.ThemeName = "VisualStudio2012Dark";
            // 
            // EnableOther
            // 
            resources.ApplyResources(this.EnableOther, "EnableOther");
            this.EnableOther.Name = "EnableOther";
            this.EnableOther.ThemeName = "VisualStudio2012Dark";
            ((Telerik.WinControls.UI.RadCheckBoxElement)(this.EnableOther.GetChildAt(0))).Text = resources.GetString("resource.Text5");
            ((Telerik.WinControls.Primitives.FillPrimitive)(this.EnableOther.GetChildAt(0).GetChildAt(0))).Visibility = Telerik.WinControls.ElementVisibility.Collapsed;
            // 
            // Versions
            // 
            this.Versions.DropDownStyle = Telerik.WinControls.RadDropDownStyle.DropDownList;
            resources.ApplyResources(this.Versions, "Versions");
            this.Versions.Name = "Versions";
            this.Versions.ThemeName = "VisualStudio2012Dark";
            this.Versions.SelectedIndexChanged += new Telerik.WinControls.UI.Data.PositionChangedEventHandler(this.Versions_SelectedIndexChanged);
            // 
            // EnableAlpha
            // 
            resources.ApplyResources(this.EnableAlpha, "EnableAlpha");
            this.EnableAlpha.Name = "EnableAlpha";
            this.EnableAlpha.ThemeName = "VisualStudio2012Dark";
            this.EnableAlpha.ToggleStateChanged += new Telerik.WinControls.UI.StateChangedEventHandler(this.EnableAlpha_ToggleStateChanged);
            ((Telerik.WinControls.UI.RadCheckBoxElement)(this.EnableAlpha.GetChildAt(0))).Text = resources.GetString("resource.Text6");
            ((Telerik.WinControls.Primitives.FillPrimitive)(this.EnableAlpha.GetChildAt(0).GetChildAt(0))).Visibility = Telerik.WinControls.ElementVisibility.Collapsed;
            // 
            // EnableBeta
            // 
            resources.ApplyResources(this.EnableBeta, "EnableBeta");
            this.EnableBeta.Name = "EnableBeta";
            this.EnableBeta.ThemeName = "VisualStudio2012Dark";
            this.EnableBeta.ToggleStateChanged += new Telerik.WinControls.UI.StateChangedEventHandler(this.EnableBeta_ToggleStateChanged);
            ((Telerik.WinControls.UI.RadCheckBoxElement)(this.EnableBeta.GetChildAt(0))).Text = resources.GetString("resource.Text7");
            ((Telerik.WinControls.Primitives.FillPrimitive)(this.EnableBeta.GetChildAt(0).GetChildAt(0))).Visibility = Telerik.WinControls.ElementVisibility.Collapsed;
            // 
            // EnableExp
            // 
            resources.ApplyResources(this.EnableExp, "EnableExp");
            this.EnableExp.Name = "EnableExp";
            this.EnableExp.ThemeName = "VisualStudio2012Dark";
            this.EnableExp.ToggleStateChanged += new Telerik.WinControls.UI.StateChangedEventHandler(this.EnableExp_ToggleStateChanged);
            ((Telerik.WinControls.UI.RadCheckBoxElement)(this.EnableExp.GetChildAt(0))).Text = resources.GetString("resource.Text8");
            ((Telerik.WinControls.Primitives.FillPrimitive)(this.EnableExp.GetChildAt(0).GetChildAt(0))).Visibility = Telerik.WinControls.ElementVisibility.Collapsed;
            // 
            // radGroupBox3
            // 
            this.radGroupBox3.AccessibleRole = System.Windows.Forms.AccessibleRole.Grouping;
            this.radGroupBox3.Controls.Add(this.ExecJava);
            this.radGroupBox3.Controls.Add(this.UseExec);
            this.radGroupBox3.Controls.Add(this.Args);
            this.radGroupBox3.Controls.Add(this.UseArgs);
            resources.ApplyResources(this.radGroupBox3, "radGroupBox3");
            this.radGroupBox3.Name = "radGroupBox3";
            this.radGroupBox3.ThemeName = "VisualStudio2012Dark";
            // 
            // ExecJava
            // 
            resources.ApplyResources(this.ExecJava, "ExecJava");
            this.ExecJava.Name = "ExecJava";
            this.ExecJava.ThemeName = "VisualStudio2012Dark";
            // 
            // UseExec
            // 
            resources.ApplyResources(this.UseExec, "UseExec");
            this.UseExec.Name = "UseExec";
            this.UseExec.ThemeName = "VisualStudio2012Dark";
            this.UseExec.ToggleStateChanged += new Telerik.WinControls.UI.StateChangedEventHandler(this.UseExec_ToggleStateChanged);
            ((Telerik.WinControls.UI.RadCheckBoxElement)(this.UseExec.GetChildAt(0))).Text = resources.GetString("resource.Text9");
            ((Telerik.WinControls.Primitives.FillPrimitive)(this.UseExec.GetChildAt(0).GetChildAt(0))).Visibility = Telerik.WinControls.ElementVisibility.Collapsed;
            // 
            // Args
            // 
            resources.ApplyResources(this.Args, "Args");
            this.Args.Name = "Args";
            this.Args.ThemeName = "VisualStudio2012Dark";
            // 
            // UseArgs
            // 
            resources.ApplyResources(this.UseArgs, "UseArgs");
            this.UseArgs.Name = "UseArgs";
            this.UseArgs.ThemeName = "VisualStudio2012Dark";
            this.UseArgs.ToggleStateChanged += new Telerik.WinControls.UI.StateChangedEventHandler(this.UseArgs_ToggleStateChanged);
            ((Telerik.WinControls.UI.RadCheckBoxElement)(this.UseArgs.GetChildAt(0))).Text = resources.GetString("resource.Text10");
            ((Telerik.WinControls.Primitives.FillPrimitive)(this.UseArgs.GetChildAt(0).GetChildAt(0))).Visibility = Telerik.WinControls.ElementVisibility.Collapsed;
            // 
            // radButton1
            // 
            resources.ApplyResources(this.radButton1, "radButton1");
            this.radButton1.Name = "radButton1";
            this.radButton1.ThemeName = "VisualStudio2012Dark";
            this.radButton1.Click += new System.EventHandler(this.radButton1_Click);
            // 
            // radButton2
            // 
            resources.ApplyResources(this.radButton2, "radButton2");
            this.radButton2.Name = "radButton2";
            this.radButton2.ThemeName = "VisualStudio2012Dark";
            this.radButton2.Click += new System.EventHandler(this.radButton2_Click);
            // 
            // radButton3
            // 
            resources.ApplyResources(this.radButton3, "radButton3");
            this.radButton3.Name = "radButton3";
            this.radButton3.TextWrap = true;
            this.radButton3.ThemeName = "VisualStudio2012Dark";
            this.radButton3.Click += new System.EventHandler(this.radButton3_Click);
            // 
            // radButton4
            // 
            resources.ApplyResources(this.radButton4, "radButton4");
            this.radButton4.Name = "radButton4";
            this.radButton4.ThemeName = "VisualStudio2012Dark";
            this.radButton4.Click += new System.EventHandler(this.radButton4_Click);
            // 
            // ProfileForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ControlBox = false;
            this.Controls.Add(this.radButton4);
            this.Controls.Add(this.radButton3);
            this.Controls.Add(this.radButton2);
            this.Controls.Add(this.radButton1);
            this.Controls.Add(this.radGroupBox3);
            this.Controls.Add(this.radGroupBox2);
            this.Controls.Add(this.radGroupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProfileForm";
            // 
            // 
            // 
            this.RootElement.ApplyShapeToControl = true;
            this.RootElement.MaxSize = new System.Drawing.Size(349, 447);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.ThemeName = "VisualStudio2012Dark";
            this.Load += new System.EventHandler(this.ProfileForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.ProfileName)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radLabel1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radGroupBox1)).EndInit();
            this.radGroupBox1.ResumeLayout(false);
            this.radGroupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.radLabel4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radLabel3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ResY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ResX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radLabel2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LState)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Gamedir)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.UseDirectory)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radGroupBox2)).EndInit();
            this.radGroupBox2.ResumeLayout(false);
            this.radGroupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.EnableOther)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Versions)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.EnableAlpha)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.EnableBeta)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.EnableExp)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radGroupBox3)).EndInit();
            this.radGroupBox3.ResumeLayout(false);
            this.radGroupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ExecJava)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.UseExec)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Args)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.UseArgs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radButton1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radButton2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radButton3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.radButton4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Telerik.WinControls.UI.RadLabel radLabel1;
        private Telerik.WinControls.UI.RadGroupBox radGroupBox1;
        private Telerik.WinControls.UI.RadLabel radLabel3;
        private Telerik.WinControls.UI.RadTextBox ResY;
        private Telerik.WinControls.UI.RadTextBox ResX;
        private Telerik.WinControls.UI.RadLabel radLabel2;
        private Telerik.WinControls.UI.RadDropDownList LState;
        private Telerik.WinControls.UI.RadTextBox Gamedir;
        private Telerik.WinControls.UI.RadCheckBox UseDirectory;
        private Telerik.WinControls.UI.RadGroupBox radGroupBox2;
        private Telerik.WinControls.UI.RadCheckBox EnableAlpha;
        private Telerik.WinControls.UI.RadCheckBox EnableBeta;
        private Telerik.WinControls.UI.RadCheckBox EnableExp;
        private Telerik.WinControls.UI.RadGroupBox radGroupBox3;
        private Telerik.WinControls.UI.RadTextBox ExecJava;
        private Telerik.WinControls.UI.RadCheckBox UseExec;
        private Telerik.WinControls.UI.RadTextBox Args;
        private Telerik.WinControls.UI.RadCheckBox UseArgs;
        private Telerik.WinControls.UI.RadButton radButton1;
        private Telerik.WinControls.UI.RadButton radButton2;
        private Telerik.WinControls.UI.RadButton radButton3;
        public Telerik.WinControls.UI.RadTextBox ProfileName;
        public Telerik.WinControls.UI.RadDropDownList Versions;
        private Telerik.WinControls.UI.RadLabel radLabel4;
        public Telerik.WinControls.UI.RadButton radButton4;
        private Telerik.WinControls.UI.RadCheckBox EnableOther;
    }
}
