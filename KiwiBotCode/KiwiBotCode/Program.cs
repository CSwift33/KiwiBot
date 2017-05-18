using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

//  AP Physics E&M Final Project: Kiwi Bot
//  Code for Kiwi Bot

//  Conner Swift
//  Nathan Petrie
//  Sam Battalio
//  Chris Dell
//  Frank Salek
//  Conrad Adams
//  Adam Dewey
//  Blake Witchie

namespace KiwiBotCode
{
    public class Program
    {
        //////////////////////  Variables and Objects for Motors  //////////////////////
        //  Talons IDs of the TalonSRXs
        const int FRONT_WHEEL_TALON_ID = 2;
        const int BACK_RIGHT_TALON_ID = 1;
        const int BACK_LEFT_TALON_ID = 3;

        //  Booleans for Invertiong the Motors
        const bool FRONT_MOTOR_INVERTED = true;
        const bool BACK_RIGHT_MOTOR_INVERTED = true;
        const bool BACK_LEFT_MOTOR_INVERTED = true;

        //  Creating an object for the Motors with their specified Talon IDs
        static CTRE.TalonSrx frontMotor = new CTRE.TalonSrx(FRONT_WHEEL_TALON_ID);
        static CTRE.TalonSrx backRightMotor = new CTRE.TalonSrx(BACK_RIGHT_TALON_ID);
        static CTRE.TalonSrx backLeftMotor = new CTRE.TalonSrx(BACK_LEFT_TALON_ID);

        //  Variables for Determining the Motor Power For Each Motor
        static float frontWheelMotorPower = 0.0f;
        static float backRightMotorPower = 0.0f;
        static float backLeftMotorPower = 0.0f;

        //  Variables for determining how fast the robot should be rotating
        const float MAX_ROTATING_MOTOR_POWER = .5f;
        static float rotatingMotorPower = 0.0f;

        //  Variables for determining how fast the robot should be steering
        const float MAX_STEERING_MOTOR_POWER = .35f;
        static float steeringMotorPower = 0.0f;

        ////////////////  Variables and Objects for Controller Inputs  /////////////////
        //  Creating an object to receive inputs from the Controller
        static CTRE.Gamepad controller = new CTRE.Gamepad(new CTRE.UsbHostDevice());

        //  Indexes of the Joysticks
        const int LEFT_Y_AXIS_INDEX = 1;
        const int LEFT_X_AXIS_INDEX = 0;
        const int RIGHT_Y_AXIS_INDEX = 5;
        const int RIGHT_X_AXIS_INDEX = 2;

        //  Button Numbers on Joysticks
        const int buttonXIndex = 1;
        const int buttonAIndex = 2;
        const int buttonBIndex = 3;
        const int buttonYIndex = 4;

        const int buttonLBIndex = 5;
        const int buttonRBIndex = 6;
        const int buttonLTIndex = 7;
        const int buttonRTIndex = 8;

        const int buttonSelect = 9;
        const int buttonStart = 10;

        //  Boolean for determining whether the Select Button is Pressed.
        //  Button is used for switching between the Driving Modes
        static bool selectButtonPressed = false;
        const bool ARCADE_DRIVE = true;
        const bool DRIVING_WITH_STEERING = false;
        static bool arcadeDriveSelection = ARCADE_DRIVE;

        //  Variables for the Values for the Joysticks
        static float deadbandedLeftJoystickYAxisValue = 0.0f;
        static float deadbandedLeftJoystickXAxisValue = 0.0f;
        static float deadbandedRightJoystickXAxisValue = 0.0f;

        //  Deadband Value for the Joystick
        const float JOYSTICK_DEADBAND_VALUE = .1f;  //  TBD

        //////////////////////// Function to Invert the Motors  ////////////////////////
        static void InitializeMotors()
        {
            //  Inverting Each Motors If Necessary
            //  True -> Invert
            //  False -> Don't Invert
            frontMotor.SetInverted(FRONT_MOTOR_INVERTED);
            backRightMotor.SetInverted(BACK_RIGHT_MOTOR_INVERTED);
            backLeftMotor.SetInverted(BACK_LEFT_MOTOR_INVERTED);
        }

        ///////////////////////// Function to Stop All Motors  /////////////////////////
        static void StopAllMotors()
        {
            //  Stopping Each Individual Motor
            frontMotor.Set(0.0f);
            backRightMotor.Set(0.0f);
            backLeftMotor.Set(0.0f);
        }

        ////////  Function to Deadband the Joysticks Given the Joystick Value  /////////
        static float DeadbandJoystick(float joystickValue)
        {
            //  Range of Joysticks is from -1.0 to +1.0
            //  If the Joystick is between the Deadband Value, don't have the program recognize the small value
            //  This is necessary because naturally, the joysticks are going to be some small value.
            //  The joysticks value will not always be at zero when it's not moved, just a value extremely close to zero
            if (joystickValue < JOYSTICK_DEADBAND_VALUE && joystickValue > -JOYSTICK_DEADBAND_VALUE)
            {
                joystickValue = 0.0f;
            }
            return joystickValue;
        }

