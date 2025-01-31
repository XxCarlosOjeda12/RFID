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
        protected string m_usuario = "eventos";
        protected string m_contrasena = "i1CQ^.?FT+S!";
        protected string m_connstring;
        public RegistroDB(string host, string DB)
        {
            m_host = host;
            m_db = DB;
            m_connstring = "server=" + m_host + "; database=" + m_db + "; Uid=" + m_usuario + "; pwd=" + m_contrasena;
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
                cmd.CommandText = "SELECT Nombre, ApellidoPaterno, ApellidoMaterno, Telefono, Movil, Email, Confirmacion, EPC, Compania, Fotografia, Registrado, Puesto FROM registro WHERE Confirmacion = '" + Confirmacion + "'";
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
        public bool ExisteEPC(string EPC)
        {
            bool ret = false;
            try
            {
                using (MySqlConnection con = new MySqlConnection(m_connstring))
                {
                    con.Open();



                    string query = "select EPC from registro where EPC = '" + EPC + "'";

                    MySqlCommand ComandoBD = new MySqlCommand(query, con);
                    MySqlDataReader reader = ComandoBD.ExecuteReader();

                    ret = reader.Read();
                    ComandoBD.Dispose();
                    reader.Dispose();
                    con.Close();
                }

            }
            catch (Exception e)
            {
                return false;
            }

            return ret;
        }

        public bool PreguntarBienvenida(string EPC)
        {
            bool ret = false;
            using (MySqlConnection con = new MySqlConnection(m_connstring))
            {

                con.Open();
                MySqlCommand cmd = con.CreateCommand();
                cmd.CommandText = "select idRegistro from registro where EPC = '" + EPC + "' and Bienvenida = 0";
                MySqlDataReader rd = cmd.ExecuteReader();
                ret=rd.Read();
                rd.Close();
                rd.Dispose();
                cmd.Dispose();
                con.Close();
            }
            return ret;

        }
        public void MarcarBienvenida(string EPC)
        {
            
            using (MySqlConnection con = new MySqlConnection(m_connstring))
            {

                con.Open();
                MySqlCommand cmd = con.CreateCommand();
                cmd.CommandText = "update registro set Bienvenida = 1 where EPC = '" + EPC + "'";
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                con.Close();
            }
            

        }


        public string[] ObtenerParametros(List<string> Parametros, string EPC)
        {
            string[] registro = null;
            try
            {
                using (MySqlConnection con = new MySqlConnection(m_connstring))
                {
                    con.Open();



                    int i = 0;
                    if (Parametros.Count != 0)
                    {
                        registro = new string[Parametros.Count+1];
                        string query = "select idRegistro,";
                        for (i = 0; i < Parametros.Count; i++)
                        {
                            if (i != 0)
                                query += ", ";
                            query += Parametros[i];
                        }
                        query += " from registro where EPC = '" + EPC + "'";


                        MySqlCommand ComandoBD = new MySqlCommand(query, con);
                        MySqlDataReader reader = ComandoBD.ExecuteReader();

                        if (!reader.Read())
                        {
                            registro = null;
                        }
                        else
                        {
                            for (i = 0; i < registro.Length; i++)
                            {
                                registro[i] = reader.GetString(i);
                            }
                        }
                        ComandoBD.Dispose();
                        reader.Dispose();
                    }


                    con.Close();

                }
            }
            catch (Exception e)
            {

                return null;
            }

            return registro;
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
