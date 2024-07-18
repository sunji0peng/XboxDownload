﻿using System.Collections;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace XboxDownload
{
    public partial class FormProxy : Form
    {
        public FormProxy()
        {
            InitializeComponent();

            if (File.Exists(Form1.resourcePath + "\\SniProxy.json"))
            {
                List<List<object>>? SniProxy = null;
                try
                {
                    SniProxy = JsonSerializer.Deserialize<List<List<object>>>(File.ReadAllText(Form1.resourcePath + "\\SniProxy.json"));
                }
                catch { }
                if (SniProxy != null)
                {
                    StringBuilder sb = new();
                    foreach (var item in SniProxy)
                    {
                        if (item.Count == 3)
                        {
                            JsonElement jeHosts = (JsonElement)item[0];
                            string? hosts = jeHosts.ValueKind == JsonValueKind.Array ? string.Join(", ", jeHosts.EnumerateArray().Select(item => item.GetString()?.Trim())) : string.Empty;
                            string? fakeHost = item[1]?.ToString()?.Trim();
                            string? ip = item[2]?.ToString()?.Trim();
                            if (string.IsNullOrEmpty(hosts)) continue;
                            if (string.IsNullOrEmpty(fakeHost) && string.IsNullOrEmpty(ip))
                                sb.AppendLine(hosts);
                            else if (!string.IsNullOrEmpty(fakeHost) && !string.IsNullOrEmpty(ip))
                                sb.AppendLine(hosts + " | " + fakeHost + " | " + ip);
                            else
                                sb.AppendLine(hosts + " | " + fakeHost + ip);
                        }
                    }
                    textBox1.Text = sb.ToString();
                }
            }

            for (int i = 0; i <= DnsListen.dohs.GetLongLength(0) - 1; i++)
            {
                cbDoh.Items.Add(DnsListen.dohs[i, 0]);
            }
            cbDoh.SelectedIndex = Properties.Settings.Default.DoHProxy >= DnsListen.dohs.GetLongLength(0) ? 3 : Properties.Settings.Default.DoHProxy;
        }

        private void CheckedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            string[] hosts1 = Array.Empty<string>();
            switch (checkedListBox1.Items[e.Index].ToString())
            {
                case "Steam 商店社区":
                    hosts1 = new string[] { "store.steampowered.com", "api.steampowered.com", "login.steampowered.com", "help.steampowered.com", "checkout.steampowered.com", "steamcommunity.com" };
                    break;
                case "GitHub":
                    hosts1 = new string[] { "*github.com", "*githubusercontent.com", "*github.io", "*github.blog", "*githubstatus.com", "*githubassets.com" };
                    break;
                case "Pixiv":
                    hosts1 = new string[] { "*pixiv.net | 210.140.92.187", "*.pximg.net | 210.140.92.141" };
                    break;
            }

            StringBuilder sb = new();
            foreach (string host in Regex.Split(textBox1.Text.Trim(), @"\n"))
            {
                string _host = host.Trim();
                if (string.IsNullOrEmpty(_host)) continue;
                if (!Array.Exists(hosts1, element => element.Equals(_host)))
                {
                    sb.AppendLine(host);
                }
            }
            string hosts2 = sb.ToString();
            if (e.NewValue == CheckState.Checked)
            {
                string hosts = string.Join("\r\n", hosts1) + "\r\n";
                textBox1.Text = hosts + hosts2;
                textBox1.Focus();
                textBox1.Select(0, hosts.Length - 2);
            }
            else textBox1.Text = hosts2;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            string ipv6Pattern = @"^((([0-9A-Fa-f]{1,4}:){7}([0-9A-Fa-f]{1,4}|:))|(([0-9A-Fa-f]{1,4}:){1,7}:)|(([0-9A-Fa-f]{1,4}:){1,6}:[0-9A-Fa-f]{1,4})|(([0-9A-Fa-f]{1,4}:){1,5}(:[0-9A-Fa-f]{1,4}){1,2})|(([0-9A-Fa-f]{1,4}:){1,4}(:[0-9A-Fa-f]{1,4}){1,3})|(([0-9A-Fa-f]{1,4}:){1,3}(:[0-9A-Fa-f]{1,4}){1,4})|(([0-9A-Fa-f]{1,4}:){1,2}(:[0-9A-Fa-f]{1,4}){1,5})|([0-9A-Fa-f]{1,4}:((:[0-9A-Fa-f]{1,4}){1,6}))|(:((:[0-9A-Fa-f]{1,4}){1,7}|:))|(::(ffff(:0{1,4}){0,1}:){0,1}((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?))|(([0-9A-Fa-f]{1,4}:){1,4}:((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)))$";
            string ipv4Pattern = @"^(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])$";
            Regex regex = new(ipv6Pattern + "|" + ipv4Pattern);

            List<List<object>> lsSniProxy = new();
            foreach (string str in textBox1.Text.Trim().Split('\n'))
            {
                ArrayList arrHost = new();
                string sni = string.Empty;
                string[] proxy = str.Trim().Split('|');
                IPAddress? ip = null;
                if (proxy.Length >= 1)
                {
                    foreach (string host in Regex.Split(proxy[0], @","))
                    {
                        string _host = Regex.Replace(host.ToLower().Trim(), @"^(https?://)?([^/|:|\s]+).*$", "$2").Trim();
                        if (!string.IsNullOrEmpty(_host))
                        {
                            arrHost.Add(_host);
                        }
                    }
                }
                if (proxy.Length == 2)
                {
                    proxy[1] = proxy[1].Trim();
                    if (!(regex.IsMatch(proxy[1]) && IPAddress.TryParse(proxy[1], out ip)))
                        sni = Regex.Replace(proxy[1].ToLower(), @"^(https?://)?([^/|:|\s]+).*$", "$2").Trim();
                }
                else if (proxy.Length >= 3)
                {
                    sni = Regex.Replace(proxy[1].ToLower().Trim(), @"^(https?://)?([^/|:|\s]+).*$", "$2").Trim();
                    _ = IPAddress.TryParse(proxy[2].Trim(), out ip);
                }
                if (arrHost.Count >= 1) lsSniProxy.Add(new List<object> { arrHost, sni, ip != null ? ip.ToString() : string.Empty });
            }
            if (lsSniProxy.Count >= 1)
            {
                if (!Directory.Exists(Form1.resourcePath)) Directory.CreateDirectory(Form1.resourcePath);
                File.WriteAllText(Form1.resourcePath + "\\SniProxy.json", JsonSerializer.Serialize(lsSniProxy, new JsonSerializerOptions { WriteIndented = true }));
            }
            else if (File.Exists(Form1.resourcePath + "\\SniProxy.json"))
            {
                File.Delete(Form1.resourcePath + "\\SniProxy.json");
            }
            HttpsListen.CreateCertificate();
            Properties.Settings.Default.DoHProxy = cbDoh.SelectedIndex;
            Properties.Settings.Default.Save();
            this.Close();
        }
    }
}
