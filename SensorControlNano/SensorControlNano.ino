#include "DHT.h"
#include <TEA5767.h>
#include <SimpleTimer.h>
#include <Wire.h>

byte arrayToSend[21];
#define DHTPINInside 6   // what pin we're connected to
#define DHTPINOutside 7
#define DHTTYPE DHT22   // DHT 22  (AM2302)
SimpleTimer timer;
SimpleTimer transmitTimer;
#define detectGearPin 3
byte incomingValues[3];

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

//Dection of reverse gear
union {
  int rev;
  char bytes[4];
} reverse;

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

//Radio Tuner variables
TEA5767 Radio;
int search_mode = 0;
int search_direction;
double oldFrequency;
double newFrequency=107.10;
byte autoTuneOn = false;
union{
  int sigLevel;
  char bytes[2];
} radioSignalLevel;

void setup() {
  Serial.begin(9600);  
  Wire.begin();
  Radio.init(); 
  Radio.set_frequency(newFrequency); 
  dhtIn.begin();
  dhtOut.begin();
  timer.setInterval(2000, getTemp);
  transmitTimer.setInterval(500, sendData);
  pinMode(3, OUTPUT);
  digitalWrite(3, LOW);
}

void loop() {
  if(digitalRead(detectGearPin) == HIGH)
    {
      //Serial.println("HIGH");
      reverse.rev=1;
    } 
    else
    {
      //Serial.println("LOW");
      reverse.rev=0;
    }

//    Serial.println(newFrequency);
//    if (Serial.available())
//    {
//      char c = Serial.read();
//      if (c == '.')
//      {
//        newFrequency += 0.05;
//      }
//      if (c == ',')
//      {
//        newFrequency -= 0.05;
//      }
//      updateFmRadio();
//    }
//    delay(300);

     timer.run();
     transmitTimer.run();
     
}

void sendData()
{
  //serialFlush();
  //outsideTemp.outTemp = 83.72;
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
  arrayToSend[12] = reverse.bytes[0];
  arrayToSend[13] = reverse.bytes[1];
  arrayToSend[14] = reverse.bytes[2];
  arrayToSend[15] = reverse.bytes[3];  
  arrayToSend[16] = radioSignalLevel.bytes[0];
  arrayToSend[17] = radioSignalLevel.bytes[1];
  for(int i=0; i<19; i++)
  {      
    Serial.write(arrayToSend[i]);//insideTemp.bytes);
  }
  getRadioFreq();
  updateFmRadio();   
}

void serialFlush(){
  while(Serial.available() > 0) {
    char t = Serial.read();
  }
}

void getRadioFreq()
{
  if(Serial.available()>=2)
    {
      for (int i=0;i<3;i++){
        incomingValues[i] = (byte)Serial.read();
      }
      //char *freqString, *stopstring; 
      char charBuf[50];
      int one = incomingValues[0];
      int two = incomingValues[1];
      String fre1 = String(one, DEC);
      String fre2 = String(two, DEC);
      String freqString = fre1 + '.' + fre2;
      freqString.toCharArray(charBuf, 50);
      newFrequency = atof(charBuf); //strtod(freqString, &stopstring);
      //autoTuneOn = incomingValues[2];
            
//     String freq = "104.55";  
//     double fre = atof(charBuf);
//      Serial.println(fre1);
//      Serial.println(fre2);
//      Serial.println(freqString);
//     Serial.println(newFrequency);
//     Serial.println();
    }
    else
    {
      serialFlush();
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
  readBatteryVoltage();
}

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
