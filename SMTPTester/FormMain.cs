using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SMTPTester
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
#if DEBUG
            txtHost.Text = "smtp.gmail.com";
            txtPort.Text = "587"; // could also be 465
            chkUseSSL.Checked = true;
            chkUseSenderAsUsername.Checked = false;
#endif

            RefreshControls();
        }

        private void txtHost_TextChanged(object sender, EventArgs e)
        {
            RefreshControls();
        }

        private void txtPort_TextChanged(object sender, EventArgs e)
        {
            txtPort.Text = FilterNumeric(txtPort.Text);
            RefreshControls();
        }

        private void chkUseSenderAsUsername_CheckedChanged(object sender, EventArgs e)
        {
            RefreshControls();
        }

        private void txtUsername_TextChanged(object sender, EventArgs e)
        {
            RefreshControls();
        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {
            RefreshControls();
        }

        private void txtFrom_TextChanged(object sender, EventArgs e)
        {
            RefreshControls();
        }

        private void txtTo_TextChanged(object sender, EventArgs e)
        {
            RefreshControls();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            btnSend.Enabled = false;

            try
            {
                SendEmail(Host, Port, Username, Password, Sender, Receivers);

                MessageBox.Show("Mail sent");
            }
            catch (Exception ex)
            {
                string tab = "";
                string details = "Message: " + ex.Message;
                details += Environment.NewLine + "Stack: " + ex.StackTrace;
                while ((ex = ex.InnerException) != null)
                {
                    tab += "\t";
                    details += Environment.NewLine + tab + "Message: " + ex.Message;
                    details += Environment.NewLine + tab + "Stack: " + ex.StackTrace.Replace(Environment.NewLine, Environment.NewLine + tab);
                }
                MessageBox.Show(details);
            }

            btnSend.Enabled = true;
        }

        private void RefreshControls()
        {
            txtUsername.Enabled = !chkUseSenderAsUsername.Checked;

            btnSend.Enabled = !String.IsNullOrEmpty(txtHost.Text)
                           && !String.IsNullOrEmpty(txtPort.Text)
                           && (chkUseSenderAsUsername.Checked || !String.IsNullOrEmpty(txtUsername.Text))
                           && !String.IsNullOrEmpty(txtPassword.Text)
                           && !String.IsNullOrEmpty(txtFrom.Text)
                           && !String.IsNullOrEmpty(txtTo.Text);
        }

        private string FilterNumeric(string text)
        {
            string filtered = "";
            foreach (char c in text)
                if (c >= '0' && c <= '9')
                    filtered += c;

            return filtered;
        }

        private void SendEmail(string host, int port, string username, string password, string from, List<string> to)
        {
            int attempt = 0;

Try_Again:
            attempt++;
            try
            {
                //Set up SMTP client
                SmtpClient client = new SmtpClient();
                client.Host = host;
                client.Port = port;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.EnableSsl = chkUseSSL.Checked;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(username, password);

                //Set up the email message
                MailMessage message = new MailMessage();
                message.From = new MailAddress(from);
                foreach (string receiver in to)
                    message.To.Add(new MailAddress(receiver));
                message.Subject = "SMTP Test";
                message.IsBodyHtml = true;
                message.Body = "This is a test e-mail from a DotNet SMTP client (generated at " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC") + ")";

                //Attempt to send the email
                client.Send(message);
            }
            catch (Exception ex)
            {
                if (ex is SmtpException)
                {
                    if ((attempt < 3) && (ex.Message.Contains("5.7.1"))) goto Try_Again;
                }

                throw ex;
            }
        }

        private string Host
        {
            get
            {
                return txtHost.Text;
            }
        }

        private int Port
        {
            get
            {
                return int.Parse(txtPort.Text);
            }
        }

        private string Sender
        {
            get
            {
                return txtFrom.Text;
            }
        }

        private bool UseSenderAsUsername
        {
            get
            {
                return chkUseSenderAsUsername.Checked;
            }
        }

        private string Username
        {
            get
            {
                if (UseSenderAsUsername)
                    return Sender;
                else
                    return txtUsername.Text;
            }
        }

        private string Password
        {
            get
            {
                return txtPassword.Text;
            }
        }

        private List<string> Receivers
        {
            get
            {
                List<string> recs = new List<string>();

                foreach (string line in txtTo.Lines)
                    if (!String.IsNullOrEmpty(line))
                        recs.Add(line);

                return recs;
            }
        }
    }
}
