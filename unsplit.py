import os
import sys

rootDirectory = sys.argv[1]
outputFilename = sys.argv[2]
maximumSize = 54702
skipSize = 4
pageSize = 264

filenames = filter(lambda x: x.startswith("Bin_Bitstream.bin."), os.listdir(rootDirectory))

totalBytesWritten = 0

with open(outputFilename, "wb") as outputStream:
    for filename in filenames:
        finalFilename = rootDirectory + "/" + filename
        print finalFilename
        with open(finalFilename, "rb") as inputStream:
            entireFile = inputStream.read();

        if (len(entireFile) % (skipSize + pageSize)) != 0:
            print "Yaaaaa!"

        for offset in range(skipSize, len(entireFile), (skipSize + pageSize)):
            bytesToWrite = min(pageSize, maximumSize - totalBytesWritten)
            outputStream.write(entireFile[offset:offset + bytesToWrite])
            totalBytesWritten += bytesToWrite
