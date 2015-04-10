//label 1: fan
//label 2: rear right
//label 3: rear left
//label 4: front right
//label 5: front left

  #include <Servo.h> 
  #include <SimpleTimer.h>
  #include "DHT.h"
  #define DHTPINInside 2   // what pin we're connected to
  #define DHTPINOutside 8
  #define DHTTYPE DHT22   // DHT 22  (AM2302)
  
  // Initialize DHT sensor for normal 16mhz Arduino
  DHT dhtIn(DHTPINInside, DHTTYPE);
  DHT dhtOut(DHTPINOutside, DHTTYPE);
  float insideTemp;
  float outsideTemp;
  
  // create servo objects to control a servos
  Servo servoFanSpeed;
  Servo servoTemperaturePositionFront;
  Servo servoTemperaturePositionRear;
  Servo servoBlowerPositionFront;
  Servo servoBlowerPositionRear;
  const int servoTemperaturePositionFrontEnabledPin = 9;
  const int servoFanPositionDirectionEnabledPin = 10;
  
  SimpleTimer timer;
  
  //Incoming string from c# application
  byte incomingValues[4];
  //int pos = 0;    // variable to store the servo position 
  
  //Temperature Conrol Variables
  //int i = 100; //70
  int blowerPositionFront;
  int blowerPositionRear;
  boolean tempAtMax = false;
  boolean tempAtMin = false;

  //int desiredTemps[] = {10, 11, 15, 17, 18, 19, 20, 24, 28};
  int desiredTemp=12;
  float actualTemp = 0;
  int tempDifference =0;
  
  //Fan Speed Variables
  int fanSpeedSetting = 10; //Angle at which to set servo arm. 10 in this case is the fan speed at 0
  boolean userHasControl = false; //Used to allow the fan speed to be controlled automatically to allow the temperature to be changed quicker
                                  //The fan can be noisey and the user may want to turn it down while still wanting the temperature adjusted.
                                  
  //Blower Direction variables
  int blowerPosition = 3;
  
  void setup() 
  { 
    Serial.begin(9600);
    pinMode(servoTemperaturePositionFrontEnabledPin, OUTPUT);
    pinMode(servoFanPositionDirectionEnabledPin, OUTPUT);
    //servoTempControl.attach(9);  // attaches the servo on pin 9 to the servo object 
    servoFanSpeed.attach(7);
    servoTemperaturePositionFront.attach(6);
    servoTemperaturePositionRear.attach(5); 
    servoBlowerPositionFront.attach(4);
    servoBlowerPositionRear.attach(3);    
    servoTemperaturePositionFront.write(165);  
    servoTemperaturePositionRear.write(0);
    blowerPositionFront = 165;
    blowerPositionRear = 0;
    dhtIn.begin();
    dhtOut.begin();
    timer.setInterval(3000, AdjustTemperatureControls);
    digitalWrite(servoTemperaturePositionFrontEnabledPin,LOW);
    digitalWrite(servoFanPositionDirectionEnabledPin,LOW);
    //getTemp();  
  } 

  void loop() 
  { 
    if(Serial.available()==4)
    {
      for (int i=0;i<4;i++){
        incomingValues[i] = (byte)Serial.read();
//        delay(20);
      }   
      
      fanSpeedSetting = incomingValues[0];
      //setFanSpeed(fanSpeedSetting);
      desiredTemp = incomingValues[1]; 
      //tempDifference = getDiff(desiredTemp, actualTemp);      
      blowerPosition = incomingValues[2];    
    }
    else 
    {
      Serial.flush();
    } 
   timer.run();   
   //char c;
   //c= Serial.read();
  
    ////////////////Adjust controls according to Temperature Difference //////////////////////

    //////////////////////////////////////////////////////////////////////////////////////////
    
    ///////////////Set the Blower Position /////////////////////////////////////////

    //////////////////////////////////////////////////////////////////////////////////////// 
  
  
    //////////////////////Adjust Fan Speed ///////////////////////////////////////////////////
//    if (c=='z')
//    {
//      setFanSpeed(0);
//    }
//    if (c=='x')
//    {
//      setFanSpeed(1);
//    }
//    if (c=='c')
//    {
//      setFanSpeed(2);
//    }
//    if (c=='v')
//    {
//      setFanSpeed(3);
//    }
//    if (c=='b')
//    {
//      setFanSpeed(4);
//    }
//    if (c=='z')
//    {
//      fanspeed=0;
//    }
//    if (c=='x')
//    {
//      fanspeed=1;
//    }
//    if (c=='c')
//    {
//      fanspeed=2;
//    }
//    if (c=='v')
//    {
//      fanspeed=3;
//    }
//    if (c=='b')
//    {
//      fanspeed=4;
//    }
//    
//    if (c=='l')
//    {
//      desiredTemp=40;
//    }
//     if (c=='k')
//    {
//      desiredTemp=10;
//    }
//    
//    if( c== 'q')
//    {
//      blowerPosition=0;
//    }
//    if( c== 'w')
//    {
//      blowerPosition=1;
//    }
//    if( c== 'e')
//    {
//      blowerPosition=2;
//    }
//    if( c== 'r')
//    {
//      blowerPosition=3;
//    }
    ////////////////////////////////////////////////////////////////////////////////////////// 
  
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
  
  void getTemp()
  {
    //This method take from the project found here: https://github.com/RobTillaart/Arduino/tree/master/libraries/DHTlib
    // Wait a few seconds between measurements.
    //delay(2000);
  
    // Reading temperature or humidity takes about 250 milliseconds!
    // Sensor readings may also be up to 2 seconds 'old' (its a very slow sensor)
    //float h = dhtIn.readHumidity();
    // Read temperature as Celsius
    insideTemp = dhtIn.readTemperature();
    outsideTemp = dhtOut.readTemperature();
  }
  
  void AdjustTemperatureControls()
  {    
    //Get the current temperature
    getTemp();   
    actualTemp =  insideTemp;    
    tempDifference = getDiff(desiredTemp, actualTemp);
    
    ///////Adjust temperature setting position /////////////////////////
    //if(desiredTemp > 0)
    if (tempDifference >=1)
      {  
        if (!userHasControl)
        {
          //fanSpeedSetting = adjustFanSpeed(tempDifference);
          //servoFanSpeed.write(adjustFanSpeed(tempDifference));
        }
                
        if (desiredTemp > actualTemp)
        {          
          if (tempAtMax == true)
          {
            digitalWrite(servoTemperaturePositionFrontEnabledPin,LOW); 
          }
          else
          {
            digitalWrite(servoTemperaturePositionFrontEnabledPin,HIGH); 
            increaseTemp();
          }
        }
        
        if (desiredTemp < actualTemp)
        {
          if (tempAtMin == true)
          {
            digitalWrite(servoTemperaturePositionFrontEnabledPin,LOW); 
          }
          else
          {
            digitalWrite(servoTemperaturePositionFrontEnabledPin,HIGH); 
            decreaseTemp();
          }
        } 
        delay(500);       
      }
//      
      digitalWrite(servoTemperaturePositionFrontEnabledPin,LOW);
  
      digitalWrite(servoFanPositionDirectionEnabledPin,HIGH);     
       //delay(200);     
      setFanSpeed(fanSpeedSetting);      
      setBlowerPosition();
      //delay(500);
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
    switch(blowerPosition)
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
  
//  void decreaseTemp()
//  {
//    //blowerPositionFront = servoTemperaturePositionFront.read();
//    //blowerPositionRear = servoTemperaturePositionRear.read();
//    if (blowerPositionRear <= 45)
//    {
//      //digitalWrite(servoTemperaturePositionFrontEnabledPin,HIGH);
//      blowerPositionFront-=20; 
//      //servoTemperaturePositionFront.write(blowerPositionFront);   
//    }
//    //if(blowerPositionFront > 170)
//    if(blowerPositionRear > 45)
//    {
//      //digitalWrite(servoTemperaturePositionFrontEnabledPin,LOW);
////      int newPos = blowerPositionFront - 180;
//      blowerPositionRear -= 20;
//      servoTemperaturePositionRear.write(blowerPositionRear);            
//    }
//    else
//    {
//      servoTemperaturePositionRear.write(45);
//      blowerPositionRear = 45;
//      //digitalWrite(servoTemperaturePositionFrontEnabledPin,HIGH);
//      if (blowerPositionFront < 10)
//      {
//        //If the servo turns below the minimun temperature then turn it back up to the minimum
//        servoTemperaturePositionFront.write(10);
//        blowerPositionFront=10;
//      }
//      else
//      {
//        servoTemperaturePositionFront.write(blowerPositionFront);
//      }
//    }
//  }

void decreaseTemp()
  {
    //blowerPositionFront = servoTemperaturePositionFront.read();
    //blowerPositionRear = servoTemperaturePositionRear.read();
    if (blowerPositionRear <= 1)
    {
      //digitalWrite(servoTemperaturePositionFrontEnabledPin,HIGH);
      blowerPositionFront-=20; 
      //servoTemperaturePositionFront.write(blowerPositionFront);   
    }
    //if(blowerPositionFront > 170)
    if(blowerPositionRear > 1)
    {
      //digitalWrite(servoTemperaturePositionFrontEnabledPin,LOW);
//      int newPos = blowerPositionFront - 180;
      blowerPositionRear -= 20;
      servoTemperaturePositionRear.write(blowerPositionRear);            
    }
    else if (tempAtMin != true)
    {
      servoTemperaturePositionRear.write(5);
      blowerPositionRear = 1;
      //digitalWrite(servoTemperaturePositionFrontEnabledPin,HIGH);
      if (blowerPositionFront < 50)
      {
        tempAtMin=true;
        //If the servo turns below the minimun temperature then turn it back up to the minimum
        servoTemperaturePositionFront.write(50);
        blowerPositionFront=50;
      }
      else if (tempAtMin != true)
      {
        tempAtMin=false;
        servoTemperaturePositionFront.write(blowerPositionFront);
      }
    }
  }
