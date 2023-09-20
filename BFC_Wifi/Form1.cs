using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.NetworkInformation;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;
using System.Windows.Controls;

namespace BFC_Wifi
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public static bool PingHost(string nameOrAddress)
        {
            bool pingable = false;
            Ping pinger = null;
            try
            {
                pinger = new Ping();
                PingReply reply = pinger.Send(nameOrAddress);
                pingable = reply.Status == IPStatus.Success;
            }
            catch (PingException)
            {
                return false;
            }
            finally
            {
                if (pinger != null)
                {
                    pinger.Dispose();
                }
            }
            return pingable;
        }


        static async Task<string> ScanWifiNetworksAsync()
        {
            string output = "";
            for (int i = 0; i < 10; i++)
            { 
                try
                {    
                    // Spustit nástroj netsh pro zjištění dostupných Wi-Fi sítí
                    Process process = new Process();
                    process.StartInfo.FileName = "netsh";
                    process.StartInfo.Arguments = "wlan show networks mode=Bssid";
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();

                    // Asynchronní čekání na ukončení procesu
                    Task.Run(() => process.WaitForExit());
                    // Přečíst výstup nástroje netsh
                    output = output + process.StandardOutput.ReadToEnd();

                    // Výstup obsahuje informace o dostupných Wi-Fi sítích



                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message,"Error");
                }
            }
            return output;
        }

        static HashSet<string> GetUniqueSsids(string input)
        {
            HashSet<string> uniqueSsids = new HashSet<string>();
            string[] lines = input.Split('\n');

            foreach (string line in lines)
            {
                if (line.StartsWith("SSID"))
                {
                    // Použijte regulární výraz k extrakci názvu SSID
                    Match match = Regex.Match(line, @"SSID (\d+) : (.+)");
                    if (match.Success)
                    {
                        string ssid = match.Groups[2].Value.Trim();
                        uniqueSsids.Add(ssid);
                    }
                }
            }

            return uniqueSsids;
        }





        private void Form1_Load(object sender, EventArgs e)
        {
            Task<string> wifiScan = ScanWifiNetworksAsync();
            string input = wifiScan.Result;  
            textBox1.Text = input;
            HashSet<string> uniqueSsids = GetUniqueSsids(input);

            foreach (string ssid in uniqueSsids)
            {
                listBox1.Items.Add(ssid);
            }




            if (PingHost("8.8.8.8"))
            {

            }
            else
            {

            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedSsid = listBox1.SelectedItem.ToString();
            MessageBox.Show(selectedSsid);
        }

        private void listBox1_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = 30;
        }

        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index >= 0)
            {
                e.DrawBackground();
                e.Graphics.DrawString(listBox1.Items[e.Index].ToString(), e.Font, Brushes.Black, e.Bounds, StringFormat.GenericDefault);
                e.DrawFocusRectangle();
            }
        }
        private void UpdateListBoxWidth()
        {
            // Nastavte šířku ListBoxu na základě počtu položek
            int maxWidth = 0;
            foreach (var item in listBox1.Items)
            {
                int itemWidth = TextRenderer.MeasureText(item.ToString(), listBox1.Font).Width;
                maxWidth = Math.Max(maxWidth, itemWidth);
            }

            listBox1.Width = maxWidth + SystemInformation.VerticalScrollBarWidth;
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
