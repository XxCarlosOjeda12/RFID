using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Impinj.OctaneSdk;
using Sitacomm.RFID;
using System.IO;

namespace Sitacomm
{
    public class RegistroDB
    {
        protected string m_host="10.1.0.100";
        protected string m_db = "eventosregistro";
        protected string m_usuario = "root";
        protected string m_contrasena = "Astalavist4";
        protected string m_connstring;
        public RegistroDB(string host, string DB)
        {
           // m_host = host;
            //m_db = DB;
            m_connstring = "server=" + m_host + "; database=" + m_db + "; Uid=" + m_usuario + "; pwd=" + m_contrasena;
        }

        public string ObtenerQueryEPC(RFIDTagReport report, string tipo)
        {
            string query = "SELECT idEPC, idRegistro, estado, EPC FROM epc WHERE EPC in (";
            if (tipo == "Impijn")
            {
                TagReport tr = ((RFIDImpijnTagReport)report).Report;
                foreach (Tag tag in tr)
                {
                    query += ("'" + tag.Epc.ToString().Replace(" ", string.Empty) + "',");
                }
            }
            else if (tipo == "OPPIOT")
            {
                List<string> tr = ((RFIDOppiotTagReport)report).Report;

                foreach (string tag in tr)
                {

                    query += ("'" + tag + "',");

                }
            }
            return query.Remove(query.Length - 1) + ')';
        }

        public string QueryBienvenida(RFIDTagReport report, string tipo)
        {
            string query = "SELECT NombreCompleto FROM registro WHERE EPC in (";
            if (tipo == "Impijn")
            {
                TagReport tr = ((RFIDImpijnTagReport)report).Report;
                foreach (Tag tag in tr)
                {
                    query += ("'" + tag.Epc.ToString().Replace(" ", string.Empty) + "',");
                }
            }
            else if (tipo == "OPPIOT")
            {
                List<string> tr = ((RFIDOppiotTagReport)report).Report;

                foreach (string tag in tr)
                {

                    query += ("'" + tag + "',");

                }
            }
            return query.Remove(query.Length - 1) + ") AND Bienvenida = 0";
        }

        public List<string> Bienvenida(RFIDTagReport report,  string tipo)
        {

            List<string> lista = new List<string>();
            using (MySqlConnection con = new MySqlConnection(m_connstring))
            {
                con.Open();
                MySqlCommand cmd = con.CreateCommand();

                cmd.CommandText = QueryBienvenida(report, tipo);
                

                MySqlDataReader rd = cmd.ExecuteReader();
                
                while (rd.Read())
                {
                    EPC epc = new EPC();
                    lista.Add(Convert.ToString(rd[0]));
                }

            }
            return lista;
        }

