using System;
using System.Data;

namespace RejectOrder.Logic
{
    public class HoliDays
    {
        //public static void prueba()
        //{
        //    ValidateDate(holiDays());
        //}
        public  DataTable holiDays(){
            DataTable holaiDays = new DataTable();
            holaiDays.Columns.Add("HoliDays", typeof(DateTime));
            DateTime fecha = DateTime.Now;
            string anoHoliday = fecha.ToString("yyyy");
            //Agregar datos
            holaiDays.Rows.Add(new object[] { "1/01" + "/"+ anoHoliday });
            holaiDays.Rows.Add(new object[] { "6/01" + "/"+ anoHoliday });
            holaiDays.Rows.Add(new object[] { "20/02" + "/"+ anoHoliday });
            holaiDays.Rows.Add(new object[] { "21/02" + "/"+ anoHoliday });
            holaiDays.Rows.Add(new object[] { "19/03" + "/"+ anoHoliday });
            holaiDays.Rows.Add(new object[] { "6/04" + "/"+ anoHoliday });
            holaiDays.Rows.Add(new object[] { "7/04" + "/"+ anoHoliday });
            holaiDays.Rows.Add(new object[] { "01/05" + "/"+ anoHoliday });
            holaiDays.Rows.Add(new object[] { "18/05" + "/"+ anoHoliday });
            holaiDays.Rows.Add(new object[] { "8/06" + "/"+ anoHoliday });
            holaiDays.Rows.Add(new object[] { "16/06" + "/"+ anoHoliday });
            holaiDays.Rows.Add(new object[] { "29/06" + "/"+ anoHoliday });
            holaiDays.Rows.Add(new object[] { "20/07" + "/"+ anoHoliday });
            holaiDays.Rows.Add(new object[] { "7/08" + "/"+ anoHoliday });
            holaiDays.Rows.Add(new object[] { "15/08" + "/"+ anoHoliday });
            holaiDays.Rows.Add(new object[] { "12/10" + "/"+ anoHoliday });
            holaiDays.Rows.Add(new object[] { "1/01" + "/"+ anoHoliday });
            holaiDays.Rows.Add(new object[] { "11/11" + "/"+ anoHoliday });
            holaiDays.Rows.Add(new object[] { "08/12" + "/"+ anoHoliday });
            holaiDays.Rows.Add(new object[] { "25/12" + "/"+ anoHoliday });
            return holaiDays;
        }

        //public static DataTable ValidateDate(DataTable holiDays)
        //{
        //    DataTable dataTable = new DataTable();
        //    dataTable = holiDays.Copy();
        //    DateTime fechasFestibos;
        //    DateTime desde = Convert.ToDateTime("9/10/2022");
        //    DateTime hasta = Convert.ToDateTime("15/10/2022");
        //    int diasHabiles = 0;
        //    DataRow row;

        //    dataTable.Columns.Add("DIAS_HABILES", typeof(string));
        //    while (desde <= hasta)
        //    {
        //        int numero_dia = Convert.ToInt16(desde.DayOfWeek.ToString("d"));
        //        if (numero_dia == 1 || numero_dia == 2 || numero_dia == 3 || numero_dia == 4 || numero_dia == 5)
        //        {
        //            for (int i = 0; i < dataTable.Rows.Count - 1; i++)
        //            {
        //                fechasFestibos = Convert.ToDateTime(dataTable.Rows[i]["HoliDays"]);
        //                if (desde == fechasFestibos && hasta > fechasFestibos)
        //                {
        //                    int numeroFestivo = Convert.ToInt32((fechasFestibos.DayOfWeek.ToString("d")));
        //                    if (numeroFestivo == 1 || numeroFestivo == 2 || numeroFestivo == 3 || numeroFestivo == 4 || numeroFestivo == 5)
        //                    {
        //                        diasHabiles--;
        //                    }
        //                }
        //                if (diasHabiles < 6)
        //                {
        //                    row = dataTable.Rows[i];   
        //                    row["DIAS_HABILES"] = diasHabiles;
                            
        //                }
        //                else
        //                {
        //                    dataTable.Rows[i].Delete();

        //                }
        //            }
        //            diasHabiles++;
                    
                    
        //        }
        //        desde = desde.AddDays(1);
        //    }
        //    Console.WriteLine("Total de dias habiles: " + diasHabiles.ToString());
        //    return dataTable;
        //}

    }
}
