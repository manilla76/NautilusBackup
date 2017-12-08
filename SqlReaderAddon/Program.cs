using NDesk.Options;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Thermo.Datapool;
using static Thermo.Datapool.Datapool;

namespace SqlReaderAddon
{
    class Program
    {
        /*  Receive pile number from datapool as arg 
            Read SQL on Stockpile System (IP received as arg)
            Select Product Info that matches that pile number
            Write tag and important product info to datapool
            Exit

            Example: SqlReaderAddon -p 2574 -i 192.168.1.12 -g Product -t Name
             */
        static void Main(string[] args)
        {
            string pileNumber = string.Empty;
            string ipAddress = string.Empty;
            string pileName = string.Empty;
            string dpGroup = string.Empty;
            string dpTag = string.Empty;
            var p = new OptionSet(){
                { "p|pile=","the {Pile} number to look up.",
                    v =>pileNumber = v.ToString()},
                { "i|ip=","Ip Address of SQL Server",
                    v =>ipAddress = v.ToString()},
                { "g|group=","the datapool {Group} to store into.",
                    v=>dpGroup = v.ToString()},
                { "t|tag=","the datapool {Tag} to store into.",
                    v=>dpTag = v.ToString()}
            };

            p.Parse(args);
            if (ipAddress == string.Empty | pileNumber == string.Empty)
                return;
            using (SqlConnection conn = new SqlConnection())
            {
                string insString = @"SELECT Top 1 * FROM [Nautilus].[dbo].[ProductTable] WHERE name LIKE '%" + pileNumber + @"%'";
                conn.ConnectionString = "Server=" + ipAddress + ";Database=nautilus;Trusted_Connection=True;";
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(insString, conn))
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        pileName = (string)reader["name"];
                    }
                }
            }

            DatapoolSvr.IpAddress = @"127.0.0.1";
            DatapoolSvr.MonitorPeriodMilliSeconds = 1000;
            Console.WriteLine(DatapoolSvr.IsConnected);
            // DatapoolSvr.MonitorTags(tagList);
            Datapool.ITagInfo tagInfo = DatapoolSvr.CreateTagInfo("car", "temp", dpTypes.FLOAT);
            tagInfo.UpdateValueEvent += TagInfo_UpdateValueEvent;
            Console.WriteLine(DatapoolSvr.IsConnected);
            /*
            // This is our Unicode string:
            string unicodeWrite = @"Write [<" + dpGroup + @"><" + dpTag + @"><" + pileName + @">]";  // datapool write...Tag must already exist
            // datapool read example:   string readCode = @"Read [<car><temp>]";

            // Convert a string to utf-8 bytes.
            byte[] utfWrite = Encoding.UTF8.GetBytes(unicodeWrite);
            byte[] uCode = new byte[utfWrite.Length * 2 + 1];
            // Convert utf-8 bytes to a string.
            // string s_unicode2 = Encoding.UTF8.GetString(utf8Bytes);

            // convert utf-8 to null seperated utf-8 terminiated by 0x04
            for (int i = 0; i < utfWrite.Length; i++)
            {
                uCode[2 * i] = utfWrite[i];
            }
            uCode[2 * utfWrite.Length] = 0x04;

            Socket soc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAdd = IPAddress.Parse(@"127.0.0.1");
            IPEndPoint remoteEP = new IPEndPoint(ipAdd, 8111);
            soc.Connect(remoteEP);
            // Byte[] buffer = new Byte[500];  A buffer that could be used during a read
            
            //soc.Send(uCode);
            soc.Send(uCode);    // send the read or write string.  If read, receive the reply.
            // int x = soc.Receive(buffer);
            // Byte[] output = new Byte[x / 2 + 1];
            // for (int i = 0; i < x; i+=2)
            // {
            //    output[i / 2] = buffer[i];
            // }
            // string s_out = Encoding.UTF8.GetString(output);
             Console.WriteLine(pileName);
            */
            /*
            QueryMonitoredTags
            MonitorTags [<dpGroup><dpTag><dptype>]
            */

        }

        private static void TagInfo_UpdateValueEvent(ITagInfo e)
        {
            Console.WriteLine(e.AsDouble);
            e.Dispose();
            DatapoolSvr.Dispose();
        }

    }
}
