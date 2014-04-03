using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Telerik.WinControls.UI;

namespace Luncher
{
    public partial class UserProfile : RadForm
    {
        public UserProfile()
        {
            InitializeComponent();
        }

        private void radRadioButton2_ToggleStateChanged(object sender, StateChangedEventArgs args)
        {
            Nickname.NullText = "Ник";
            Password.Enabled = false;
            Password.Text = null;
            UsePassword.Checked = false;
            UsePassword.Enabled = false;
        }

        private void radRadioButton1_ToggleStateChanged(object sender, StateChangedEventArgs args)
        {
            Nickname.NullText = "Логин";
            Password.Enabled = true;
            Password.Text = null;
            UsePassword.Enabled = true;
        }
    }
}
