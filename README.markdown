Prototype Engineering, LLC Netduino FPGA Shield
===============================================

This is the source package for the Prototype Engineering FPGA shield.

Fundamentals Of Operation
-------------------------

The Prototype Engineering Netduino FPGA Shield uses a Xilinx 3S50AN FPGA. The ISF (In-System Flash) of the device has enough space for two bitstreams, so one bitstream is used as a "bootloader" to allow the user to upload a user-specified bitstream using the Netduino.

Prerequisites
-------------

In order to get up and running, you will need:

1. A running Netduino / Netduino Plus development environment. http://netduino.com
2. A 1Mbit bitstream targeted for a Xilinx 3S50AN. This should be in the form of a single 54,702 byte file.
3. A snapshot of the code from Github. https://github.com/wramsdell/FPGA-Loader
4. The Prototype Engineering Netduino FPGA shield installed on your Netduino
