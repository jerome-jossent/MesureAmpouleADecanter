/*
  Program for controlling an unknown Contact Image Sensor (CIS)
  with Ardino UNO and processing

  (c) Heli Tejedor, http://heli.xbot.es
 
  This Software is distributed under license
  Creative Commons 3.0 Attribution , Non Commercial, Share-alike  (CC BY-NC-SA 3.0)
  http://creativecommons.org/licenses/by-nc-sa/3.0/deed.es_ES
  http://creativecommons.org/licenses/by-nc-sa/3.0/
*/

/*
Connection to Arduino UNO:
Arduino   CIS Module DL100-05EUJK
 A0        (1) Analog Out 0-2V
 GND       (2) GND 
 +5V       (3) +5V
 A2        (4) SP
 A1        (5) CLK
 GND       (6) K LED     
 +5V       (7) A LED (Led probably 12V or 24V)        
*/

#define AnalogPin A0
#define ClkPin    A1
#define SpPin     A2
#define LedRPin   A3
#define LedGPin   A4
#define LedBPin   A5

  // Default configuration, can be overriden by serial commands
int Pixels = 1729;  // 1728 dots, 1720 usable (DL100-05EUJK)
int Tail = 0;       // Clocks after actual read
int Fast = 1;       // 1 = normal, >1 FASTDAC
bool LED_ON = LOW;  // Common K leds
bool Sp_ON = HIGH;  // Logic SP signal            
bool Clk_ON = LOW;  // Logic CLK singnal      

  // Magic string 
#define VERSION F("CIS Test, (c) Heli Tejedor, V0.1 Noviembre 2017")

// Used to setting and clearing register bits
#ifndef cbi
#define cbi(sfr, bit) (_SFR_BYTE(sfr) &= ~_BV(bit))
#endif
#ifndef sbi
#define sbi(sfr, bit) (_SFR_BYTE(sfr) |= _BV(bit))
#endif

// ===========================================================================
void SetFastADC() 
{
    // Set prescaler to 16, improve DAC speed by 10
  sbi(ADCSRA,ADPS2);
  cbi(ADCSRA,ADPS1);
  cbi(ADCSRA,ADPS0);
}

// ===========================================================================
bool ReadONorOFF (void)
{
  while (!Serial.available());
  if (Serial.read() == '1') return LED_ON;     
  return !LED_ON;
}

// ===========================================================================
void SetONorOFF (int Pin, bool Status)
{
  if (Status == LED_ON) pinMode(Pin, OUTPUT); 
  else pinMode(Pin, INPUT);    // Prevents undesired returns
  digitalWrite(Pin, Status);  
}

// ===========================================================================
void ResetLEDS ()
{
  SetONorOFF(LedRPin, !LED_ON);   // Reset Leds 
  SetONorOFF(LedGPin, !LED_ON);  
  SetONorOFF(LedBPin, !LED_ON);   
}

// ===========================================================================
void setup() 
{
  Serial.begin( 921600 );

  pinMode(AnalogPin, INPUT);
  analogReference(DEFAULT);         // EXTERNAL or INTERNAL (1.1V atmega328p)
  if (Fast > 1) SetFastADC();
  
  pinMode(ClkPin, OUTPUT);
  pinMode(SpPin,  OUTPUT);
//  pinMode(LedRPin,OUTPUT);        // Do by ResetLEDS(); 
//  pinMode(LedGPin,OUTPUT);
//  pinMode(LedBPin,OUTPUT);

  digitalWrite(ClkPin, !Clk_ON); 
  digitalWrite(SpPin,  !Sp_ON);
//  digitalWrite(LedRPin,!LED_ON);  // Do by ResetLEDS(); 
//  digitalWrite(LedGPin,!LED_ON);
//  digitalWrite(LedBPin,!LED_ON);
   ResetLEDS();                     // Init pins and default values
}

// ===========================================================================
void loop() 
{
  if (Serial.available() > 0) 
  {
    switch (Serial.read())
    {      
      case 'v': Serial.println(VERSION); break;              // Send version
      case 'p': Pixels = Serial.parseInt(); break;           // Receive Pixels
      case 't': Tail = Serial.parseInt(); break;             // Receive tail clocks   
      case 'f': Fast = Serial.parseInt();                    // Receive ADC speed
                if (Fast > 1) SetFastADC(); break;      
      case 'l': LED_ON = ReadONorOFF(); break;               // Leds ON logic state
      case 'r': SetONorOFF(LedRPin, ReadONorOFF()); break;   // Leds ON or OFF
      case 'g': SetONorOFF(LedGPin, ReadONorOFF()); break;
      case 'b': SetONorOFF(LedBPin, ReadONorOFF()); break;
      case 'd': SendLine(); break;
      case ' ': ResetLEDS();                                 // Reset all LED
                break; 
    }  
  }
}

// ===========================================================================
void SendLine(void)
{        
  digitalWrite(SpPin,Sp_ON);            // Start pulse ~13us 
  delayMicroseconds(1);
  digitalWrite(ClkPin,!Clk_ON);         // Clock pulse 5us = 200Khz 
  delayMicroseconds(1);
  digitalWrite(ClkPin,Clk_ON);   
  delayMicroseconds(1);
  digitalWrite(SpPin,!Sp_ON);           
  
    // If Fast = 1 25us = 40Khz
  for(int cnt=0; cnt<Pixels; cnt++)     // Loop take 120us = 8,3Khz 
  {     
         
    delayMicroseconds(Fast);            // Delay...   
    word val = analogRead(AnalogPin);   // Takes 100us
    digitalWrite(ClkPin,!Clk_ON);       // Clock pulse 5us = 200Khz 
 //   delayMicroseconds(1);    
    digitalWrite(ClkPin,Clk_ON);  
              
    Serial.write((byte)(val>>2));       // Send high 8 bits
    if (Serial.available() > 0) 
    {
      if (Serial.read() == ' ')         // Cancel command 
      {
        ResetLEDS();
        break;  
      }
    }      
  }
  for(int cnt=0; cnt<Tail; cnt++)
  {    
    digitalWrite(ClkPin,Clk_ON);
    delayMicroseconds(1);              
    digitalWrite(ClkPin,!Clk_ON);
    delayMicroseconds(1);
  }
}
