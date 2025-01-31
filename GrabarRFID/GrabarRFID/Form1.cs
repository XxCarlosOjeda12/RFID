using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sitacomm.RFID;
using Sitacomm;
using System.Threading;
namespace GrabarRFID
{
    public partial class Form1 : Form
    {
        protected RFID105Reader m_RFID = new RFID105Reader();
        protected RegistroDB m_db = new RegistroDB("localhost", "eventosregistro");
        public Form1()
        {
            InitializeComponent();
          //  textBox1.Text=
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length != 0)
            {
                string subs = textBox1.Text.Substring(textBox1.Text.Length - 4, 4);

                try
                {

                    
                    int ret = m_RFID.EscribirEPC(textBox1.Text, "00000000");
                    Thread.Sleep(300);
                    if (ret == 0)
                    {
                        string epc = m_RFID.LeerEPC();
                        if (epc == textBox1.Text)
                        {
                            try
                            {
                                if (m_db.ObtenerEPC(epc) == 0)
                                {
                                    m_db.AgregarEPC(epc);
                                    label2.Text = "Validado";
                                    long cont = long.Parse(subs, System.Globalization.NumberStyles.HexNumber);
                                    cont++;
                                    cont %= 0x10000;
                                    subs = cont.ToString("X4");

                                    textBox1.Text = textBox1.Text.Remove(textBox1.Text.Length - 4) + subs;
                                }
                                else
                                {
                                    label2.Text = "Chip ya registrado";
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Error: " + ex.Message);
                            }


                        }

                    }
                    else
                    {
                        label2.Text = "Error.";
                    }
                    




                }
                catch(Exception ex)
                {
                    MessageBox.Show("Error", ex.Message);
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            m_RFID.AbrirLector();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = m_RFID.LeerEPC();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
