const int enPin = 8;
//const int stepXPin = 2; //X.STEP
//const int dirXPin = 5; // X.DIR
const int stepYPin = 3; //Y.STEP
const int dirYPin = 6; // Y.DIR
//const int stepZPin = 4; //Z.STEP
//const int dirZPin = 7; // Z.DIR

//356.5 mm entre d_min et d_max
int stepPin = stepYPin;
int dirPin = dirYPin;

const int stepsPerRev = 200; // ou 1.8° par step
int pulseWidthMicros = 500;  // microseconds
int millisBtwnSteps = 1000;

bool firstlap;

// 2 mm => 1 tour => 200 steps
// 2 / 200 = 0,00975 mm/step
const float d_step_mm = 0.01f;
float d;

#define y_limit_min 10
#define y_limit_max 11

float d_abs_mm;

void setup() {
  Serial.begin(9600);
  pinMode(enPin, OUTPUT);
  digitalWrite(enPin, LOW);
  
  pinMode(stepYPin, OUTPUT);
  pinMode(dirYPin, OUTPUT);
  
  pinMode(y_limit_min, INPUT_PULLUP);
  pinMode(y_limit_max, INPUT_PULLUP);
  
  firstlap = true;
  Serial.println(F("CNC Shield Initialized"));
  d_abs_mm = 200;
}

void loop() {

  digitalWrite(enPin, LOW);
  DemandeEtalonnage(true, 10);
  digitalWrite(enPin, HIGH);
  
  while(true){    
  }
}

void PrintD(){
  Serial.print("d_abs_mm = ");
  Serial.println(d_abs_mm);  
}

bool Deplacement(float d_rel_mm, bool testlimit){
  int steps = d_rel_mm / d_step_mm;
  
  bool sens = d_rel_mm > 0;
  if (sens)
    digitalWrite(dirYPin, HIGH);
  else{
    digitalWrite(dirYPin, LOW);
    steps = -steps;
  }

  if (sens){
    //MONTE
    for (int i = 0; i < steps; i++) {
      if (testlimit){
        bool extremite = digitalRead(y_limit_max);
        if (extremite == LOW)
          return false;
      }
      
      digitalWrite(stepYPin, HIGH);
      delayMicroseconds(pulseWidthMicros);
  
      d_abs_mm += d_step_mm;
      
      digitalWrite(stepYPin, LOW);
      delayMicroseconds(millisBtwnSteps);
    }
  }else{
    //DESCEND    
    for (int i = 0; i < steps; i++) {
      if (testlimit){
        bool extremite = digitalRead(y_limit_min);
        if (extremite == LOW)
          return false;
      }
          
      digitalWrite(stepYPin, HIGH);
      delayMicroseconds(pulseWidthMicros);
  
      d_abs_mm -= d_step_mm;
      
      digitalWrite(stepYPin, LOW);
      delayMicroseconds(millisBtwnSteps);
    }
  }
  return true;
}

void DemandeEtalonnage(bool termineExtremiteHaute, float degagement){
  Etalonnage(termineExtremiteHaute);

  //dégagement du contact de x mm
  if (degagement != 0f)
  {
    if (degagement < 0)
      degagement = -degagement;
    if (termineExtremiteHaute)
      Deplacement(-degagement, false);    
    else
      Deplacement(degagement, false); 
  }  
}

float Etalonnage(bool termineExtremiteHaute){
  if(termineExtremiteHaute)
  { 
    //on descend jusqu'à Y min par pas de 10mm 
    while(Deplacement(-10, true)){}
    Serial.println("Y min atteint");
    d_abs_mm = 0;  
    //on remonte jusqu'à Y max  
    while(Deplacement(10, true)){}  
    Serial.println("Y max atteint");
    Serial.print("D max = ");
    Serial.println(d_abs_mm);  
  }else{
    //on monte jusqu'à Y max par pas de 10mm 
    while(Deplacement(10, true)){}
    Serial.println("Y max atteint");
    d_abs_mm = 0;
    //on descend jusqu'à Y min  
    while(Deplacement(-10, true)){}  
    Serial.println("Y min atteint");
    Serial.print("D max = ");
    Serial.println(-d_abs_mm);    
    d_abs_mm = 0;
  }
  
  Serial.println("Etalonnage terminé");   
}