        public List<EPC> Salida(RFIDTagReport report, int sala, string tipo)
        {
        
            List<EPC> lista = new List<EPC>();
            using (MySqlConnection con = new MySqlConnection(m_connstring))
            {
                con.Open();
                MySqlCommand cmd = con.CreateCommand();

                cmd.CommandText = ObtenerQueryEPC(report, tipo);
                //Console.WriteLine("Salida: " + cmd.CommandText);

                MySqlDataReader rd = cmd.ExecuteReader();
                int nuevos = 0;
                while (rd.Read())
                {
                    EPC epc = new EPC();
                    epc.Estado = Convert.ToInt32(rd[2]);
                   // epc.IdRegistro = Convert.ToInt32(rd[1]);
                    epc.IdEPC = Convert.ToInt32(rd[0]);
                    epc.epc = Convert.ToString(rd[3]);
                    epc.ActualizaEstado = false;
                    epc.InsertaEvento = false;

                    if (epc.Estado == 0)
                    {
                        epc.ActualizaEstado = true;

                    }
                    else if (epc.Estado == 2)
                    {
                        epc.ActualizaEstado = true;
                        epc.InsertaEvento = true;//entrar!!
                        nuevos++;
                    }
                    epc.Estado = 1;
                    lista.Add(epc);
                    /*try
                    {
                        File.WriteAllText("EPC.TXT", DateTime.Now.ToString() + " " + epc.IdEPC + " " + epc.epc + " " + 1 + "\n");
                    }
                    catch
                    {
                    }*/


                }
                rd.Close();
                rd.Dispose();





                if (lista.Count() != 0)
                {
                    string hora = DateTime.Now.ToString("yyyy-MM-dd H:mm:ss");
                    string query = "";

                    if (nuevos != 0)
                    {
                        //entrar
                        int asistencia = 0;

                         cmd.CommandText = "SELECT count(*) as asistencia FROM  eventosregistro.epc where estado = 1";
;
                  //      System.Console.WriteLine("Salida: " + cmd.CommandText);
                        rd = cmd.ExecuteReader();
                        asistencia = ((!rd.Read()) ? 0 : Convert.ToInt32(rd[0])) + nuevos;

                        rd.Close();
                        rd.Dispose();

                        cmd.CommandText = "UPDATE asistenciaactual SET asistencia = " + asistencia + " WHERE idsala = " + sala;
                        cmd.ExecuteNonQuery();

                        cmd.CommandText = "INSERT INTO asistenciasala (idSala, Asistencia, Hora) VALUES(" + sala + ", " + asistencia + ", '" + hora + "')";
                    //        System.Console.WriteLine("Salida: " + cmd.CommandText);
                        cmd.ExecuteNonQuery();
                        query = "INSERT INTO asistencia (idSala, epc, Hora, Ubicacion) VALUES ";

                        foreach (EPC e in lista)
                        {
                            if (e.InsertaEvento)
                                query += "(" + sala + ", '" + e.epc + "', '" + hora + "', 1) ,";
                        }

                        cmd.CommandText = query.Remove(query.Length - 1);

                      //  System.Console.WriteLine("Salida: " + cmd.CommandText);
                        cmd.ExecuteNonQuery();
                    }
                    int i=0;
                    query = "UPDATE epc SET estado = 1 WHERE idEPC in (";
                    foreach (EPC e in lista)
                    {
                        if (e.ActualizaEstado)
                        {
                            i++;
                            query += e.IdEPC + ",";
                        }
                    }
                    if (i != 0)
                    {
                        cmd.CommandText = query.Remove(query.Length - 1) + ')';

                        //System.Console.WriteLine("Salida: " + cmd.CommandText);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            return lista;
        }
        public List<EPC> Entrada(RFIDTagReport report, int sala, string tipo)
        {

            List<EPC> lista = new List<EPC>();
            using (MySqlConnection con = new MySqlConnection(m_connstring))
            {
                con.Open();
                MySqlCommand cmd = con.CreateCommand();

                cmd.CommandText = ObtenerQueryEPC(report, tipo);
                //System.Console.WriteLine("Entrada: " + cmd.CommandText);

                MySqlDataReader rd = cmd.ExecuteReader();
                int nuevos = 0;
                while (rd.Read())
                {
                    EPC epc = new EPC();
                    epc.Estado = Convert.ToInt32(rd[2]);                    
                    epc.IdEPC = Convert.ToInt32(rd[0]);
                    epc.epc = Convert.ToString(rd[3]);
                    epc.ActualizaEstado = false;
                    epc.InsertaEvento = false;

                    if (epc.Estado == 0)
                    {
                        epc.ActualizaEstado = true;

                    }
                    else if (epc.Estado == 1)
                    {
                        epc.ActualizaEstado = true;
                        epc.InsertaEvento = true;//salir!!
                        nuevos++;
                    }
                    epc.Estado = 2;
                    lista.Add(epc);
                    /*try
                    {
                        File.WriteAllText("EPC.TXT", DateTime.Now.ToString() + " " + epc.IdEPC + " " + epc.epc + " " + 2 + "\n");
                    }
                    catch
                    {
                    }*/
            }
                rd.Close();
                rd.Dispose();





                if (lista.Count() != 0)
                {
                    string hora = DateTime.Now.ToString("yyyy-MM-dd H:mm:ss");
                    string query = "";

                    if (nuevos != 0)
                    {
                        //salir
                        int asistencia = 0;

                        cmd.CommandText = "SELECT count(*) as asistencia FROM  eventosregistro.epc where estado = 1";
                  //      System.Console.WriteLine("ENTRADA: " + cmd.CommandText);
                        rd = cmd.ExecuteReader();
                        asistencia = ((!rd.Read()) ? 0 : Convert.ToInt32(rd[0])) - nuevos;

                        if(asistencia < 0)
                        {
                            asistencia = 0;
                        }

                        rd.Close();
                        rd.Dispose();



                        cmd.CommandText = "UPDATE asistenciaactual SET asistencia = " + asistencia + " WHERE idsala = " + sala;
                        cmd.ExecuteNonQuery();

                        cmd.CommandText = "INSERT INTO asistenciasala (idSala, Asistencia, Hora) VALUES(" + sala + ", " + asistencia + ", '" + hora + "')";
                    //    System.Console.WriteLine("Salida: " + cmd.CommandText);
                        cmd.ExecuteNonQuery();
                        query = "INSERT INTO asistencia (idSala, epc, Hora, Ubicacion) VALUES ";

                        foreach (EPC e in lista)
                        {
                            if (e.InsertaEvento)
                                query += "(" + sala + ", '" + e.epc + "', '" + hora + "', 0) ,";
                        }

                        cmd.CommandText = query.Remove(query.Length - 1);

//                        System.Console.WriteLine("ENTRADA: " + cmd.CommandText);
                        cmd.ExecuteNonQuery();
                    }
                    int i = 0;
                    query = "UPDATE epc SET estado = 2 WHERE idEPC in (";
                    foreach (EPC e in lista)
                    {
                        if (e.ActualizaEstado)
                        {
                            i++;
                            query += e.IdEPC + ",";
                        }
                    }
                    if (i != 0)
                    {
                        cmd.CommandText = query.Remove(query.Length - 1) + ')';

  //                      System.Console.WriteLine("ENTRADA: " + cmd.CommandText);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            return lista;
        }




        /*        public bool Salida(string EPC, int sala)
                {
                    bool ret = false;
                    using (MySqlConnection con = new MySqlConnection(m_connstring))
                    {

                        con.Open();
                        MySqlCommand cmd = con.CreateCommand();
                        bool updateEPC = false;
                        bool updateAsistencia = false;

                        cmd.CommandText = "SELECT idEPC, idRegistro, estado FROM epc WHERE EPC='" + EPC + "'";
                        //System.Console.WriteLine("Salida: " + cmd.CommandText);
                        MySqlDataReader rd = cmd.ExecuteReader();
                        if (rd.Read())
                        {
                            int estado = Convert.ToInt32(rd[2]);
                            int idRegistro = Convert.ToInt32(rd[1]);
                            int idEPC = Convert.ToInt32(rd[0]);
                            string hora = DateTime.Now.ToString("yyyy-MM-dd H:mm:ss");
                            rd.Close();
                            rd.Dispose();

                            if (estado == 0)
                            {
                                updateEPC = true;

                            }
                            else if (estado == 2)
                            {
                                updateEPC = true;
                                updateAsistencia = true;//entrar!!
                                ret = true;
                            }
                            estado = 1;
                            if (updateAsistencia)
                            {
                                //entrar
                                int asistencia = 0;

                                //                        cmd.CommandText = "LOCK TABLES asistenciasala, asistencia WRITE";
                                //                        System.Console.WriteLine("Salida: " + cmd.CommandText);
                                //                        cmd.ExecuteNonQuery();

                                cmd.CommandText = "SELECT asistencia  FROM asistenciaactual WHERE  idsala = " + sala;
                                //      System.Console.WriteLine("Salida: " + cmd.CommandText);
                                rd = cmd.ExecuteReader();
                                if (!rd.Read())
                                {
                                    asistencia = 1;
                                }
                                else
                                {
                                    asistencia = Convert.ToInt32(rd[0]) + 1;


                                }
                                rd.Close();
                                rd.Dispose();
                                cmd.CommandText = "UPDATE asistenciaactual SET asistencia = " + asistencia + " WHERE idsala = " + sala;
                                cmd.ExecuteNonQuery();

                                cmd.CommandText = "INSERT INTO asistenciasala (idSala, Asistencia, Hora) VALUES(" + sala + ", " + asistencia + ", '" + hora + "')";
                            //    System.Console.WriteLine("Salida: " + cmd.CommandText);
                                cmd.ExecuteNonQuery();
                                //                          cmd.CommandText = "UNLOCK TABLES";
                                //cmd.ExecuteNonQuery();

                                cmd.CommandText = "INSERT INTO asistencia (idSala, idregistro, Hora, Ubicacion) VALUES(" + sala + ", " + idRegistro + ", '" +
                                    hora + "', 1)";
                              //  System.Console.WriteLine("Salida: " + cmd.CommandText);
                                cmd.ExecuteNonQuery();
                            }
                            if (updateEPC)
                            {
                                //                        cmd.CommandText = "LOCK TABLES epc WRITE";
                                //cmd.ExecuteNonQuery();

                                cmd.CommandText = "UPDATE epc SET estado = " + estado + " WHERE idEPC = " + idEPC;
                                //System.Console.WriteLine("Salida: " + cmd.CommandText);
                                cmd.ExecuteNonQuery();

                                //                        cmd.CommandText = "UNLOCK TABLES";
                                //cmd.ExecuteNonQuery();

                            }

                        }
                        else
                        {
                            rd.Close();
                            rd.Dispose();
                        }
                        cmd.Dispose();
                        con.Close();


                    }

                    return ret;
                }

                public bool Entrada(string EPC, int sala)
                {
                    bool ret = false;
                    using (MySqlConnection con = new MySqlConnection(m_connstring))
                    {

                        con.Open();
                        MySqlCommand cmd = con.CreateCommand();
                        bool updateEPC = false;
                        bool updateAsistencia = false;

                        cmd.CommandText = "SELECT idEPC, idRegistro, estado FROM epc WHERE EPC='" + EPC + "'";
                                  //      System.Console.WriteLine("Entrada: " + cmd.CommandText);
                        MySqlDataReader rd = cmd.ExecuteReader();
                        if (rd.Read())
                        {
                            int estado = Convert.ToInt32(rd[2]);
                            int idRegistro = Convert.ToInt32(rd[1]);
                            int idEPC = Convert.ToInt32(rd[0]);
                            string hora = DateTime.Now.ToString("yyyy-MM-dd H:mm:ss");
                            rd.Close();
                            rd.Dispose();

                            if (estado == 0)
                            {
                                updateEPC = true;

                            }
                            else if (estado == 1)
                            {
                                updateEPC = true;
                                updateAsistencia = true;//entrar!!
                                ret = true;
                            }
                            estado = 2;
                            if (updateAsistencia)
                            {
                                //entrar
                                int asistencia = 0;

                                //                        cmd.CommandText = "LOCK TABLES asistenciasala, asistencia WRITE";
                                //                        System.Console.WriteLine("Salida: " + cmd.CommandText);
                                //                        cmd.ExecuteNonQuery();

                                cmd.CommandText = "SELECT asistencia  FROM asistenciaactual WHERE  idsala = " + sala;
                                //System.Console.WriteLine("Entrada: " + cmd.CommandText);
                                rd = cmd.ExecuteReader();
                                if (!rd.Read())
                                {
                                    asistencia = 0;
                                }
                                else
                                {
                                    asistencia = Convert.ToInt32(rd[0]) ;
                                    asistencia = (asistencia > 0) ? asistencia - 1 : 0;

                                }
                                rd.Close();
                                rd.Dispose();

                                cmd.CommandText = "UPDATE asistenciaactual SET asistencia = " + asistencia + " WHERE idsala = " + sala;
                                cmd.ExecuteNonQuery();

                                cmd.CommandText = "INSERT INTO asistenciasala (idSala, Asistencia, Hora) VALUES(" + sala + ", " + asistencia + ", '" + hora + "')";
                                  //                          System.Console.WriteLine("Entrada: " + cmd.CommandText);
                                cmd.ExecuteNonQuery();
                                //                          cmd.CommandText = "UNLOCK TABLES";
                                //cmd.ExecuteNonQuery();

                                cmd.CommandText = "INSERT INTO asistencia (idSala, idregistro, Hora, Ubicacion) VALUES(" + sala + ", " + idRegistro + ", '" +
                                    hora + "', 0)";
                                    //                   System.Console.WriteLine("Entrada: " + cmd.CommandText);
                                cmd.ExecuteNonQuery();

                            }
                            if (updateEPC)
                            {
                                //                        cmd.CommandText = "LOCK TABLES epc WRITE";
                                //cmd.ExecuteNonQuery();

                                cmd.CommandText = "UPDATE epc SET estado = " + estado + " WHERE idEPC = " + idEPC;
                                      //                  System.Console.WriteLine("Entrada: " + cmd.CommandText);
                                cmd.ExecuteNonQuery();

                                //                        cmd.CommandText = "UNLOCK TABLES";
                                //cmd.ExecuteNonQuery();

                            }

                        }
                        else
                        {
                            rd.Close();
                            rd.Dispose();
                        }
                        cmd.Dispose();
                        con.Close();


                    }

                    return ret;
                }
        */


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
        public bool ExisteEPC(string EPC)
        {
            bool ret = false;
            try
            {
                using (MySqlConnection con = new MySqlConnection(m_connstring))
                {
                    con.Open();



                    string query = "select EPC from personas where EPC = '" + EPC + "'";

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
                cmd.CommandText = "select idRegistro from personas where EPC = '" + EPC + "' and Bienvenida = 0";
                MySqlDataReader rd = cmd.ExecuteReader();
                ret=rd.Read();
                rd.Close();
                rd.Dispose();
                cmd.Dispose();
                con.Close();
            }
            return ret;

        }
        public void MarcarBienvenida(string Nombre)
        {
            
            using (MySqlConnection con = new MySqlConnection(m_connstring))
            {

                con.Open();
                MySqlCommand cmd = con.CreateCommand();
                cmd.CommandText = "update registro set Bienvenida = 1 where NombreCompleto = '" + Nombre + "'";
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
                        registro = new string[Parametros.Count];
                        string query = "select ";
                        for (i = 0; i < registro.Length; i++)
                        {
                            if (i != 0)
                                query += ", ";
                            query += Parametros[i];
                        }
                        query += " from personas where EPC = '" + EPC + "'";


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
