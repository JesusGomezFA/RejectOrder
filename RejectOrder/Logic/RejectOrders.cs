using Oracle.ManagedDataAccess.Client;
using RejectOrder.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;

namespace RejectOrder.Logic
{
    internal class RejectOrders
    {
        public static void RejectOrder()
        {
            SetDatatable(ValidateDate(BussinesDays(GetdataOracle(SetDataSql()))),GenerateDate());
            CreateFile(GenerateDate());

        }
        //obtiene los datos  que se van a consultar con devoluciones solo 10 dias atras desde la fecha actual
        public static DataTable SetDataSql()
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionSql()))
            {
                conn.Open();
                DateTime fechaActual = DateTime.Now;
                string fechaConsulta ;
                int dias = 9;
                DataTable datosSql = new DataTable();
                Console.WriteLine("llamando informacion");
                for (int i = 0; i < dias; i++)
                {
                    fechaConsulta = fechaActual.AddDays(-dias+i).ToString("d/MM/yyyy");
                    string sqlConsulta = "select * from MM_DTMovistarP where ESTADO_ORDEN <> 'Cancelado' and ESTADO_ORDEN <> 'en proceso' and FECHA_CIERRE_ORDEN like '%" + fechaConsulta + "%'";
                    SqlDataAdapter setData = new SqlDataAdapter(sqlConsulta, conn);
                    try
                    {
                        setData.Fill(datosSql);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error de Adaptador " + ex.Message);
                    }
                }
                return datosSql;
            }
           
        }
        //Obtiene las ordenes que tienen una devolucion en DB
        public static DataTable GetdataOracle(DataTable datosSql)
        {
            using (SqlConnection connectioSql = new SqlConnection(GetConnectionSql()))
            {
                connectioSql.Open ();
                using (OracleConnection oracleConnection = new OracleConnection(GetConnectionOracle()))
                {
                    oracleConnection.Open();
                    Console.WriteLine("Inicio Consulta Oracle");
                    List<string> listaConsulta = new List<string>();
                    string queryMV;
                    string union;
                    string query = "select * from MM_PGeneral where id = '5'";
                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(query, connectioSql);
                    DataTable consultaOracle = new DataTable();
                    consultaOracle = datosSql.Copy();
                    DataTable dbOracle = new DataTable();
                    DataTable dataTableOracle = new DataTable();
                    sqlDataAdapter.Fill(dbOracle);
                    for (int i = 0; i <= consultaOracle.Rows.Count - 1; i++)
                    {
                        listaConsulta.Add("'" + consultaOracle.Rows[i]["NUMERO_CELULAR"].ToString() + "'");
                        if (listaConsulta.Count == 999 || i == consultaOracle.Rows.Count - 1)
                        {
                            union = string.Join(",", listaConsulta);
                            queryMV = dbOracle.Rows[0]["Query"].ToString();
                            queryMV = queryMV.Replace("()", "(" + union + ")");
                            OracleCommand oracleCommand = new OracleCommand(queryMV, oracleConnection);
                            oracleCommand.CommandType = CommandType.Text;
                            OracleDataReader oracleDataReader = oracleCommand.ExecuteReader();
                            dataTableOracle.Load(oracleDataReader);
                            listaConsulta.Clear();
                            queryMV = "";

                        }
                    }
                    Console.WriteLine("Fin De consulta");
                    oracleConnection.Close();
                    return dataTableOracle;
                }
            }
        }
        //El metodo separa y deja ordenes en las cuales la orden de venta sea mayor a la devolucion.
        public static DataTable BussinesDays(DataTable dataTableOracle)
        {
            try
            {
                DateTime FECHA_CIERRE_ORDEN_VENTA;
                DateTime FECHA_ORDEN_DEVOLUCION;
                DataTable consultaDays = new DataTable();
                consultaDays = dataTableOracle.Copy();
                for (int i = 0; i < consultaDays.Rows.Count - 1; i++)
                {
                    FECHA_CIERRE_ORDEN_VENTA = Convert.ToDateTime(consultaDays.Rows[i]["FECHA_CIERRE_ORDEN_VENTA"]);
                    FECHA_ORDEN_DEVOLUCION = Convert.ToDateTime(consultaDays.Rows[i]["FECHA_ORDEN_DEVOLUCION"]);
                    if (FECHA_CIERRE_ORDEN_VENTA > FECHA_ORDEN_DEVOLUCION)
                    {
                        consultaDays.Rows.RemoveAt(i);
                    }
                }
                return consultaDays;
            }
            catch (Exception ex)
            {
                ErrorMessage(GenerateDate(), ex.Message);
                return null;
            }
           
        }
        //indica los dias habiles apartir de una fecha inicio hasta la fecha de devolucion teniendo en cuenta los festivos
        public static DataTable ValidateDate(DataTable holiDays)
        {
            HoliDays hDais = new HoliDays();
            DateTime FECHA_CIERRE_ORDEN_VENTA;
            DateTime FECHA_ORDEN_DEVOLUCION;
            DataTable consultaDays = new DataTable();
            consultaDays = holiDays.Copy();
            DataTable dais = new DataTable();
            DataTable dataTable = new DataTable();
            dais = hDais.holiDays();
            dataTable = hDais.holiDays().Copy();
            DateTime fechasFestibos;
            DataRow row;
            consultaDays.Columns.Add("DIAS_HABILES", typeof(string));
            try
            {
                for (int i = 0; i < consultaDays.Rows.Count; i++)
                {
                    FECHA_CIERRE_ORDEN_VENTA = Convert.ToDateTime(consultaDays.Rows[i]["FECHA_CIERRE_ORDEN_VENTA"]);
                    FECHA_ORDEN_DEVOLUCION = Convert.ToDateTime(consultaDays.Rows[i]["FECHA_ORDEN_DEVOLUCION"]);
                    int diasHabiles = 0;
                    while (FECHA_CIERRE_ORDEN_VENTA <= FECHA_ORDEN_DEVOLUCION)
                    {
                        int numero_dia = Convert.ToInt16(FECHA_CIERRE_ORDEN_VENTA.DayOfWeek.ToString("d"));
                        if (numero_dia == 1 || numero_dia == 2 || numero_dia == 3 || numero_dia == 4 || numero_dia == 5)
                        {
                            for (int j = 0; j < dataTable.Rows.Count - 1; j++)
                            {
                                fechasFestibos = Convert.ToDateTime(dataTable.Rows[j]["HoliDays"]);
                                if (FECHA_CIERRE_ORDEN_VENTA == fechasFestibos && FECHA_ORDEN_DEVOLUCION > fechasFestibos)
                                {
                                    int numeroFestivo = Convert.ToInt32((fechasFestibos.DayOfWeek.ToString("d")));
                                    if (numeroFestivo == 1 || numeroFestivo == 2 || numeroFestivo == 3 || numeroFestivo == 4 || numeroFestivo == 5)
                                    {
                                        diasHabiles--;
                                    }
                                }
                            }
                            diasHabiles++;
                            if (diasHabiles >= 6)
                            {
                                consultaDays.Rows.RemoveAt(i);
                            }
                            else
                            {
                                row = consultaDays.Rows[i];
                                row["DIAS_HABILES"] = diasHabiles;
                            }
                        }
                        FECHA_CIERRE_ORDEN_VENTA = FECHA_CIERRE_ORDEN_VENTA.AddDays(1);
                    }
                    if (consultaDays.Rows[i]["DIAS_HABILES"].ToString() == null)
                    {
                        row = consultaDays.Rows[i];
                        row["DIAS_HABILES"] = 1;
                    }
                }
            }
            catch (Exception ex)
            {

                ErrorMessage(GenerateDate(), ex.Message);
            }
            return consultaDays;
        }
        //envia DataTable a la base de datos SQL
        public static void SetDatatable(DataTable validateDate , string archivo)
        {

            using (SqlConnection connection = new SqlConnection(GetConnectionSql()))
            {
                connection.Open();
                DataTable dataSql = new DataTable();
                dataSql = validateDate.Copy();
                try
                {
                    Console.WriteLine("inicio de envio informacion a base de datos");
                    string nombreArchivo = "RejetOrder_" + archivo + ".csv";
                    // la devoluciion se debe de agregar en otra faze, tener en cuanta que la consulta a un no la identifica
                    string devolucion = "N/D";
                    Console.WriteLine("enviando datos a base de datos");
                    if (dataSql.Rows.Count > 0)
                    {
                        
                        for (int i = 0; i < dataSql.Rows.Count; i++)
                        {
                            SqlCommand sqlCommand = new SqlCommand("RejectOrder_Prueba", connection);
                            sqlCommand.CommandType = CommandType.StoredProcedure;
                            sqlCommand.Parameters.AddWithValue("@imei", dataSql.Rows[i]["IMEI"].ToString()); 
                            sqlCommand.Parameters.AddWithValue("@orderID", dataSql.Rows[i]["ORDER_ID"].ToString()); 
                            sqlCommand.Parameters.AddWithValue("@codigo_cliente", dataSql.Rows[i]["CODIGO_CLIENTE"].ToString()); 
                            sqlCommand.Parameters.AddWithValue("@tipo_identificacion", dataSql.Rows[i]["TIPO_IDENTIFICACION"].ToString()); 
                            sqlCommand.Parameters.AddWithValue("@numero_identificacion", dataSql.Rows[i]["NUMERO_IDENTIFICACION"].ToString()); 
                            sqlCommand.Parameters.AddWithValue("@fecha_cierre_orden_venta", dataSql.Rows[i]["FECHA_CIERRE_ORDEN_VENTA"].ToString()); 
                            sqlCommand.Parameters.AddWithValue("@fecha_entrega_equipo_cliente", dataSql.Rows[i]["FECHA_ENTREGA_EQUIPO_CLIENTE"].ToString()); 
                            sqlCommand.Parameters.AddWithValue("@orden_id_devolucion", dataSql.Rows[i]["ORDEN_ID_DEVOLUCION"].ToString()); 
                            sqlCommand.Parameters.AddWithValue("@valor_orden_devolucion", dataSql.Rows[i]["VALOR_ORDEN_DEVOLUCION"].ToString()); 
                            sqlCommand.Parameters.AddWithValue("@fecha_devolucion", dataSql.Rows[i]["FECHA_ORDEN_DEVOLUCION"].ToString()); 
                            sqlCommand.Parameters.AddWithValue("@estado_orden_devolucion", dataSql.Rows[i]["ESTADO_ORDEN_DEVOLUCION"].ToString()); 
                            sqlCommand.Parameters.AddWithValue("@motivo_devolucion", devolucion.ToString()/* dataSql.Rows[i]["MOTIVO_DEVOLUCION"].ToString()*/); 
                            sqlCommand.Parameters.AddWithValue("@dias_habiles", dataSql.Rows[i]["DIAS_HABILES"].ToString()); 
                            sqlCommand.Parameters.AddWithValue("@archivo",nombreArchivo.ToString());
                            sqlCommand.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        ErrorMessage(GenerateDate(), "No hay datos para enviar a base de datos");
                    }

                }
                catch (Exception ex)
                {
                    ErrorMessage(GenerateDate(), ex.Message);
                }
            }


        }
        //Genera Archivo que se envia a servidor
        public static void CreateFile(string archivo)
        {
            using (SqlConnection sqlConnection = new SqlConnection(GetConnectionSql()))
            {
                sqlConnection.Open();
                int contador = 0;
                SendFile sendFile = new SendFile();
                DateTime fecha = DateTime.Now;
                string fechaArchivo = archivo;
                SqlDataAdapter AdapMV = new SqlDataAdapter("SELECT * FROM MM_DTMovistarP WHERE  FECHA_CIERRE_ORDEN LIKE '%" + fecha + "%'", sqlConnection);
                DataTable consulaData = new DataTable();
                try
                {
                    AdapMV.Fill(consulaData);
                }
                catch (Exception ex)
                {

                    ErrorMessage(GenerateDate(), ex.Message);
                }
                try
                {
                    
                    if (consulaData.Rows.Count>0)
                    {
                        StreamWriter sw = new StreamWriter(@"E:\Documentos\RejectOrders_" + fechaArchivo + ".csv", false, Encoding.UTF8);
                        long cantidadColumnas = consulaData.Columns.Count;
                        for (int ncolumna = 0; ncolumna < cantidadColumnas; ncolumna++)
                        {
                            if (consulaData.Columns[ncolumna].ColumnName== "DIAS_HABILES" || consulaData.Columns[ncolumna].ColumnName == "ARCHIVO")
                            {

                            }
                            else
                            {
                                sw.Write(consulaData.Columns[ncolumna]);
                                if (ncolumna < cantidadColumnas - 1)
                                {
                                    sw.Write("|");
                                }
                            }
                        }
                        sw.Write(sw.NewLine); //saltamos linea
                        foreach (DataRow renglon in consulaData.Rows)
                        {
                            for (int ncolumna = 0; ncolumna < cantidadColumnas; ncolumna++)
                            {
                                if (consulaData.Columns[ncolumna].ColumnName == "DIAS_HABILES" || consulaData.Columns[ncolumna].ColumnName == "ARCHIVO")
                                {

                                }
                                else
                                {
                                    if (!Convert.IsDBNull(renglon[ncolumna]))
                                    {
                                        sw.Write(renglon[ncolumna]);
                                    }
                                    if (ncolumna < cantidadColumnas)
                                    {
                                        sw.Write("|");
                                    }
                                }

                            }
                            sw.Write(sw.NewLine); //saltamos linea
                            contador++;
                        }
                        if (contador < consulaData.Rows.Count)
                        {
                            string error = "No se envian todos los datos en el archivos";
                            ErrorMessage(GenerateDate(), error);
                        }
                        sw.Close();
                        sqlConnection.Close();
                        sendFile.Send(@"E:\Documentos\RejectOrders_" + fechaArchivo + ".csv");
                        sqlConnection.Dispose();
                    }
                    else
                    {
                        //CREAMOS ARCHIVO CSV
                        StreamWriter sw = new StreamWriter(@"E:\Documentos\RejectOrders_" + fechaArchivo + ".csv", false, Encoding.UTF8);
                        sw.Write("no se encontraron registros");
                        sw.Write(sw.NewLine); //saltamos linea
                        sw.Close();
                        sqlConnection.Close();
                        sendFile.Send(@"E:\Documentos\RejectOrders_" + fechaArchivo + ".csv");
                        sqlConnection.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    if (sqlConnection.State == ConnectionState.Open)
                    {
                        ErrorMessage(GenerateDate(), ex.Message);
                    }
                    else
                    {
                        ErrorMessage(GenerateDate(), ex.Message);
                    }
                }
            }
        }
        //metodo para realizar conexion con base de datos Oracle
        public static string GetConnectionOracle()
        {

            try
            {
                Connection connection = new Connection();
                OracleConnection connectionOracle = new OracleConnection();
                return connectionOracle.ConnectionString = connection.GetConnectionOracle(); ;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en conexion con oracle");
                using (SqlConnection cn = new SqlConnection(GetConnectionSql()))
                {
                    cn.Open();
                    string insert = $"insert into MM_Log(Fecha,Problema,Consola) values ('{FechaArchivo()}','problema al conectar con base de datos ORACLE','RejectOrders')";
                    SqlCommand comando = new SqlCommand(insert, cn);
                    comando.ExecuteNonQuery();
                    cn.Close();
                }
                Console.WriteLine("error: " + ex.Message);
                return null;

            }

        }
        //Conexion con Sql
        public static string GetConnectionSql()
        {
            Connection connection = new Connection();
            try
            {
                SqlConnection connectionSql = new SqlConnection();
                return connectionSql.ConnectionString = connection.GetConnectionMySql(); ;
            }
            catch (Exception ex)
            {

                Console.WriteLine("Error en conexion con SQL");
                SqlConnection connectionSql = new SqlConnection();
                connectionSql.Open();
                string insert = $"insert into MM_Log(Fecha,Problema,Consola) " +
                    $"values ('{FechaArchivo()}','problema al conectar con base de datos SQL','RejectOrders')";
                SqlCommand comando = new SqlCommand(insert, connectionSql);
                comando.ExecuteNonQuery();
                connectionSql.Close();
                Console.WriteLine("error: " + ex.Message);
                return null;
            }
        }
        // el metodo agrega la fecha actual con la hora para los mensajes de error
        public static string FechaArchivo()
        {
            DateTime fechaArchivo = DateTime.Now;
            string fecha_menos = fechaArchivo.AddDays(-1).ToString("dd/MM/yyyy");
            return fecha_menos;
        }
        //metodo que genera mensaje de error
        public static void ErrorMessage(string archivo, string error)
        {
            using (SqlConnection con = new SqlConnection(GetConnectionSql()))
            {
                con.Open();
                string FechaArchivo = DateTime.Now.ToString("dd-MM-yyyy");
                string horaArchivo = DateTime.Now.ToString("HH-mm");
                string insert = "insert into Errors(Fecha,Problema,Consola) values ('" + FechaArchivo + "_" + horaArchivo + "','Error_ " + error + "_al crear el archivo RejectOrders_" + archivo + ".csv','RejectOrders')";
                SqlCommand comando = new SqlCommand(insert, con);
                comando.ExecuteNonQuery();
                con.Close();
            }
            Console.WriteLine("Error Enviado a DB");
        }
        //Genera la fecha con la cual se va a guardar el archivo
        public static string GenerateDate()
        {
            string fechaArchivo = DateTime.Now.ToString("dd-MM-yyyy");
            string horaArchivo = DateTime.Now.ToString("HH-mm");
            string archivo = "" + fechaArchivo + "_" + horaArchivo + "";
            return archivo;
        }
    }
}
