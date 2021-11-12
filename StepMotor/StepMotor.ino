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
// 2 / 200 = 0,00975 mm/step
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

void setup() {
  Serial.begin(9600);
  // init CNC shield
  pinMode(enPin, OUTPUT);
  digitalWrite(enPin, HIGH); //arrêt
  pinMode(stepYPin, OUTPUT);
  pinMode(dirYPin, OUTPUT);  
  pinMode(y_limit_min, INPUT_PULLUP);
  pinMode(y_limit_max, INPUT_PULLUP);
  
  Serial.println(F("CNC Shield Initialized"));
  d_abs_mm = 0;

  todo = arret;
  au = false;
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

bool InteractionManager(){
  if(SerialManager()){
    CommandManager();
    return true;
  }
  return false;
}

bool SerialManager() {
  if (Serial.available()==0)
    return false;
  while (Serial.available() > 0)
  {
    static unsigned int message_pos = 0;
    
    //Read the next available byte in the serial receive buffer
    char inByte = Serial.read();
    
    //Message coming in (check not terminating character) and guard for over message size
    if (inByte != '\n' && (message_pos < MAX_MESSAGE_LENGTH - 1) )
    {
      //Add the incoming byte to our message
      message[message_pos] = inByte;
      message_pos ++;
    }else{//Full message received...
      //Add null character to string
      message[message_pos] = '\0';      
      //Reset for the next message
      message_pos = 0;
    }
  }
  return true;
}

void CommandManager(){
  float val; //à déclarer en dehors du switch !
  switch(message[0]){
    case 'e':
    case 'E':
      Serial.println("Etalonnage demandé");
      au = false;
      todo = etalonnage;
      break;
      
    case 'a':
    case 'A':
    case 's':
    case 'S':
      Serial.println("ARRET D'URGENCE");
      au = true;
      digitalWrite(enPin, HIGH); //arrêt
      break;
      
    case 'd':
    case 'D':    
      Serial.print("D max = ");
      Serial.println(d_abs_mm); 
      break;
      
    case 'p':
    case 'P':
      PrintPosition();
      break;
      
    case 'b':
    case 'B':
      Serial.println("descente demandée");
      val = GetVal();
      if (val == 0) val = -1;
      if (val > 0)  val = -val;      
      digitalWrite(enPin, LOW);
      Deplacement(val);
      digitalWrite(enPin, HIGH);
      PrintPosition();
      break;
      
    case 'h':
    case 'H':
      Serial.println("monté demandée");
      val = GetVal();
      if (val == 0) val = 1;
      if (val < 0)  val = -val;
      digitalWrite(enPin, LOW);
      Deplacement(val);
      digitalWrite(enPin, HIGH);
      PrintPosition();
      break;      
  }
}

float GetVal(){ // retourne en float la suite du message
  return String(message).substring(1).toFloat();
}

void DemandeEtalonnage(bool termineExtremiteHaute, float degagement){
  Etalonnage(termineExtremiteHaute);

  //dégagement du contact de x mm
  if (degagement != 0)
  {
    if (degagement < 0)
      degagement = -degagement;
    if (termineExtremiteHaute)
      Deplacement(-degagement);    
    else
      Deplacement(degagement); 
  }  
}

float Etalonnage(bool termineExtremiteHaute){
  Serial.println("Etalonnage démarré");
  if(termineExtremiteHaute)
  { 
    //on descend jusqu'à Y min par pas de 10mm 
    while(Deplacement(-10) && !au){}
    if (au) return;
    Serial.println("Y min atteint");
    //d_abs_mm = 0;
    d = 0;
    //on remonte jusqu'à Y max  
    while(Deplacement(10) && !au){}
    if (au) return;
    Serial.println("Y max atteint");
    d_abs_mm = d;
     
  }else{
    
    //on monte jusqu'à Y max par pas de 10mm 
    while(Deplacement(10) && !au){}
    if (au) return;
    Serial.println("Y max atteint");
    d = 0;
    //on descend jusqu'à Y min  
    while(Deplacement(-10) && !au){}
    if (au) return;
    Serial.println("Y min atteint");
    d_abs_mm = -d;
    d = 0;
  }
  
  Serial.print("D max = ");
  Serial.print(d_abs_mm);
  Serial.println("mm");
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
      if (digitalRead(y_limit_max) == LOW)
        return false;
      if (InteractionManager()) 
        return false;
      Move1Step();
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
        return false;
      if (InteractionManager()) 
        return false;
      Move1Step();
      d += d_step_mm_signed;
    }
  }
  return true;
}

void Move1Step(){
  digitalWrite(stepYPin, HIGH);
  delayMicroseconds(pulseWidthMicros);
  digitalWrite(stepYPin, LOW);
  delayMicroseconds(millisBtwnSteps);  
}

void PrintPosition(){
  Serial.print("Position = ");
  Serial.println(d);
}
