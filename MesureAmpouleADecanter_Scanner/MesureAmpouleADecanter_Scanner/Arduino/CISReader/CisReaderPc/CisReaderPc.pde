/*
  Program for controlling an unknown Contact Image Sensor (CIS)
 (c) Heli Tejedor Noviembre 2017, helitp@gmail.com http://heli.xbot.es
  
  This Software is distributed under license
  Creative Commons 3.0 Attribution , Non Commercial, Share-alike  (CC BY-NC-SA 3.0)
  http://creativecommons.org/licenses/by-nc-sa/3.0/deed.es_ES
  http://creativecommons.org/licenses/by-nc-sa/3.0/
*/

import javax.swing.JOptionPane;           
import static javax.swing.JOptionPane.*;  // Dialog Messages ERROR_MESSAGE etc
//import processing.awt.PSurfaceAWT;

String Title = "CIS Test";

String SerialPortName = "COM7";           // Actual serial port                           
int ZeroLine;
int CurrentLine;
int MaxLevel=0, MinLevel=255;
char CalibrateColor = ' ';               // Calibrate Color 'r', 'g', 'b' or ' ' none
  
/*
// DL100-05EUJK parameters
final int WhiteLevel = 145;              // White level ~ 2,8V = 143 from 255 
final int BlackLevel = 5;                // Offset (black level)
final int NPixels = 1729;                // 1728 usable pixels
final int NTail = 10;                    // 
final int Speed = 1;                     //  1 = slow, 2 to 255 fastest to less fast 
final int Offset = 0;                    // Start data window from pixel...
final boolean MonocromeMode = true;      // Mono
*/
// LED probably works at 12V or 24V, because with 5V show low bright (~80 of 255)
// CIS     ARDUINO
// 1: OUT  (A0)
// 2: GND  (GND)
// 3: VCC  (+5V)
// 4: SP   (A2)
// 5: CLK  (A1)
// 6: K LED
// 7: A LED 

/*
// DL100-10AFJK parameters
final int WhiteLevel = 255;              // White level ~ 2,8V = 143 from 255 
final int BlackLevel = 110;              // Offset (black level)
final int NPixels = 1729;                // 1728 usable pixels
final int NTail = 10;                    // 
final int Speed = 1;                     //  1 = slow, 2 to 255 fastest to less fast 
final int Offset = 0;                    // Start data window from pixel...
final boolean MonocromeMode = true;      // Mono
*/
// LED probably works at 12V or 24V, because with 5V show low bright 
// CIS     ARDUINO
// 1: OUT  (A0)
// 2: GND  (GND)
// 3: VCC  (+5V)
// 4: GND  (GND)
// 5: GND  (GND)
// 6: SP   (A2)
// 7: GND  (GND)
// 8: CLK  (A1)
// 9: K LED (internal 470 Ohmios)
// 10:A LED 

/*
// SS08012C parameters
final int WhiteLevel = 247;              // White level ~ 5V = 246 from 255 
final int BlackLevel = 0;                // Offset (black level)
final int NPixels = 1741;                // 1740 pixels minus 12 dummy
final int NTail = 10;                    // 
final int Speed = 1;                     //  1 = slow, 2 to 255 fastest to less fast 
final int Offset = 0;                    // Start data window from pixel...
final boolean MonocromeMode = true;      // Mono
// LED probably works at 24V
*/

// SS30009B parameters
final int WhiteLevel = 212;              // White level ~ 4.5V = 212 from 255 
final int BlackLevel = 40;               // Offset (black level)
final int NPixels = 2581;                // 2581 pixels minus  dummy (copy this value in size() function. line 207!!!   
final int NTail = 1;                     // 
final int Speed = 10;                    //  1 = slow, 2 to 255 fastest to less fast 
final int Offset = 500;                  // Start data window from pixel...
final boolean MonocromeMode = false;     // Color

// LED probably works at 5V low current R > 1K!
// CIS     ARDUINO
// 1: A LED 
// 2: K LED B   (A5)
// 3: K LED G   (A4)
// 4: K LED R   (A3)
// 5: VCC    (+3.3V)
// 6: CLK       (A1)
// 7: SP        (A2) 
// 8: GND      (GND)
// 9: GND      (GND) 
// 9: OUT       (A0) 

