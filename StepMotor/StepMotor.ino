//shield CNC Driver v3 ============================================
#define enPin 8         //enable
#define stepYPin 3      //Y.STEP
#define dirYPin 6       //Y.DIR
#define y_limit_min 10  //End stop LOW
#define y_limit_max 11  //End stop HIGH

const int stepsPerRev = 200; // <=> 1.8° par step
int pulseWidthMicros = 500;  // microseconds //def 100
int millisBtwnSteps = 1000;

// 2 mm => 1 tour => 200 steps
// 2 / 200 = 0,01 mm/step
const float d_step_mm = 0.01f;
float d;
float d_abs_mm;

//Serial exchange =================================================
const unsigned int MAX_MESSAGE_LENGTH = 12;
//Create a place to hold the incoming message
static char message[MAX_MESSAGE_LENGTH];

//System Manager ==================================================
enum Action {arret, etalonnage, STOP};
Action todo;
bool au;

//Memory Manager ==================================================
#include <AT24C256.h>
AT24C256 eeprom = AT24C256();
int Add_d = 0;
int Add_dmax = 10;

union Float_Bytes{
  float f;
  byte by[4];
};

void setup() {
  Serial.begin(9600);
  // init CNC shield
  pinMode(enPin, OUTPUT);
  digitalWrite(enPin, HIGH); //arrêt
  pinMode(stepYPin, OUTPUT);
  pinMode(dirYPin, OUTPUT);  
  pinMode(y_limit_min, INPUT_PULLUP);
  pinMode(y_limit_max, INPUT_PULLUP);
  
  todo = arret;
  au = false;
    
  d = Read_d(Add_d);
  PrintPosition();
    
  d_abs_mm = Read_d(Add_dmax);
  PrintD();
  
  Serial.println(F("CNC Shield Initialized"));
}

void loop() {  
  InteractionManager();
  
  switch(todo){
    case STOP:      
      delay(20);
      break;
    
    case arret:
      delay(20);
      break;

    case etalonnage:
      digitalWrite(enPin, LOW);
      DemandeEtalonnage(true, 10);
      //356.5 mm entre d_min et d_max
      digitalWrite(enPin, HIGH);
      todo = arret;
      break;
  }
}

void InteractionManager(){
  if(SerialManager()){
    CommandManager();
  }
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
    }else{// Full message received...
      // Add null character to string
      message[message_pos] = '\0';      
      // Reset for the next message
      message_pos = 0;
    }
  }
  return true;
}

void CommandManager(){
  float val; // à déclarer en dehors du switch !
  if (message[0] != 'a')
      au = false; 
  
  switch(message[0]){
    case 'e':
      Serial.println("Etalonnage demandé");
      todo = etalonnage;
      break;
      
    case 'a':
      Serial.println("ARRET D'URGENCE");
      au = true;
      digitalWrite(enPin, HIGH); //arrêt
      break;
      
    case 'd':   
      PrintD();
      break;
      
    case 'p':
      PrintPosition();
      break;
      
    case 'b':
      Serial.println("descente demandée");
      val = GetVal();
      if (val == 0) val = -1;
      if (val > 0)  val = -val;      
      digitalWrite(enPin, LOW);
      Deplacement(val);
      digitalWrite(enPin, HIGH);
      Save_d(Add_d, d);
      PrintPosition();
      break;
      
    case 'h':
      Serial.println("monté demandée");
      val = GetVal();
      if (val == 0) val = 1;
      if (val < 0)  val = -val;
      digitalWrite(enPin, LOW);
      Deplacement(val);
      digitalWrite(enPin, HIGH);
      Save_d(Add_d, d);
      PrintPosition();
      break; 

    case 'B':   
      digitalWrite(enPin, LOW);
      Deplacement_Min();
      digitalWrite(enPin, HIGH);
      PrintPosition();
      break;
      
    case 'H':   
      digitalWrite(enPin, LOW);
      Deplacement_Max();
      digitalWrite(enPin, HIGH);
      PrintPosition();
      PrintD();
      break;
  }
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
    Save_d(Add_d, d);
  }
  PrintPosition();
}

void Deplacement_Max(){
  Serial.println("position max demandée");
  // on remonte jusqu'à Y max  
  while(Deplacement(10) && !au){}
  Save_d(Add_d, d);
  d_abs_mm = d;
  Save_d(Add_dmax, d_abs_mm);
  if (au) return;
  Serial.println("position max atteinte");
}

void Deplacement_Min(){
  Serial.println("position min demandée");
  // on descend jusqu'à Y min par pas de 10mm
  while(Deplacement(-10) && !au){}
  d = 0;
  Save_d(Add_d, d);
  if (au) return;
  Serial.println("position min atteinte");
}

float Etalonnage(bool termineExtremiteHaute){
  Serial.println("Etalonnage démarré");
  if (termineExtremiteHaute)
  {  
    Deplacement_Min();
    if (au) return;
    d = 0;
    
    Deplacement_Max();
    if (au) return;
    d_abs_mm = d;
         
  }else{
    
    Deplacement_Max();
    if (au) return;
    d = 0;
    
    Deplacement_Min();
    if (au) return;
    d_abs_mm = -d;
    d = 0;
  }
  Save_d(Add_dmax, d_abs_mm);
  PrintD();
  Serial.println("Etalonnage terminé");
}

bool Deplacement(float d_rel_mm){
  float d_step_mm_signed;  
  int steps;
  
  if (d_rel_mm > 0){
    //on monte
    digitalWrite(dirYPin, HIGH);
    steps = d_rel_mm / d_step_mm;
    d_step_mm_signed = d_step_mm;

    for (int i = 0; i < steps; i++) {
      if (digitalRead(y_limit_max) == LOW) {
        Serial.println("Y max atteint");
        return false;
      }
      Move1Step();
      if (au) return false;
      d += d_step_mm_signed;
    }
  }
  else{
    //on descend
    digitalWrite(dirYPin, LOW);
    steps = - d_rel_mm / d_step_mm;
    d_step_mm_signed = -d_step_mm;

    for (int i = 0; i < steps; i++) {
      if (digitalRead(y_limit_min) == LOW) 
      {
        Serial.println("Y min atteint");
        return false;
      }
      Move1Step();
      if (au) return false;
      d += d_step_mm_signed;
    }
  }
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
  Serial.print(d);
  Serial.println("mm");  
}

void PrintD(){
  Serial.print("D max = ");
  Serial.print(d_abs_mm);
  Serial.println("mm");  
}
