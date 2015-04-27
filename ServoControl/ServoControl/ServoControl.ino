  //label 1: fan
  //label 2: rear right
  //label 3: rear left
  //label 4: front right
  //label 5: front left
  
  #include <Servo.h> 
  #include <SimpleTimer.h>
  #include <stdlib.h>
  #include <stdio.h>
  #include <StopWatch.h>
  #include <TEA5767.h>
  #include <Wire.h>
  StopWatch StopwatchTempSetting; //Create the stopwatch object
  StopWatch StopwatchBlowerDirectionFanSpeed;
  #include "DHT.h"
  #define DHTPINInside 2   // what pin we're connected to
  #define DHTPINOutside 8
  #define DHTTYPE DHT22   // DHT 22  (AM2302)
  
  // Initialize DHT sensor for normal 16mhz Arduino
  DHT dhtIn(DHTPINInside, DHTTYPE);
  DHT dhtOut(DHTPINOutside, DHTTYPE);
  union {
        float inTemp;
        char bytes[4];
      } insideTemp;
  union {
        float outTemp;
        char bytes[4];
      } outsideTemp;
  //float insideTemp = 32.1;
  //float outsideTemp = 4.2;
  
  // create servo objects to control a servos
  Servo servoFanSpeed;
  Servo servoTemperaturePositionFront;
  Servo servoTemperaturePositionRear;
  Servo servoBlowerPositionFront;
  Servo servoBlowerPositionRear;
  const int servoTemperaturePositionFrontEnabledPin = 9;
  const int servoFanPositionDirectionEnabledPin = 10;
  
  SimpleTimer timer;
  SimpleTimer timerCheckStopwatch;
  
  //Incoming string from c# application
  byte incomingValues[8];
  byte arrayToSend[50];
  byte resetBoard=0;
  //int pos = 0;    // variable to store the servo position 
  
  //Temperature Conrol Variables
  //int i = 100; //70
  int blowerPositionFront = 50;
  int blowerPositionRear = 5;
  int previousblowerPositionFront = 50;
  int previousblowerPositionRear = 5;
  boolean tempAtMax = false;
  boolean tempAtMin = false;
  
  int desiredTemps[] = {10, 11, 15, 17, 18, 19, 20, 24, 28};
  byte desiredTemp=1;
  float actualTemp = 0;
  int tempDifference =0;
  
  //Fan Speed Variables
  int fanSpeedSetting = 0; //Angle at which to set servo arm. 10 in this case is the fan speed at 0
  int previousFanSpeedSetting=0;
  boolean userHasControl = false; //Used to allow the fan speed to be controlled automatically to allow the temperature to be changed quicker
  //The fan can be noisey and the user may want to turn it down while still wanting the temperature adjusted.
  
  //Blower Direction variables
  int blowerDirection= 3;
  int previousBlowerDirection = 3;

  //Radio Tuner variables
  TEA5767 Radio;
  int search_mode = 0;
  int search_direction;
  double oldFrequency;
  double newFrequency=94.90;
  byte autoTuneOn = false;
  union{
    int sigLevel;
    char bytes[2];
    } radioSignalLevel;
  
  //Read Battery Voltage variables
  int analogInput = 0;
  int value = 0;
  float vout = 0.0;
  float vin = 0.0;
  float R1 = 101000.0; // resistance of R1 (100K)
  float R2 = 10000.0; // resistance of R2 (10K)
  union{
    float voltsIn;
    char bytes[4];
  }voltage;

  void(* resetFunc) (void) = 0; //declare reset function @ address 0

  void setup() 
  { 
    Serial.begin(9600);  
    Wire.begin();
    Radio.init(); 
    Radio.set_frequency(newFrequency);  
    pinMode(analogInput, INPUT);
    pinMode(servoTemperaturePositionFrontEnabledPin, OUTPUT);
    pinMode(servoFanPositionDirectionEnabledPin, OUTPUT);
    //servoTempControl.attach(9);  // attaches the servo on pin 9 to the servo object 
    digitalWrite(6,LOW);
    servoFanSpeed.attach(7);
    servoTemperaturePositionFront.attach(11);
    servoTemperaturePositionRear.attach(5); 
    servoBlowerPositionFront.attach(4);
    servoBlowerPositionRear.attach(3);    
    dhtIn.begin();
    dhtOut.begin();
    timer.setInterval(2000, getTemp);
    timerCheckStopwatch.setInterval(500, checkStopwatch);
    digitalWrite(servoTemperaturePositionFrontEnabledPin,LOW);
    digitalWrite(servoFanPositionDirectionEnabledPin,LOW);
    servoTemperaturePositionFront.write(50);
  
    //getTemp();  
  } 

  void loop() 
  { 
    
    
    if(Serial.available()>=7)
    {
      for (int i=0;i<7;i++){
        incomingValues[i] = (byte)Serial.read();
        //        delay(20);
      }    
      previousFanSpeedSetting = fanSpeedSetting;
    previousblowerPositionFront = blowerPositionFront;
    previousblowerPositionRear = blowerPositionRear;
    previousBlowerDirection = blowerDirection;
      fanSpeedSetting = incomingValues[0];            
      blowerPositionFront = incomingValues[1]; 
      blowerPositionRear = incomingValues[2]; 
      //tempDifference = getDiff(desiredTemp, actualTemp);            
      oldFrequency = newFrequency;
      blowerDirection = incomingValues[3];
      char *freqString, *stopstring; 
      freqString = freqString += incomingValues[4] + '.' + incomingValues[5];
      newFrequency = strtod(freqString, &stopstring); 
      //freqString.concat(incomingValues[4],'.');
      //freqString += ".";
      //freqString += String)incomingValues[5]);    
      //resetBoard = incomingValues[5]; 
      //autoTuneOn = incomingValues[6];
      resetBoard = incomingValues[6];   
      //Serial.flush();
      
      
  
  
      //Serial.write(outsideTemp*100);
    }
    else 
    {
      serialFlush();
      //Serial.flush();
      //      while(Serial.available())
      //      Serial.read();
    } 
  
    //    if (resetBoard == 1)
    //      resetFunc();  //reset the board
  
    timer.run(); 
    timerCheckStopwatch.run();
    AdjustTemperatureControls();  
    
    //Check if the temperature setting has changed
    //Check if the timer is running and if not skip the next step and start the timer
    //Check if the timer has been running for 2 seconds or more and if so disable the relay.
//    if (previousblowerPositionFront == blowerPositionFront && previousblowerPositionRear == blowerPositionRear)
//    {
////      if (StopwatchTempSetting.isRunning())
////      {
////        if (StopwatchTempSetting.elapsed() > 500)
//        {
//           digitalWrite(servoTemperaturePositionFrontEnabledPin,LOW);        
//        }
//      //}
////      else
////      {
////        digitalWrite(servoTemperaturePositionFrontEnabledPin,LOW);
////        StopwatchTempSetting.start();
////      }
//    }
//    else
//    {
////      StopwatchTempSetting.reset();
////      StopwatchTempSetting.start();      
//      digitalWrite(servoTemperaturePositionFrontEnabledPin,HIGH);    
//    }
//    
//    //Check if the fan speed or blower direction has changed
//    //Check if the timer is running and if not skip the next step and start the timer
//    //Check if the timer has been running for 2 seconds or more and if so disable the relay.
//    if (previousFanSpeedSetting == fanSpeedSetting && previousBlowerDirection == blowerDirection)
//    {
//      //if (StopwatchBlowerDirectionFanSpeed.isRunning())
//      {
//        //if (StopwatchBlowerDirectionFanSpeed.elapsed() > 500)
//        {
//           digitalWrite(servoFanPositionDirectionEnabledPin,LOW);        
//        }
//      }
////      else
////      {
////        digitalWrite(servoFanPositionDirectionEnabledPin,LOW);
////        StopwatchBlowerDirectionFanSpeed.start();
////      }
//    }
//    else
//    {
////      StopwatchBlowerDirectionFanSpeed.reset();
////      StopwatchBlowerDirectionFanSpeed.start();      
//      digitalWrite(servoFanPositionDirectionEnabledPin,HIGH);    
//    }
  }

  void sendData()
  {
     outsideTemp.outTemp = 83.72;
    arrayToSend[0] = insideTemp.bytes[0];
    arrayToSend[1] = insideTemp.bytes[1];
    arrayToSend[2] = insideTemp.bytes[2];
    arrayToSend[3] = insideTemp.bytes[3];
    arrayToSend[4] = outsideTemp.bytes[0];
    arrayToSend[5] = outsideTemp.bytes[1];
    arrayToSend[6] = outsideTemp.bytes[2];
    arrayToSend[7] = outsideTemp.bytes[3];
    arrayToSend[8] = voltage.bytes[0];
    arrayToSend[9] = voltage.bytes[1];
    arrayToSend[10] = voltage.bytes[2];
    arrayToSend[11] = voltage.bytes[3];
    arrayToSend[12] = radioSignalLevel.bytes[0];
    arrayToSend[13] = radioSignalLevel.bytes[1];
    
    
    for(int i=0; i<13; i++)
    {      
      Serial.write(arrayToSend[i]);//insideTemp.bytes);
    }   
  }
   
  void checkStopwatch()
  {
    //Check if the temperature setting has changed
    //Check if the timer is running and if not skip the next step and start the timer
    //Check if the timer has been running for 2 seconds or more and if so disable the relay.
    if (previousblowerPositionFront == blowerPositionFront && previousblowerPositionRear == blowerPositionRear)
    {
      if (StopwatchTempSetting.isRunning())
      {
        if (StopwatchTempSetting.elapsed() < 500)
        {
           digitalWrite(servoTemperaturePositionFrontEnabledPin,LOW);        
           //StopwatchTempSetting.stop();
        }
      }
      else
      {        
        StopwatchTempSetting.reset();
        StopwatchTempSetting.start();
      }
    }
    else
    {
      StopwatchTempSetting.reset();
      StopwatchTempSetting.start();      
      digitalWrite(servoTemperaturePositionFrontEnabledPin,HIGH);    
    }
    
    //Check if the fan speed or blower direction has changed
    //Check if the timer is running and if not skip the next step and start the timer
    //Check if the timer has been running for 2 seconds or more and if so disable the relay.
    if (previousFanSpeedSetting == fanSpeedSetting && previousBlowerDirection == blowerDirection)
    {
      if (StopwatchBlowerDirectionFanSpeed.isRunning())
      {
          if (StopwatchBlowerDirectionFanSpeed.elapsed() < 500)
        {A
           //StopwatchTempSetting.stop();      
        }
      }
      else
      {  
        StopwatchTempSetting.reset();
        StopwatchBlowerDirectionFanSpeed.start();
      }
    }
    else
    {
      StopwatchBlowerDirectionFanSpeed.reset();
      StopwatchBlowerDirectionFanSpeed.start();   
      digitalWrite(servoFanPositionDirectionEnabledPin,HIGH);       
      
    }
  }


