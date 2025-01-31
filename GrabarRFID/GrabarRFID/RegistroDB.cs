using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
namespace Sitacomm
{
    public class RegistroDB
    {
        protected string m_host;
        protected string m_db;
        protected string m_usuario = "root";
        protected string m_contrasena = "Astalavist4";
        protected string m_connstring;
        public RegistroDB(string host, string DB)
        {
            m_host = host;
            m_db = DB;
            m_connstring = "server=" + m_host + "; database=" + m_db + "; Uid=" + m_usuario + "; pwd=" + m_contrasena;
        }
        public long AgregarEPC(string EPC)
        {
            long id = 0;
            using (MySqlConnection con = new MySqlConnection(m_connstring))
            {
                con.Open();
                MySqlCommand cmd = con.CreateCommand();

                cmd.CommandText = "SELECT idEPC from epc WHERE EPC = '" + EPC + "'";
                MySqlDataReader rd = cmd.ExecuteReader();
                if (!rd.Read())
                {
                    rd.Close();
                    rd.Dispose();
                    cmd.CommandText = "INSERT INTO epc (EPC) VALUES('" + EPC + "')";
                    cmd.ExecuteNonQuery();
                    id = cmd.LastInsertedId;
                }
                else
                {
                    id = Convert.ToInt32(rd[0]);
                    rd.Close();
                    rd.Dispose();

                }

                con.Close();
            }
            return id;
        }

        public long ObtenerEPC(string EPC)
        {
            long id = 0;
            using (MySqlConnection con = new MySqlConnection(m_connstring))
            {
                con.Open();
                MySqlCommand cmd = con.CreateCommand();
                cmd.CommandText = "SELECT idEPC from epc where epc = '" + EPC + "'";
                MySqlDataReader rd =  cmd.ExecuteReader();
                if (rd.Read())
                {
                    id = Convert.ToInt64(rd[0]);
                }
                rd.Close();
                rd.Dispose();
                con.Close();
            }
            return id;
        }

        public void ActualizarPersona(Persona asistente)
        {

            using (MySqlConnection con = new MySqlConnection(m_connstring))
            {
                con.Open();
                MySqlCommand cmd = con.CreateCommand();

                cmd.CommandText = "UPDATE registro SET Nombre = '" + asistente.Nombre + "', ApellidoPaterno ='" + asistente.ApellidoPaterno +
                    "', ApellidoMaterno = '" + asistente.ApellidoMaterno + "', Telefono = '" + asistente.Telefono + "', Movil = '" + asistente.Movil +
                    "', Email ='" + asistente.CorreoElectronico + "', EPC = '" + asistente.EPC + "', Fotografia = " + asistente.Fotografia +
                    ", Registrado = " + asistente.Registrado + ", Puesto ='" + asistente.Puesto + "' WHERE Confirmacion = '" + asistente.Confirmacion + "'";
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                con.Close();

            }
        }


        public Persona ObtenerPersona(string Confirmacion)
        {
            Persona asistente = null;
            using (MySqlConnection con = new MySqlConnection(m_connstring))
            {
                con.Open();
                MySqlCommand cmd = con.CreateCommand();
                cmd.CommandText = "SELECT Nombre, ApellidoPaterno, ApellidoMaterno, Telefono, Movil, Email, Confirmacion, EPC, Compania, Fotografia, Registrado, Puesto FROM personas WHERE Confirmacion = '" + Confirmacion + "'";
                MySqlDataReader rd = cmd.ExecuteReader();
                if (rd.Read())
                {
                    asistente = new Persona();
                    asistente.Nombre = Convert.ToString(rd[0].ToString());
                    asistente.ApellidoPaterno = Convert.ToString(rd[1].ToString());
                    asistente.ApellidoMaterno = Convert.ToString(rd[2].ToString());
                    asistente.Telefono = Convert.ToString(rd[3].ToString());
                    asistente.Movil = Convert.ToString(rd[4].ToString());
                    asistente.CorreoElectronico = Convert.ToString(rd[5].ToString());
                    asistente.Confirmacion = Convert.ToString(rd[6].ToString());
                    asistente.EPC = Convert.ToString(rd[7].ToString());
                    asistente.Compania = Convert.ToString(rd[8].ToString());
                    asistente.Fotografia = Convert.ToInt32(rd[9].ToString());
                    asistente.Registrado = Convert.ToInt32(rd[10].ToString());
                    asistente.Puesto = Convert.ToString(rd[11].ToString());
                }
                rd.Close();
                rd.Dispose();
                cmd.Dispose();
                con.Close();
            }
            return asistente;
        }

    }
    public class Persona
    {
        public string Nombre;
        public string ApellidoPaterno;
        public string ApellidoMaterno;
        public string Telefono;
        public string Movil;
        public string Compania;
        public string Puesto;
        public string Confirmacion;
        public string EPC;
        public string CorreoElectronico;
        public int Fotografia;
        public int Registrado;
    }
}
