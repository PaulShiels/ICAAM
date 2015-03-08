  
  #include <Servo.h> 
  #include "DHT.h"
  #define DHTPIN 2   // what pin we're connected to
  #define DHTTYPE DHT22   // DHT 22  (AM2302)
  
  // Initialize DHT sensor for normal 16mhz Arduino
  DHT dht(DHTPIN, DHTTYPE);
  
  // create servo objects to control a servos
  Servo servoTempControl;  
  Servo servoFanSpeed;
  Servo servoBlowerPosition; 
  
  //Incoming string from c# application
  byte incomingValues[4];
  //int pos = 0;    // variable to store the servo position 
  
  //Temperature Conrol Variables
  int i = 100; //70
  boolean atMaxTemp = false;
  boolean atMinTemp = false;
  int desiredTemps[] = {10, 11, 15, 17, 18, 19, 20, 24, 28};
  int desiredTemp = 0;
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
  int blowerLs0 = 13;
  int blowerLs1 = 12;
  int blowerLs2 = 10;
  int blowerLs3 = 7;
  
  void setup() 
  { 
    pinMode(blowerLs0, INPUT);
    pinMode(blowerLs1, INPUT);
    pinMode(blowerLs2, INPUT);
    pinMode(blowerLs3, INPUT); 
    Serial.begin(9600);
    servoTempControl.attach(9);  // attaches the servo on pin 9 to the servo object 
    servoFanSpeed.attach(11);
    servoBlowerPosition.attach(8);
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
      previousBlowerPosition = blowerPosition;
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
          if (atMaxTemp == false)
          {
            if (loopCounter >= 5000)
            {
              increaseTempPosition();
            }
            atMinTemp = false;
            //Check if the heat is turned up full
            if (digitalRead(12) == LOW)
            {
              atMaxTemp = true;
            }
            else
            {
              atMaxTemp = false;
            }
          }
        }
        
        if (desiredTemp < actualTemp)
        {
          //Check is heat turned down full
          if (atMinTemp == false)
          {
            if (loopCounter >= 5000)
            {
              decreaseTempPosition();
            }
            atMaxTemp = false;
            if (digitalRead(12) == LOW)
            {
              atMinTemp = true;
            }
            else
            {
              atMinTemp = false;
            }
          }
        }
      } 
    }
    servoTempControl.write(i);
    //////////////////////////////////////////////////////////////////////////////////////////
    
    ///////////////Set the Blower Position /////////////////////////////////////////
    
    char b = Serial.read();
    if (b == 'c')
      blowerPosition = 0;
    if (b == 'v')
      blowerPosition = 1;
      if (b == 'b')
      blowerPosition = 2;
      if (b == 'n')
      blowerPosition = 3;
      
      Serial.print("Position = ");
      Serial.println(blowerPosition);
      
      //if (digitalRead(blowerLs0) == LOW || digitalRead(blowerLs1) || digitalRead(blowerLs2) == LOW || digitalRead(blowerLs3))
          
          
      switch (blowerPosition)
      {
      case 0:
      {
        if (digitalRead(blowerLs0) == LOW)
          servoBlowerPosition.write(100);        
        else
          if (digitalRead(blowerLs0) == HIGH)
              servoBlowerPosition.write(130);
        break;
      }
        
      case 1:
      {
        if (digitalRead(blowerLs1) == LOW)
          servoBlowerPosition.write(100);        
        else 
        {
          if(previousBlowerPosition >1)
          {
            if (digitalRead(blowerLs1) == HIGH)
              servoBlowerPosition.write(130);            
          }
          else
            if (digitalRead(blowerLs1) == HIGH)
              servoBlowerPosition.write(70);
        } 
          servoBlowerPosition.write(100);  
        break;
      }
      
      case 2:
      {
        if (digitalRead(blowerLs2) == LOW)
          servoBlowerPosition.write(100);        
        else
        {
          if(previousBlowerPosition >2)
          {
            if (digitalRead(blowerLs2) == HIGH)
              servoBlowerPosition.write(130);
          }
          else
           if   (previousBlowerPosition < 2)
          {
            if (digitalRead(blowerLs2) == HIGH)
              servoBlowerPosition.write(70);
          }
        }
          servoBlowerPosition.write(100);   
        break;
      }
        
      case 3:
      {
        if (digitalRead(blowerLs3) == LOW)
          servoBlowerPosition.write(100);        
        else
          if (digitalRead(blowerLs3) == HIGH)
            servoBlowerPosition.write(70);
        break;
      }
    }