float getDiff(float num1, float num2)
{
  if (num1 >= num2)
  {
    return num1 - num2;
  }
  if (num2 > num1)
  {
    return num2 - num1;
  }
}

void serialFlush(){
  while(Serial.available() > 0) {
    char t = Serial.read();
  }
} 

void getTemp()
{
  //This method take from the project found here: https://github.com/RobTillaart/Arduino/tree/master/libraries/DHTlib
  // Wait a few seconds between measurements.
  //delay(2000);

  // Reading temperature or humidity takes about 250 milliseconds!
  // Sensor readings may also be up to 2 seconds 'old' (its a very slow sensor)
  //float h = dhtIn.readHumidity();
  // Read temperature as Celsius
  insideTemp.inTemp = dhtIn.readTemperature();
  outsideTemp.outTemp = dhtOut.readTemperature();
  updateFmRadio();
  readBatteryVoltage();
  sendData();   
}

void AdjustTemperatureControls()
{    
  //Get the current temperature
  //getTemp();   
  //actualTemp =  insideTemp;    
  //tempDifference = getDiff(desiredTemp, actualTemp);

  ///////Adjust temperature setting position /////////////////////////
  
  servoTemperaturePositionFront.write(blowerPositionFront);
  servoTemperaturePositionRear.write(blowerPositionRear);
  
//if (desiredTemp > 175)
//{
//  servoTemperaturePositionFront.write(175);
//  int rearServoPos = desiredTemp-175;
//  servoTemperaturePositionRear.write(rearServoPos);
//}
//else
//{
//  servoTemperaturePositionRear.write(5);
//  servoTemperaturePositionFront.write(desiredTemp);
//}

//  if (desiredTemp == 0)
//  {
//    //digitalWrite(servoTemperaturePositionFrontEnabledPin,HIGH);
//    blowerPositionFront += 5;
//    servoTemperaturePositionFront.write(blowerPositionFront);
//    //decreaseTemp();
//    delay(50);
//    //digitalWrite(servoTemperaturePositionFrontEnabledPin,LOW);
//  }
//
//  if (desiredTemp == 2)
//  {
//    //digitalWrite(servoTemperaturePositionFrontEnabledPin,HIGH);
////    blowerPositionFront -= 5;
//    servoTemperaturePositionFront.write(desiredTemp);
//    //increaseTemp();
//    delay(0);
//    //digitalWrite(servoTemperaturePositionFrontEnabledPin,LOW);
//  }

  //if(desiredTemp > 0)
  //if (tempDifference >=1)
  //{  
  if (!userHasControl)
  {
    //fanSpeedSetting = adjustFanSpeed(tempDifference);
    //servoFanSpeed.write(adjustFanSpeed(tempDifference));
  }

  //        if (desiredTemp > actualTemp)
  //        {          
  //          if (tempAtMax == true)
  //          {
  //            digitalWrite(servoTemperaturePositionFrontEnabledPin,LOW); 
  //          }
  //          else
  //          {
  //            digitalWrite(servoTemperaturePositionFrontEnabledPin,HIGH); //Turn on the relay
  //            increaseTemp();            
  //          }
  //          tempAtMin = false;
  //        }
  //        
  //        if (desiredTemp < actualTemp)
  //        {
  //          if (tempAtMin == true)
  //          {
  //            digitalWrite(servoTemperaturePositionFrontEnabledPin,LOW); 
  //          }
  //          else
  //          {
  //            digitalWrite(servoTemperaturePositionFrontEnabledPin,HIGH); 
  //            decreaseTemp();
  //          }
  //          tempAtMax = false;
  //        } 
  //        delay(500);       
  //      }
  //      digitalWrite(servoTemperaturePositionFrontEnabledPin,LOW);

  //if (previousBlowerDirection != blowerPosition || previousFanSpeedSetting != fanSpeedSetting)
  {
    //digitalWrite(servoFanPositionDirectionEnabledPin,HIGH);  
    setFanSpeed(fanSpeedSetting);      
    setBlowerPosition();   
    //digitalWrite(servoFanPositionDirectionEnabledPin,LOW);
  }
  //digitalWrite(servoFanPositionDirectionEnabledPin,LOW);    
  //      Serial.print("Fan Speed = ");
  //      Serial.println(fanspeed);
  //      Serial.print("Blower Position = ");
  //      Serial.println(blowerPosition);
  //      Serial.println(" ");
  ////////////////////////////////////////////////////////////////////
}



