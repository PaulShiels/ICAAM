
  #include <Servo.h> 
  #include "DHT.h"
  #define DHTPIN 2   // what pin we're connected to
  #define DHTTYPE DHT22   // DHT 22  (AM2302)
  
  // Initialize DHT sensor for normal 16mhz Arduino
  DHT dht(DHTPIN, DHTTYPE);
  
  // create servo objects to control a servos
  Servo servoTempControl;  
  Servo servoFanSpeed;
  
  //int pos = 0;    // variable to store the servo position 
  //Temperature Conrol Variables
  int i = 100; //70
  boolean atMax = false;
  boolean atMin = false;
  int desiredTemps[] = {10, 11, 15, 17, 18, 19, 20, 24, 28};
  int desiredTemp = 0;
  float actualTemp = 0;
  int tempDifference =0;
  int loopCounter = 0;
  int adjustTempControlLoopCounter = 0;
  
  //Fan Speed Variables
  int fanSpeedSetting = 50; //Angle at which to set servo arm. 50 in this case is the fan speed at 0
  boolean userHasControl = false; //Used to allow the fan speed to be controlled automatically to allow the temperature to be changed quicker
  //The fan can be noisey and the user may want to turn it down while still wanting the temperature adjusted.
  
  void setup() 
  { 
    Serial.begin(9600);
    servoTempControl.attach(9);  // attaches the servo on pin 9 to the servo object 
    servoFanSpeed.attach(11);
    dht.begin();
    actualTemp = getTemp();  
  } 
  
  
  void loop() 
  { 
    char c = Serial.read();
  
    //////Set desired temp /////////////////////////////
    if (c=='a')
    {
      desiredTemp = desiredTemps[0];
      tempDifference = getDiff(desiredTemp, actualTemp);
    }
    if (c=='s')
    {
      desiredTemp = desiredTemps[1];
      tempDifference = getDiff(desiredTemp, actualTemp);
    }
    if (c=='d')
    {
      desiredTemp = desiredTemps[2];
      tempDifference = getDiff(desiredTemp, actualTemp);
    }
    if (c=='f')
    {
      desiredTemp = desiredTemps[3];
      tempDifference = getDiff(desiredTemp, actualTemp);
    }
    if (c=='g')
    {
      desiredTemp = desiredTemps[4];
      tempDifference = getDiff(desiredTemp, actualTemp);
    }
    if (c=='h')
    {
      desiredTemp = desiredTemps[5];
      tempDifference = getDiff(desiredTemp, actualTemp);
    }
    if (c=='j')
    {
      desiredTemp = desiredTemps[6];
      tempDifference = getDiff(desiredTemp, actualTemp);
    }
    if (c=='k')
    {
      desiredTemp = desiredTemps[7];
      tempDifference = getDiff(desiredTemp, actualTemp);
    }
    if (c=='l')
    {
      desiredTemp = desiredTemps[8];
      tempDifference = getDiff(desiredTemp, actualTemp);
    }
    ///////////////////////////////////////////////
  
    ////////////////Adjust controls according to Temperature Difference //////////////////////
    if (adjustTempControlLoopCounter > 19)
    {
      if (tempDifference >=1)
      {
        if (!userHasControl)
        {
          //fanSpeedSetting = adjustFanSpeed(tempDifference);
          servoFanSpeed.write(adjustFanSpeed(tempDifference));
        }
        if (desiredTemp > actualTemp)
        {
          increaseTempPosition();
        }
        if (desiredTemp < actualTemp)
        {
          decreaseTempPosition();
        }
      }
      adjustTempControlLoopCounter = 0;  
    }
    else
    {
      adjustTempControlLoopCounter++;
    }
    //////////////////////////////////////////////////////////////////////////////////////////
  
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
  
    Serial.print("Desired Temp = ");
    Serial.print(desiredTemp);
    Serial.print("         ");
    Serial.print("Actual Temp = ");
    Serial.print(actualTemp);
    Serial.print("         ");
    Serial.print("Difference = ");
    Serial.print(tempDifference);
    Serial.print("         ");
    Serial.print("Motor Value = ");
    Serial.print(i);
    Serial.print("         ");
    Serial.print("Fan Speed = ");
    Serial.println(fanSpeedSetting);
  
    servoTempControl.write(i);
  
    if (loopCounter >50)
    {
      actualTemp = getTemp();
      loopCounter=0;
    }
    else
    {
      //   delay(300);
      loopCounter++;
    }
    
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
  
  int adjustFanSpeed(float tempDifference)
  {
    if (tempDifference >= 0 && tempDifference < 3)
    {
      fanSpeedSetting=1;
      return 115;
    }
    if (tempDifference >= 3 && tempDifference < 5)
    {
      fanSpeedSetting=2;
      return 90;
    }
    if (tempDifference >= 5 && tempDifference < 7)
    {
      fanSpeedSetting=3;
      return 65;
    }
    if (tempDifference >= 7)
    {
      fanSpeedSetting=4;
      return 40;
    }
  }



