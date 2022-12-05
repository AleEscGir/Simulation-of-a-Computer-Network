using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Proyecto_de_Redes
{
    public abstract class Device //Clase que representa todos los Dispositivos de la Red
    {
        #region Atributos y Métodos de la Clase Abstracta
        public int last_actualization { get; protected set; }
        //Muestra el tiempo en el que fue realizada la última actualización
        public string name { get; protected set; }
        //Nombre del dispositivo
        protected List<string> historial { get; set; }
        //Muestra el historial de bits enviados o recibidos, así como conexiones inválidas realizadas
        protected List<Device> devices { get; set; }
        //Muestra los dispositivos a los que se conecta mediante un cable
        protected string info { get; set; }
        //Muestra la información guardada actualmente en el dispositivo
        public bool already_sending { get; protected set; }
        //Muestra si el dispositivo está realizando la acción de enviar información
        public bool already_receiving { get; protected set; }
        //Muestra si el dispositivo está realizando la acción de recibir información

        public abstract void Receive_Info(string bit, int time, string sender);
        //Define el cómo recibirá información el Dispositivo
        public abstract bool Actualization_Info(string information, int time);
        //Define cómo actualizará la información el dispositivo
        public abstract bool Show_Info(string name);
        //Muestra si el dispositivo le ha enviado alguna información al dispositivo remitente
        public abstract int Connect(Device device, int port);
        //Conecta uno de los puertos del dispositivo a otro dispositivo
        public abstract int Disconnect(int port);
        //Desconecta uno de los puertos del dispositivo
        public abstract int Device_Search(string name);
        //Permite buscar un dispositivo en los puertos a partir de su nombre
        public abstract string Device_Search(int port);
        //Permite buscar el nombre de un dispositivo a partir del puerto en el que se encuentra
        public abstract void Show_Historial(string directory);
        //Muestra el historial de un dispositivo
        public abstract string Restart();
        //Reinicia la información del dispositivo en caso de que ocurra un problema, y devuelve un array
        //con las informaciones pertinentes
        public abstract void Receiving_Verification();
        //Verifica si el dispositivo está recibiendo información por sus puertos
        #endregion
    }



    public class PC : Device
    {//Dispositivo con un solo puerto, capaz de enviar y recibir información

        #region Atributos y Constructor
        private int actual_info { get; set; }
        //Muestra el bit actual que se está enviando de una cadena completa de bits
        public string mac { get; private set; }
        //Muestra el mac de la PC, guardado en binario
        private List<string> link_layer_historial { get; set; }
        //Muestra el historial de datos de la capa de enlace que ha llegado al dispositivo
        private string data { get; set; }
        //Muestra la información actual de la capa de enlace
        private string error_detection { get; set; }
        //Muestra cuál es el método para detectar errores que será usado
        public string ip { get; private set; }
        //Muestra el ip de la PC, guardado como un número binario de 32 bits
        private string mask { get; set; }
        //Muestra la máscara de la PC, guardado como un número binario de 32 bits
        private Dictionary<string, string> ip_mac { get; set; }
        //Muestra todas las mac que han sido descubiertas mediante el ARP
        private Queue<string> arp_in { get; set; }
        //Guarda los ARP Response que están listos para ser enviados
        private int wait_time_arp { get; set; }
        //Muestra el tiempo de espera hasta notar que falló el protocolo ARP
        private string info_incomplete { get; set; }
        //Aquí se almacena la información que aún no puede enviarse
        //por falta de datos
        private List<string> network_layer_historial { get; set; }
        //Muestra el historial de datos de la capa de enlace que ha llegado al dispositivo
        private List<Route> routes { get; set; }
        //Aquí se guardarán todas las rutas que contiene el Router
        private int time_ping { get; set; }
        //Aquí se guarda el intervalo entre cada uno de los ping
        private int number_of_pins { get; set; }
        //Aquí se guarda la cantidad de ping seguidos que han sido enviados
        private Queue<string> wait_pong { get; set; }
        //Aquí se guardan todos los pong que están listos para ser enviados
        private int interval { get; set; }
        //Aquí se guarda el intervalo de tiempo entre cada actualización

        //Constructor de la clase
        public PC(string name, string error_detection, int interval) //Solo recibe el nombre de la PC
        { //Inicializamos las variables pertinentes
            this.last_actualization = -1;
            this.name = name;
            this.devices = new List<Device>();
            this.devices.Add(null);
            this.historial = new List<string>();
            this.actual_info = -1;
            this.info = "";
            this.already_sending = false;
            this.already_receiving = false;
            this.mac = "";
            this.link_layer_historial = new List<string>();
            this.data = "";
            this.error_detection = error_detection;
            this.ip = "";
            this.mask = "";
            this.ip_mac = new Dictionary<string, string>();
            this.arp_in = new Queue<string>();
            this.wait_time_arp = -1;
            this.info_incomplete = "";
            this.network_layer_historial = new List<string>();
            this.time_ping = 0;
            this.number_of_pins = 3;
            this.wait_pong = new Queue<string>();
            this.routes = new List<Route>();
            this.interval = interval;
        }
        #endregion

        #region Métodos de PC

        //Método para actualizar la información actual en la PC, ya sea para enviar un nueva
        //o continuar enviando una pendiente
        public override bool Actualization_Info(string information, int time)
        { //Recibe como parámetros la información a enviar y el tiempo actual, en caso de que
          //la información esté vacía, solamente actualiza los parámetros

            if (information != "") //En caso de que exista información para enviar modificamos los parámetros
            {
                this.info_incomplete = information;
                this.Link_Layer_Out(); //Primero procesamos la información en la capa de enlace
                this.already_sending = true; //Declaramos que el dispositivo ahora está enviando información
                this.actual_info = -1; //Reiniciamos el contador
                this.last_actualization = time; //Actualiza el tiempo actual del dispositivo
            }

            if (this.last_actualization < time) //En caso de que el tiempo no esté actualizado
            {
                this.last_actualization = time;
                this.already_receiving = false;
            }

            if (this.wait_time_arp != -1) //En caso de que aún no hallamos recibido el Arp Response
            {
                if (this.wait_time_arp == 0)
                {
                    this.already_sending = false;
                    this.wait_time_arp = -1;
                    this.info_incomplete = "";
                }
                else
                    this.wait_time_arp--;
            }

            

            if (this.time_ping > 0) //En caso de que estemos contando el tiempo para enviar otro ping
            {
                this.time_ping -= interval; //Disminuímos el tiempo que ha pasado

                if (this.time_ping < 1) //En caso de que ya se haya agotado el tiempo
                    this.actual_info = -1;         //Reiniciamos a info, para que comience a enviarse
            }

            this.actual_info++; //Aumentamos el contador


            if (this.actual_info >= this.info.Length || this.time_ping > 0)
            {    //En caso de que ya no tengamos información para procesar o estemos enviando pings

                if (time_ping > 0) //En caso de que aún debamos esperar por el siguiente ping
                    return true;   //Detenemos trabajar

                //Primero debemos revisar si fue un ping lo que enviamos

                bool is_ping = false;
                

                if (this.info != "") //Si acabamos de termina de enviar información 
                {
                    int length = 8 * Transformation_Codes.Binary_to_Decimal(this.info.Substring(32, 8));

                    string packet = this.info.Substring(48, length); //Obtenemos el paquete

                    if( packet.Length == 96 &&                    //En caso de que sea un ping
                        packet.Substring(72, 8) == "00000001" && 
                        packet.Substring(88) == "00001000")
                    {
                        is_ping = true; //Hacemos true la variable
                    }
                }

                if(is_ping && this.number_of_pins > 0) //Si es un ping, y aún nos quedan por enviar
                {
                    this.number_of_pins--; //Disminuímos la cantidad de pings
                    this.time_ping = 100;
                }
                else
                {
                    this.actual_info = 1; //Actualizamos el contador
                    this.info = ""; //Reiniciamos la información

                    if (this.wait_time_arp == -1) //En caso de que no estemos esperando respuesta del protocolo arp
                        this.already_sending = false; //Declaramos que el dispositivo no está trabajando

                    this.number_of_pins = 3; //Restauramos el contador de los ping
                    this.time_ping = 0;      //y el tiempo restante
                }
            }
            else //en caso de que tengamos información para procesar
            {

                if (this.devices[0] != null) //Si existe un dispositivo conectado
                {
                    //Si el dispositivo al que enviamos información ya la está recibiendo y además está actualizado, entonces
                    //quiere decir que ya está trabajando en este momento, por lo que nos detenemos
                    if (this.devices[0].already_receiving && this.devices[0].last_actualization == time)
                    {
                        //Añadimos al historial el reporte de fallo
                        this.historial.Add(time + " " + this.name + " send " + this.info[this.actual_info] + " collision");
                        return false; //Retornamos falso para indicar que el programa no fue correctamente ejecutado
                    }
                    else //en caso de que no esté actualizado o no esté trabajando
                    {
                        //Entregamos la información al dispositivo
                        this.devices[0].Receive_Info(this.info[actual_info].ToString(), time, this.name);
                        //Añadimos el reporte de envío al historial
                        this.historial.Add(time + " " + this.name + " send " + this.info[this.actual_info] + " ok");
                    }

                }
                else //En caso de que no haya ningún dispositivo conectado
                    this.historial.Add(time + " " + this.name + " send " + this.info[this.actual_info] + " ok");
            }

            if (this.already_sending == false) //En caso de que no estemos enviando información
            {

                if (this.arp_in.Count > 0)
                {   //En caso de que tengamos para enviar un ARPR, y no estemos enviando información,
                    //Lo preparamos para enviar
                    this.already_sending = true;
                    this.actual_info = -1;
                    this.info_incomplete = this.arp_in.Dequeue();
                    this.Link_Layer_Out();
                }
                else //En caso de que no tengamos que enviar un ARPR
                {
                    if(this.wait_pong.Count() > 0) //Si nos queda por enviar un pong
                    {
                        this.already_sending = true;
                        this.actual_info = -1;
                        this.info_incomplete = this.wait_pong.Dequeue();
                        this.Link_Layer_Out();
                    }
                }
            }

            return true;
        }

        //Método para procesar la información que será enviada, a modo de trama
        public void Link_Layer_Out(string new_mac = "")
        { //En caso de que reciba el parámetro new_mac, entonces significa que ya completamos la información de esta capa
            
            string receptor_mac = this.info_incomplete.Substring(0, 16);

            if (receptor_mac == "0000000000000000") //En caso de que no tengamos aún el mac del receptor
            {
                if(new_mac == "")  //Si no recibimos la nueva
                    Network_Layer_Out(this.info_incomplete.Substring(48, 32)); //Continuamos en la capa de red con el ip receptor
                else //Si ya recibimos la mac faltante
                {
                    this.info_incomplete = new_mac + this.info_incomplete.Substring(16); //Completamos la información
                    this.actual_info = -1; //Reiniciamos el contador para comenzar a enviar la información de nuevo
                    this.Link_Layer_Out(); //Llamamos recursivo para actualizar el resto de parámetros
                }
            }
            else //En caso de que ya tengamos el mac del receptor, completamos la información 
                 //con el método de cifrado para enviarla
            {
                string sender_mac = this.info_incomplete.Substring(16, 16); //Separamos los diferentes campos
                string data_length = this.info_incomplete.Substring(32, 8);
                string data = this.info_incomplete.Substring(48);

                Data_Verification temp = new Data_Verification(); //Instanciamos el diccionario con los métodos
                temp.Set_Defaul_Values();                         //de cifrado y colocamos sus valores por default

                string verificator = temp.Evaluate(data, this.error_detection); //Obtenemos el cifrado

                //Calculamos su tamaño (que será un múltiplo de 8, rellenando con ceros, puesto que hablamos de bytes)
                int verificator_length = verificator.Length / 8;
                verificator_length += verificator.Length % 8 == 0 ? 0 : 1;

                //Agregamos los 0s para completar cada información
                receptor_mac = Transformation_Codes.Add_Zero(receptor_mac, 16);
                sender_mac = Transformation_Codes.Add_Zero(sender_mac, 16);
                data_length = Transformation_Codes.Add_Zero(data_length, 8);
                verificator = Transformation_Codes.Add_Zero(verificator, verificator_length * 8);
                string verificator_length_string = Transformation_Codes.Add_Zero(
                                                   Transformation_Codes.Decimal_to_Binary(verificator_length), 8);

                //Agregamos la información completa con el método de cifrado
                this.info = receptor_mac + sender_mac + data_length + verificator_length_string +
                            data + verificator;

                this.wait_time_arp = -1;
                this.info_incomplete = "";
            }
        }

        //Método que procesa la información que será enviada, a modo de paquete
        public void Network_Layer_Out(string receptor_ip)
        { //Recibe el ip al que serán enviados los datos

            //Primera analizamos las rutas disponibles y seleccionamos un ip receptor
            //a partir del ip en los parámetros de entrada
            string new_receptor_ip = this.Route_Selector(receptor_ip);

            if(new_receptor_ip == "") //Si está vacío, no existe forma de enrutarlo
            {                         //por lo que desechamos la información
                this.info_incomplete = "";
                return;
            }

            string receptor_mac; //Desigamos una variable que nos dirá si conocemos
                                 //la mac de este ip

            //Intentamos obtener su valor
            bool exist = this.ip_mac.TryGetValue(new_receptor_ip, out receptor_mac);
            
            if(exist) //Si lo tenemos, enviamos la información
                this.Link_Layer_Out(receptor_mac);
            else      //Si no lo tenemos, debemos averiguar la mac de ese ip
                this.ARP_Quest(new_receptor_ip);
        }

        //Método que prepara el dispositivo para enviar el ARP Quest
        public void ARP_Quest(string ip)
        {   //Recibe el ip del dispositivo al que se le hará el ARP Quest
            
            /*
             askii:
             a - 97  = 01100001 
             r - 114 = 01110010
             p - 112 = 01110000
             q - 113 = 01110001
             */

            string receptor_mac = "1111111111111111"; //Utilizamos la dirección de broadcast
            string sender_mac = this.mac; //Colocamos el mac del sender
            string length = "00001000";   //Designamos que tenemos 8 bytes de datos
            string verificator_length = "00000000"; //No poseemos verificador
            string data = "01100001011100100111000001110001"; //Los datos son ARPQ en binario
            data += ip; //Le sumamos a los datos el ip del host receptor

            string temp = this.info_incomplete; //Guardamos la información en un temporal

            this.info_incomplete = receptor_mac + sender_mac +
                        length + verificator_length + data; //Actualizamos la información

            this.Link_Layer_Out(); //Hacemos que la información se complete con la Link Layer
            
            //Luego de Link Layer queda guardado en info la trama anterior completa

            this.info_incomplete = temp;  //Retornamos a tempo a su variable
            this.wait_time_arp = this.info.Length * 8; //Designamos el tiempo de espera para el arp
        }
        
        //Método para analizar a qué dispositivo se debe enviar la información a partir
        //de las tablas de turas
        public string Route_Selector(string receptor_ip)
        {
            for (int i = 0; i < this.routes.Count(); i++) //Iteramos por todas las rutas
            {
                Route route = this.routes[i]; //Guardamos la ruta actual

                //Hacemos AND entre el Ip destino y la máscara de la ruta
                string ip_result = Transformation_Codes.AND(receptor_ip, route.mask);

                if (ip_result == route.destination) //Si coincide con destination
                {
                    //En caso de que el gateway será 0.0.0.0, entonces el dispositivo está
                    //en nuestra subred, por lo que devolvemos su ip
                    if (route.gateway == "00000000000000000000000000000000")
                        return receptor_ip;
                    else                        //En otro caso, devolvemos el ip del dispositivo
                        return route.gateway;  //al que enviaremos la información
                }
            }

            return ""; //Si ninguna ruta coincide, devolvemos un ip vacío
        }

        //Método para recibir información de otro dispositivo
        public override void Receive_Info(string bit, int time, string sender)
        { //Recibe un bit de información, el tiempo actual y el nombre del dispositivo que lo envió
            this.last_actualization = time; //Actualiza el tiempo actual del dispositivo
            this.already_receiving = true; //Alerta que se encuentra recibiendo información
            this.historial.Add(time + " " + this.name + " receive " + bit + " ok"); //La información se añade al historial

            this.Link_Layer_In(bit); //Ahora pasamos a transformar la información para la capa de enlace
        }

        //Método que transforma la información proveniente de otro dispositivo 
        //de la capa física a la capa de enlace
        private void Link_Layer_In(string bit)
        {//Recibe un bit

            /*La información en la capa de enlace funciona de la siguiente forma:
              Primeros 16 bits: dirección MAC a la que está dirigida la trama, en caso de que sea FFFF, es para todo el mundo
              Próximos 16 bits: dirección MAC del host que transmite.
              Próximos 8 bits: n - tamaño de los datos (en bytes).
              Próximos 8 bits: m - tamaño de la verificación de error de datos (en bytes)
              Próximos n bits: datos
              Próximos m bits: verificación de error
            */

            this.data += bit; //Le añade a la data de la capa de enlace el nuevo bit

            if (this.data.Length == 16) //Si ya fue escrito el host receptor
            {
                //Si la trama no va dirigida a este host
                if (this.data != this.mac && this.data != "1111111111111111")
                    this.data = ""; //Limpiamos los datos
            }

            //En caso de que ya hayan sido escritos el host receptor, remitente, tamaño de los datos y tamaño del verificador
            if (this.data.Length > 48)
            {
                //Calculamos el tamaño de los datos que vienen a continuación, que como están en bytes
                //se multiplica por 8 para convertir en bits
                int data_length = Transformation_Codes.Binary_to_Decimal(this.data.Substring(32, 8)) * 8;
                int verification_error_length = Transformation_Codes.Binary_to_Decimal(this.data.Substring(40, 8)) * 8;

                if (this.data.Length == 48 + data_length                //En caso de que ya hayan llegado los datos,
                                           + verification_error_length)
                {
                    Data_Verification temp = new Data_Verification(); //Creamos el diccionario para el cifrado
                    temp.Set_Defaul_Values();

                    //Evaluamos la entrada en el método de cifrado
                    string verification = temp.Evaluate(this.data.Substring(48, data_length), this.error_detection);

                    //Si coincide con el cifrado de la trama (en decimal ambos)
                    if (Transformation_Codes.Binary_to_Decimal(verification) ==
                        Transformation_Codes.Binary_to_Decimal(
                        this.data.Substring(48 + data_length, verification_error_length)))
                    { //Agregamos que fue satisfactoria la operación

                        string data_binary = this.data.Substring(48, data_length);

                        string data_hexadecimal = Transformation_Codes.Binary_to_Hexadecimal(data_binary);

                        this.link_layer_historial.Add(
                            this.last_actualization.ToString() + " " +
                            Transformation_Codes.Binary_to_Hexadecimal(this.data.Substring(16, 16)) + " " + data_hexadecimal);



                        if (data_length == 64) //Si los datos tuvieron específicamente 64 bits, son parte 
                        {                      //de un protocolo arp entonces

                            string arp = data_binary.Substring(0, 32);          //Separamos el código arp
                            string ip_receptor = data_binary.Substring(32, 32); //Separamos el ip que se está buscando

                            if (arp == "01100001011100100111000001110001") //Si es un ARP Quest
                            {
                                if (ip_receptor == this.ip) //En caso de que coincida con nuestro ip
                                {
                                    this.ARP_Response(); //Pasamos a responder el ARP Quest
                                }

                            }

                            if (arp == "01100001011100100111000001110010") //Si es un ARP Response
                            {
                                this.ip_mac.Add(ip_receptor, this.data.Substring(16, 16));
                                this.Link_Layer_Out();
                            }
                        }
                        else //En caso de que no sea parte del protocolo arp, es un paquete
                        {
                            if (data_binary.Substring(72, 8) == "00000001" && //Si es es parte del
                                data_binary.Substring(80, 8) == "00000001")   //protocolo ICMP
                            {
                                if (data_binary.Substring(88, 8) == "00000000") //Si es un pong
                                {
                                    this.Network_Layer_In(data_binary, "echo reply");
                                }

                                if (data_binary.Substring(88, 8) == "00001000") //Si es un ping
                                {
                                    this.Pong_Response();
                                    this.Network_Layer_In(data_binary, "echo request");
                                }

                                if(data_binary.Substring(88, 8) == "00000011") //Si es un DHU
                                {
                                    this.Network_Layer_In(data_binary, "destination host unreachable");
                                }


                            }
                            else //En otro caso, simplemente es un paquete común
                                this.Network_Layer_In(data_binary);
                        }

                    }
                    else
                    { //En  otro caso avisamos que la información llegó con error
                        this.link_layer_historial.Add(
                            this.last_actualization.ToString() + " " +
                            Transformation_Codes.Binary_to_Hexadecimal(this.data.Substring(16, 16)) + " " +
                            Transformation_Codes.Binary_to_Hexadecimal(this.data.Substring(48, data_length))
                            + " ERROR");

                    }

                    this.data = ""; //Reiniciamos la data
                    this.already_receiving = false;
                }
            }
        }

        //Método que procesa la información de la capa de red que se encuentra
        //en una trama de la capa de enlace
        private void Network_Layer_In(string data, string extra_info = "")
        { //Recibe un extra_info, que indica si hay que agregar información
          //al final del mensaje, en caso de que sea un ICMP

            string ip_sender_binary = data.Substring(32, 32);
            string ip = Transformation_Codes.Binary_to_Ip(ip_sender_binary);

            int length = Transformation_Codes.Binary_to_Decimal(data.Substring(80, 8)) * 8;

            this.network_layer_historial.Add(this.last_actualization + " " + ip + " " + 
                                             Transformation_Codes.Binary_to_Hexadecimal
                                             (data.Substring(88, length)) + " " + extra_info);

        }

        //Método que prepara el dispositivo para enviar el ARP Response
        public void ARP_Response()
        {
            int data_length = Transformation_Codes.Binary_to_Decimal(this.data.Substring(32, 8));
            string packet = this.data.Substring(48, data_length * 8); //Multiplicamos porque el tamaño está
                                                                        //expresado en bytes

            string sender_mac = this.data.Substring(16, 16);
            string length = "00001000";   //Designamos que tenemos 8 bytes de datos
            string verificator_length = "00000000"; //No poseemos verificador
            string new_data = "01100001011100100111000001110010"; //Los datos son ARPQ en binario
            new_data += packet.Substring(32, 32); //Le sumamos a los datos el ip del host receptor

            string arp_response = sender_mac + this.mac +
                                  length + verificator_length +
                                  new_data;

            this.arp_in.Enqueue(arp_response);
            this.already_sending = true;
        }

        //Método que prepara el dispositivo para responder un pong a partir de un ping
        public void Pong_Response()
        {
            int data_length = Transformation_Codes.Binary_to_Decimal(this.data.Substring(32, 8));
            string packet = this.data.Substring(48, data_length * 8); //Multiplicamos porque el tamaño está
                                                                      //expresado en bytes

            string length = "00001100";   //Designamos que tenemos 12 bytes de datos
            string verificator_length = "00000000"; //No poseemos verificador

            //Los datos a continuación son un ping, pero con los ip invertidos, protocol en 1, y data en 0.

            string ip_receptor = packet.Substring(32, 32);
            string ip_sender = packet.Substring(0, 32);
            string ttl = "00000000";
            string protocol = "00000001";
            string payload_size = "00000001";
            string payload = "00000000";

            string new_data = ip_receptor + ip_sender + ttl +
                              protocol + payload_size + payload;

            string pong_response = "0000000000000000" + this.mac +
                                   length + verificator_length +
                                   new_data;
            //La mac a enviar se deja en 0 porque no sabemos aún a qué
            //dispositivo responder

            this.wait_pong.Enqueue(pong_response);
            this.already_sending = true;
        }

        //Método para reiniciar la PC
        public override string Restart()
        {
            this.actual_info = -1; //Reiniciamos el contador

            string temp = info;
            this.info = ""; //Borramos la información

            //Devolvemos un string que será la información y el puerto de la PC, que en este caso es único
            return temp;
        }

        //Método para Revelar si el dispositivo está enviando información
        public override bool Show_Info(string name)
        {
            return this.info != "";
        }

        //Método para conectar a la PC con otro dispositivo
        public override int Connect(Device device, int port)
        { //Recibe el dispositivo y el puerto al que será conectado
            if (this.devices[0] == null) //En caso de que el puerto esté vacío
            {
                this.devices[0] = device; //Conecta el dispositivo a ese puerto
                return 0; //Retornamos 0 para indicar que fue posible la conexión
            }
            else //En caso de que ya haya un dispositivo conectado a ese puerto
            {
                this.historial.Add("The port " + port + 1 + " has already connected"); //Añadimos le eror al historial
                return -1; //Retornamos -1 para indicar que no fue posible la conexión
            }
        }

        //Método para desconectar la PC de otro dispositivo
        public override int Disconnect(int port)
        {//Recibe el puerto al que se desea conectar
            if (this.devices[0] != null) //En caso de que existe un dispositivo conectado a ese puerto
            {
                this.devices[0] = null; //Desconectamos el dispositivo
                return 0; //Retornamos 0 para indicar que la operación fue realizada
            }
            else //En caso de que no haya ningún dispositivo conectado
            {
                this.historial.Add("The port " + port + 1 + " has no connected"); //Añadimos el error al historial
                return -1; //Retornamos -1 para indicar que ocurrió un problema
            }
        }

        //Método para buscar el puerto al que está conectado un dispositivo
        public override int Device_Search(string name)
        { //Recibe el nombre del dispositivo
            if (this.devices[0] == null || this.devices[0].name != name) //Si no hay dispositivo conectado o no es
                return -1;                                               //el dispositivo requerido, devolvemos -1
            else //En otro caso caso devolvemos el único puerto que existe
                return 0;
        }

        //Método para buscar el nombre de un dispositivo conectado en un puerto
        public override string Device_Search(int port)
        { //Recibe el índice del puerto seleccionado, aunque en este caso siempre hay un único puerto
            if (this.devices[0] == null) //En caso de que no hay nada conectado
                return null; //Retornamos que no existe dicho dispositivo
            else
                return this.devices[0].name; //En otro caso devolvemos el nombre del dispositivo
        }

        //Método para mostrar el historial del Dispositivo
        public override void Show_Historial(string directory)
        {//Este método recibe el directorio donde se quiere guardar un txt que tenga el historial de este dispositivo

            //Utilizamos la clase SteamWrite, la cual, mediante un directorio crea un archivo
            StreamWriter writer = new StreamWriter(directory);

            for (int i = 0; i < this.historial.Count(); i++) //Iteramos por cada una de las posiciones del historial
            {
                writer.WriteLine(this.historial[i]); //Copiamos esa posición en el txt
            }

            writer.Dispose(); //Le damos formato al txt

            //Pasamos a llamar al método que muestra el historial para la capa de enlace
            this.Show_Link_Layer_Historial_Data(directory.Substring(0, directory.Length - 4) + "_data.txt");
        }

        //Método para mostrar el historial de datos del Dispositivo, correspondiente a la Capa de Enlace
        public void Show_Link_Layer_Historial_Data(string directory)
        {//Este método recibe el directorio donde se quiere guardar un txt que tenga el historial de este dispositivo

            //Utilizamos la clase SteamWrite, la cual, mediante un directorio crea un archivo
            StreamWriter writer = new StreamWriter(directory);

            for (int i = 0; i < this.link_layer_historial.Count(); i++) //Iteramos por cada una de las posiciones del data_historial
            {
                writer.WriteLine(this.link_layer_historial[i]); //Copiamos esa posición en el txt
            }

            writer.Dispose(); //Le damos formato al txt

            //Pasamos a llamar al método que muestra el historial para la capa de red
            this.Show_Network_Layer_Historial_Data(directory.Substring(0, directory.Length - 9) + "_payload.txt");
        }

        //Método para mostrar el historial de datos del Dispositivo, correspondiente a la Capa de Enlace
        public void Show_Network_Layer_Historial_Data(string directory)
        {//Este método recibe el directorio donde se quiere guardar un txt que tenga el historial de este dispositivo

            //Utilizamos la clase SteamWrite, la cual, mediante un directorio crea un archivo
            StreamWriter writer = new StreamWriter(directory);

            for (int i = 0; i < this.network_layer_historial.Count(); i++) //Iteramos por cada una de las posiciones del historial
            {
                writer.WriteLine(this.network_layer_historial[i]); //Copiamos esa posición en el txt
            }

            writer.Dispose(); //Le damos formato al txt
        }

        //Método para poder darle un mac a la PC
        public void Actualizate_Mac(string mac)
        {
            if (this.mac == "") //En caso de que no tengamos mac, lo actualizamos
                this.mac = mac; //Guardamos la mac en binario
        }

        //Método para poder darle un ip y una mask a la PC
        public void Actualizate_IP_Mask(string ip, string mask)
        {
            if (this.ip == "") //En caso de que no tengamos ip, lo actualizamos
            {
                this.ip = ip;
                this.mask = mask;
            }
        }

        //Verifica si el dispositivo está recibiendo información
        public override void Receiving_Verification()
        {
            //Si el dispositivo al que está conectada la PC no está enviando información
            if (this.devices[0] != null && this.devices[0].Show_Info(this.name) == false)
            {
                this.data = ""; //Reiniciamos los datos, ya que pueden estar corruptos
                this.already_receiving = false; //Declaramos que no estamos recibiendo información
            }
        }

        //Verifica de antemano si el dispositivo creará una colisión al enviar información
        public bool Can_Send()
        { //Devuelve true si el dispositivo puede enviar información, y false si creará una losición
            if (this.already_sending || (this.devices[0] != null && this.devices[0].already_receiving))
                return false;
            else
                return true;
        }

        //Método que añade una ruta a la lista de rutas
        public void Add_Route(string destination, string mask, string gateway, int interface_port)
        {
            Route temp = new Route(destination, mask, gateway, interface_port); //Creamos un ruta con
                                                                                //estos elementos

            if (this.routes.Count() == 0 || //Si no hay elementos en la lista o temp es el de menor prioridad
                this.routes[this.routes.Count() -1].priority >= temp.priority)
            {
                this.routes.Add(temp); //Lo añadimos al final de la lista
                return;
            }

            for (int i = 0; i < this.routes.Count(); i++) //Buscamos por la lista de rutas
            {
                if (temp.priority > this.routes[i].priority) //Lo colocamos ordenado en la lista
                {                                           //según el de mayor prioridad
                    this.routes.Insert(i, temp);
                    break;
                }
            }
        }

        //Método que elimina una ruta a la lista de rutas
        public void Delete_Route(string destination, string mask, string gateway, int interface_port)
        {
            for (int i = 0; i < this.routes.Count(); i++) //Buscamos por la lista de rutas
            {
                Route temp = this.routes[i]; //Colocamos la ruta actual en un temporal
                                             //para acceder más fácilmente a ella                       

                if (temp.destination == destination && //Si todos sus parámetros coinciden
                    temp.mask == mask &&
                    temp.gateway == gateway &&
                    temp.interface_port == interface_port)
                {
                    this.routes.RemoveAt(i); //La removemos de la lista
                    break;
                }

            }
        }

        #endregion
    }



    public class HUB : Device
    {
        #region Atributos y Constructor
        //Constructor de la clase
        public HUB(string name, int ports)
        {//Inicializamos los atributos de la clase
            this.last_actualization = -1;
            this.name = name;

            this.devices = new List<Device>();
            for (int i = 0; i < ports; i++) //Declaramos las posiciones que tendrán los diferentes puertos
            {
                this.devices.Add(null);
            }

            this.historial = new List<string>();
            this.info = "";
            this.already_sending = false;
            this.already_receiving = false;
        }
        #endregion

        #region Métodos de HUB

        //Método para actualizar la información actual en el HUB
        public override bool Actualization_Info(string information, int time)
        { //Recibe una información y el tiempo actual, donde la información no tiene importancia
            if (time > this.last_actualization) //En caso de que aún no haya sido actualizado
            {
                this.last_actualization = time; //Actualizamos el tiempo
                this.already_sending = false; //Denotamos que no está trabajando el dispositivo
                this.already_receiving = false; //Denotamos que no está trabajando el dispositivo
                this.info = ""; //Borramos la información que se encontraba en el dispositivo
            }
            return true;//Retornamos true para indicar que la operación fue satisfactoria
        }


        //Método para conectar otro dispositivo en un puerto
        public override int Connect(Device device, int port)
        {//Recibe un diispositivo y el puerto en el que se desea conectarlo
            if (this.devices[port] == null) //En caso de que no haya algún dispositivo conectado
            {
                this.devices[port] = device; //Conectamos el dispositivo
                return 0; //Retornamos 0 para indicar que la operación fue satisfactoria
            }
            else //Si ya existe un dispositivo conectado a ese puerto
            {
                this.historial.Add("The port " + port + 1 + " has already connected"); //Añadimos al error al historial
                return -1; //Retornamos -1 para indicar que fue impposible la operación
            }
        }


        //Método para buscar el puerto al que está conectado un dispositivo
        public override int Device_Search(string name)
        { //Recibe el nombre del dispositivo
            for (int i = 0; i < this.devices.Count(); i++)//Se itera por todos los puertos
            {
                if (this.devices[i] != null && this.devices[i].name == name) //Si hay algo conecctado y es el dispositivo
                {
                    return i; //Retornamos la posición dle mismo
                }
            }
            return -1; //Si no existe retornamos -1
        }


        //Método para buscar el nombre de un dispositivo conectado en un puerto
        public override string Device_Search(int port)
        { //Recibe el puerto donde se quiere revisar el dispositivo

            if (this.devices[port] != null) //Si hay algo conectado
                return this.devices[port].name; //Devolvemos el nombre del dispositivo
            else
                return null; //En otro caso no devolvemos nada
        }


        //Método para desconectar un dispositivo de uno de los puertos del HUB
        public override int Disconnect(int port)
        { //Recibe el puerto desde el que se quiere desconectar el dispositivo
            if (this.devices[port] != null) //Si en el puerto hay algo conectado
            {
                this.devices[port] = null; //Desconectamos el dispositivo
                return 0; //Retornamos 0 para indicar que la operación fue satisfactoria
            }
            else //En otro caso
            {
                this.historial.Add("The port " + port + 1 + " has no connected"); //Añadimos el error al historial
                return -1; //Retornamos -1 para indicar que ocurrió un error
            }
        }


        //Método para desconectar un dispositivo de uno de los puertos, mediante su nombre
        public int Disconnect(string name)
        {//Para ello recibe el nombre del dispositivo
            for (int i = 0; i < this.devices.Count(); i++) //BUscamos por todos los puertos
            {
                if (this.devices[i] != null && this.devices[i].name == name) //Si existe un dispositivo en ese puerto
                {                                                           //y es el que buscamos
                    this.devices[i] = null; //Desconectamos ese dispositivo
                    return 0; //Retornamos 0 para indicar que la operación fue satisfactoria
                }
            }
            //En caso de que le dispositivo no se encuentre en ninguno de los puertos, añadimos el historial el error
            this.historial.Add("There is no device with name " + name + " to disconnect");
            return -1; //Retornamos -1 para indicar que no fue posible realizar la operación
        }


        //Método para recibir información de otro dispositivo
        public override void Receive_Info(string bit, int time, string sender)
        { //Recibe un bit de información, el tiempo actual y el nombre del dispositivo que la envió

            this.last_actualization = time; //Actualizamos el tiempo actual del dispositivo
            this.already_sending = true; //Indicamos que ya estamos trabajando
            this.already_receiving = true;
            this.info = bit; //Actualizamos la información actual

            for (int i = 0; i < this.devices.Count(); i++) //Iteramos por todos sus puertos
            {
                if (this.devices[i] != null && this.devices[i].name == sender) //En caso de que encontremos el dispositivo
                {                                                             //que envió la información
                    this.historial.Add(time + " " + this.name + "_" + (i + 1) + " receive " + bit); //añadimos la información
                }                                                                           //al historial
            }

            for (int i = 0; i < this.devices.Count(); i++) //Iteramos nuevamente por el resto de dispositivos
            {
                //Si no es el que envió la información y no está recibiendo información actualmente
                if (this.devices[i] != null && this.devices[i].name != sender &&
                   (this.devices[i].already_receiving == false || this.devices[i].last_actualization < time))
                {
                    this.historial.Add(time + " " + this.name + "_" + (i + 1) + " send " + bit); //Añadimos al historial
                    this.devices[i].Receive_Info(bit, time, this.name); //Enviamos la información al dispositivo
                }
            }
        }


        //Método para Revelar la información actual en el Dispositivo
        public override bool Show_Info(string name)
        {
            return this.info != ""; //Retornamos la información actual
        }


        //Método para mostrar el historial del Dispositivo
        public override void Show_Historial(string directory)
        {//Este método recibe el directorio donde se quiere guardar un txt que tenga el historial de este dispositivo

            //Utilizamos la clase SteamWrite, la cual, mediante un directorio crea un archivo,
            StreamWriter writer = new StreamWriter(directory);

            for (int i = 0; i < this.historial.Count(); i++) //Iteramos por cada una de las posiciones del historial
            {
                writer.WriteLine(this.historial[i]); //Copiamos esa posición en el txt
            }

            writer.Dispose(); //Le damos formato al txt
        }

        //Método para reiniciar los valores del Dispositivo
        public override string Restart()
        {
            string temp = this.info; //Salvamos la información
            this.info = ""; //La eliminamos
            return temp; //Luego, devolvemos la información, el puerto no es necesario
        }

        //método para saber si los dispositivos alrededor del hub están enviando información
        public override void Receiving_Verification()
        { //En el caso del hub, este no realiza ninguna información en particular
            return;
        }
        
        #endregion
    }

    public class Switch : Device
    {
        #region Atributos y Constructor

        //Aquí se guardarán todas las mac que hasta ahora conoce el Switch
        private List<List<string>> known_mac { get; set; }

        //Aquí se guardará toda la información que está recibiendo cada puerto
        private List<string> receiving_data { get; set; }

        //Aquí se guardará toda la información que debe transmitir un puerto
        private List<Queue<string>> sending_data { get; set; }

        //Aquí se guardará la información que actualmente se está enviando por cada puerto
        private List<string> information { get; set; }


        public Switch(string name, int ports)
        {
            //Inicializamos los atributos de la clase
            this.last_actualization = -1;
            this.name = name;

            this.devices = new List<Device>();
            this.known_mac = new List<List<string>>();
            this.sending_data = new List<Queue<string>>();
            this.receiving_data = new List<string>();
            this.information = new List<string>();

            for (int i = 0; i < ports; i++) //Declaramos las posiciones que tendrán los diferentes puertos
            {
                this.devices.Add(null);
                this.known_mac.Add(new List<string>());
                this.sending_data.Add(new Queue<string>());
                this.receiving_data.Add("");
                this.information.Add("");
            }

            this.historial = new List<string>();
            this.info = "";
            this.already_sending = false;
            this.already_receiving = false;


        }
        #endregion

        #region Métodos de Switch

        //Actualiza la información en el dispositivo
        public override bool Actualization_Info(string information, int time)
        {
            this.last_actualization = time; //Actualizamos el tiempo

            for (int i = 0; i < this.devices.Count(); i++) //Vamos por todos los dispositivos
            {
                //Actualizamos la información actual que estamos enviando por el puerto actual
                this.information[i] = "";

                //Si hay un dispositivo conectado, no está recibiendo información, 
                //y hay información para enviarle, pues entonces enviamos la información
                if (this.devices[i] != null && this.devices[i].already_receiving == false
                                            && this.sending_data[i].Count() != 0)
                {
                    //Avisamos qué información estamos enviando por ese puerto
                    this.information[i] = this.sending_data[i].Dequeue();

                    //La enviamos
                    this.devices[i].Receive_Info(this.information[i], this.last_actualization, this.name);

                    //Añadimos al historial
                    this.historial.Add(time + " " + this.name + "_" + (i + 1) + " send " + this.information[i]);
                }
            }
            return true; //Retornamos true para indicar que fue posible neviar la información
        }

        //Permite conectar un dispositivo en uno de los puertos del Switch
        public override int Connect(Device device, int port)
        {
            if (this.devices[port] == null) //En caso de que no haya algún dispositivo conectado
            {
                this.devices[port] = device; //Conectamos el dispositivo
                return 0; //Retornamos 0 para indicar que la operación fue satisfactoria
            }
            else //Si ya existe un dispositivo conectado a ese puerto
            {
                this.historial.Add("The port " + port + 1 + " has already connected"); //Añadimos al error al historial
                return -1; //Retornamos -1 para indicar que fue impposible la operación
            }
        }

        //Método para buscar el nombre de un dispositivo conectado en un puerto
        public override string Device_Search(int port)
        { //Recibe el puerto donde se quiere revisar el dispositivo

            if (this.devices[port] != null) //Si hay algo conectado
                return this.devices[port].name; //Devolvemos el nombre del dispositivo
            else
                return null; //En otro caso no devolvemos nada
        }

        //Método para buscar el puerto al que está conectado un dispositivo
        public override int Device_Search(string name)
        { //Recibe el nombre del dispositivo
            for (int i = 0; i < this.devices.Count(); i++)//Se itera por todos los puertos
            {
                if (this.devices[i] != null && this.devices[i].name == name) //Si hay algo conecctado y es el dispositivo
                {
                    return i; //Retornamos la posición dle mismo
                }
            }
            return -1; //Si no existe retornamos -1
        }

        //Método para desconectar un dispositivo de uno de los puertos, mediante su nombre
        public int Disconnect(string name)
        {//Para ello recibe el nombre del dispositivo
            for (int i = 0; i < this.devices.Count(); i++) //BUscamos por todos los puertos
            {
                if (this.devices[i] != null && this.devices[i].name == name) //Si existe un dispositivo en ese puerto
                {                                                           //y es el que buscamos
                    this.devices[i] = null; //Desconectamos ese dispositivo
                    return 0; //Retornamos 0 para indicar que la operación fue satisfactoria
                }
            }
            //En caso de que le dispositivo no se encuentre en ninguno de los puertos, añadimos el historial el error
            this.historial.Add("There is no device with name " + name + " to disconnect");
            return -1; //Retornamos -1 para indicar que no fue posible realizar la operación
        }

        //Método para desconectar un dispositivo de uno de los puertos del Switch
        public override int Disconnect(int port)
        { //Recibe el puerto desde el que se quiere desconectar el dispositivo
            if (this.devices[port] != null) //Si en el puerto hay algo conectado
            {
                this.devices[port] = null; //Desconectamos el dispositivo
                return 0; //Retornamos 0 para indicar que la operación fue satisfactoria
            }
            else //En otro caso
            {
                this.historial.Add("The port " + port + 1 + " has no connected"); //Añadimos el error al historial
                return -1; //Retornamos -1 para indicar que ocurrió un error
            }
        }

        //Permite al dispositivo recibir información a partir de uno de sus puertos
        public override void Receive_Info(string bit, int time, string sender)
        {
            for (int i = 0; i < this.devices.Count(); i++) //Iteramos por todos sus puertos
            {
                if (this.devices[i] != null && this.devices[i].name == sender) //En caso de que encontremos el dispositivo
                {                                                             //que envió la información
                    this.historial.Add(time + " " + this.name + "_" + (i + 1) + " receive " + bit); //añadimos la información
                }                                                                             //al historial
            }

            int pos = this.Device_Search(sender);

            this.receiving_data[pos] += bit;

            this.Link_Layer(pos);
        }

        //Método para decodificar la información y pasarla a la capa de enlace
        public void Link_Layer(int pos)
        {//Recibe la posición en la que se encuentra la información

            string data = this.receiving_data[pos]; //Guardamos la información en data

            if (data.Length == 32) //Si ya fue escrito el host remitente
            {
                //Extraemos el host remitente
                string mac_receive = data.Substring(16, 16);

                this.Actualization_Mac(mac_receive, pos); //Añadimos esta mac a la lista de macs
            }

            //En caso de que ya hayan sido escritos el host receptor, remitente, campo extra y tamaño de los datos
            if (data.Length > 48)
            {
                //Calculamos el tamaño de los datos que vienen a continuación, que como están en bytes
                //se multiplica por 8 para convertir en bits
                int data_length = Transformation_Codes.Binary_to_Decimal(data.Substring(32, 8)) * 8;
                int verification_error_length = Transformation_Codes.Binary_to_Decimal(data.Substring(40, 8)) * 8;

                    if (data.Length == 48 + data_length                //En caso de que ya hayan llegado los datos,
                                          + verification_error_length)
                {
                    string mac = data.Substring(0, 16); //Separamos el mac del host al que va dirigida la trama

                    int temp = this.Search_Mac(mac); //Buscamos su posición en la lista de macs

                    if (temp == -1) //Si no se encontraba entre los macs
                    { //Colocamos la información en todos los puertos menos el emisor para enviarla
                        for (int i = 0; i < this.sending_data.Count(); i++)
                        {
                            if (i != pos && this.devices[i] != null) //Si no nos encontramos en el mismo puerto
                                                                     //por el que vino la información
                            {
                                for (int j = 0; j < data.Length; j++)
                                {
                                    this.sending_data[i].Enqueue(data[j].ToString()); //La enviamos
                                }
                            }
                        }
                    }
                    else //Si la mac fue encontrada
                    {
                        for (int j = 0; j < data.Length; j++) //Colocamos toda la información en el puerto correspondiente
                        {
                            this.sending_data[temp].Enqueue(data[j].ToString());
                        }
                    }

                    this.receiving_data[pos] = ""; //Reiniciamos los datos
                }
            }
        }

        //Método que añade una mac a la lista de macs
        public void Actualization_Mac(string mac, int pos)
        {
            //Lo primero es borrar este mac de todas las listas

            for (int i = 0; i < this.known_mac.Count(); i++)
            {
                for (int j = 0; j < this.known_mac[i].Count(); j++) //Iteramos por todas las macs
                {
                    if (this.known_mac[i][j] == mac) //Si lo encontramos
                    {
                        this.known_mac[i].RemoveAt(j); //Lo removemos
                    }
                }
            }

            this.known_mac[pos].Add(mac); //Añadimos el mac a la nueva posición
        }

        //Método para saber en cuál de las listas de macs se encuentra una mac
        public int Search_Mac(string mac)
        {//Recibe una mac
            for (int i = 0; i < this.known_mac.Count(); i++)
            {
                for (int j = 0; j < this.known_mac[i].Count(); j++) //Iteramos por todas las macs
                {
                    if (this.known_mac[i][j] == mac) //Si lo encontramos
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        //Permite al dispositivo saber si alguno de los dispositivos conectados dejó de enviar información
        public override void Receiving_Verification()
        {
            for (int i = 0; i < this.devices.Count(); i++) //Vamos por todos los dispositivos
            { //Si no existe un dispositivo conectado a ese puerto o no está enviando información
                if (this.devices[i] == null || this.devices[i].already_sending == false)
                    this.receiving_data[i] = ""; //Eliminamos la información que habíamos recibido
            }
        }

        //Método para reiniciar el dispositivo
        public override string Restart()
        {
            //Reiniciamos los valores de todas las listas

            this.devices = new List<Device>();
            this.known_mac = new List<List<string>>();
            this.sending_data = new List<Queue<string>>();
            this.receiving_data = new List<string>();
            this.information = new List<string>();

            for (int i = 0; i < this.devices.Count(); i++)
            {
                this.devices.Add(null);
                this.known_mac.Add(new List<string>());
                this.sending_data.Add(new Queue<string>());
                this.receiving_data.Add("");
                this.information.Add("");
            }
            return null;
        }

        //Método para mostrar el historial del Dispositivo
        public override void Show_Historial(string directory)
        {//Este método recibe el directorio donde se quiere guardar un txt que tenga el historial de este dispositivo

            //Utilizamos la clase SteamWrite, la cual, mediante un directorio crea un archivo,
            StreamWriter writer = new StreamWriter(directory);

            for (int i = 0; i < this.historial.Count(); i++) //Iteramos por cada una de las posiciones del historial
            {
                writer.WriteLine(this.historial[i]); //Copiamos esa posición en el txt
            }

            writer.Dispose(); //Le damos formato al txt
        }

        //Método para mostrar la información de un dispositivo
        public override bool Show_Info(string name) //Recibe el nombre de un dispositivo
        {
            for (int i = 0; i < this.devices.Count(); i++) //Busca por todos los dispositivos conectados
            {
                //Si encontramos el dispositivo que coincida
                if (this.devices[i] != null && this.devices[i].name == name)
                    return this.information[i] != ""; //Retornamos la información de ese puerto
            }
            return false; //Si no existe ese dispositivo retornamos que no hay información
        }

        #endregion
    }

    public class Router : Device
    {
        #region Atributos y Constructor

        private string error_detection { get; set; }
        //Aquí se guarda el verificador de erorres que se está usando

        private List<Route> routes { get; set; }
        //Aquí se guardarán todas las rutas que contiene el Router

        private List<string> receiving_data { get; set; }
        //Aquí se guardará toda la información que está recibiendo cada puerto

        private List<Queue<string>> sending_data { get; set; }
        //Aquí se guardará toda la información que debe transmitir un puerto

        private List<string> information { get; set; }
        //Aquí se guardará el bit que actualmente se está enviando por cada puerto

        private List<string> ip_list { get; set; }
        //Aquí se guardará una lista que contiene el ip de cada interfaz

        private List<string> mask_list { get; set; }
        //Aquí se guardará una lista que contiene la máscara de cada interfaz

        private List<string> mac_list { get; set; }
        //Aquí se guardará una lista que contiene la mac de cada interfaz

        private List<Queue<string>> info_waiting_arp { get; set; }
        //Aquí se guardará toda información que todavía no puede ser enviada por falta de ARP Response

        private Dictionary<string, string> ip_mac { get; set; }
        //Diccionario que muestra todas las mac que han sido descubiertas mediante el ARP

        private List<int> wait_time_arp { get; set; }
        //Muestra el tiempo de espera de cada intefaz hasta notar que falló el protocolo ARP


        public Router(string name, int ports, string error_detection)
        {
            //Inicializamos los atributos de la clase
            this.last_actualization = -1;
            this.name = name;

            this.error_detection = error_detection;
            this.devices = new List<Device>();
            this.routes = new List<Route>();
            this.sending_data = new List<Queue<string>>();
            this.receiving_data = new List<string>();
            this.information = new List<string>();
            this.ip_list = new List<string>();
            this.mask_list = new List<string>();
            this.mac_list = new List<string>();
            this.info_waiting_arp = new List<Queue<string>>();
            this.wait_time_arp = new List<int>();
            this.ip_mac = new Dictionary<string, string>();

            for (int i = 0; i < ports; i++) //Declaramos las posiciones que tendrán los diferentes puertos
            {                               //e interfaces
                this.devices.Add(null);
                this.sending_data.Add(new Queue<string>());
                this.receiving_data.Add("");
                this.information.Add("");
                this.ip_list.Add("");
                this.mask_list.Add("");
                this.mac_list.Add("");
                this.info_waiting_arp.Add(new Queue<string>());
                this.wait_time_arp.Add(-1);
            }

            this.historial = new List<string>();
            this.info = "";
            this.already_sending = false;
            this.already_receiving = false;

        }
        #endregion

        #region Métodos de Router

        //Actualiza la información en el dispositivo
        public override bool Actualization_Info(string information, int time)
        {
            this.last_actualization = time; //Actualizamos el tiempo

            for (int i = 0; i < this.devices.Count(); i++) //Vamos por todos los dispositivos
            {
                //Actualizamos la información actual que estamos enviando por el puerto actual
                this.information[i] = "";

                //Lo primero es actualizar el tiempo de aquellas informaciones que
                //esperan respuesta de un protocolo ARP Quest
                if(this.devices[i] != null && this.devices[i].already_sending == false &&
                    this.wait_time_arp[i] > 0)
                    this.wait_time_arp[i]--;

                if(this.wait_time_arp[i] == 0) //En caso de que se haya agotado el tiempo
                {
                    string info_trash = this.info_waiting_arp[i].Dequeue(); //Desechamos esa información
                    this.DHU_Response(info_trash);
                    this.wait_time_arp[i] = -1;
                }

                if (this.wait_time_arp[i] == -1 && this.info_waiting_arp[i].Count() > 0)
                    this.ARP_Quest(i);


                //Si hay un dispositivo conectado, no está recibiendo información, 
                //y hay información para enviarle, pues entonces enviamos la información
                if (this.devices[i] != null && this.devices[i].already_receiving == false
                                            && this.sending_data[i].Count() != 0)
                {
                    //Avisamos qué información estamos enviando por ese puerto
                    this.information[i] = this.sending_data[i].Dequeue();

                    //La enviamos
                    this.devices[i].Receive_Info(this.information[i], this.last_actualization, this.name);

                    //Añadimos al historial
                    this.historial.Add(time + " " + this.name + "_" + (i + 1) + " send " + this.information[i]);
                }
            }
            return true; //Retornamos true para indicar que fue posible neviar la información
        }

        //Permite al dispositivo recibir información a partir de uno de sus puertos
        public override void Receive_Info(string bit, int time, string sender)
        {
            for (int i = 0; i < this.devices.Count(); i++) //Iteramos por todos sus puertos
            {
                if (this.devices[i] != null && this.devices[i].name == sender) //En caso de que encontremos el dispositivo
                {                                                             //que envió la información
                    this.historial.Add(time + " " + this.name + "_" + (i + 1) + " receive " + bit); //añadimos la información
                }                                                                                   //al historial
            }

            int pos = this.Device_Search(sender);

            this.receiving_data[pos] += bit;

            this.Link_Layer(pos);
        }

        //Método para decodificar la información y pasarla a la capa de enlace
        public void Link_Layer(int pos)
        {//Recibe la posición en la que se encuentra la información

            string info = this.receiving_data[pos]; //Guardamos la información en info

            if(info.Length == 16)
            {
                if(info != this.mac_list[pos] && info != "1111111111111111")
                    this.receiving_data[pos] = ""; //Borramos la información
            }

            //En caso de que ya hayan sido escritos el host receptor, remitente, campo extra y tamaño de los datos
            if (info.Length > 48)
            {

                //Calculamos el tamaño de los datos que vienen a continuación, que como están en bytes
                //se multiplica por 8 para convertir en bits
                int data_length = Transformation_Codes.Binary_to_Decimal(info.Substring(32, 8)) * 8;
                int verification_error_length = Transformation_Codes.Binary_to_Decimal(info.Substring(40, 8)) * 8;

                if (info.Length == 48 + data_length                //En caso de que ya hayan llegado los datos,
                                      + verification_error_length)
                {
                    string data = info.Substring(48, data_length); //Separamos el paquete que trae la trama

                    if (data_length == 64) //Si los datos tuvieron específicamente 64 bits, son parte 
                    {                      //de un protocolo arp entonces

                        string arp = data.Substring(0, 32);          //Separamos el código arp
                        string ip_receptor = data.Substring(32, 32); //Separamos el ip que se está buscando

                        if (arp == "01100001011100100111000001110001") //Si es un ARP Quest
                        {
                            if (ip_receptor == this.ip_list[pos]) //En caso de que coincida con nuestro ip
                            {
                                this.ARP_Response(info, pos); //Pasamos a responder el ARP Quest
                            }

                        }

                        if (arp == "01100001011100100111000001110010") //Si es un ARP Response
                        {
                            this.ip_mac.Add(ip_receptor, info.Substring(16, 16));
                            this.Network_Layer(this.info_waiting_arp[pos].Dequeue());
                        }
                    }
                    else //En otro caso son parte de un paquete
                    {
                        this.Network_Layer(info);
                    }

                    this.receiving_data[pos] = ""; //Borramos la información
                }
            }
        }

        //Método que procesa la información recibida, a modo de paquete, y decide
        //por qué puerto será enviada
        public void Network_Layer(string info)
        { //Recibe la información que fue transmitida hacia este dispositivo

            int data_length = Transformation_Codes.Binary_to_Decimal(info.Substring(32, 8)) * 8;
            string data = info.Substring(48, data_length);

            string receptor_ip = data.Substring(0, 32);

            //Primera analizamos las rutas disponibles y seleccionamos un ip receptor
            //a partir del ip en los parámetros de entrada
            Tuple<string, int> selector = this.Route_Selector(receptor_ip);

            string new_receptor_ip = selector.Item1;
            int port = selector.Item2;

            if (new_receptor_ip == "") //Si está vacío, no existe forma de enrutarlo 
            {
                if (data.Length == 96 &&     //En caso de que sea un host unreachable no puede continuar
                    data.Substring(72, 8) == "00000001" &&  //porque formaría un ciclo infinito
                    data.Substring(88, 8) == "00000011")
                    return;
                else
                {
                    this.DHU_Response(info); //En otro caso desechamos la información
                    return;
                }
            }

            //En caso de que sea 



            string receptor_mac; //Desigamos una variable que nos dirá si conocemos
                                 //la mac de este ip

            //Intentamos obtener su valor
            bool exist = this.ip_mac.TryGetValue(new_receptor_ip, out receptor_mac);

            if (exist) //Si lo tenemos, enviamos la información
            {
                info = receptor_mac + info.Substring(16);
                for(int i = 0; i < info.Length; i++)
                {
                    this.sending_data[port].Enqueue(info[i].ToString());
                }
            }
            else      //Si no lo tenemos, debemos averiguar la mac de ese ip
                this.info_waiting_arp[port].Enqueue(info);
        }

        //Método para analizar a qué dispositivo se debe enviar la información a partir
        //de las tablas de rutas, devuelve el ip receptor y el puerto
        public Tuple<string, int> Route_Selector(string receptor_ip)
        {
            for (int i = 0; i < this.routes.Count(); i++) //Iteramos por todas las rutas
            {
                Route route = this.routes[i]; //Guardamos la ruta actual

                //Hacemos AND entre el Ip destino y la máscara de la ruta
                string ip_result = Transformation_Codes.AND(receptor_ip, route.mask);

                if (ip_result == route.destination) //Si coincide con destination
                {
                    //En caso de que el gateway será 0.0.0.0, entonces el dispositivo está
                    //en nuestra subred, por lo que devolvemos su ip
                    if (route.gateway == "00000000000000000000000000000000")
                        return new Tuple<string, int>(receptor_ip, route.interface_port);
                    else                        //En otro caso, devolvemos el ip del dispositivo
                        return new Tuple<string, int>(route.gateway, route.interface_port);  //al que enviaremos la información
                }
            }

            return new Tuple<string, int>("", -1); //Si ninguna ruta coincide, devolvemos un ip vacío
        }

        //Método que prepara el dispositivo para enviar el ARP Response
        public void ARP_Response(string data, int pos)
        {
            int data_length = Transformation_Codes.Binary_to_Decimal(data.Substring(32, 8));
            string packet = data.Substring(48, data_length * 8); //Multiplicamos porque el tamaño está
                                                                 //expresado en bytes

            string sender_mac = data.Substring(16, 16);
            string length = "00001000";   //Designamos que tenemos 8 bytes de datos


            string new_data = "01100001011100100111000001110010"; //Los datos son ARPQ en binario
            new_data += packet.Substring(32, 32); //Le sumamos a los datos el ip del host receptor

            Data_Verification temp = new Data_Verification(); //Instanciamos el diccionario con los métodos
            temp.Set_Defaul_Values();                         //de cifrado y colocamos sus valores por default

            string verificator = temp.Evaluate(new_data, this.error_detection); //Obtenemos el cifrado

            //Calculamos su tamaño (que será un múltiplo de 8, rellenando con ceros, puesto que hablamos de bytes)
            int verificator_length = verificator.Length / 8;
            verificator_length += verificator.Length % 8 == 0 ? 0 : 1;

            verificator = Transformation_Codes.Add_Zero(verificator, verificator_length * 8);
            string verificator_length_string = Transformation_Codes.Add_Zero(
                                               Transformation_Codes.Decimal_to_Binary(verificator_length), 8);


            //Construimos la trama
            string arp_response = sender_mac + this.mac_list[pos] +
                                  length + verificator_length_string
                                  + new_data + verificator;

            //La colocamos para ser enviada
            for(int i = 0; i < arp_response.Length; i++)
            {
                this.sending_data[pos].Enqueue(arp_response[i].ToString());
            }
        }

        //Método que prepara el dispositivo para enviar el ARP Quest
        public void ARP_Quest(int pos)
        {   //Recibe la posición de la información a la que se le hará el ARP Quest

            string info = this.info_waiting_arp[pos].Peek();

            /*
             askii:
             a - 97  = 01100001 
             r - 114 = 01110010
             p - 112 = 01110000
             q - 113 = 01110001
             */

            string receptor_mac = "1111111111111111"; //Utilizamos la dirección de broadcast
            string sender_mac = this.mac_list[pos]; //Colocamos el mac del sender
            string length = "00001000";   //Designamos que tenemos 8 bytes de datos
            string data = "01100001011100100111000001110001"; //Los datos son ARPQ en binario

            //Ahora debemos hallar el ip del host que recibirá la información

            int info_length = Transformation_Codes.Binary_to_Decimal(info.Substring(32, 8)) * 8;
            string receptor_ip = info.Substring(48, info_length).Substring(0, 32); //Extraemos el ip

            Tuple<string, int> selector = this.Route_Selector(receptor_ip);

            string new_receptor_ip = selector.Item1;

            data += new_receptor_ip; //Le sumamos a los datos el ip del host receptor

            //Ahora debemos construir el verificador de error

            Data_Verification temp = new Data_Verification(); //Instanciamos el diccionario con los métodos
            temp.Set_Defaul_Values();                         //de cifrado y colocamos sus valores por default

            string verificator = temp.Evaluate(data, this.error_detection); //Obtenemos el cifrado

            //Calculamos su tamaño (que será un múltiplo de 8, rellenando con ceros, puesto que hablamos de bytes)
            int verificator_length = verificator.Length / 8;
            verificator_length += verificator.Length % 8 == 0 ? 0 : 1;


            verificator = Transformation_Codes.Add_Zero(verificator, verificator_length * 8);
            string verificator_length_string = Transformation_Codes.Add_Zero(
                                               Transformation_Codes.Decimal_to_Binary(verificator_length), 8);


            string arp_quest = receptor_mac + sender_mac + length +
                               verificator_length_string + data + verificator;
            
            for(int i = 0; i < arp_quest.Length; i++)
            {
                this.sending_data[pos].Enqueue(arp_quest[i].ToString());
            }

            this.wait_time_arp[pos] = info.Length * 8; //Designamos el tiempo de espera para el arp
        }

        //Método para responderle a un dispositivo un destination host unreachable
        public void DHU_Response(string info)
        {
            int data_length = Transformation_Codes.Binary_to_Decimal(info.Substring(32, 8)) * 8;
            string data = info.Substring(48, data_length);

            string sender_ip = data.Substring(32, 32);
            string new_data_length = "00001100";

            //Primera analizamos las rutas disponibles y seleccionamos un ip receptor
            //a partir del ip en los parámetros de entrada
            Tuple<string, int> selector = this.Route_Selector(sender_ip);

            int port = selector.Item2;

            if (port == -1)
                return;


            string new_data = sender_ip + this.ip_list[port] + "00000000" +
                               "00000001" + "00000001" + "00000011";

            //Ahora debemos crear el verificador de error

            Data_Verification temp = new Data_Verification(); //Instanciamos el diccionario con los métodos
            temp.Set_Defaul_Values();                         //de cifrado y colocamos sus valores por default

            string verificator = temp.Evaluate(new_data, this.error_detection); //Obtenemos el cifrado

            //Calculamos su tamaño (que será un múltiplo de 8, rellenando con ceros, puesto que hablamos de bytes)
            int verificator_length = verificator.Length / 8;
            verificator_length += verificator.Length % 8 == 0 ? 0 : 1;

            verificator = Transformation_Codes.Add_Zero(verificator, verificator_length * 8);
            string verificator_length_string = Transformation_Codes.Add_Zero(
                                               Transformation_Codes.Decimal_to_Binary(verificator_length), 8);


            string dhu = "0000000000000000" + this.mac_list[port] +
                         new_data_length + verificator_length_string +
                         new_data + verificator;

            this.info_waiting_arp[port].Enqueue(dhu);
        }

        //Permite conectar un dispositivo en uno de los puertos del Switch
        public override int Connect(Device device, int port)
        {
            if (this.devices[port] == null) //En caso de que no haya algún dispositivo conectado
            {
                this.devices[port] = device; //Conectamos el dispositivo
                return 0; //Retornamos 0 para indicar que la operación fue satisfactoria
            }
            else //Si ya existe un dispositivo conectado a ese puerto
            {
                this.historial.Add("The port " + port + 1 + " has already connected"); //Añadimos al error al historial
                return -1; //Retornamos -1 para indicar que fue imposible la operación
            }
        }

        //Método para buscar el nombre de un dispositivo conectado en un puerto
        public override string Device_Search(int port)
        { //Recibe el puerto donde se quiere revisar el dispositivo

            if (this.devices[port] != null) //Si hay algo conectado
                return this.devices[port].name; //Devolvemos el nombre del dispositivo
            else
                return null; //En otro caso no devolvemos nada
        }

        //Método para buscar el puerto al que está conectado un dispositivo
        public override int Device_Search(string name)
        { //Recibe el nombre del dispositivo
            for (int i = 0; i < this.devices.Count(); i++)//Se itera por todos los puertos
            {
                if (this.devices[i] != null && this.devices[i].name == name) //Si hay algo conecctado y es el dispositivo
                {
                    return i; //Retornamos la posición del mismo
                }
            }
            return -1; //Si no existe retornamos -1
        }

        //Método para desconectar un dispositivo de uno de los puertos, mediante su nombre
        public int Disconnect(string name)
        {//Para ello recibe el nombre del dispositivo
            for (int i = 0; i < this.devices.Count(); i++) //BUscamos por todos los puertos
            {
                if (this.devices[i] != null && this.devices[i].name == name) //Si existe un dispositivo en ese puerto
                {                                                           //y es el que buscamos
                    this.devices[i] = null; //Desconectamos ese dispositivo
                    return 0; //Retornamos 0 para indicar que la operación fue satisfactoria
                }
            }
            //En caso de que le dispositivo no se encuentre en ninguno de los puertos, añadimos el historial el error
            this.historial.Add("There is no device with name " + name + " to disconnect");
            return -1; //Retornamos -1 para indicar que no fue posible realizar la operación
        }

        //Método para desconectar un dispositivo de uno de los puertos del HUB
        public override int Disconnect(int port)
        { //Recibe el puerto desde el que se quiere desconectar el dispositivo
            if (this.devices[port] != null) //Si en el puerto hay algo conectado
            {
                this.devices[port] = null; //Desconectamos el dispositivo
                return 0; //Retornamos 0 para indicar que la operación fue satisfactoria
            }
            else //En otro caso
            {
                this.historial.Add("The port " + port + 1 + " has no connected"); //Añadimos el error al historial
                return -1; //Retornamos -1 para indicar que ocurrió un error
            }
        }

        //Permite al dispositivo saber si alguno de los dispositivos conectados dejó de enviar información
        public override void Receiving_Verification()
        {
            for (int i = 0; i < this.devices.Count(); i++) //Vamos por todos los dispositivos
            { //Si no existe un dispositivo conectado a ese puerto o no está enviando información
                if (this.devices[i] == null || this.devices[i].already_sending == false)
                    this.receiving_data[i] = ""; //Eliminamos la información que habíamos recibido
            }
        }

        //Método para reiniciar el dispositivo
        public override string Restart()
        {
            //Reiniciamos los valores de todas las listas

            this.devices = new List<Device>();
            this.routes = new List<Route>();
            this.sending_data = new List<Queue<string>>();
            this.receiving_data = new List<string>();
            this.information = new List<string>();

            for (int i = 0; i < this.devices.Count(); i++)
            {
                this.devices.Add(null);
                this.sending_data.Add(new Queue<string>());
                this.receiving_data.Add("");
                this.information.Add("");
            }
            return null;
        }

        //Método para mostrar el historial del Dispositivo
        public override void Show_Historial(string directory)
        {//Este método recibe el directorio donde se quiere guardar un txt que tenga el historial de este dispositivo

            //Utilizamos la clase SteamWrite, la cual, mediante un directorio crea un archivo,
            StreamWriter writer = new StreamWriter(directory);

            for (int i = 0; i < this.historial.Count(); i++) //Iteramos por cada una de las posiciones del historial
            {
                writer.WriteLine(this.historial[i]); //Copiamos esa posición en el txt
            }

            writer.Dispose(); //Le damos formato al txt
        }

        //Método para mostrar la información de un dispositivo
        public override bool Show_Info(string name) //Recibe el nombre de un dispositivo
        {
            for (int i = 0; i < this.devices.Count(); i++) //Busca por todos los dispositivos conectados
            {
                //Si encontramos el dispositivo que coincida
                if (this.devices[i] != null && this.devices[i].name == name)
                    return this.information[i] != ""; //Retornamos la información de ese puerto
            }
            return false; //Si no existe ese dispositivo retornamos que no hay información
        }

        //Método para poder darle una mac a una de las interfaces del Router
        public void Actualizate_Mac(string mac, int port)
        {
            if (port >= 0 && port < this.mac_list.Count()) //En caso de que no tengamos mac, lo actualizamos
            {
                if(this.mac_list[port] == "")
                    this.mac_list[port] = mac; //Guardamos la mac en binario
            }
        }

        //Método para poder darle un ip y una mask a una interfaz del Router
        public void Actualizate_IP_Mask(string ip, string mask, int port)
        {
            if(port >= 0 && port < this.ip_list.Count())
            {
                if (this.ip_list[port] == "") //En caso de que no tengamos ip, lo actualizamos
                {
                    this.ip_list[port] = ip;
                    this.mask_list[port] = mask;
                }
            }
            
        }

        //Método que añade una ruta a la lista de rutas
        public void Add_Route(string destination, string mask, string gateway, int interface_port)
        {
            Route temp = new Route(destination, mask, gateway, interface_port); //Creamos un ruta con
                                                                                //estos elementos

            if (this.routes.Count() == 0 || //Si no hay elementos en la lista o temp es el de menor prioridad
                this.routes[this.routes.Count() - 1].priority >= temp.priority)
            {
                this.routes.Add(temp); //Lo añadimos al final de la lista
                return;
            }

            for (int i = 0; i < this.routes.Count(); i++) //Buscamos por la lista de rutas
            {
                if (temp.priority > this.routes[i].priority) //Lo colocamos ordenado en la lista
                {                                           //según el de mayor prioridad
                    this.routes.Insert(i, temp);
                    break;
                }
            }
        }

        //Método que elimina una ruta a la lista de rutas
        public void Delete_Route(string destination, string mask, string gateway, int interface_port)
        {
            for (int i = 0; i < this.routes.Count(); i++) //Buscamos por la lista de rutas
            {
                Route temp = this.routes[i]; //Colocamos la ruta actual en un temporal
                                             //para acceder más fácilmente a ella                       

                if (temp.destination == destination && //Si todos sus parámetros coinciden
                    temp.mask == mask &&
                    temp.gateway == gateway &&
                    temp.interface_port == interface_port)
                {
                    this.routes.RemoveAt(i); //La removemos de la lista
                    break;
                }

            }
        }


        #endregion


    }

    public class Route
    {
        #region Atributos y constructor
        public string destination { get; private set; }
        //Indica el campo destino de una ruta
        public string mask { get; private set; }
        //Indica la máscara que utilizará la ruta
        public string gateway { get; private set; }
        //Indica a qué dispositivo se le enviará la información
        public int interface_port { get; private set; }
        //Indica el puerto por el que será enviada la información
        public int priority { get; private set; }
        //Indica la cantidad de 1s en la máscara, que será la prioridad de la misma

        public Route(string destination, string mask, string gateway, int interface_port)
        {
            this.destination = destination;
            this.mask = mask;
            this.gateway = gateway;
            this.interface_port = interface_port;
            this.priority = Transformation_Codes.Number_of_One(mask);
        }
        #endregion
    }
}
