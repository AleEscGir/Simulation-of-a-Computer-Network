using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Proyecto_de_Redes
{
    public static class Transformation_Codes
    {//Esta clase estática sirve para hacer transformaciones entre el lenguaje binario, decimal y hexadecimal

        //Método para transformar del lenguaje Binario al Decimal
        public static int Binary_to_Decimal(string binary)
        {

            int result = 0;
            int j = 0;

            for (int i = binary.Length - 1; i >= 0; i--)
            {
                result += int.Parse(binary[i].ToString()) * (int)Math.Pow(2, j);
                j++;
            }
            return result;
        }

        //Método para transformar del lenguaje Decimal al Binario
        public static string Decimal_to_Binary(int value)
        {
            if (value == 0)
                return "0";
            if (value == 1)
                return "1";

            return Decimal_to_Binary(value / 2) + (value % 2).ToString();
        }

        //Método para transformar del lenguaje Hexadecimal al Decimal
        public static int Hexadecimal_to_Decimal(string hexadecimal)
        {
            int result = 0;
            int j = 0;
            for (int i = hexadecimal.Length - 1; i >= 0; i--)
            {
                result += Hexadecimal_to_Decimal_One_Bit(hexadecimal[i].ToString()) * (int)Math.Pow(16, j);
                j++;
            }
            return result;
        }

        //Método para transformar del lenguaje Decimal al Hexadecimal
        public static string Decimal_to_Hexadecimal(int value)
        {
            if (value < 16)
                return Decimal_to_Hexadecimal_One_Bit(value);

            return Decimal_to_Hexadecimal(value / 16) + Decimal_to_Hexadecimal_One_Bit((value % 16));
        }

        //Método para transformar un bit del lenguaje Hexadecimal al Decimal
        private static int Hexadecimal_to_Decimal_One_Bit(string hexadecimal)
        {
            try
            {
                return int.Parse(hexadecimal);
            }
            catch
            {
                if (hexadecimal == "A")
                    return 10;
                if (hexadecimal == "B")
                    return 11;
                if (hexadecimal == "C")
                    return 12;
                if (hexadecimal == "D")
                    return 13;
                if (hexadecimal == "E")
                    return 14;
                if (hexadecimal == "F")
                    return 15;
            }

            return 0;
        }

        //Método para transformar un bit del lenguaje Decimal al Hexadecimal
        private static string Decimal_to_Hexadecimal_One_Bit(int value)
        {
            if (value < 10)
                return value.ToString();
            else
            {
                if (value == 10)
                    return "A";
                if (value == 11)
                    return "B";
                if (value == 12)
                    return "C";
                if (value == 13)
                    return "D";
                if (value == 14)
                    return "E";
                if (value == 15)
                    return "F";
            }

            return "0";
        }

        //Método para transformar del lenguaje Binario al Hexadecimal
        public static string Binary_to_Hexadecimal(string binary)
        {
            
            if (binary.Length % 4 != 0)
                binary = Add_Zero(binary, binary.Length / 4 + 1);

            string data = "";

            for(int i = 0; i < binary.Length; i+=4)
            {
                int value = Binary_to_Decimal(binary.Substring(i, 4));
                data += Decimal_to_Hexadecimal(value);
            }

            return data;
        }

        //Método para transformar del lenguaje Hexadecimal al Binario
        public static string Hexadecimal_to_Binary(string hexadecimal)
        {
            string data = "";

            for (int i = 0; i < hexadecimal.Length; i++)
            {
                int value = Hexadecimal_to_Decimal(hexadecimal[i].ToString());
                data += Add_Zero(Decimal_to_Binary(value), 4);
            }

            return data;
        }

        //Método para agregar ceros a la izquierda de un número
        public static string Add_Zero(string number, int length)
        { //Recibe el número, y un length que representa la cantidad de cifras
            //que debe tener al agregarle ceros

            length -= number.Length;

            for (int i = 0; i < length; i++)
            {
                number = "0" + number;
            }

            return number;
        }

        //Método para transformar un número de ip en un número binario de 32 bits
        public static string Ip_to_Binary(string ip)
        {
            string binary = "";

            string[] new_ip = ip.Split('.'); //Dividimos el ip según los .

            for (int i = 0; i < new_ip.Length; i++) //Vamos por cada uno de los números
            {                                  //y los añadimos transformados

                binary += Add_Zero(Decimal_to_Binary(int.Parse(new_ip[i])), 8);
            }

            return binary;
        }

        //Método para transformar un número binario de 32 bits en un ip
        public static string Binary_to_Ip(string binary)
        {
            string ip = "";

            ip += Binary_to_Decimal(binary.Substring(0, 8)) + ".";
            ip += Binary_to_Decimal(binary.Substring(8, 8)) + ".";
            ip += Binary_to_Decimal(binary.Substring(16, 8)) + ".";
            ip += Binary_to_Decimal(binary.Substring(24, 8));

            return ip;
        }

        //Método para comprobar si un número se encuentra en binario
        public static bool Is_Binary(string binary)
        {
            for(int i = 0; i < binary.Length; i++)
            {
                if (binary[i] != '0' && binary[i] != '1')
                    return false;
            }

            return true;
        }

        //Método que devuelve la operación AND entre dos números binarios
        public static string AND(string binary_1, string binary_2)
        {
            string and = ""; //Aquí guardaremos el resultado

            if (binary_1.Length != binary_2.Length) //En caso de que no coincidan en tamaño,
                return and;                         //la operación devuelve la cadena vacía

            for(int i = 0; i < binary_1.Length; i++) //Iteramos por toda la cadena
            {
                if (binary_1[i] == '1' && binary_2[i] == '1') //Ejecutamos AND
                    and += "1";
                else
                    and += "0";
            }

            return and; //Retornamos
        }

        //Método que devuelve la cantidad de 1 en un número binario
        public static int Number_of_One(string binary)
        {
            int cont = 0; //Inicializamos el contador en 0

            for(int i = 0; i < binary.Length; i++) //Iteramos por toda la cadena
            {
                if (binary[i] == '1') //Cada vez que econtramos un 1 aumentamos el contador
                    cont++;
            }

            return cont; //Retornamos el contador
        }
    }
}
