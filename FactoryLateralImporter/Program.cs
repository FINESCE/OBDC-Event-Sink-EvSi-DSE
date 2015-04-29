using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace FactoryLateralImporter
{
    class Program
    {
        static void Main(string[] args)
        {

            // read data via Telnet

            TcpClient client = new TcpClient();
            client.Connect("137.226.151.158", 1500);
            Byte[] data = new Byte[0];

            NetworkStream stream = client.GetStream();
            stream.Write(data, 0, data.Length);

            
            data = new Byte[2048];
            String responseData = String.Empty;
            int bytes = stream.Read(data, 0, data.Length);
            while (bytes > 0)
            {
                
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                
                bytes = 0;
                bytes = stream.Read(data, 0, data.Length);
            }

            client.Close();

            // open database connection to store values 

            SqlConnection sqlConn = new SqlConnection("Network Address = 137.226.134.54; User ID = finesce_importer; Password = finesce123; Database = finesce; Connection Timeout = 30");
            try
            {
                sqlConn.Open();

                // parse data from Telnet to retrieve values

                string fullResponse = responseData.ToString();

                string[] valueRows = fullResponse.Split('\n');
                foreach (string valueRow in valueRows)
                {

                    string[] values = valueRow.Split(';');

                    DateTime time = new DateTime(int.Parse(values[0].Substring(0, 4)), int.Parse(values[0].Substring(5, 2)), int.Parse(values[0].Substring(8, 2)), int.Parse(values[0].Substring(11, 2)), int.Parse(values[0].Substring(14, 2)), 0);
                    string[] powerDecimals = values[1].Trim().Split('.');
                    Int64 powerBeforeDecimal = Int64.Parse(powerDecimals[0]);
                    Int64 powerBehindDecimal = 0;
                    if (powerDecimals[1].Length >= 3) powerBehindDecimal = Int64.Parse(powerDecimals[1].Substring(0, 3));
                    else powerBehindDecimal = Int64.Parse(powerDecimals[1]);

                    Int64 power = powerBeforeDecimal * 1000 + powerBehindDecimal;

                    string insertTime = time.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    string insertPower = power.ToString();

                    SqlCommand insertCmd = new SqlCommand("INSERT INTO finesce.dbo.energyConsumption (machine, timestamp, effective, isCumulative) Values ('000000000000', '" + insertTime + "', " + insertPower + ", 1)", sqlConn);
                    insertCmd.ExecuteNonQuery();

                }

                // close connection to sql

                sqlConn.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }
    }
}