void setBlowerPosition()
{
  switch(blowerDirection)
  {
  case 0:
    servoBlowerPositionFront.write(2);
    servoBlowerPositionRear.write(0);
    break;
  case 1:
    servoBlowerPositionFront.write(90);
    servoBlowerPositionRear.write(0);
    break;
  case 2:
    servoBlowerPositionFront.write(170);
    servoBlowerPositionRear.write(5);
    break;
  case 3:
    servoBlowerPositionFront.write(170);
    servoBlowerPositionRear.write(60);
    break;
  }
}

void setFanSpeed(byte fanspeed)
{
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


//  void increaseTemp()
//  {    
//    blowerPositionFront+=20;
//    if(blowerPositionFront > 180)
//    {
//      //digitalWrite(servoTemperaturePositionFrontEnabledPin,LOW);
//      servoTemperaturePositionFront.write(180);
//      blowerPositionFront=180;
//      blowerPositionRear += 20;
//      if (blowerPositionRear > 130)
//      {
//        servoTemperaturePositionRear.write(130);
//        blowerPositionRear=130;
//      }
//      {
//      servoTemperaturePositionRear.write(blowerPositionRear);
//      }
//    }
//    else
//    {
//      //digitalWrite(servoTemperaturePositionFrontEnabledPin,HIGH);
//      servoTemperaturePositionFront.write(blowerPositionFront);
//    }
//    //digitalWrite(servoTemperaturePositionFrontEnabledPin,LOW);
//  }

void increaseTemp()
{    
  if (blowerPositionRear < 165  && tempAtMax != true)
  {
    tempAtMax=false;
    blowerPositionRear += 20;
    servoTemperaturePositionRear.write(blowerPositionRear);
  }
  else if (tempAtMax != true)
  {
    tempAtMax=false;
    servoTemperaturePositionRear.write(165);
    blowerPositionRear=165;
    blowerPositionFront+=20;
    servoTemperaturePositionFront.write(blowerPositionFront);
  }

  if(blowerPositionFront > 160 && tempAtMax != true)
  {
    //digitalWrite(servoTemperaturePositionFrontEnabledPin,LOW);
    servoTemperaturePositionFront.write(160);
    blowerPositionFront=160;
    tempAtMax = true;
    //      blowerPositionRear += 20;
    //      if (blowerPositionRear > 130)
    //      {
    //        servoTemperaturePositionRear.write(130);
    //        blowerPositionRear=130;
    //      }
    //      {
    //      servoTemperaturePositionRear.write(blowerPositionRear);
  }
}
//else
//{
//digitalWrite(servoTemperaturePositionFrontEnabledPin,HIGH);
//servoTemperaturePositionFront.write(blowerPositionFront);
//}
//digitalWrite(servoTemperaturePositionFrontEnabledPin,LOW);
//}  

void decreaseTemp()
{
  //blowerPositionFront = servoTemperaturePositionFront.read();
  //blowerPositionRear = servoTemperaturePositionRear.read();
  if (blowerPositionRear <= 45)
  {
    //digitalWrite(servoTemperaturePositionFrontEnabledPin,HIGH);
    blowerPositionFront-=20; 
    //servoTemperaturePositionFront.write(blowerPositionFront);   
  }
  //if(blowerPositionFront > 170)
  if(blowerPositionRear > 45)
  {
    //digitalWrite(servoTemperaturePositionFrontEnabledPin,LOW);
    //      int newPos = blowerPositionFront - 180;
    blowerPositionRear -= 20;
    servoTemperaturePositionRear.write(blowerPositionRear);            
  }
  else
  {
    servoTemperaturePositionRear.write(45);
    blowerPositionRear = 45;
    //digitalWrite(servoTemperaturePositionFrontEnabledPin,HIGH);
    if (blowerPositionFront < 10)
    {
      //If the servo turns below the minimun temperature then turn it back up to the minimum
      servoTemperaturePositionFront.write(10);
      blowerPositionFront=10;
    }
    else
    {
      servoTemperaturePositionFront.write(blowerPositionFront);
    }
  }
}

//void decreaseTemp()
//  {
//    tempAtMax=false;
//    //blowerPositionFront = servoTemperaturePositionFront.read();
//    //blowerPositionRear = servoTemperaturePositionRear.read();
//    if (blowerPositionRear <= 1)
//    {
//      //digitalWrite(servoTemperaturePositionFrontEnabledPin,HIGH);
//      blowerPositionFront-=20; 
//      //servoTemperaturePositionFront.write(blowerPositionFront);   
//    }
//    //if(blowerPositionFront > 170)
//    if(blowerPositionRear > 1)
//    {
//      //digitalWrite(servoTemperaturePositionFrontEnabledPin,LOW);
////      int newPos = blowerPositionFront - 180;
//      blowerPositionRear -= 20;
//      servoTemperaturePositionRear.write(blowerPositionRear);            
//    }
//    else if (tempAtMin != true)
//    {
//      servoTemperaturePositionRear.write(5);
//      blowerPositionRear = 1;
//      //digitalWrite(servoTemperaturePositionFrontEnabledPin,HIGH);
//      if (blowerPositionFront < 50)
//      {
//        tempAtMin=true;
//        //If the servo turns below the minimun temperature then turn it back up to the minimum
//        servoTemperaturePositionFront.write(50);
//        blowerPositionFront=50;
//      }
//      else if (tempAtMin != true)
//      {
//        tempAtMin=false;
//        servoTemperaturePositionFront.write(blowerPositionFront);
//      }
//    }
//  }


/////////////////////////// FM Radio /////////////////////////////////////////////////////
  void updateFmRadio()
  {
    unsigned char buf[5];
    int stereo;
    double current_freq;    
    unsigned long current_millis = millis();
    
    if (oldFrequency != newFrequency)
    {
      Radio.set_frequency(newFrequency); 
      radioSignalLevel.sigLevel = Radio.signal_level(buf);
    }
    
    if (Radio.read_status(buf) == 1) {
      current_freq =  floor (Radio.frequency_available (buf) / 100000 + .5) / 10;
      stereo = Radio.stereo(buf);
      radioSignalLevel.sigLevel = Radio.signal_level(buf);
    }
    
//    if (search_mode == 1) {
//        if (Radio.process_search (buf, search_direction) == 1) {
//            search_mode = 0;
//        }
//    }
    
    if (autoTuneOn == 1)
    {
      //last_pressed = current_millis;
      search_mode = 1;
      search_direction = TEA5767_SEARCH_DIR_UP;
      Radio.search_up(buf);
      delay(300);
    }
//    
//    if (btn_backward.isPressed()) {
//      last_pressed = current_millis;
//      search_mode = 1;
//      search_direction = TEA5767_SEARCH_DIR_DOWN;
//      Radio.search_down(buf);
//      delay(300);
//    } 
  }
////////////////////////////////////////////////////////////////////////////////////////

  void readBatteryVoltage()
  {
     value = analogRead(analogInput);
     vout = (value * 5.00) / 1024.0; // see text
     vin = vout / (R2/(R1+R2)); 
     if (vin < 2.09) 
     {
       vin=0.0;//statement to quash undesired reading !
     }     
     voltage.voltsIn = vin;
  }
