using System;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace TcpToHttp;

internal class Program
{
    static void Main(string[] args)
    {
        int port = 8080;    
        bool run = true;

        using (TcpListener tcpListener = new TcpListener(System.Net.IPAddress.Loopback, port))
        { 
            tcpListener.Start();

            byte[] buffer = new byte[256];
            StringBuilder data = new StringBuilder();

            Console.WriteLine($"Listening on {tcpListener.LocalEndpoint}...");

            while (run)
            { 
                Console.WriteLine("Waiting for a connection...");

                using (TcpClient client = tcpListener.AcceptTcpClient())
                {
                    Console.WriteLine($"Client connected: {client?.Client?.RemoteEndPoint?.AddressFamily ?? AddressFamily.Unknown}");

                    data.Clear();
                    NetworkStream stream = client?.GetStream() ?? throw new InvalidOperationException("TcpListener.AcceptTcpClient returned null TcpClient.");

                    int i;

                    while ((i = stream.Read(buffer,0,buffer.Length)) != 0)
                    {

                        string chunk = Encoding.ASCII.GetString(buffer, 0, i);
                        //Translate the bytes to ASCII string.
                        data.Append(chunk);
                        Console.WriteLine($"Received: {data}");

                        if (chunk.Contains("\r\n\r\n"))
                        {

                            string response =  "HTTP/1.1 200 OK\r\n" +
                                            "Content-Length: 13\r\n" +
                                            "Content-Type: text/plain\r\n" +
                                            "\r\n" +
                                            "Hello, World!";

                            byte[] responseBytes = Encoding.ASCII.GetBytes(response);
                            stream.Write(responseBytes, 0, responseBytes.Length);
                            stream.Flush();
                            break;
                        }
                    }
                }

            }
        }
    }
}