using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Resources;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Telerik.WinControls;

namespace Luncher
{
    public partial class ModList : Telerik.WinControls.UI.RadForm
    {
        public ModList()
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Program.lang);
            InitializeComponent();
        }

        ResourceManager LocRM = new ResourceManager("Luncher.ModList", typeof(ModList).Assembly);

        private void ModList_Load(object sender, EventArgs e)
        {
             try
            {
                BackgroundWorker bgw = new BackgroundWorker();
                bgw.DoWork += new DoWorkEventHandler(getList);
                bgw.RunWorkerAsync();
            }
            catch
            {
            }
        }

        private string ParseArray(JArray jr, int i, string token)
        {
            string toreturn = "";
            JArray array = (JArray) jr[i][token];
            for (int e = 0; e < array.Count; e++)
            {
                toreturn = toreturn + jr[i][token][e].ToString() + ", ";
            }
            return toreturn.Remove(toreturn.LastIndexOf(','));
        }

        private void radButton1_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start((string) radListView1.SelectedItem[4]);
            }
            catch
            {
            }
        }
        private void radListView1_SelectedItemChanged(object sender, EventArgs e)
        {
            try { radLabel1.Text = (string) radListView1.SelectedItem[5];
                modVersions.Text = (string) radListView1.SelectedItem[6];
                string source = (string) radListView1.SelectedItem[7];
                if (!String.IsNullOrEmpty(source))
                {
                    sourceLink.Text = (string)radListView1.SelectedItem[7];
                    sourceLink.Cursor = System.Windows.Forms.Cursors.Hand;
                }
                else
                {
                    sourceLink.Text = LocRM.GetString("source.closed");
                    sourceLink.Cursor = System.Windows.Forms.Cursors.Arrow;
                }
                radButton1.Enabled = true;
            } catch { }
        }

        private void sourceLink_Click(object sender, EventArgs e)
        {
            if (sourceLink.Text.Contains("http"))
            {
                Process.Start(sourceLink.Text);
            }
        }

        private void radLabel4_Click(object sender, EventArgs e)
        {
            Process.Start(@"http://modlist.mcf.li/api/v3/docs");
        }

        Stopwatch sw = new Stopwatch();
        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar1.Value1 = e.ProgressPercentage;
            string downloaded = string.Format("{0} MB's / {1} MB's", (e.BytesReceived / 1024d / 1024d).ToString("0.00"), (e.TotalBytesToReceive / 1024d / 1024d).ToString("0.00"));
            string speed = string.Format("{0} kb/s", (e.BytesReceived / 1024d / sw.Elapsed.TotalSeconds).ToString("0.00"));
            ChangeStatusText(LocRM.GetString("statuslabel.downloading") + radSplitButtonElement1.Text + "...  [" + speed + " | " + downloaded + "]");
        }

        private void radButtonElement1_Click(object sender, EventArgs e)
        {
            SetDefault();
            ChangeStatusText(LocRM.GetString("statuslabel.prepearingtodownload"));
            WebClient webc = new WebClient();
            sw.Start();
            webc.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
            webc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
            webc.DownloadFileAsync(new Uri("http://modlist.mcf.li/api/v3/" + radSplitButtonElement1.Text + ".json"), Variables.MCFolder + "/luncher/ml.json");
        }

        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            progressBar1.Value1 = 0;
            ChangeStatusText(LocRM.GetString("statuslabel.done"));
            BackgroundWorker bgw = new BackgroundWorker();
            bgw.DoWork += new DoWorkEventHandler(getList);
            bgw.RunWorkerAsync();
        }

        void ChangeStatusText(string text)
        {
            radLabelElement2.Text = text;
        }

        void getList(object sender, EventArgs e)
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Program.lang);
            MethodInvoker mi = new MethodInvoker(delegate() { this.radListView1.Items.Clear(); });
            this.Invoke(mi);
            JArray jr = JArray.Parse(File.ReadAllText(Variables.MCFolder + "/luncher/ml.json"));
            for (int i = 0; i < jr.Count; i++)
            {
                new MethodInvoker(delegate() { this.ChangeStatusText(LocRM.GetString("statuslabel.constructing")); }).Invoke();
                string name = "null";
                string author = "null";
                string dependencies = "null";
                string other = "";
                string link = "null";
                string type = "null";
                string version = "";
                string desc = "null";
                string source = null;
                try
                {
                    name = (string) jr[i]["name"];
                    author = ParseArray(jr, i, "author");
                    link = (string) jr[i]["link"];
                    dependencies = ParseArray(jr, i, "dependencies");
                    desc = (string) jr[i]["desc"];
                    type = ParseArray(jr, i, "type");
                    version = ParseArray(jr, i, "versions");
                }
                catch
                {
                }
                try
                {
                    other = (string) jr[i]["other"];
                }
                catch
                {
                }
                try
                {
                    source = (string) jr[i]["source"];
                }
                catch
                {
                }
                MethodInvoker mi1 = new MethodInvoker(delegate() { this.radListView1.Items.Add(other + name, type, author, dependencies, link, desc, version, source); });
                this.Invoke(mi1);
            }
            new MethodInvoker(delegate() { this.ChangeStatusText(LocRM.GetString("statuslabel.idling")); }).Invoke();
        }

        void SetDefault()
        {
            sourceLink.Text = "...";
            sourceLink.Cursor = System.Windows.Forms.Cursors.Arrow;
            modVersions.Text = "...";
            radLabel1.Text = "...";
        }
    }
}
