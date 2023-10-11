/*
 * Click nbfs://nbhost/SystemFileSystem/Templates/Licenses/license-default.txt to change this license
 * Click nbfs://nbhost/SystemFileSystem/Templates/Classes/Class.java to edit this template
 */
package realchat;

/**
 *
 * @author Carlo
 */
import java.net.InetAddress;
import java.net.Socket; //clases necesarias para trabajar con sockets y entrada/salida de datos. 
import java.nio.charset.StandardCharsets;
import java.io.*;
import java.util.Arrays;
import java.util.Base64;
import java.util.HashMap;
import java.util.HashSet;
import java.util.Set;
import java.util.StringTokenizer;
import javax.swing.JOptionPane;

public class Cliente {

    private Socket socket; //Objeto para establecer la conexión
    private String server; //cadena que almacenará la dirección der servidor que se ingrese por el usuario 
    private int puerto; //un entero que almacenará el numero de puerto del servidor que se ingrese por el usuario
    private DataInputStream netIn; //Objeto para recibir datos desde el server 
    private DataOutputStream netOut; //Objeto que se utilizará para enviar datos al servidor
    private String ruta;
    public String usuario;
    HashMap<String, Socket> InfoSocket = new HashMap();

    public Cliente(String server, String puerto, String usuario) throws Exception { //Constructor de la clase Cliente
        this.server = server; //Recibe server y puerto como cadenas
        this.puerto = Integer.parseInt(puerto); //Convierte puerto a un int y los almacena en variables de instancia 
        this.usuario = usuario;
        ruta = "";
    }// El constructor puede llmar una excepción si el "puerto" no es un número válido

    public void recibirArchivo(File file, String usuario, String usuarioSelected) {
        try {
            enviaComando("sDmFile^" + usuario + "^" + usuarioSelected);
            int totalLen = (int) file.length();
            BufferedInputStream bis = new BufferedInputStream(new FileInputStream(file));
            byte[] buffer = new byte[1024];
            int bytesRead = 0;
            int length = 1024;
            int rest = 0;
            while (true) {
                rest = totalLen - bytesRead;
                if (rest > length) {
                    int read = bis.read(buffer, 0, length);
                    byte[] dataFragment = Arrays.copyOf(buffer, read);
                    String encodedFragment = Base64.getEncoder().encodeToString(dataFragment);
                    // Envía el fragmento de datos al servidor utilizando enviaComando
                    enviaComando("receiveFragment:" + encodedFragment);
                } else {
                    int read = bis.read(buffer, 0, rest);
                    byte[] dataFragment = Arrays.copyOf(buffer, read);
                    String encodedFragment = Base64.getEncoder().encodeToString(dataFragment);
                    // Envía el último fragmento y marca la finalización de la transferencia
                    enviaComando("receiveCompleted:" + encodedFragment);
                    System.out.println("Se envió al servidor");
                    return;
                }
                bytesRead += 1024;
            }
        } catch (IOException ioe) {
            System.out.println(ioe.getMessage());
        }
    }

    private void enviaComando(String command) throws IOException {
        byte[] buffer = command.getBytes(StandardCharsets.UTF_8); // Crea un arreglo de bytes donde este sera igual a los bytes del mensaje los cuales son UTF8
        socket.getOutputStream().write(buffer); // Se mandan atraves de GetOutPutStream y se manda lo que hay en el arreglo.
    }

    public String recibirMensaje() throws IOException { // metodo para recibir el mensaje
        InputStream inputStream = socket.getInputStream(); // Recibe y guarda los datos del socket en un inputStream
        byte[] buffer = new byte[1024];// Crea un arreglo de bytes
        int bytes = inputStream.read(buffer); // crea una varible entero donde sera igual a lo que recibio el inputsream y los leera en el arreglode bytes
        String comando = new String(buffer, 0, bytes); // Creara un Strin con los bytes que tenemos en el buffer y en el entero
        System.out.println(comando);
        return comando;//Regresa el mensaje que se ha recibido
    }

    public void inicializaFlujo(Socket socket) throws IOException {//Este método toma un objeto "socket" como argumento y crea dos flujos de datos:
        netIn = new DataInputStream(socket.getInputStream()); //Recibe datos desde el servidor
        netOut = new DataOutputStream(socket.getOutputStream());//Envia datos desde el servidor 
    }//El método puede lanzar una excepción de E/S (IOException) si ocurre algun error durante la inicialización de los flujos 

    public void startDm(String usuarioSelected) { // Empieza la conexion con privada
        try {
            String msg = "sDm^" + usuario + "^" + usuarioSelected; // Crea el mensaje con el comando que recibira el servidor para empezar la conexion privada
            enviaComando(msg); // Envia el mensaje
        } catch (IOException ioe) {
            System.out.println(ioe); // Mensaje de error
        }
    }

    public void recibirDm(String mensaje, String usuario, String usuarioSelected)throws IOException { // Recibe la peticion de un mensaje 
        String res = "MsgDm^";  // Se crea el comando con el que se hara la peticion al servidor con el mensaje del cliente, su emisor y su destinatario
        res += usuario;
        res += "^";
        res += usuarioSelected;
        res += "^";
        res += mensaje;
        enviaComando(res); // Envia el mensaje
    }

    public void recibirComando(String mensaje) throws IOException { // Recibe el comando para mandar mensajes generales
        InetAddress ip = InetAddress.getLocalHost(); // Obtiene la direccion local
        String res = "m^";
        res += usuario;
        res += "@";
        res += ip.getHostAddress(); 
        res += "^";
        res += mensaje;
        enviaComando(res); // Envia el comando con el mensaje
    }

    public Socket conectar() { //Este método se encarga de la lógica principal del cliente
        try {
            socket = new Socket(server, puerto);//Crea un objeto "socket" que intenta conectarse al servidor especificado por "server" y "puerto"
            inicializaFlujo(socket);//Despues se llama al método "inicializaFlujo" para configurar los flujos de entrada y salida de datos. 
            InetAddress ip = InetAddress.getLocalHost(); // Se crea una variable para guardar la ip
            String res = "j^"; // Comando que se manda
            res += usuario; // Usuario del cliente
            res += "@";
            res += ip.getHostAddress();//La ip del cliente
            enviaComando(res); //Se envia el mensaje mediante el metodo enviaComando
            return socket; // Se regresa el socket con el que se mandaron los datos 
        } catch (Exception ex) { //Si se produce una excepción durante la conexión
            ex.printStackTrace(); // Se captura y se imprime el error
            return null;
        }
    }
}