/*
// CANNON CNE-60216B-000 (also marked with 72512 and barcode 57200817F) parameters
final int WhiteLevel = 220;              // White level ~ 4.6V = 222 from 255 
final int BlackLevel = 88;               // Offset (black level)
final int NPixels = 2601;                // 2600 usable pixels
final int NTail = 5;                     // 
final int Speed = 5;                     //  1 = slow, 2 to 255 fastest to less fast 
final int Offset = 0;                    // Start data window from pixel...
final boolean MonocromeMode = false;      // Mono
*/
// Single LEDs, resistors deeded!!!
// CIS     ARDUINO
// 1: K LED IR? (NC)
// 2: K LED R   (A3)
// 3: K LED G   (A4)
// 4: K LED B   (A5)
// 5: A LED     (+3.3V)
// 6: CLK       (A1)
// 7: SP        (A2)
// 8: Ref?       NC
// 9: VCC       (+5V)
// 10: GND      (GND)
// 11: GND      (GND)
// 12: OUT      (A0)

// CSI CA 4B41-A (marked with "1200") parameters
/*
final int WhiteLevel = 255;              // White level ~ 4.6V = 222 from 255 
final int BlackLevel = 0;               // Offset (black level)
final int NPixels = 2601;                // 2600 usable pixels
final int NTail = 5;                     // 
final int Speed = 5;                     //  1 = slow, 2 to 255 fastest to less fast 
final boolean MonocromeMode = true;      // Mono
*/
// Single LEDs, resistors deeded!!!
// CIS     ARDUINO
// 1:  ?
// 2:  ?  
// 3:  GND      (GND)
// 4:  ?
// 5:  ? 
// 6:  ?
// 7:  GND      (GND)
// 8:  VCC       (+5V)
// 9:  K LED R   (A3)
// 10: K LED G   (A4)
// 11: K LED B   (A5)
// 12: A LED     (+3.3V)

//  CSI CANON HH7-2254 (Marked in PCB with 604616) from CANNON FAX-L300
/*
final int WhiteLevel = 208;              // White level ~ 4.6V = 222 from 255 
final int BlackLevel = 75;               // Offset (black level)
final int NPixels = 896;                // 895 usable pixels minus 26 dummy
final int NTail = 5;                     // 
final int Speed = 2;                     //  1 = slow, 2 to 255 fastest to less fast 
final int Offset = 0;                    // Start data window from pixel...
final boolean MonocromeMode = true;      // Mono
*/
// 12V LED with 220 Ohms resistor onboard
// CIS     ARDUINO
// 1:  OUT      (A0)
// 2:  GND      (GND)
// 3:  VCC       (+5V)
// 4:  Vref?     NC
// 5:  GND      (GND)
// 6:  CLK       (A1)
// 7:  GND      (GND)
// 8:  SP        (A2)
// 9:  K LED     (A3)
// 10: A LED     (+5V)

//  CSI Marked in PCB with E164671 S1
/*
final int WhiteLevel = 255;              // White level ~ 4.6V = 222 from 255 
final int BlackLevel = 0;               // Offset (black level)
final int NPixels =3101;                // 895 usable pixels minus 26 dummy
final int NTail = 5;                     // 
final int Speed = 1;                     //  1 = slow, 2 to 255 fastest to less fast 
final int Offset = 0;                    // Start data window from pixel...
final boolean MonocromeMode = true;      // Mono
*/
// 24V LED with 830 Ohms resistor onboard
// CIS     ARDUINO
// 1:  A LED     (+5V)
// 2:  K LED     (A3)
// 3:  CLK       (A1)
// 4:  GND      (GND)
// 5:  SP        (A2)
// 6:  NC        NC
// 7:  NC        NC
// 8:  VCC       (+5V)
// 9:  GND      (GND)
// 10: OUT      (A0)
// ===========================================================================
void setup() 
{
/*
  PSurfaceAWT awtSurface = (PSurfaceAWT)surface;
    PSurfaceAWT.SmoothCanvas smoothCanvas = (PSurfaceAWT.SmoothCanvas)awtSurface.getNative();
    smoothCanvas.getFrame().setAlwaysOnTop(true);
    smoothCanvas.getFrame().removeNotify();
    smoothCanvas.getFrame().setUndecorated(true);
    smoothCanvas.getFrame().setLocation(10, 10);
    smoothCanvas.getFrame().addNotify();  
*/
    // Tamaño de la ventana 1000 puntos x 512 alto (2*256)
  size(2581,600);   // Set x to NPixels!!!!   

  println ("Display WIDHT: " + displayWidth); 
  println ("Display HEIGHT: "+ displayHeight);
  CurrentLine = ZeroLine = 256;          // Levels windows is 256 pixels height
  
    // Init Pixel array
  loadPixels();    

  surface.setTitle(Title + " ( Conectando con " + SerialPortName + " )");     
//  surface.setResizable(true);

    // Abre la fuente de datos "COMxx" o "Test", retorna true si comunicacion real o false si TEST 
  OpenSerialPort();
  if (ModoTest)
    surface.setTitle(Title + " ( Modo TEST )");  
  else 
    surface.setTitle(Title + " ( Conectado en " + SerialPortName + " )");     
}