//    blowerPositionPot = analogRead(blowerPotPin);
//    Serial.println(blowerPositionPot);
//    switch (blowerPosition)
//    {
//      case 0:
//      {
//        if (blowerPositionPot > 10)
//        {
//          while (blowerPositionPot > 10)
//          {
//            blowerPositionPot = analogRead(blowerPotPin);
//          //Serial.println("Case 0");
//            servoBlowerPosition.write(130);
//          }
//        }
//        break;
//      }
//        
//      case 1:
//      {
//        if (blowerPositionPot > 170) //310
//        {
//          while (blowerPositionPot > 170)
//          {
//            blowerPositionPot = analogRead(blowerPotPin);
//          //Serial.println("Case 1, 1");
//            servoBlowerPosition.write(130);
//          }        
//        }
//        if (blowerPositionPot < 130) //270
//        {
//          while (blowerPositionPot < 130)
//          {
//            blowerPositionPot = analogRead(blowerPotPin);
//          //Serial.println("Case 1, 2");
//            servoBlowerPosition.write(70);
//          }
//        }
//        break;
//      }
//      
//      case 2:
//      {
//        if (blowerPositionPot > 530) //710
//        {
//          while (blowerPositionPot > 530)
//          {
//            blowerPositionPot = analogRead(blowerPotPin);
//          //Serial.println("Case 2, 1");
//            servoBlowerPosition.write(130);
//          }
//        }
//        if (blowerPositionPot < 490)
//        {
//          while (blowerPositionPot < 490) //660
//          {
//            blowerPositionPot = analogRead(blowerPotPin);
//          //Serial.println("Case 2, 2");
//            servoBlowerPosition.write(70);
//          }          
//        }
//        break;
//      }
//      
//      case 3:
//      {
//        if (blowerPositionPot < 770) //1010)
//        {
//          while (blowerPositionPot < 770) //1010)
//          {
//            blowerPositionPot = analogRead(blowerPotPin);
//          //Serial.println("Case 3");
//            servoBlowerPosition.write(70);
//          }          
//        }
//        break;
//      }
//    }    
//    servoBlowerPosition.write(100);
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
    
    
      //  char c=' ';
//    //////Set desired temp /////////////////////////////
//    if (c=='a')
//    {
//      desiredTemp = desiredTemps[0];
//      tempDifference = getDiff(desiredTemp, actualTemp);
//    }
//    if (c=='s')
//    {
//      desiredTemp = desiredTemps[1];
//      tempDifference = getDiff(desiredTemp, actualTemp);
//    }
//    if (c=='d')
//    {
//      desiredTemp = desiredTemps[2];
//      tempDifference = getDiff(desiredTemp, actualTemp);
//    }
//    if (c=='f')
//    {
//      desiredTemp = desiredTemps[3];
//      tempDifference = getDiff(desiredTemp, actualTemp);
//    }
//    if (c=='g')
//    {
//      desiredTemp = desiredTemps[4];
//      tempDifference = getDiff(desiredTemp, actualTemp);
//    }
//    if (c=='h')
//    {
//      desiredTemp = desiredTemps[5];
//      tempDifference = getDiff(desiredTemp, actualTemp);
//    }
//    if (c=='j')
//    {
//      desiredTemp = desiredTemps[6];
//      tempDifference = getDiff(desiredTemp, actualTemp);
//    }
//    if (c=='k')
//    {
//      desiredTemp = desiredTemps[7];
//      tempDifference = getDiff(desiredTemp, actualTemp);
//    }
//    if (c=='l')
//    {
//      desiredTemp = desiredTemps[8];
//      tempDifference = getDiff(desiredTemp, actualTemp);
//    }
    ///////////////////////////////////////////////
  
  
  
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
  
//    Serial.print("Desired Temp = ");
//    Serial.print(desiredTemp);
//    Serial.print("         ");
//    Serial.print("Actual Temp = ");
//    Serial.print(actualTemp);
//    Serial.print("         ");
//    Serial.print("Difference = ");
//    Serial.print(tempDifference);
//    Serial.print("         ");
//    Serial.print("Motor Value = ");
//    Serial.print(i);
//    Serial.print("         ");
//    Serial.print("Fan Speed = ");
//    Serial.println(fanSpeedSetting);
    
    
    
//    if (c=='m')
//    {    
//      increaseTempPosition();
//    }
//    if (c=='z')
//    {
//      decreaseTempPosition();
//    }
//    if (c=='w')
//    {
//      setToMax();
//    }
//    if (c=='q')
//    {
//      setToMin();
//    }
//    if (c=='p')
//    {
//      i++;
//    }
//    if (c=='o')
//    {
//      i--;
//    }   
  }
  
  void increaseTempPosition()
  {
    servoTempControl.write(140);
    delay(70);
  }
  
  void decreaseTempPosition()
  {
    servoTempControl.write(60);
    delay(70);
  }
  
  void setToMax()
  {
    servoTempControl.write(120);
    delay(1400);
  }
  
  void setToMin()
  {
    servoTempControl.write(80);
    delay(1400);
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
  
  void setBlowerPosition()
  {
  }
  
//  int adjustFanSpeed(float tempDifference)
//  {
//    if (tempDifference >= 0 && tempDifference < 3)
//    {
//      fanSpeedSetting=1;
//      return 115;
//    }
//    if (tempDifference >= 3 && tempDifference < 5)
//    {
//      fanSpeedSetting=2;
//      return 90;
//    }
//    if (tempDifference >= 5 && tempDifference < 7)
//    {
//      fanSpeedSetting=3;
//      return 65;
//    }
//    if (tempDifference >= 7)
//    {
//      fanSpeedSetting=4;
//      return 40;
//    }
//  }



