import os
import sys

if len(sys.argv) < 3:
    print "usage: %s filename part_size" % (sys.argv[0])
    sys.exit(1)

part_size = int(sys.argv[2])
current_file = 1

with open(sys.argv[1], "rb") as f:
    while True:
        pathname = sys.argv[1] + "." + str(current_file)
        read_data = f.read(part_size)
        if (len(read_data) == 0):
            break
        print "%s" % (pathname)
        with open(pathname, "wb") as out_f:
            out_f.write(read_data)
        current_file += 1
