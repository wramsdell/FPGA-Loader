import sys

if len(sys.argv) < 2:
    print "usage: %s filename" % (sys.argv[0])
    sys.exit(1)

bytesperline = 15

with open(sys.argv[1], "rb") as f:
    thislinecount = 0
    while True:
        thisbyte = f.read(1)
        if len(thisbyte) == 0:
            break
        thislinecount += 1
        if (thislinecount >= bytesperline):
            print "0x%02x," % (ord(thisbyte))
            thislinecount = 0
        else:
            print "0x%02x," % (ord(thisbyte)),
