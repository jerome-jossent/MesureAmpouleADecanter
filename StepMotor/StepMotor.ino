//STEP MOTOR ============================================
#define enPin 8         //enable
#define stepYPin 4      //Z.STEP
#define dirYPin 7       //Z.DIR
#define y_limit_min 10  //End stop LOW
#define y_limit_max 11  //End stop HIGH

const int stepsPerRev = 200; // <=> 1.8° par step
int pulseWidthMicros = 1000;  // microseconds //def 500
int millisBtwnSteps = 1000;

// 2 mm => 1 tour => 200 steps
// 2 / 200 = 0,01 mm/step
const float mm_p_tour = 2;
const float d_step_mm = 0.01f;

//Serial exchange =================================================
const unsigned int MAX_MESSAGE_LENGTH = 12;
static char message[MAX_MESSAGE_LENGTH]; //incoming message

//System Manager ==================================================
bool au;

//Memory Manager ==================================================
#include <AT24C256.h>
AT24C256 eeprom = AT24C256();
int Add_coder = 0;
int Add_codermax = 10;

union Float_Bytes{
  float f;
  byte by[4];
};

//Coder Manager ==================================================
#include "Thread.h"
Thread coderThread = Thread();

#define pinA 2
#define pinB 5
#define pinZ 3
long coder = 0 ;
long coder_last = 0 ;
long coder_updated = 0;

long coder_max;
long coder_min;

long coder_lastZ = 0 ;
bool coder_last_dir;
int delta_tour;
bool z_init = true;
int i_tour = 1000;

//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
void setup() {
  Serial.begin(9600);
  Serial.println(F("System Starting"));
  
  //init STEP Motor
  pinMode(enPin, OUTPUT);
  digitalWrite(enPin, HIGH); //arrêt
  pinMode(stepYPin, OUTPUT);
  pinMode(dirYPin, OUTPUT);  
  pinMode(y_limit_min, INPUT_PULLUP);
  pinMode(y_limit_max, INPUT_PULLUP);
  
  au = false;
  
  //get coder saved values
  coder = Read_d(Add_coder);
  PrintPosition();    
  coder_max = Read_d(Add_codermax);
  PrintD();
  
  //init coder
  pinMode(pinA,INPUT_PULLUP);
  pinMode(pinB,INPUT_PULLUP);
  pinMode(pinZ,INPUT_PULLUP);
  attachInterrupt(digitalPinToInterrupt(pinA), changementA, RISING); //CHANGE
  attachInterrupt(digitalPinToInterrupt(pinZ), changementZ, RISING); //CHANGE

	coderThread.onRun(CoderCallback);
	coderThread.setInterval(100);
  
  Serial.println(F("System Initialized"));
}

void loop() {
  if (coderThread.shouldRun())
		coderThread.run();

  InteractionManager();
  delay(20);
}

void CoderCallback(){
  if (coder != coder_last){
    Serial.println(String("Coder = ") + String(coder));
    coder_last = coder;
  }
}

void InteractionManager(){
  if (!SerialManager()) return;

  //Traitement du message reçu
  
  //à déclarer en dehors du switch, sinon CASSE le switch !
  float val; 
  bool terminEnHaut;
  
  //reset arrêt d'urgence (sauf si AU demandé)
  if (message[0] != 'e')
      au = false;
  
  switch (message[0]){
    case 'c':
      Serial.println("Calibration asked");        
      terminEnHaut = true;
      if(message[1] == '0') terminEnHaut = false;
      if(message[1] == '1') terminEnHaut = true;
      DemandeEtalonnage(terminEnHaut, 10);
      break;
      
    case 'e':
      Serial.println("Emergency Stop");
      au = true;
      digitalWrite(enPin, HIGH); //arrêt
      break;
      
    case 'y':   
      PrintD();
      break;
      
    case 'p':
      PrintPosition();
      break;
      
    case 'd':
      val = GetVal();
      if (val == 0) val = -1;
      if (val > 0)  val = -val;  
      Serial.print("Go down asked : ");
      Serial.println(String(val));
      Deplacement(val);
      Save_d(Add_coder, coder);
      PrintPosition();
      break;
      
    case 'u':
      val = GetVal();
      if (val == 0) val = 1;
      if (val < 0)  val = -val;
      Serial.print("Go up asked");
      Serial.println(String(val));
      Deplacement(val);
      Save_d(Add_coder, coder);
      PrintPosition();
      break; 

    case 'D':   
      Deplacement_Min();
      PrintPosition();
      break;
      
    case 'U':   
      Deplacement_Max();
      PrintPosition();
      PrintD();
      break;
      
    default:
      Serial.print("inconnu : ");
      Serial.println(message);      
  }
  Serial.println("Waiting");
}

bool SerialManager() {
  if (Serial.available()==0)
    return false;
    
  while (Serial.available() > 0)
  {
    static unsigned int message_pos = 0;
    
    // Read the next available byte in the serial receive buffer
    char inByte = Serial.read();
    
    // Message coming in (check not terminating character) and guard for over message size
    if (inByte != '\n' && (message_pos < MAX_MESSAGE_LENGTH - 1) )
    {
      // Add the incoming byte to our message
      message[message_pos] = inByte;
      message_pos ++;
    } else {// Full message received...
      // Add null character to string
      message[message_pos] = '\0';      
      // Reset for the next message
      message_pos = 0;
    }
  }
  return true;
}

