using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Data.Odbc;

namespace Winschoolexport
{
    public partial class WinSchoolExport : Form
    {
        public WinSchoolExport()
        {
            InitializeComponent();
        }
        private void sendMail(string subject, string text)
        {
            string to = "mailaddress";
            string from = "WinSchoolExport <noreply@oszimt.de>";
            System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage(from, to);
            message.Subject = subject;
            message.Body = text;
            System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient();
            // Credentials are necessary if the server requires the client 
            // to authenticate before it will send e-mail on the client's behalf.
            System.Net.CredentialCache mycache = new System.Net.CredentialCache();
            mycache.Add(new Uri("https://bscw-oszimt.de"), 
                "Basic", new System.Net.NetworkCredential("mailaddress", "password"));
            client.Credentials = mycache;
            client.Host = "bscw-oszimt.de";
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential("mailaddress", "password");
            client.Send(message);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string server = txtServer.Text;
            string user = txtUser.Text;
            string password = txtPassword.Text;            
            string connectionString = "Data Source="+server+";Initial Catalog="+txtDatenbank.Text+";Persist Security Info=True;User ID="+user+";Password="+password;
            this.Text = "Exportiere ....";
            string queryString = "SELECT SchuelerTable.NR, Vorname, Name, Klasse, Tutor, Betrieb.Name1, Geschlecht FROM dbo.SchuelerTable LEFT JOIN dbo.Betrieb ON Kürzel=SchuelerTable.Ausbildungsbetrieb;";
            int anzahl = 0;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(queryString, connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    try
                    {
                        System.IO.StreamWriter sw = System.IO.File.CreateText("schueler.txt");
                        while (reader.Read())
                        {
                            for (int i = 0; i < 7; i++)
                                sw.Write(reader[i] + "|");
                            sw.WriteLine(reader[7]);
                            anzahl++;
                            this.Text = anzahl.ToString();
                            //listBox1.Items.Add(reader[1]+" "+reader[2]);
                        }
                        sw.Close();
                    }
                    finally
                    {
                        // Always call Close when done reading.
                        reader.Close();
                    }
                    connection.Close();
                }
            }
            catch (System.Data.SqlClient.SqlException err)
            {                
                System.IO.File.Delete("schueler.txt");
                sendMail("WinSchool-Fehler beim Export B1", 
                    "ACHTUNG:\n\nBeim Exportieren ist ein Fehler aufgetreten:\n\n" + 
                    err.Message + "\n\nBenutzte Einstellungen:\nBenutzer: "+
                    txtUser.Text+"\nServer: "+txtServer.Text+"\n\nDiese Nachricht wurde automatisch generiert " + DateTime.Now.ToString());
                System.Windows.Forms.MessageBox.Show("Es ist ein Fehler beim Exportieren aufgetreten:\n"+
                    err.Message+"\n\nBitte prüfen Sie die Erreichbarkeit des Servers mit dem Befehl 'ping "+
                    txtServer.Text+"' auf der Kommandozeile.", "Fehler!",
                   MessageBoxButtons.OK, MessageBoxIcon.Exclamation);              
            }
            sendeDaten(anzahl);
            this.Text = "WinSchoolExport";
        }
        private void sendeDaten(int anzahl)
        {
            // Daten senden
            if (System.IO.File.Exists("schueler.txt"))
            {
                System.Net.WebClient client = new System.Net.WebClient();
                client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                //Creating an instance of a credential cache, 
                //and passing the username and password to it
                System.Net.CredentialCache mycache = new System.Net.CredentialCache();
                mycache.Add(new Uri("https://url/winschool/testfile.php"),
                    "Basic", new System.Net.NetworkCredential("user", "password"));
                client.Credentials = mycache;
                try
                {
                    client.UploadFile("https://url/winschool/testfile.php", "POST", "schueler.txt");
                    //System.Windows.Forms.MessageBox.Show("Daten exportiert. Lokal sollte die Datei schueler.txt vorhanden sein.", "Erfolg!",
                    //   MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    sendMail("WinSchoolExport- Erfolg " + DateTime.Now.ToString(), "Export ist erfolgreich verlaufen.\nEs wurden " +
                        anzahl + " Datensätze exportiert.\n\nDie Daten wurden auf den Webserver hochgeladen.\n\nAutomatisch generiert am " + DateTime.Now.ToString());
                    System.Windows.Forms.MessageBox.Show("Daten exportiert. Lokal sollte die Datei schueler.txt vorhanden sein.", "Erfolg!",
                       MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                }
                catch (Exception e)
                {
                    System.Windows.Forms.MessageBox.Show("Daten konnten nicht hochgeladen werden. Lokal sollte die Datei schueler.txt vorhanden sein. "+e.Message, "Problem!",
                       MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            sendMail("Testsubjekt", "Dies ist ein Test!\nEs sollte eine E-Mail angekommen sein, die diesen Text enthält.");
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            {
                string server = txtServer.Text;
                string user = txtUser.Text;
                string password = txtPassword.Text;
                string connectionString = "DSN="+server+";UID=" + user + ";PWD=" + password+";Database="+txtDatenbank.Text;
                this.Text = "Exportiere ....";
                string queryString = "SELECT SchuelerTable.NR, Vorname, Name, Klasse, Tutor, Betrieb.Ausbildungsbetrieb, Geburtsdatum, Geschlecht FROM SchuelerTable LEFT JOIN dbo.Betrieb ON Kürzel=SchuelerTable.Ausbildungsbetrieb;";
                int anzahl = 0;
                try
                {
                    using (OdbcConnection connection = new OdbcConnection(connectionString))
                    {
                        OdbcCommand command = new OdbcCommand(queryString, connection);
                        connection.Open();
                        OdbcDataReader reader = command.ExecuteReader();
                        try
                        {
                            System.IO.StreamWriter sw = System.IO.File.CreateText("schueler.txt");
                            while (reader.Read())
                            {
                                for (int i = 0; i < 7; i++)
                                    sw.Write(reader[i] + "|");
                                sw.WriteLine(reader[7]);
                                anzahl++;
                                this.Text = anzahl.ToString();
                                //listBox1.Items.Add(reader[1]+" "+reader[2]);
                            }
                            sw.Close();
                        }
                        finally
                        {
                            // Always call Close when done reading.
                            reader.Close();
                        }
                        connection.Close();
                    }
                }
                catch (System.Data.Odbc.OdbcException err)
                {
                    System.IO.File.Delete("schueler.txt");
                    sendMail("WinSchool-Fehler beim Export B3",
                        "ACHTUNG:\n\nBeim Exportieren ist ein Fehler aufgetreten:\n\n" +
                        err.Message + "\n\nBenutzte Einstellungen:\nBenutzer: " +
                        txtUser.Text + "\nServer: " + txtServer.Text + "\n\nDiese Nachricht wurde automatisch generiert " + DateTime.Now.ToString());
                    System.Windows.Forms.MessageBox.Show("Es ist ein Fehler beim Exportieren aufgetreten:\n" +
                        err.Message + "\n\nBitte prüfen Sie die Erreichbarkeit des Servers mit dem Befehl 'ping " +
                        txtServer.Text + "' auf der Kommandozeile.", "Fehler!",
                       MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                sendeDaten(anzahl);
                this.Text = "WinSchoolExport";
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            System.Net.NetworkInformation.Ping pingSender = new System.Net.NetworkInformation.Ping();
            System.Net.NetworkInformation.PingOptions options = new System.Net.NetworkInformation.PingOptions();

            // Use the default Ttl value which is 128,
            // but change the fragmentation behavior.
            options.DontFragment = true;

            // Create a buffer of 32 bytes of data to be transmitted.
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 120;
            try
            {
                System.Net.NetworkInformation.PingReply reply = pingSender.Send(txtServer.Text, timeout, buffer, options);
                if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                {
                    System.Windows.Forms.MessageBox.Show("Rechner verfügbar!");
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Rechner nicht erreichbar!");
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Ping-Fehler: " + ex.Message);
            }

        }
    }
}