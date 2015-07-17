/*
 Controlling a servo position using a potentiometer (variable resistor)
 by Michal Rinott <http://people.interaction-ivrea.it/m.rinott>

 modified on 8 Nov 2013
 by Scott Fitzgerald
 http://www.arduino.cc/en/Tutorial/Knob
*/

#include <Servo.h>
#include <StopWatch.h>
#include <Wire.h>
Servo servo1;  //Green = Windscreen - Feet  45 - 80 
Servo servo2; //Orange = Temperature - 7-75
Servo servo3; //Yellow = Windscreen - Face 30 - 90
Servo servoFanSpeed;
int servoVal = 40;
int servoRelayPin = 8;
bool servoRelayEnabledState = false;
int previousBlowerDirection;
byte incomingValues[6];//Incoming string from C# application
byte arrayToSend[50];
StopWatch relayStopwatch;

void setup()
{  
  Serial.begin(9600);
  servo1.attach(9);  // attaches the servo on pin 9 to the servo object
  servo2.attach(10);
  servo3.attach(11);
  servoFanSpeed.attach(12);
  servo1.write(50);
  servo2.write(30);
  servo3.write(50);
  servoFanSpeed.write(10);
  pinMode(servoRelayPin, OUTPUT);
  digitalWrite(servoRelayPin, LOW); //Initially disable all the motors
}

void loop()
{
  if(Serial.available()>=5)
  {
    for (int i=0;i<5;i++){
      incomingValues[i] = (byte)Serial.read();
      Serial.println(incomingValues[i]);
    } 
    setFanSpeed(incomingValues[0]);
    setTemperaturePosition(incomingValues[1]);
    setBlowerPosition(incomingValues[2]);
  }
  else 
  {
    serialFlush();
  }

  //Check is the relay has been on for 5 or more seconds
  Serial.println(relayStopwatch.elapsed());
  if (relayStopwatch.elapsed() > 5000)
  {
    digitalWrite(servoRelayPin, LOW); //Disable the relay
    Serial.println("Relay disbled");
  }

  delay(500);

  
//  char c = Serial.read();
//  if (c == '.')
//  {
//    ToggleServoRelay();
//    servoVal += 14;
//    servo3.write(servoVal);
//    delay(100);
//    servoVal-=7;
//    servo3.write(servoVal);
//    Serial.println(servoVal);
//    ToggleServoRelay();
//  }
//  else if (c == ',')
//  {
//    ToggleServoRelay();
//    servoVal -= 14;
//    servo3.write(servoVal);
//    delay(100);
//    servoVal+=7;
//    servo3.write(servoVal);
//    Serial.println(servoVal);
//    ToggleServoRelay();
//  }
  
}

void serialFlush(){
  while(Serial.available() > 0) {
    char t = Serial.read();
  }
}

void ToggleServoRelay()
{
  //Check if the relay is already enables and if not enable the relay for 5 seconds
  if (relayStopwatch.isRunning())
  {
    relayStopwatch.stop();
    relayStopwatch.reset();
    relayStopwatch.start();
    digitalWrite(servoRelayPin, HIGH); //Enable the relay  
    Serial.println("Relay enabled");
  }
  else
  {
    relayStopwatch.start();
    digitalWrite(servoRelayPin, HIGH); //Enable the relay  
    Serial.println("Relay enabled");
  }
}

void setFanSpeed(byte fanspeed)
{
  if (servoFanSpeed.read() != fanspeed)
  {
    ToggleServoRelay();
    if (fanspeed == 0)
    {
      servoFanSpeed.write(10);
    }
    if (fanspeed == 1)
    {
      servoFanSpeed.write(40);
    }
    if (fanspeed == 2)
    {
      servoFanSpeed.write(70);
    }
    if (fanspeed == 3)
    {
      servoFanSpeed.write(100);
    }
    if (fanspeed == 4)
    {
      servoFanSpeed.write(130);
    }
  }
}

void setTemperaturePosition(int pos)
{
  if (servo2.read() != pos)
  {
    ToggleServoRelay();
    servo2.write(pos);
  }
}

void setBlowerPosition(int blowerPosition)
{
  if (previousBlowerDirection != blowerPosition)
  {
    switch(blowerPosition)
    {
    case 0:
      servo1.write(45);
      servo3.write(30);
      break;
    case 1:
      servo1.write(45);
      servo3.write(60);
      break;
    case 2:
      servo1.write(45);
      servo3.write(90);
      break;
    case 3:
      servo1.write(80);
      servo3.write(90);
      break;
    }
  }
  previousBlowerDirection = blowerPosition;
}

