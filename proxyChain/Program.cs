using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace proxyChain
{
    public class IP
    {
        public static IP Parse(string ip)
        {
            var result = ip.Split(new[] {":"}, StringSplitOptions.None);
            return new IP(result[0], int.Parse(result[1]));
        }
        public IP(){}
        public IP(string host, int port)
        {
            Host = host;
            Port = port;
        }
        public string Host { get; set; }
        public int Port { get; set; }
    }

    class Program
    {
        private const string TunnelReuest = "CONNECT {0}:{1}  HTTP/1.1\r\nHost:{0}\r\n\r\n";
        private static readonly IList<IP> ProxiesChain = new List<IP>() { IP.Parse("61.91.89.28:80"), IP.Parse("223.25.195.68:8080"), IP.Parse("219.159.105.180:8080") };

        static void Main(string[] args)
        {
            string host = "ya.ru";
            byte[] buffer = new byte[2048];
            int bytes;
            var client = new TcpClient(ProxiesChain[0].Host, ProxiesChain[0].Port);
            NetworkStream stream = client.GetStream();
            if (ProxiesChain.Count > 1)
            {
                for (int i = 1; i < ProxiesChain.Count; i++)
                {
                    byte[] tunnelRequest = Encoding.UTF8.GetBytes(String.Format(TunnelReuest, ProxiesChain[i].Host, ProxiesChain[i].Port));
                    stream.Write(tunnelRequest, 0, tunnelRequest.Length);
                    stream.Flush();
                    bytes = stream.Read(buffer, 0, buffer.Length);
                    Console.Write(Encoding.UTF8.GetString(buffer, 0, bytes));
                }
            }
            byte[] request = Encoding.UTF8.GetBytes(String.Format("GET https://{0}/  HTTP/1.1\r\nHost: {0}\r\n\r\n", host));
            stream.Write(request, 0, request.Length);
            stream.Flush();
            do
            {
                bytes = stream.Read(buffer, 0, buffer.Length);
                Console.Write(Encoding.UTF8.GetString(buffer, 0, bytes));
            } while (bytes != 0);
            client.Close();
            Console.ReadKey();
        }
    }
}
