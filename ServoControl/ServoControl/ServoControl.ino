  
  #include <Servo.h> 
  #include "DHT.h"
  #define DHTPIN 2   // what pin we're connected to
  #define DHTTYPE DHT22   // DHT 22  (AM2302)
  
  // Initialize DHT sensor for normal 16mhz Arduino
  DHT dht(DHTPIN, DHTTYPE);
  
  // create servo objects to control a servos
  Servo servoFanSpeed;
  Servo servoBlowerPositionFront;
  Servo servoBlowerPositionRear;
  
  //Incoming string from c# application
  byte incomingValues[4];
  //int pos = 0;    // variable to store the servo position 
  
  //Temperature Conrol Variables
  //int i = 100; //70
  int blowerPositionFront;
  int blowerPositionRear;

  int desiredTemps[] = {10, 11, 15, 17, 18, 19, 20, 24, 28};
  int desiredTemp = 10;
  float actualTemp = 0;
  int tempDifference =0;
  int loopCounter = 0;
  
  //Fan Speed Variables
  int fanSpeedSetting = 50; //Angle at which to set servo arm. 50 in this case is the fan speed at 0
  boolean userHasControl = false; //Used to allow the fan speed to be controlled automatically to allow the temperature to be changed quicker
                                  //The fan can be noisey and the user may want to turn it down while still wanting the temperature adjusted.
                                  
  //Blower Direction variables
  int blowerPotPin = A0;
  int previousBlowerPosition;
  int blowerPosition = 3;
  
  void setup() 
  { 
    Serial.begin(9600);
    //servoTempControl.attach(9);  // attaches the servo on pin 9 to the servo object 
    servoFanSpeed.attach(11);
    servoBlowerPositionFront.attach(9);
    servoBlowerPositionRear.attach(10);        
    servoBlowerPositionFront.write(165);  
    servoBlowerPositionRear.write(45);
    blowerPositionFront = 165;
    blowerPositionRear = 45;
    dht.begin();
    actualTemp = getTemp();  
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
      setFanSpeed(fanSpeedSetting);
      desiredTemp = incomingValues[1]; 
      tempDifference = getDiff(desiredTemp, actualTemp);      
      blowerPosition = incomingValues[2];    
    }
    else 
    {
      Serial.flush();
    }    
   
  
    ////////////////Adjust controls according to Temperature Difference //////////////////////
    if (loopCounter >= 0)
    {
      if (tempDifference >=1)
      {        
        if (!userHasControl)
        {
          //fanSpeedSetting = adjustFanSpeed(tempDifference);
          //servoFanSpeed.write(adjustFanSpeed(tempDifference));
        }
                
        if (desiredTemp > actualTemp)
        {
          increaseTemp();
        }
        
        if (desiredTemp < actualTemp)
        {
          decreaseTemp();
        }
      } 
    }
    //servoTempControl.write(i);
    //////////////////////////////////////////////////////////////////////////////////////////
    
    ///////////////Set the Blower Position /////////////////////////////////////////

    ////////////////////////////////////////////////////////////////////////////////////////
  
    
  
    if (loopCounter >5001)
    {
      actualTemp = getTemp();
      loopCounter=0;
    }
    else
    {
      //   delay(300);
      loopCounter++;
    }
    
    
 
  
  
    //////////////////////Adjust Fan Speed ///////////////////////////////////////////////////
//    if (c == '0')
//    {
//      servoFanSpeed.write(140);
//      fanSpeedSetting = 0;
//    }
    //  if (c == '1')
    //  {
    //    servoFanSpeed.write(115);
    //    fanSpeedSetting = 1;
    //  }
    //  if (c == '2')
    //  {
    //    servoFanSpeed.write(90);
    //    fanSpeedSetting = 2;
    //  }
    //  if (c == '3')
    //  {
    //    servoFanSpeed.write(65);
    //    fanSpeedSetting = 3;
    //  }
    //  if (c == '4')
    //  {
    //    servoFanSpeed.write(40);
    //    fanSpeedSetting = 4;
    //  }
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
  
  float getTemp()
  {
    //This method take from the project found here: https://github.com/RobTillaart/Arduino/tree/master/libraries/DHTlib
    // Wait a few seconds between measurements.
    //delay(2000);
  
    // Reading temperature or humidity takes about 250 milliseconds!
    // Sensor readings may also be up to 2 seconds 'old' (its a very slow sensor)
    float h = dht.readHumidity();
    // Read temperature as Celsius
    float t = dht.readTemperature();
    return t;
  }
  
  void setFanSpeed(byte fanspeed)
  {
    if (fanspeed == 0)
    {
      servoFanSpeed.write(140);
    }
    if (fanspeed == 1)
    {
      servoFanSpeed.write(115);
    }
    if (fanspeed == 2)
    {
      servoFanSpeed.write(90);
    }
    if (fanspeed == 3)
    {
      servoFanSpeed.write(65);
    }
    if (fanspeed == 4)
    {
      servoFanSpeed.write(40);
    }
  }
  
  
    void increaseTemp()
  {
    blowerPositionFront+=20;
    if(blowerPositionFront > 180)
    {
      servoBlowerPositionFront.write(180);
      blowerPositionFront=180;
      blowerPositionRear += 20;
      if (blowerPositionRear > 140)
      {
        servoBlowerPositionRear.write(140);
        blowerPositionRear=140;
      }
      {
      servoBlowerPositionRear.write(blowerPositionRear);
      }
    }
    else
    {
      servoBlowerPositionFront.write(blowerPositionFront);
    }
    delay(100);
  }
  
  void decreaseTemp()
  {
    if (blowerPositionRear <= 45)
    {
      blowerPositionFront-=20;    
    }
    //if(blowerPositionFront > 170)
    if(blowerPositionRear > 45)
    {
//      int newPos = blowerPositionFront - 180;
      blowerPositionRear -= 20;
      servoBlowerPositionRear.write(blowerPositionRear);            
    }
    else
    {
      servoBlowerPositionRear.write(45);
      blowerPositionRear = 45;
      if (blowerPositionFront < 10)
      {
        //If the servo turns below the minimun temperature then turn it back up to the minimum
        servoBlowerPositionFront.write(10);
        blowerPositionFront=10;
      }
      else
      {
        servoBlowerPositionFront.write(blowerPositionFront);
      }
    }
    delay(100);
  }
  




