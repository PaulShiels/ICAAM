
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
#include "DHT.h"
Servo servo1;  //Green = Windscreen - Feet  45 - 80
Servo servo2; //Orange = Temperature - 7-75
Servo servo3; //Yellow = Windscreen - Face 30 - 90
Servo servoFanSpeed;
int servoVal = 40;
int servoRelayPin = 8;
bool servoRelayEnabledState = false;
int previousBlowerDirection;
int previousFanSpeed;
int previousTemperatureSetting;
bool relayOn = false;
byte incomingValues[8];
byte arrayToSend[1];
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
  if (Serial.available() >= 5)
  {
    for (int i = 0; i < 5; i++) {
      incomingValues[i] = (byte)Serial.read();
    }
    setFanSpeed(incomingValues[0]);
    setTemperaturePosition(incomingValues[1]);
    setBlowerPosition(incomingValues[2]);

    if (incomingValues[3] == 1)
      ToggleServoRelay();
    //else
      //relayOn = false;
  }
  else
  {
    serialFlush();
  }

  //Check is the relay has been on for 5 or more seconds
  if (relayStopwatch.elapsed() > 1500)
  {
    digitalWrite(servoRelayPin, LOW); //Disable the relay
    relayOn = false;
    //Serial.println("Relay disbled");
  }

  delay(100);


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

void serialFlush() {
  while (Serial.available() > 0) {
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
    //Serial.println("Relay enabled");
  }
  else
  {
    relayStopwatch.start();
    digitalWrite(servoRelayPin, HIGH); //Enable the relay
    //Serial.println("Relay enabled");
  }
}

void setFanSpeed(byte fanspeed)
{
  if (previousFanSpeed != fanspeed)
  {
//    if (relayOn)
//      ToggleServoRelay();

    if (fanspeed == 0)
    {
      servoFanSpeed.write(5);
    }
    if (fanspeed == 1)
    {
      servoFanSpeed.write(35);
    }
    if (fanspeed == 2)
    {
      servoFanSpeed.write(65);
    }
    if (fanspeed == 3)
    {
      servoFanSpeed.write(95);
    }
    if (fanspeed == 4)
    {
      servoFanSpeed.write(125);
    }
  }
  previousFanSpeed = fanspeed;
}

void setTemperaturePosition(int pos)
{
  if (previousTemperatureSetting != pos)
  {
//    if (relayOn)
//      ToggleServoRelay();

    switch (pos)
    {
      case 0:
        servo2.write(5);
        break;
      case 1:
        servo2.write(13);
        break;
      case 2:
        servo2.write(21);
        break;
      case 3:
        servo2.write(39);
        break;
      case 4:
        servo2.write(47);
        break;
      case 5:
        servo2.write(59);
        break;
      case 6:
        servo2.write(67);
        break;
      case 7:
        servo2.write(78);
        break;
      case 8:
        servo2.write(85);
        break;
      case 9:
        servo2.write(100);
        break;
    }
  }
  previousTemperatureSetting = pos;
  //  int modAns;
  //  if (servo2.read() >= pos)
  //  {
  //    modAns = servo2.read() % pos;
  //  }
  //  else
  //  {
  //    modAns = pos % servo2.read();
  //  }
  //
  //  if (modAns >= 10)
  //  //if (servo2.read() != pos)
  //  {
  //    ToggleServoRelay();
  ////    pos += 10;
  ////    servo2.write(pos);
  ////    delay(200);
  ////    pos-=5;
  //    servo2.write(pos);
  //  }
}

void setBlowerPosition(int blowerPosition)
{
  if (previousBlowerDirection != blowerPosition)
  {
//    if (relayOn)
//      ToggleServoRelay();

    switch (blowerPosition)
    {
      case 0:
        servo1.write(50);
        servo3.write(30);
        break;
      case 1:
        servo1.write(50);
        servo3.write(60);
        break;
      case 2:
        servo1.write(50);
        servo3.write(100);
        break;
      case 3:
        servo1.write(120);
        servo3.write(100);
        break;
    }
  }
  previousBlowerDirection = blowerPosition;
  arrayToSend[0] = 1;
  Serial.write(arrayToSend[0]);
}


