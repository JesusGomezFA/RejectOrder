namespace RejectOrder
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Logic.RejectOrders.RejectOrder();
            //Logic.HoliDays.prueba();
            System.Threading.Thread.Sleep(60000);
        }
    }
}