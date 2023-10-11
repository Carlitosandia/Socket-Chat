using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.IO;
using ServerHiloFtp;
using System.Threading;

namespace Servidor
{
    public class Servidor
    {
        //Se declaran variables para inicializar server
        private static int PORT = 800;
        private IPHostEntry host;
        private String ip;
        private static List<Socket> coleccionSockets = new List<Socket>(); // Listado donde tenemos todos los clientes que se iran conectando
        private Socket incializaServer() //Metodo para crear el socket
        {
            try
            {
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                return socket; // Retorna el socket creado
            }
            catch (IOException ioe)
            {
                Console.WriteLine("Ocurrió una excepción de E/S (I/O): " + ioe.Message); //Mensaje de error
            }
            return null;
        }
        public String obtenerIP()
        {
            try
            {
                String localIP = ""; 
                host = Dns.GetHostEntry(Dns.GetHostName()); // Se consigue el host
                foreach (IPAddress ip in host.AddressList) // Recorremos las ip
                {
                    if (ip.AddressFamily.ToString() == "InterNetwork")
                    {
                        localIP = ip.ToString(); 
                        return localIP; // Se obtiene la ip y se regresa 
                    }
                }
            }
            catch (IOException ioe)
            {
                Console.WriteLine(ioe);
            }
            return null;
        }
        public Servidor() 
        {
            Socket welcomeSocket = incializaServer(); // Se crea un socket llamado welcomeSocket y este sera igual al meotdo inicializar
            ip = obtenerIP(); //Obtenemos la ip
            Console.WriteLine(ip); // Se escribe la ip en la consola
            IPEndPoint connect = new IPEndPoint(IPAddress.Parse(ip), PORT); // se crea instancia con la ip y puerto para estables una conexion de red 
            welcomeSocket.Bind(connect); //Se asocia el socket con la ip 
            welcomeSocket.Listen(1900); // Se establecen el numero de conexiones permitidas
            Console.WriteLine("Servidor iniciado en el puerto " + PORT); // Se escribe en la terminal
            if (welcomeSocket != null)
            {
                while (true) // Ciclo infinito 
                {
                    try
                    {
                        Socket socket = welcomeSocket.Accept(); // Nuevo socket donde esperar nuevas conexiones
                        Console.WriteLine("Conexion iniciada"); // Si hay conexion se escribe esto
                        coleccionSockets.Add(socket); // Se guarda el socket dentro de esta lista
                        HiloFtp hiloFtp = new HiloFtp(socket,connect,coleccionSockets,ip); // Se instancia la clase y se le pasan los valores requeridos
                        Thread thread = new Thread(new ThreadStart(hiloFtp.Run)); // Se empiza a correr en un hilo
                        thread.Start();// lo que viene siendo el metodo Run de hilo ftp
                    }
                    catch (IOException ioe)
                    {
                        Console.WriteLine("Ocurrió una excepción de E/S (I/O): " + ioe.Message);
                    }
                }
            }
        }

        public static void Main() //Se ejecuta el programa y empieza creando una nueva instancia del servidor
        {
            new Servidor();
        }
    }
}
