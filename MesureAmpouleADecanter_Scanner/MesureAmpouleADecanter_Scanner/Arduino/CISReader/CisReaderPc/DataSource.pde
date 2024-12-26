/*
 Programa de visualizacion para CIS en tiempo real, extractado del
 programa de Registro y Visualizacion en tiempo real
 (c) Heli Tejedor Noviembre 2017 helitp@gmail.com http://heli.xbot.es
*/

import processing.serial.*;

Serial serialPort = null;      
boolean ModoTest = true;                // false = puerto serie real, true = generar datos para TEST

int Interval = 20;  

// =================================================================== 
// Intenta abrir el puerto serie para leer datos 
// si no puede pasa a modo TEST y genera datos simulados  
// =================================================================== 
void OpenSerialPort() 
{
  int Choice=0; 
  ModoTest=true;
  
  while (true)
  {
      // If data source NOT Test...
    if (!SerialPortName.equals("Test")) 
    {
        // Bucle infinito hasta que se abre un puerto o TEST      
      while (true)
      {
        // Starts serial communication
        try 
        {
          serialPort = new Serial(this, SerialPortName, 921600);   
          break;        // Exit from first while
        }
        catch (Exception e)
        {
      //     showMessageDialog (null, "Imposible abrir el puerto " + SerialPortName + "\nUsando datos de TEST","Error", ERROR_MESSAGE);
          Object[] options = {"Modo TEST", "Cambiar Puerto"};     
          Choice = JOptionPane.showOptionDialog(frame,
                   "Imposible abrir el puerto " + SerialPortName, "Error!",
                   JOptionPane.PLAIN_MESSAGE,
                   JOptionPane.QUESTION_MESSAGE,
                   null,
                   options,
                   options[0]);       
            // Candelado o cerrado      
          if (Choice==0 | Choice==-1) { SerialPortName = "Test"; return; }   // Return to test mode
            // Seleccionado cambiar puerto
          if (Choice==1) { SerialPortName = SelectSerialPort(); }            // New port, attempt to use it
        } 
      }     
        // Si pudo abrir puerto serie fuente de datos real
      if (serialPort != null) 
      {
        serialPort.clear();      // Flush puerto serie
          // Espera dos antes de continuar a que ARDUINO inicialice
        delay (2000);   
        println ("Configuring...");         
          // Envia la configuaracion a la placa de adquisición
        if (ConfigBoard() == 1) 
        {     
          println ("Configured!");
       //   SaveJsonItem("SerialPort", SerialPortName);     // Salva el nuevo puerto en el fichero de configuracion  
          ModoTest = false;                               // Using real datas       
          break;                                          // Exit from loop
        }
      }
    }
    else return;
  }
}

// =================================================================== 
String SelectSerialPort() 
{
  String COMx, COMlist = "";

  try 
  {
    int i = Serial.list().length;
    if (i != 0) 
    {
      for (int j = 0; j < i;) 
      {
        COMlist += char(j+'a') + " = " + Serial.list()[j];
        if (++j < i) COMlist += ",  ";
      }
      COMx = showInputDialog("Seleccionar el puerto ARDUINO (a,b,..):\n"+COMlist);
      if (COMx == null) return null;
      else if (COMx.isEmpty()) return null;
      else i = int(COMx.toLowerCase().charAt(0) - 'a') + 1;

      String portName = Serial.list()[i-1];
      return portName;
    }
    else 
      showMessageDialog(frame,"No hay puertos serie disponibles");   
  }
  catch (Exception e)
  { 
    showMessageDialog(frame,"Puerto COM no dsponible, puede estar en uso por otro programa");
 //   println("Error:", e);
  }
  return "";
}