float GetVal(){ // retourne en float la suite du message
  return String(message).substring(1).toFloat();
}

void DemandeEtalonnage(bool termineExtremiteHaute, float degagement){ 
  Etalonnage(termineExtremiteHaute);
  if (au) return;
  
  // dégagement du contact de x mm
  if (degagement != 0)
  {
    if (degagement < 0)
      degagement = -degagement;
    if (termineExtremiteHaute)
      Deplacement(-degagement);    
    else
      Deplacement(degagement);
      
    Save_d(Add_coder, coder);
  }
  PrintPosition();
}

void Deplacement_Max(){
  Serial.println("Up max asked");
  // on monte par pas de 10mm jusqu'au fin de course
  while (Deplacement(10) && !au) {}
  if (au) return;
  coder_max = coder;
  Save_d(Add_coder, coder);
  Save_d(Add_codermax, coder_max);
}

void Deplacement_Min(){
  Serial.println("Down min asked");
  // on descend par pas de 10mm jusqu'au fin de course
  while (Deplacement(-10) && !au) {}
  if (au) return;
  coder = 0;
  coder_min = coder;
  Save_d(Add_coder, coder);
}

void Etalonnage(bool termineExtremiteHaute){
  Serial.println("Calibration start");
  if (termineExtremiteHaute)
  {  
    Deplacement_Min();
    if (au) return;
    Deplacement_Max();
    if (au) return;         
  } else {
    
    Deplacement_Max();
    if (au) return;    
    Deplacement_Min();
    if (au) return;
  }
  
  coder_max = coder_max - coder_min;
  coder_min = 0;

  Save_d(Add_codermax, coder_max);
  PrintD();
  
  Serial.print("coder max : ");
  Serial.println(coder_max);

  Serial.println("Calibration end");
}

bool Deplacement(float d_rel_mm){
  int steps;
  digitalWrite(enPin, LOW);
      
  if (d_rel_mm > 0){
    //on monte
    digitalWrite(dirYPin, HIGH);
    steps = d_rel_mm / d_step_mm;

    for (int i = 0; i < steps; i++) {
      if (digitalRead(y_limit_max) == LOW) {
        Serial.println("Up max reached !");
        digitalWrite(enPin, HIGH); 
        return false;
      }
      Move1Step();
      if (au) {
        digitalWrite(enPin, HIGH); 
        return false;
        }
    }
  } else {
    //on descend
    digitalWrite(dirYPin, LOW);
    steps = - d_rel_mm / d_step_mm;

    for (int i = 0; i < steps; i++) {
      if (digitalRead(y_limit_min) == LOW) 
      {
        Serial.println("Down min reached !");
        digitalWrite(enPin, HIGH); 
        return false;
      }
      Move1Step();
      if (au) {
        digitalWrite(enPin, HIGH); 
        return false;
      }
    }
  }

  digitalWrite(enPin, HIGH); 
  return true;
}

void Move1Step(){
  InteractionManager();
  if (au) return;
  digitalWrite(stepYPin, HIGH);
  delayMicroseconds(pulseWidthMicros);
  digitalWrite(stepYPin, LOW);
  delayMicroseconds(millisBtwnSteps); 
}

void Save_d(int Add, float val){
  Float_Bytes D;
  D.f = val;
  eeprom.write(D.by[0], Add);
  eeprom.write(D.by[1], Add + 1);
  eeprom.write(D.by[2], Add + 2);
  eeprom.write(D.by[3], Add + 3);
}

float Read_d(int Add){
  Float_Bytes D;
  D.by[0] = eeprom.read(Add);
  D.by[1] = eeprom.read(Add + 1);
  D.by[2] = eeprom.read(Add + 2);
  D.by[3] = eeprom.read(Add + 3);
  return D.f;
}

void PrintPosition(){
  Serial.print("Position = ");
  Serial.print(Distance_mmFromCoderValue(coder));
  Serial.println("mm");   
}

void PrintD(){
  Serial.print("D max = ");
  Serial.print(Distance_mmFromCoderValue(coder_max));
  Serial.println("mm");  
}

float Distance_mmFromCoderValue(long coderval){
  //i_tour [int] = 1000 i / tour
  //mm_p_tour [float] = 2 mm / tour
  return mm_p_tour * coderval / i_tour;
}

//-----------------CODER-------------------
void changementA(){
  // Si B different de l'ancien état de A alors
  if (digitalRead(pinB))
    coder ++;
  else
    coder --;  
}

void changementZ(){
  if (z_init)
  {
    coder_lastZ = coder;
    coder_last_dir = dirYPin;
    z_init = false;
  } else {    
    if (coder_last_dir == dirYPin){
      delta_tour = coder - coder_lastZ;      
      if (delta_tour > 0)      
        coder = coder_lastZ + i_tour;
      else
        coder = coder_lastZ - i_tour;            
    }
    
    coder_lastZ = coder;
    coder_last_dir = dirYPin;
  }
}


































