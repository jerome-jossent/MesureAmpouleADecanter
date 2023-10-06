/*
sur d = 326.7 mm
avec T1000 et t1000 : on a une vitesse (montante = descendante) de 19.1 mm/sec
avec T1000 et t700 : on a une vitesse de 22.4 mm/sec


*/

// System caracteristics ============================================
float bar_mm_by_turn = 8.0f;

// Step motor ============================================
#define enPin 8         //Enable
#define stepYPin 4      //Pulse / Step
#define dirYPin 7       //Direction
#define y_limit_min 10  //End stop LOW
#define y_limit_max 11  //End stop HIGH

int motor_steps_by_turn = 200;  // <=> 1.8° par step
int motor_step_duration = 1000; //1000
int motor_step_pause = 1000;    //1000

// Coder manager ==================================================
#include "Thread.h" //ArduinoThread by Ivan Seidel
Thread coderThread = Thread();

#define pinA 2 // codeur fil noir
#define pinB 5 // codeur fil blanc
#define pinZ 3 // codeur fil orange
int coder_imp_by_turn = 1000;

long coder = 0 ;
long coder_last = 0 ;
long coder_updated = 0;

long coder_max;
long coder_min;

long coder_lastZ = 0 ;
bool coder_last_dir;
int delta_tour;
bool z_init = true;

// System Manager ==================================================
bool au;
bool scanmode = false;

// Serial exchange =================================================
const unsigned int MAX_MESSAGE_LENGTH = 12;
static char message[MAX_MESSAGE_LENGTH]; //incoming message

//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
void setup() {
  Serial.begin(115200);
  Serial.println("System Starting");
  
  //init STEP Motor
  pinMode(enPin, OUTPUT);
  digitalWrite(enPin, HIGH); //arrêt
  pinMode(stepYPin, OUTPUT);
  pinMode(dirYPin, OUTPUT);  
  pinMode(y_limit_min, INPUT_PULLUP);
  pinMode(y_limit_max, INPUT_PULLUP);
  
  au = false;
  Serial.println("STEP Motor OK");
  
  //get coder saved values
  //coder = Read_d(Add_coder);
  //PrintPosition();    
  //coder_max = Read_d(Add_codermax);
  //PrintD();
  //Serial.println("Memory OK");
  
  //init coder
  pinMode(pinA,INPUT_PULLUP);
  pinMode(pinB,INPUT_PULLUP);
  pinMode(pinZ,INPUT_PULLUP);
  attachInterrupt(digitalPinToInterrupt(pinA), changementA, RISING); //CHANGE
  attachInterrupt(digitalPinToInterrupt(pinZ), changementZ, RISING); //CHANGE
  Serial.println("Coder OK");

  coderThread.enabled = true; // Default enabled value is true
  coderThread.setInterval(10); // Setts the wanted interval to be 10ms
  coderThread.onRun(CoderCallback);
  Serial.println("Coder Thread OK");

//INFOS
  PrintInfos();

  Serial.println("System Initialized");
}

//Reset by code - call reset with : resetFunc();
void(* resetFunc) (void) = 0;//declare reset function at address 0

