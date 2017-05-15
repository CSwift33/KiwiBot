using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace KiwiBotCode
{
    public class Program
    {
        //////////////////////  Variables and Objects for Motors  //////////////////////
        //  Talons IDs of the TalonSRXs
        const int FRONT_WHEEL_TALON_ID = 1;
        const int BACK_RIGHT_TALON_ID = 2;
        const int BACK_LEFT_TALON_ID = 3;

        //  Booleans for Inverted the Motors
        const bool FRONT_MOTOR_INVERTED = false;
        const bool BACK_RIGHT_MOTOR_INVERTED = false;
        const bool BACK_LEFT_MOTOR_INVERTED = false;

        //  Creating an object for the Motors with their specified Talon IDs
        static CTRE.TalonSrx frontMotor = new CTRE.TalonSrx(FRONT_WHEEL_TALON_ID);
        static CTRE.TalonSrx backRightMotor = new CTRE.TalonSrx(BACK_RIGHT_TALON_ID);
        static CTRE.TalonSrx backLeftMotor = new CTRE.TalonSrx(BACK_LEFT_TALON_ID);

        //  Variables for Determining the Motor Power For Each Motor
        static float overallMotorPowerFactor = 0.0f;

        static float frontWheelMotorPower = 0.0f;
        static float backRightMotorPower = 0.0f;
        static float backLeftMotorPower = 0.0f;

        static float factoredFrontWheelMotorPower = 0.0f;
        static float factoredBackRightMotorPower = 0.0f;
        static float factoredBackLeftMotorPower = 0.0f;
        ////////////////////////////////////////////////////////////////////////////////
        ////////////////  Variables and Objects for Controller Inputs  /////////////////
        //  Creating an object to receive inputs from the Controller
        static CTRE.Gamepad controller = new CTRE.Gamepad(new CTRE.UsbHostDevice());

        //  Indexes of the Joysticks
        const int LEFT_Y_AXIS_INDEX = 1;
        const int LEFT_X_AXIS_INDEX = 0;
        const int RIGHT_Y_AXIS_INDEX = 5;
        const int RIGHT_X_AXIS_INDEX = 2;

        //  Variables for the Values for the Joysticks
        static float deadbandedLeftJoystickYAxisValue = 0.0f;
        static float deadbandedLeftJoystickXAxisValue = 0.0f;
        static float deadbandedRightJoystickXAxisValue = 0.0f;

        //  Deadband Value for the Joystick
        const float JOYSTICK_DEADBAND_VALUE = .1f;
        ////////////////////////////////////////////////////////////////////////////////
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

        ////////////////////////////////////////////////////////////////////////////////
        ///////////////////////// Function to Stop All Motors  /////////////////////////
        static void StopAllMotors()
        {
            //  Stopping Each Individual Motor
            frontMotor.Set(0.0f);
            backRightMotor.Set(0.0f);
            backLeftMotor.Set(0.0f);
        }

        ////////////////////////////////////////////////////////////////////////////////
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

        ////////////////////////////////////////////////////////////////////////////////
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

        static void RotateRobot(float rightJoystickXAxis)
        {
            frontMotor.Set(rightJoystickXAxis);
            backRightMotor.Set(rightJoystickXAxis);
            backLeftMotor.Set(rightJoystickXAxis);
        }

        public static void Main()
        {

            InitializeMotors();

            while (true)
            {
                if (controller.GetConnectionStatus() == CTRE.UsbDeviceConnection.Connected)
                {
                    deadbandedRightJoystickXAxisValue = DeadbandJoystick(controller.GetAxis(RIGHT_X_AXIS_INDEX));
                    if (deadbandedRightJoystickXAxisValue != 0.0f)
                    {
                        RotateRobot(deadbandedRightJoystickXAxisValue);
                    }
                    else
                    {
                        deadbandedLeftJoystickYAxisValue = DeadbandJoystick(controller.GetAxis(LEFT_Y_AXIS_INDEX));
                        deadbandedLeftJoystickXAxisValue = DeadbandJoystick(controller.GetAxis(LEFT_X_AXIS_INDEX));
                        DriveKiwiBotWithJoystickValuesNoRotation(deadbandedLeftJoystickYAxisValue, deadbandedLeftJoystickXAxisValue);
                    }
                }
                else
                {
                    StopAllMotors();
                }

                CTRE.Watchdog.Feed();

                System.Threading.Thread.Sleep(10);
            }
        }
    }
}
