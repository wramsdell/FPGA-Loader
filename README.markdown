Prototype Engineering, LLC Netduino FPGA Shield
===============================================

This is the source package for the Prototype Engineering FPGA shield.

Fundamentals Of Operation
-------------------------

The Prototype Engineering Netduino FPGA Shield uses a Xilinx 3S50AN FPGA. The ISF (In-System Flash) of the device has enough space for two bitstreams, so one bitstream is used as a "bootloader" to allow the user to upload a user-specified bitstream using the Netduino.

Prerequisites
-------------

In order to get up and running, you will need:

1. A running [Netduino / Netduino Plus](http://netduino.com) development environment.
2. A 1Mbit bitstream targeted for a Xilinx 3S50AN. This can be in either a .bit or a .bin format.
3. A [snapshot of the code](https://github.com/wramsdell/FPGA-Loader) from Github.
4. The Prototype Engineering Netduino FPGA shield installed on your Netduino

Loading a User Bitstream
------------------------

1. Prepare the bitstream with the Filesplit.exe tool. Your current directory must be the root of the FPGA Shield software distribution (that contains this file).
2. Open the FpgaFlashLoader\FpgaFlashLoader.sln solution in Visual Studio.
3. Rebuild the project.
4. Unplug your Netduino.
5. Holding the button down on the FPGA Shield (NOT on the Netduino), plug your Netduino back in, and release the button. This should put the FPGA in bootloader mode.
6. Deploy the FpgaFlashLoader program. Your bitstream is embedded and will be automatically uploaded.
7. The Netduino LED should light when the program is running. The red and green LED on the FPGA Shield should light while programming.
8. When the green LED on the FPGA Shield is lit by itself, that's your indicator that the FPGA programming was successful. If you unplug and replug the Netduino, the FPGA should be using your uploaded bitstream.
9. If the red LED on the FPGA shield is lit by itself, it indicates an error while programming. Consult the debug output in Visual Studio or contact us to help diagnose the problem. Also, try running the FpgaFlashLoader again.
10. If there is no indication on the FPGA Shield LEDs when running the FpgaFlashLoader program, and the Netduino LED flashes, this means you're not in bootloader mode. Try step 5 again, waiting longer before releasing the button.