        ////  Function to Drive Robot With Magnitude and Direction of Left Joystick  ///
        static void DriveKiwiBotWithJoystickValuesNoRotation(float leftJoystickYAxis, float leftJoystickXAxis)
        {
            //  Individual Motor Powers Determined with Physics Equations
            frontWheelMotorPower = leftJoystickXAxis;
            backRightMotorPower = ((float)(((-1.0 * ((System.Math.Sqrt(3.0)) / (2.0))) * leftJoystickYAxis) - (.5 * leftJoystickXAxis)));
            backLeftMotorPower = ((float)((((System.Math.Sqrt(3.0)) / (2.0)) * leftJoystickYAxis) - (.5 * leftJoystickXAxis)));

            //  Run the motors at their desired Motor Power
            frontMotor.Set(frontWheelMotorPower);
            backRightMotor.Set(backRightMotorPower);
            backLeftMotor.Set(backLeftMotorPower);
        }
        ////////////////////////////////////////////////////////////////////////////////
        ///////  Function to Rotate Robot with the X Axis of the Right Joystick  ///////
        static void RotateRobot(float rightJoystickXAxis)
        {
            //  If the Right Joystick is directed to the right, rotate the robot right
            //  If the Right Joystick is directed to the left, rotate the robot left

            rotatingMotorPower = (rightJoystickXAxis * MAX_ROTATING_MOTOR_POWER);

            frontMotor.Set(rotatingMotorPower);
            backRightMotor.Set(rotatingMotorPower);
            backLeftMotor.Set(rotatingMotorPower);
        }
        ////////////////////////////////////////////////////////////////////////////////
        ////////////  Function to Drive and Turn the Robot simultaneously  /////////////
        static void DriveRobotWithSteeringControl(float deadbandedLeftJoystickYAxis, float deadbandedRightJoystickXAxis)
        {
            //  The two Back Motors will drive the robot forward
            backRightMotor.Set(-deadbandedLeftJoystickYAxis);
            backLeftMotor.Set(deadbandedLeftJoystickYAxis);

            //  The Front wheel will control the steering of the robot
            //  Note: The motor power for steering control is scaled down
            steeringMotorPower = (deadbandedRightJoystickXAxis * MAX_STEERING_MOTOR_POWER);
            frontMotor.Set(steeringMotorPower);
        }

        //////////////////  Function where all the code is executed  ///////////////////
        public static void Main()
        {
            //  Invert the Motors to the correct direction
            InitializeMotors();

            //  Everything in the While Loop will continuously run until the robot is turned off
            while (true)
            {
                //  If the Controller is connected to the HERO Development Board, the user can drive the robot
                if (controller.GetConnectionStatus() == CTRE.UsbDeviceConnection.Connected)
                {
                    //  Get the Joystick Values and Deadband them
                    //  Since moving the Joysticks up on the y-axis is in the negative direction, the value of the 
                    //  left Joystick Y Axis is multiplied by -1 so it's treated as being positive
                    deadbandedLeftJoystickYAxisValue = -DeadbandJoystick(controller.GetAxis(LEFT_Y_AXIS_INDEX));
                    deadbandedLeftJoystickXAxisValue = DeadbandJoystick(controller.GetAxis(LEFT_X_AXIS_INDEX));
                    deadbandedRightJoystickXAxisValue = DeadbandJoystick(controller.GetAxis(RIGHT_X_AXIS_INDEX));

                    //  Button used to switch between different Driving Modes
                    //  The Button is programmed as a toggle between the two Driving Modes
                    if (controller.GetButton(buttonSelect))
                    {
                        if (selectButtonPressed == false)
                        {
                            arcadeDriveSelection = !arcadeDriveSelection;
                            selectButtonPressed = true;
                        }
                    }
                    else
                    {
                        selectButtonPressed = false;
                    }

                    if (arcadeDriveSelection == ARCADE_DRIVE)
                    {
                        //  If the Right X Axis Joystick Value does not equal zero, the robot will rotate.
                        //  Control with the Left Joystick will be disabled if the user decides to rotate the robot
                        //  This essentially allows the user to rotate the robot if he/she moves the Right X Axis Joystick Value out
                        //  of the Deadband
                        if (deadbandedRightJoystickXAxisValue != 0.0f)
                        {
                            RotateRobot(deadbandedRightJoystickXAxisValue);
                        }
                        //  If the Right X Axis Joystick Value is zero, control the direction and magnitude of the speed
                        //  of the robot with the Left Joystick
                        else
                        {
                            DriveKiwiBotWithJoystickValuesNoRotation(deadbandedLeftJoystickYAxisValue, deadbandedLeftJoystickXAxisValue);
                        }
                    }
                    else if (arcadeDriveSelection == DRIVING_WITH_STEERING)
                    {
                        DriveRobotWithSteeringControl(deadbandedLeftJoystickYAxisValue, deadbandedRightJoystickXAxisValue);
                    }
                }
                //  If the Controller is not connected to the HERO Development Board, the user cannot move the robot
                else
                {
                    StopAllMotors();
                }

                //  Update the Motor Powers calculated in the above code
                CTRE.Watchdog.Feed();

                //  Wait 10ms
                //  All the code in the While Loop will execute every 10ms
                System.Threading.Thread.Sleep(10);
            }
        }
    }
}