// Draw level bar Lenght 0-255 at pixel x
// ===========================================================================
void LevelCurrentLine(int x, int Lenght)
{
  int V;
    // Dibuja con negro
  for (V = 0; V < Lenght; V++) 
    pixels[x + ((ZeroLine-V)*width)] = color(0); //Negro
   // Rellena con blanco
  while (V < ZeroLine)
    pixels[x + ((ZeroLine-V++)*width)] = color(255);
}
  
// ===========================================================================
void draw() 
{  
  int WindowL = 0+Offset;
  int WindowH = width-Offset;
  if (WindowH > width) WindowH = width;
  
    // Request datas
  if (MonocromeMode) ReadLine('m');  
  else
  {    
    ReadLine('r');
    ReadLine('g');  
    ReadLine('b');    
  }
  
    // Pixels loop      
  for (int Pixel = 0; Pixel < WindowH; Pixel++) 
  {     
    int RawR, RawG, RawB, RawBlack;
    int Red, Green, Blue, Black;
    color c;    
    if (MonocromeMode) 
    {
      RawBlack = Values[Pixel+Offset][0]; 
      Black = int (map (RawBlack, BlackLevel, WhiteLevel, 0, 255));   
      if (Black > 255) { print ("OVERFLOW Black: "); println (RawBlack); Black = 255;}       
      c = color(Black); 
    }
    else 
    {
      RawR = Values[Pixel+Offset][1]; 
      RawG = Values[Pixel+Offset][0]; 
      RawB = Values[Pixel+Offset][2];       
    
      Red =   int (map (RawR, BlackLevel, WhiteLevel, 0, 255));
      Green = int (map (RawG, BlackLevel, WhiteLevel, 0, 255));
      Blue =  int (map (RawB, BlackLevel, WhiteLevel, 0, 255));
    
      if (Red > 255) { print ("OVERFLOW Red: "); println (RawR); Red = 255;}        
      if (Green > 255) { print ("OVERFLOW Green: "); println (RawG); Green = 255;}    
      if (Blue > 255) { print ("OVERFLOW Blue: "); println (RawB); Blue = 255;}          
        // Nivel de luminancia convertido de RGB a Y    
      Black = int (Red*0.2126 + Green*0.7152 + Blue*0.0722);
      c = color(Red, Green, Blue);       
    }
    if (CalibrateColor != ' ' )  // TEST !!
    {
      MaxLevel = max(Black, MaxLevel);
      MinLevel = min(Black, MinLevel);    
      println ("Max: " + MaxLevel);
      println ("Min: " + MinLevel);    
    }     // END TEST

//    println (Black);
    LevelCurrentLine (Pixel, Black);       
    pixels[Pixel + (CurrentLine*width)] = c;
  }
     // One line every frame
  if (++CurrentLine >= height) CurrentLine = ZeroLine;  
    // Update imagen
  updatePixels(); 
//  delay (10);  
}


// ===========================================================================
void exit()
{
   println ("Closing!");   
   if (!ModoTest) serialPort.write("  ");          // Stop pixel read and reset LEDS in Arduino
   delay(200);
   super.exit();   
}