/*
 * This program is ran on the arduino
 */

#include <string.h>
#include <Servo.h>

#define XPWM_PIN 11//X-axis motor speed
#define XDIR_PIN 12//X-axis motor direction
#define YPWM_PIN 5 //Y-axis servo pwm

#define G_PWM_PIN 9 //Gun pwm, might not need this
#define G_EN_PIN 4 //Gun enable
#define L_EN_PIN 7 //Light enable

//#define CAMFOC_PIN 10  //Camera Focus servo pwm
//#define CAMZOOM_PIN 3  //Camera Zoom  servo pwm
#define Y_DOWNLIMIT 3
#define Y_UPLIMIT 10

const byte numChars = 32;
char receivedChars[numChars];   //array to store the received data
boolean newData = false;

int xpwmSpeed = 0;
int ypwmSpeed = 90; //105-180-255 range in this servo
Servo yServo;

//Critique: It would probably be smarter to store all of these booleans bitshifted into an integer.
//          Then mask them out when needed.
boolean xdir = false;    //x motor direction
boolean genable = false; //gun enable
boolean lenable = false; //light enable
boolean lstrobo = false; //light strobo enable
boolean lstroboOut = false; //light strobo output

const long lstrobodelay = 28; //strobo timing
unsigned long lstrobotime = 0;


void setup() {
  Serial.begin(31250);
  pinMode(XPWM_PIN, OUTPUT);
  pinMode(XDIR_PIN, OUTPUT);
  pinMode(L_EN_PIN, OUTPUT);
  pinMode(G_EN_PIN, OUTPUT);
  pinMode(G_PWM_PIN, OUTPUT);
  analogWrite(XPWM_PIN, xpwmSpeed);
  yServo.attach(YPWM_PIN);
  yServo.write(ypwmSpeed);
  pinMode(Y_UPLIMIT, INPUT_PULLUP);
  pinMode(Y_DOWNLIMIT, INPUT_PULLUP);
}

void loop() {
  recvWithEndMarker();
  showNewData();
}

void recvWithEndMarker() {
  static byte ndx = 0;
  char endMarker = '\n';
  char rc;

  while (Serial.available() > 0 && newData == false)
  {
    rc = Serial.read();
    if (rc != endMarker)
    {
      receivedChars[ndx] = rc;
      ndx++;
      if (ndx >= numChars)
        ndx = numChars - 1;
    }
    else
    {
      ndx = 0;
      newData = true;
    }
  }

  analogWrite(XPWM_PIN, xpwmSpeed);
  if ((digitalRead(Y_UPLIMIT) == LOW && ypwmSpeed > 90) ||
      (digitalRead(Y_DOWNLIMIT) == LOW && ypwmSpeed < 90))
    ypwmSpeed = 90; //Limit hit, don't move further in that direction
  yServo.write(ypwmSpeed);
  digitalWrite(G_EN_PIN, genable);
  digitalWrite(G_PWM_PIN, genable);
  digitalWrite(XDIR_PIN, xdir);

  if (lstrobo == true) {
    unsigned long curtime = millis();
    if (curtime > lstrobotime + lstrobodelay) {
      lstroboOut = !lstroboOut;
      lstrobotime = curtime;
    }
    digitalWrite(L_EN_PIN, lstroboOut);
  }
  else
    digitalWrite(L_EN_PIN, lenable);
}

void showNewData() {
  if (newData == true)
  {
    //Critique: No reason for this to be a string, just send the data as an integer.
    char xpwmstr [4] = "000";
    xpwmstr[0] = receivedChars[1];
    xpwmstr[1] = receivedChars[2];
    xpwmstr[2] = receivedChars[3];
    xpwmSpeed = atoi(xpwmstr);

    char ypwmstr [4] = "090";
    ypwmstr[0] = receivedChars[6];
    ypwmstr[1] = receivedChars[7];
    ypwmstr[2] = receivedChars[8];
    ypwmSpeed = atoi(ypwmstr);

    char xdirc = receivedChars[5];
    xdir = (xdirc == 'r');

    char lenc = receivedChars[12];
    lenable = (lenc == 'e');
    lstrobo = (lenc == 's');

    char gen = receivedChars[9];
    genable = (gen == 'o');

    newData = false;
  }
}