// =================================================================== 
int ConfigBoard() 
{
   byte inBuffer[] = new byte[NPixels];   

   try 
    {
      serialPort.clear();      // Flush serial port       
      serialPort.write ("v");  // Ask for version  (magic string)
      delay(100);
      
      if (serialPort.available() > 0)               // Response?
      {
        serialPort.readBytesUntil('\r', inBuffer);  // Read from serial port until carriage return
        String Str = new String (inBuffer);    
        println ("Received magic string: "+ Str);
        String[] Msgs = split(Str, ',');           // Split message        
        if (Msgs[0].equals("CIS Test") & Msgs[1].equals(" (c) Heli Tejedor"));
        else 
        {
          serialPort.stop();
          return 0;
        }
      }
      else return 0;
    }
    catch (Exception e) 
    {
       showMessageDialog (null, "Falló ConfigBoard en puerto " + SerialPortName + "\nImposible continuar","Error", ERROR_MESSAGE);
       exit(); // From draw() system.exit() could be problematic
    }   
    
   serialPort.write ("p" + NPixels + "\r\n");    // Set line width in Arduino
   serialPort.write ("t" + NTail + "\r\n");      // Set clocks after line read in Arduino
   serialPort.write ("f" + Speed + "\r\n");      // Set ADC speed and delay in Arduino   
   serialPort.clear();      // Flush serial port   
   return 1;
}
 
int Values[][] = new int[NPixels][3]; // R, G, B  

int EllapsedMillis=0;
long lastTime = millis();
// =================================================================== 
void ReadLine (char Channel)
{
  int Idx = 0; 
  if (Channel == 'r') Idx = 0;
  if (Channel == 'g') Idx = 1;
  if (Channel == 'b') Idx = 2; 
    // if channel == 'm' monocrome mode, copy R channel to G & B
    // Calibrate channel set mono mode in R, G or B channel
  if (CalibrateColor != ' ') Channel = 'm';
  if (ModoTest)
  { 
    int Mill = millis();
      // Solo genera dato según el periodo configurado
    if (Mill - EllapsedMillis > Interval)
    {      
      EllapsedMillis = Mill;
       
      for (int i = 0; i<NPixels; i++) 
      {
        /* Genera valores aleatorios entre 15 y 0 con saltos de 1,5 máximo */
        Values[i][Idx] += random(-WhiteLevel/10, WhiteLevel/10);
        if (Values[i][Idx]>WhiteLevel) Values[i][Idx] = WhiteLevel;
        if (Values[i][Idx]<BlackLevel) Values[i][Idx] = BlackLevel;       
      }
    }
    delay(Interval/10); // Ajustar de acuerdo con la velocidad de refresco necesitada para descargar consumo CPU
  }
  else  // NO datos de TEST: hay puerto serie abierto, leer los datos 
  {
    try 
    {
      serialPort.clear();    
      if (Channel == 'm') serialPort.write(CalibrateColor + "1");  // Red LED ON
      else serialPort.write(Channel+"1");          // Channel LED ON
      delay(10);                                   // LED warm up time
      serialPort.write("d");
      
      for (int Pixel = 0; Pixel < width; Pixel++) 
      {       
        int Data;        
        int Mill = millis();
        do
        {
          if (millis()-Mill > 100) return;           // Frame sync fault 
          Data = serialPort.read();
        } while (Data == -1);
        Values[Pixel][Idx] = Data;  
        if (Channel == 'm') 
        {
          Values[Pixel][1] = Data;  
          Values[Pixel][2] = Data;           
        }
      }
      if (Channel == 'm') serialPort.write(CalibrateColor + "0");  // Red LED OFF
      else serialPort.write(Channel+"0");          // Channel LED OFF
      //serialPort.write(" ");          // Optional abort pixel read in Arduino
      serialPort.clear();          
    }
    catch (Exception e) 
    {
       showMessageDialog (null, "Falló ReadLine en puerto " + SerialPortName + "\nImposible continuar","Error", ERROR_MESSAGE);
       exit(); // Salir del programa desde draw() system.exit() podría dar problemas
    }
  }
}