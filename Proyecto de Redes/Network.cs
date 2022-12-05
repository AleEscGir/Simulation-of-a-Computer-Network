using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Proyecto_de_Redes
{
    public class Network
    {
        #region Atributos y Constructor
        private int time { get; set; }
        //Representa el tiempo global de ejecución
        private int next_time { get; set; }
        //Representa el próximo momento en el que se actualizarán todos los dispositivos
        private int signal_time { get; set; }
        //Representa el intervalo de tiempo entre una actualización y otra
        private string error_detection { get; set; }
        //Representa el método utilizado para detectar errores
        private List<Device> devices { get; set; }
        //Representa la lista de dispositivos activos
        private List<string[]> commands { get; set; }
        //Representa los comandos del txt que aún no han podido ser ejecutados
        public Network(int signal_time, string error_detection)
        {
            this.time = 0;
            this.signal_time = signal_time;
            this.error_detection = error_detection;
            this.next_time = 0;
            this.devices = new List<Device>();
            this.commands = new List<string[]>();
        }

        #endregion

        //Este método inicializa el funcionamiento de la red
        public void Inicialize_Network(string directory)
        {//Recibe el directorio donde se encuentra el txt con los comandos

            //Mediante esta clase (tomada de System.IO), podemos leer lo que está escrito en un documento legible
            StreamReader charger = new StreamReader(directory);

            string temp = charger.ReadLine(); //Leemos lo primero que hay en el txt

            //Comenzaremos a iterar mientras podamos leer del txt información y algún dispositivo esté trabajando
            while ((temp != null && temp != "") || this.Already_Info() == true)
            {
                this.time = this.next_time; //Actualizamos el tiempo actual al próximo intervalo
                this.next_time = this.time + signal_time; //Actualizamos al intervalo

                this.Actualization(); //Mandamos a actualizar el tiempo de todos los dispositivos
                this.Execute_Possible_Commands(); //Ejecutamos los comandos que sean posibles

                string[] actual_order = new string[0]; //Declaramos un string que tendrá la siguiente instrucción 
                if (temp != null) //En caso de que la instrucción no sea nula
                    actual_order = temp.Split(' '); //Separamos lo que leímos del txt y lo guardamos

                //Mientras podamos leer instrucciones del txt, y el tiempo de esta no sea superior al próximo intervalo
                while ((temp != null && temp != "") && int.Parse(actual_order[0]) < this.next_time)
                {
                    this.time = int.Parse(actual_order[0]); //Actualizamos el tiempo actual
                    this.Execute_Order(actual_order); //Mandamos a ejecutar dicha orden
                    temp = charger.ReadLine(); //Leemos la próxima instrucción del txt y la guardamos
                    if (temp != null) //En caso de que la instrucción no sea nula
                        actual_order = temp.Split(' '); //Separamos nuevamente lo que leímos del txt
                }

                this.Already_Receiving_Devices(); //Hacemos que todos los dispositivos actualicen su estado
            }
        }


        //Este método se encarga de ejecutar un comando
        public void Execute_Order(string[] order)
        {   //Recibe un array con la orden, que varía según el tipo

            //<time> create hub <name> <number-of-ports>
            //<time> create pc <name>
            //<time> create switch <name> <number-of-ports>
            if (order[1] == "create") //En caso de que la orden sea de crear un dispositivo
            {
                //En el caso de la PC, se inserta de primera en la lista, en el caso del resto
                //de dispositivos, se insertan al final, esto se hace para que a la hora de actualizar
                //haya una mejor organización, siendo los primeros en enviar información las PC

                if (order[2] == "hub") //Si es un HUB
                    this.devices.Add(new HUB(order[3], int.Parse(order[4]))); //Añadimos un HUB con su respectivo nombre
                                                                              //y cantidad de dispositivos

                if (order[2] == "host") //Si es una PC
                    this.devices.Insert(0, new PC(order[3],                      //Añadimos una PC con sus parámetros
                                       this.error_detection, this.signal_time)); 


                if (order[2] == "switch") //Si es un switch
                    this.devices.Add(new Switch(order[3], int.Parse(order[4]))); //Añadimos un Switch con su respectivo nombre
                                                                                 //y cantidad de dispositivos

                if (order[2] == "router") //Si es un router
                    this.devices.Add(new Router(order[3], int.Parse(order[4]),
                                                        this.error_detection)); //Añadimos un Router con su respectivo nombre
                                                                                 //y cantidad de dispositivos
            }



            //<time> mac <host-name>[:<interface>] <mac-address>
            if (order[1] == "mac") //En caso de que la orden sea de agregar una mac
            {
                string[] port = order[2].Split(':'); //Dividimos para saber a qué interface se refiere

                int pos = this.Search(port[0]); //Hallamos el dispositivo al que se le va a agregar

                string new_mac = Transformation_Codes.Hexadecimal_to_Binary(order[3]);

                if (this.devices[pos] is PC) //Si el dispositivo es un pc, solo tiene una interfaz,
                {                            //por lo que no hay que revisar nada más
                    ((PC)this.devices[pos]).Actualizate_Mac(new_mac); //Le actualizamos la mac al dispositivo
                }

                if (this.devices[pos] is Router && port.Length == 2) //Si el dispositivo es un router,
                                                                     //al tener varias interfaces
                {                                                    //debemos avisarle a cuál será
                    //Le actualizamos la mac a la interfaz del dispositivo
                    ((Router)this.devices[pos]).Actualizate_Mac(new_mac, int.Parse(port[1]) - 1); 
                }

            }


            //<time> ip <host-name>[:<interface>] <ip-address> <mask>
            if (order[1] == "ip")
            {
                string[] port = order[2].Split(':'); //Dividimos para saber a qué interface se refiere

                int pos = this.Search(port[0]); //Hallamos el dispositivo al que se le va a agregar

                string new_ip = Transformation_Codes.Ip_to_Binary(order[3]);
                string new_mask = Transformation_Codes.Ip_to_Binary(order[4]);

                if (this.devices[pos] is PC) //Si el dispositivo es una pc, solo tiene una interfaz,
                {                            //por lo que no hay que revisar nada más
                    ((PC)this.devices[pos]).Actualizate_IP_Mask(new_ip, new_mask); //Le actualizamos la mac al dispositivo
                }

                if (this.devices[pos] is Router && port.Length == 2) //Si el dispositivo es una router,
                                                                     //tiene varias interfaces,
                {                                                    //por lo que no hay que revisar a cuál se refiere
                    //Le actualizamos el ip al dispositivo
                    ((Router)this.devices[pos]).Actualizate_IP_Mask(new_ip, new_mask, int.Parse(port[1]) - 1); 
                }
            }


            //<time> connect <port1> <port2>
            if (order[1] == "connect") //Si la orden es de conectar dos dispositivos
            {
                //Nuevamente cortamos la orden para saber el nombre de los dispositivos y el puerto correspondiente
                string[] device_1 = order[2].Split('_');
                string[] device_2 = order[3].Split('_');

                //Buscamos ambos dispositivos en la lista
                int pos_1 = this.Search(device_1[0]);
                int pos_2 = this.Search(device_2[0]);

                //Los conectamos
                this.devices[pos_1].Connect(this.devices[pos_2], int.Parse(device_1[1]) - 1);
                this.devices[pos_2].Connect(this.devices[pos_1], int.Parse(device_2[1]) - 1);
                //Note que se debe corregir el puerto puesto que las listas comienzan en 0, no en 1
            }


            //<time> disconnect <port>
            if (order[1] == "disconnect") //Si la orden es de desconectar
            {
                string[] device_1 = order[2].Split('_'); //Recortamos la orden nuevamente
                int pos_1 = this.Search(device_1[0]); //Hallamos el primer dispositivo en la lista

                //Hallamos el dispositivo 2 buscando el que se encuentra en el puerto del 1
                string device_2 = this.devices[pos_1].Device_Search(int.Parse(device_1[1]));

                if (device_2 != null) //Si existía un dispositivo en ese puerto
                {
                    this.devices[pos_1].Disconnect(int.Parse(device_1[1])); //Desconectamos el dispositivo 1

                    int pos_2 = this.Search(device_2); //Buscamos la posición del dispositivo 2 

                    int pos = this.devices[pos_2].Device_Search(device_1[0]); //Buscamos el puerto correspondiente
                                                                              //del dispositivo 2

                    this.devices[pos_2].Disconnect(pos); //Desconectamos el dispositivo 2
                }
            }


            //<time> route reset <router-name>
            //<time> route add <name> <destination> <mask> <gateway> <interface>
            //<time> route delete <name> <destination> <mask> <gateway> <interface>
            if (order[1] == "route") //Si la orden es referida a una ruta
            {

                string destination = "0"; //Designamos los posibles campos a enviar
                string mask = "0";
                string gateway = "0";
                int interface_port = 0;

                int pos = this.Search(order[3]); //Buscamos el dispositivo en la lista

                if (order[2] == "reset") //En caso de que se desee reiniciar la tabla de rutas
                    this.devices[pos].Restart();

                if(order.Length == 8) //En caso de que tenga length 8, es un add o un delete
                {
                    destination = Transformation_Codes.Ip_to_Binary(order[4]);
                    mask = Transformation_Codes.Ip_to_Binary(order[5]);
                    gateway = Transformation_Codes.Ip_to_Binary(order[6]);
                    interface_port = int.Parse(order[7]);
                }


                if (order[2] == "add" && this.devices[pos] is Router) //En caso de que se desee añadir una ruta nueva a un router
                    ((Router)this.devices[pos]).Add_Route(destination, mask, gateway, interface_port - 1);

                if (order[2] == "add" && this.devices[pos] is PC) //En caso de que se desee añadir una ruta nueva a una pc
                    ((PC)this.devices[pos]).Add_Route(destination, mask, gateway, interface_port - 1);


                if (order[2] == "delete") //En caso de que se desee eliminar una ruta a un router
                    ((PC)this.devices[pos]).Delete_Route(destination, mask, gateway, interface_port - 1);

                if (order[2] == "delete") //En caso de que se desee eliminar una ruta a una pc
                    ((PC)this.devices[pos]).Delete_Route(destination, mask, gateway, interface_port - 1);

            }


            string protocol = "00000000"; //Designamos el protocolo
                                          //que usarán los send

            bool procesed = false; //Con este bool, cada orden de envío de datos
                                   //sabe si es la primera en recibir la información
                                   //en caso de que sea falso, y en caso de que sea
                                   //verdadero quiere decir que una orden superior ya
                                   //procesó la información

            //<time> ping <host-name> <ip-address>
            if (order[1] == "ping")
            {
                protocol = "00000001";
                order = new string[] { order[0], "send_packet",
                                       order[2], order[3], "00001000"};
                procesed = true;
            }


            //<time> send_packet <host-name> <ip-address> <data>
            if (order[1] == "send_packet")
            {
                string time = order[0]; //Primero guardamos el tiempo
                string host_name = order[2]; //Guardamos el nombre del host emisor
                string ip = Transformation_Codes.Ip_to_Binary(order[3]); //Guardamos el ip del host receptor en binario

                string data = "";

                if (procesed) //En caso ya los datos hallan sido procesados
                    data = order[4];   //la data se encuentra en binario
                else                   //En otro caso, hay que transformarlos
                    data = Transformation_Codes.Hexadecimal_to_Binary(order[4]);

                int pos = this.Search(host_name); //Buscamos el host emisor en la lista

                string ip_2 = ((PC)this.devices[pos]).ip; //Buscamos el ip del host emisor

                string ttl = "00000000"; //Designamos el ttl
                

                //Calculamos el payload_size, que es un byte que representa la cantidad de bytes de data
                string payload_size = Transformation_Codes.Add_Zero(
                                      Transformation_Codes.Decimal_to_Binary(data.Length / 8), 8);
                                                                             //Dividimos entre 8
                                                                             //para convertir de byte a bit

                //Transformamos la orden en un send_frame, que utiliza como data el paquete anterior,
                //el campo <mac-destino> queda vacío puesto que todavía no sabemos la mac del host receptor
                order = new string[] { time, "send_frame", host_name, "0000000000000000",
                                       ip + ip_2 + ttl + protocol + payload_size + data};

                procesed = true;
            }

            //<time> send_frame <host> <mac-destino> <data>
            if (order[1] == "send_frame") //En caso de que el comando sea Send_Frame,
            {                            //correspondiente a la capa de enlace

                int pos = this.Search(order[2]); //Buscamos ese dispositivo en la lista

                //Primero separamos el mac del receptor y el remitente y los transformamos a binario
                //Debemos comparar si fue procesada ya la información

                string receptor = "";

                if (procesed)
                    receptor = order[3];
                else
                    receptor = Transformation_Codes.Hexadecimal_to_Binary(order[3]);
                
                
                
                
                string sender = ((PC)devices[pos]).mac;

                //Luego separamos los datos a entregar (revisamos si se encuentran en binario primero)

                string data = "";

                if (procesed) //En caso de que estén en binario los utilizamos
                    data = order[4];
                else //En caso de que no lo estén, los transformamos
                    data = Transformation_Codes.Hexadecimal_to_Binary(order[4]);
                //Nota: Esta comprobación es innecesaria si recibimos un send_packet, solo se hace
                //por temas de extensibilidad


                //Luego vemos el tamaño de los datos, el cual se divide entre 8 para transformar de bit a bytes
                string data_length = Transformation_Codes.Decimal_to_Binary(data.Length / 8);

                string verificator_data_length = "00000000"; //Por ahora no será añadido el método de cifrado,
                                                             //así que este campo continuará en 0.

                //Agregamos los 0s para completar cada información
                receptor = Transformation_Codes.Add_Zero(receptor, 16);
                sender = Transformation_Codes.Add_Zero(sender, 16);
                data_length = Transformation_Codes.Add_Zero(data_length, 8);

                //Transformamos la orden en un send
                order = new string[] {order[0], "send", order[2],
                                      receptor + sender + data_length
                                      + verificator_data_length + data};
                
                procesed = true;
            }

            //<time> send <host-name> <data>
            if (order[1] == "send") //Si la orden es enviar
            {
                int pos = this.Search(order[2]); //Buscamos ese dispositivo en la lista

                if (((PC)this.devices[pos]).Can_Send() == false) //Si no puede enviar información
                {
                    this.commands.Add(order); //Ponemos la orden en espera
                }
                else //En otro caso
                {
                    this.devices[pos].Actualization_Info(order[3], this.time); //Lo mandamos a enviar la orden
                }
            }
        }


        //Este método muestra si alguno de los dispositivos de la red aún está trabajando
        public bool Already_Info()
        {
            for (int i = 0; i < this.devices.Count(); i++) //Iteramos por todos los dispositivos
            {
                if (this.devices[i].already_sending || this.devices[i].already_receiving) //Si al menos uno está enviando o recibiendo información
                    return true;                     //avisamos de que todavía la red está trabajando
            }

            return false; //En otro caso avisamos de que la red no está trabajando
        }


        //Este método, dado el nombre de un dispositivo, busca su posición en la lista
        public int Search(string name)
        { //Recibe le nombre del dispositivo
            for (int i = 0; i < this.devices.Count(); i++) //Iteramos por la lista de dispositivos
            {
                if (this.devices[i].name == name) //Si encontramos el dispositivos, devolvemos ssu poisición
                    return i;
            }

            return -1; //Si no lo encuentra devolvemos -1
        }


        //Este método actualiza todos los dispositivos actuales
        public void Actualization()
        {
            for (int i = 0; i < this.devices.Count(); i++) //Iteramos por todos los dispositivos
            {
                //Actualizamos us información, si esta da false, es que es imposible que trabaje, por lo que
                //rescatamos la orden para ejecutarla más tarde
                if (this.devices[i].Actualization_Info("", this.time) == false)
                {
                    string data = this.devices[i].Restart(); //Reiniciamos el dispositivo, devolviéndonos así
                                                                //la información que estaba enviando
                    string[] temp = { "0", "send", this.devices[i].name, data }; //Reestructuramos la orden
                    this.commands.Add(temp); //la añadimos a la lista de órdenes sin ejecutar
                }
            }
        }


        //Este método hace a todos los dispositivos escribir su historial
        public void Writer(string directory)
        { //Recibe un directorio en el que se guardarán todos los txt
            for (int i = 0; i < this.devices.Count(); i++) //Se itera por todos los dispositivos
            {//Se les manda escribir en txt cada uno según su nombre
                this.devices[i].Show_Historial(directory + this.devices[i].name + ".txt");
            }
        }


        //Este método ejecuta de forma al azar los comandos que sean posible de entre los que no han sido ejecutados
        public void Execute_Possible_Commands()
        {
            List<string[]> temp = this.commands; //Guardamos en un temporal la lista de comandos

            this.commands = new List<string[]>(); //Reiniciamos la lista de comandos

            int cont = temp.Count(); //Guardamos un contador para ir seleccionando los elementos al azar

            for (int i = 0; i < temp.Count(); i++) //Iteramos por todos los comandos posibles
            {
                int random = new Random().Next(0, cont); //Seleccionamos un número al azar
                cont--; //Disminuímos el contador
                this.Execute_Order(temp[random]); //Ejecutamos esa orden al azar
                temp.RemoveAt(random); //La removemos de la lista

                //Nótese que, si un comando no se puede ejecutar, será colocado nuevamente en this.commands
                //por el método 
            }
        }


        //Este método hace que los dispositivos actualizen su estado con respecto a si reciben información
        public void Already_Receiving_Devices()
        {
            for (int i = 0; i < this.devices.Count(); i++) //Vamos por todos los dispositivos
            {
                this.devices[i].Receiving_Verification(); //Los actualizamos
            }
        }
    }
}
