using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace ServerHiloFtp
{

    public class HiloFtp : IRunnable{

        //Declaracion de variables que se requieren
        private NetworkStream networkStream;
        private BinaryReader netIn;
        private BinaryWriter netOut;
        private Socket socket;  
        private List<Socket> sockets;
        private String relativePath;
        private IPEndPoint conexion;
        private String ip;
        private String message;
        private String usuario;
        private static Dictionary<String, Socket> infoSocket = new Dictionary<String, Socket>();
        private static List<String> usuarios = new List<String>();
        public void leerStringdatos()
        {
            try {
                byte[] data = new byte[1024]; // Crea un arreglo de bytes
                int bytesRead = socket.Receive(data); // Crea una variable int para guardar los datos que recibe el socket en el arreglo
                message = Encoding.UTF8.GetString(data, 0, bytesRead); // Se guarda el mensaje en una variable mensaje, el mensaje se pasara a UTF8
                Console.WriteLine(message); //Se escribe el mensaje en la consola
                if (message.StartsWith("put:"))
                {
                    // Este mensaje indica una transferencia de archivo
                    string[] parts = message.Split(':');
                    if (parts.Length >= 3)
                    {
                        string fileName = parts[1];
                        int fileSize = int.Parse(parts[2]);

                        // Aquí puedes llamar a un método para recibir y reenviar el archivo
                        reenviarArchivo(fileName, fileSize);
                    }
                }

            }
            catch(Exception e)
            {   
                Console.WriteLine("Error de lectura: " + e.Message);
            }
        }
        public void reenviarArchivo(String fileName, int fileSize)
        {
            try
            {
                byte[] buffer = new byte[1024];
                int bytesRead = 0;
                String res = "getFile^";
                while (fileSize > 0)
                {
                    int chunkSize = Math.Min(fileSize, buffer.Length);
                    bytesRead = socket.Receive(buffer, chunkSize, SocketFlags.None);
                    if (bytesRead <= 0)
                    {
                        break;
                    }

                    // Reenviar los datos al otro cliente

                }
            }
            catch (IOException ioe) {
                Console.WriteLine("Error al recibir y reenviar el archivo"+ fileName + ":" + ioe.Message);
            }
        }
        public void mandarMensaje(String message) // Manda mensaje a todos los sockets
        {
            foreach(Socket socket in sockets) { //Recorre los sockets que hay en la lista de sockets
                byte[] buffer = Encoding.UTF8.GetBytes(message); //El mensaje lo convirte a bytes para guardarlo en un arreglo
                socket.Send(buffer);// Manda el arreglo de bytes del mensaje a traves del socket
                
            }
        }
        public void mandarDm(String usuario, String message)
        {
            if (infoSocket.ContainsKey(usuario))
            {
                Socket socketDestino = infoSocket[usuario];
                // Convierte el mensaje a bytes y envíalo al socket destino
                byte[] mensajeBytes = Encoding.UTF8.GetBytes(message);
                socketDestino.Send(mensajeBytes);
            }
            else
            {
                // El nombre de usuario no existe en el diccionario, puedes manejar esto de acuerdo a tus necesidades
                Console.WriteLine("El usuario " + usuario + " no está conectado.");
            }
        }
        public void incializaFlujos()
        {
            networkStream = new NetworkStream(socket); // conexión entre el NetworkStream y el socket, o sea que cualquier operación de lectura o escritura realizada en el NetworkStream se reflejará en el socket
            netIn = new BinaryReader(networkStream);
            netOut = new BinaryWriter(networkStream);
        }
        public HiloFtp(Socket socket, IPEndPoint conexion, List<Socket> sockets, String ip) // Se pasan los valores que requiere la clase para ejecutarse
        {
            this.socket = socket;
            this.conexion = conexion;
            this.sockets = sockets;
            this.ip = ip;
        }

        public void Run() // Se ejecuta el metodo Run 
        {
            try
            {
                incializaFlujos(); // Se inicial el flujo
            }catch(IOException ioe)
            {
                Console.WriteLine("Ocurrió una excepción de E/S (I/O): " + ioe.Message);
            }
            while (true) // Ciclo infinito 
            {
                try
                {
                    leerStringdatos(); // Metodo para leer los datos que ingresan 

                    String[] tokens = message.Split('^'); // Se divide el string en tokens separado por ^
                    String token = tokens[0]; // Se guarda el comando en una variable token el cual sera igual al primer token del mensaje
                    if (token.StartsWith("m"))
                    {
                        String nombreIP = tokens[1];
                        int index = nombreIP.IndexOf('@');
                        String nombre = nombreIP.Substring(0, index);
                        String ipCliente = nombreIP.Substring(index + 1);
                        String mensaje = tokens[2];
                        String res = "m~" +nombre + ": " + mensaje + "\n\r";
                        mandarMensaje(res);
                    }
                    else
                    {
                        if (token.StartsWith("j")) //Si el token empieza en j 
                        {
                            String nombreIP = tokens[1]; // Se guardara el usuario y su ip en una variable nombreIP, ejemplo: usuario@127.0.0.1
                            int index = nombreIP.IndexOf('@'); //Se le asgina un index el cual sera el @
                            String nombre = nombreIP.Substring(0, index); // Guarda el nombre en la posicion 0 del index
                            String ipCliente = nombreIP.Substring(index + 1); // Guarda la ip en la posicion 1 del index
                            infoSocket.Add(nombre,socket);//Asigna el usuario con el socket para guardarlo en un dictionary(HashMap)
                            usuarios.Add(nombre);// agrega al usuario a una lista 
                            String res = "l^";//Comienza la respuesta del servidor
                            foreach (String usuario in usuarios) { //Recorre la lista de usuarios
                                Console.WriteLine(usuario);
                                res += usuario + "^";//Agrega los usuarios que se han guardado
                            }
                            mandarMensaje(res); // Manda un mensaje al cliente con el comando y los usuarios que hay guardados
                        }
                        else
                        {
                            if (token.StartsWith("sDm"))
                            {
                                String usuario = tokens[1];
                                String usuarioSelected = tokens[2];
                                String message = "sDm^" + usuario + "^" + usuarioSelected;
                                mandarDm(usuario, message);
                                mandarDm(usuarioSelected, message);
                            }
                            else {
                                if (token.StartsWith("MsgDm")) {
                                    Console.WriteLine("Llego msgDM");
                                    String usuario = tokens[1];
                                    String usuarioSelected = tokens[2];
                                    String message = tokens[3];
                                    String res = "MsgDm^" + usuario + "^" + usuarioSelected + "^" + message;
                                    Console.WriteLine(res);
                                    mandarDm(usuario, res);
                                    mandarDm(usuarioSelected, res);
                                }
                            }
                        }
                    }
                }
                catch (IOException e)
                {
                    Console.WriteLine(e.StackTrace);
                }
            }

        }
    }
}