void loop() {
  if (coderThread.shouldRun()){
		coderThread.run();
  }
  
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

  //debug
  Serial.println(message);   
  
  //à déclarer en dehors du switch, sinon CASSE le switch !
  float val; 
  bool terminEnHaut;
  
  //reset arrêt d'urgence (sauf si AU demandé)
  if (message[0] != 'e')
      au = false;
  
  //i : info
  //c : calibration
  //e : emergency stop
  //y : PrintD
  //p : PrintPosition
  //d : go down 
  //u : go up  
  //D : go down MIN 
  //U : go up MAX
  //s : scan mode On/Off  
  //T : set temps motor_step_duration
  //t : set temps motor_step_pause
  //C : set coder_imp_by_turn
  //B : set bar_mm_by_tr
  //M : set motor_steps_by_tr

  switch (message[0]){
    case 'i':
      PrintInfos();
    break;
    
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
      //Save_d(Add_coder, coder);
      PrintPosition();
      break;
      
    case 'u':
      val = GetVal();
      if (val == 0) val = 1;
      if (val < 0)  val = -val;
      Serial.print("Go up asked : ");
      Serial.println(String(val));
      Deplacement(val);
      //Save_d(Add_coder, coder);
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

    case 'T':
      //set temps motor_step_duration
      val = GetVal();
      motor_step_duration = (int)val;
      Printmotor_step_duration();
      break;

    case 't':
      //set temps motor_step_pause
      val = GetVal();
      motor_step_pause = (int)val;
      Printmotor_step_pause();
      break;

    case 's':// EN TRAVAUX !!!
      //Scan Mode ON/OFF
      scanmode = !scanmode;
      if(scanmode){
        Serial.println("Scan mode : ON");  
        Scan(); 
      }else{
        Serial.println("Scan mode : OFF");   
      }
      break;

    case 'C':
      //set coder_imp_by_turn
      coder_imp_by_turn = (int)GetVal();
      Print_coder_imp_by_turn();
      break;

    case 'M':
      //set motor_steps_by_tr
      motor_steps_by_turn = (int)GetVal();
      Print_motor_steps_by_turn();
      break;

    case 'B':
      //set bar_mm_by_tr
      bar_mm_by_turn = GetVal();
      Print_bar_mm_by_turn();
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

float GetVal(){ // retourne en float la suite du message (retire le 1er caractère (code))
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
      
    //Save_d(Add_coder, coder);
  }
  PrintPosition();
}

void Deplacement_Max(){
  Serial.println("Up max asked");
  // on monte par pas de 10mm jusqu'au fin de course
  while (Deplacement(10) && !au) {}
  if (au) return;
  coder_max = coder;
  //Save_d(Add_coder, coder);
  //Save_d(Add_codermax, coder_max);
}

void Deplacement_Min(){
  Serial.println("Down min asked");
  // on descend par pas de 10mm jusqu'au fin de course
  while (Deplacement(-10) && !au) {}
  if (au) return;
  coder = 0;
  coder_min = coder;
  //Save_d(Add_coder, coder);
}

void Etalonnage(bool termineExtremiteHaute){
  Serial.println("Calibration start");
  int ta, tz;
  if (termineExtremiteHaute)
  {  
    Deplacement_Min();
    if (au) return;
    ta = millis();
    Deplacement_Max();
    if (au) return;
    tz = millis();
  } 
  else   {    
    Deplacement_Max();
    ta = millis();
    if (au) return;    
    Deplacement_Min();
    if (au) return;
    tz = millis();
  }
  
  coder_max = coder_max - coder_min;
  coder_min = 0;

  //Save_d(Add_codermax, coder_max);
  PrintD();
  PrintSpeed(tz - ta);
  
  Serial.print("coder max : ");
  Serial.println(coder_max);

  Serial.println("Calibration end");
}

void Scan(){
//  scanmode
//DEV EN COURS
}

bool Deplacement(float d_rel_mm){
  int steps;
  digitalWrite(enPin, LOW);

  float d_step_in_mm = bar_mm_by_turn / motor_steps_by_turn;

  if (d_rel_mm > 0){
    //on monte
    digitalWrite(dirYPin, HIGH);
    steps = d_rel_mm / d_step_in_mm;

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
    steps = - d_rel_mm / d_step_in_mm;

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
  delayMicroseconds(motor_step_duration);
  digitalWrite(stepYPin, LOW);
  delayMicroseconds(motor_step_pause); 
}


void PrintInfos(){
  Serial.println("-----INFOS-----" ); 
  Print_bar_mm_by_turn();
  Print_coder_imp_by_turn();  
  Print_motor_steps_by_turn();  
  Printmotor_step_duration();
  Printmotor_step_pause();
  PrintD();
  PrintPosition();
  // Serial.println("\"Tx\" : set x motor_step_duration" );
  // Serial.println("\"tx\" : set x motor_step_pause" );
  Serial.println("---------------" );
}

void PrintPosition(){
  Serial.print("Position = ");
  Serial.print(Distance_mmFromCoderValue(coder));
  Serial.println(" mm");   
}

void PrintD(){
  Serial.print("D max = ");
  Serial.print(Distance_mmFromCoderValue(coder_max));
  Serial.println(" mm");  
}

void PrintSpeed(int t){
  float vitesse = Distance_mmFromCoderValue(coder_max) * 1000 / t;
  Serial.print("V = ");
  Serial.print(vitesse);
  Serial.println(" mm / sec");
}

void Printmotor_step_duration(){
  Serial.print("Motor step duration : ");
  Serial.print(String(motor_step_duration));
  Serial.println(" µs");  
}

void Printmotor_step_pause(){
  Serial.print("Motor step pause : ");
  Serial.print(String(motor_step_pause));
  Serial.println(" µs");  
}

void Print_coder_imp_by_turn(){
  Serial.print("Coder : ");
  Serial.print(String(coder_imp_by_turn));
  Serial.println(" imp/tr");
}

void Print_motor_steps_by_turn(){
  Serial.print("Motor : ");
  Serial.print(String(motor_steps_by_turn));
  Serial.println(" steps/tr");
}

void Print_bar_mm_by_turn(){
  Serial.print("Bar : ");
  Serial.print(String(bar_mm_by_turn));
  Serial.println(" mm/tr");  
}


float Distance_mmFromCoderValue(long coderval){
  //bar_mm_by_turn [float] = 2 mm / tour
  //coder_imp_by_turn [int] = 1000 i / tour
  return bar_mm_by_turn * coderval / coder_imp_by_turn;
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
        coder = coder_lastZ + coder_imp_by_turn;
      else
        coder = coder_lastZ - coder_imp_by_turn;            
    }
    
    coder_lastZ = coder;
    coder_last_dir = dirYPin;
  }
}
