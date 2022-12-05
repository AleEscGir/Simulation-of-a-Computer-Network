Para ejecutar el proyecto es necesario seguir las siguientes reglas.
Una vez compilado, puesto que el lenguaje es C#, se creará una carpeta Debug,
dentro se deben colocar:

-un txt de nombre script.txt, donde puede colocar las órdenes
que ejecutará el proyecto

-una carpeta de nombre output, donde serán guardados
todos los txt de cada dispositivo luego de la ejecución del programa

-de manera opcional, un txt de nombre config.txt, el mismo puede estar vacío, o, 
puede colocarle en sus líneas los parámetros de entrada del proyecto de la siguiente manera:
"<nombre_de_variable> <valor_de_variable>". Estos parámetros deben usar sus correspondientes
variables con nombres específicos y sus parámetros correctamente, en este caso
son signal_time y error_decection.

-Se pide por favor no utilizar como mac de una PC 00, puesto que este número
es utilizado internamente por el programa como una mac referica a cuando no ha sido
descubierta una mac a aprtir de su ip, antes de un ARP Quest.