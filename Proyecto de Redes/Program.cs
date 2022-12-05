using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Proyecto_de_Redes
{
    class Program
    {
        static void Main(string[] args)
        {
            //Ejecución Principal del Programa
            string path = Environment.CurrentDirectory; //Este método devuelve la dirección
                                                        //desde donde se compila el archivo

            string script_path = System.IO.Path.Combine(path, "script.txt");   //Creamos un nuevo string que contendrá
                                                                               //la ubicación del txt a leer

            string config_path = System.IO.Path.Combine(path, "config.txt");   //Creamos un nuevo string que contendrá
                                                                               //la ubicación de los parámetros de entrada

            int signal_time = 10; //Predefinimos el intervalo de tiempo como 10 milisegundos
            string error_detection = "Sum_Codificator";


            //Ahora intentaremos encontrar el documento config y leerlo para guardar los parámetros de entrada
            try
            {
                StreamReader reader = new StreamReader(config_path); //Creamos un streamReader para leerlo

                string[] temp = reader.ReadLine().Split(' '); //Aquí guardaremos lo que vayamos leyendo del documento

                while (temp != null) //Mientras podamos leer del documento
                {
                    if (temp[0] == "error_detection")
                        error_detection = temp[1];

                    if (temp[0] == "signal_time")
                        signal_time = int.Parse(temp[1]);

                    temp = reader.ReadLine().Split(' ');
                }

            }
            catch
            {

            }


            Network network = new Network(signal_time, error_detection); //Instanciamos la clase Network
            network.Inicialize_Network(script_path); //Lo inicializamos con la dirección del txt
            network.Writer(System.IO.Path.Combine(path, "output", " "));
        }
    }
}
