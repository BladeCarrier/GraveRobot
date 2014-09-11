from wrapper import *
#debug section
print '!mark: test start'
sy = getSystem()
bit0 = Bitmap(width,height)

bit1 = Bitmap(width,height)
bit1.SetPixel(2,2,Color.Red)

bit2 = Bitmap(width,height)
for i in range(width):
    for j in range(height):
        bit2.SetPixel(i,j,Color.White)

bit3 = Bitmap(width,height)
for i in range(width):
    for j in range(height):
        if j<=height/2: bit3.SetPixel(i,j,Color.Green)
        else: bit3.SetPixel(i,j,Color.AliceBlue)
    

import helpers
sysLog = helpers.logWriter()
sysDump = helpers.logWriter()
flog = open("lastLog.log",'w')
fdump = open("lastDump.log",'w')

sy.setLogWriter(sysLog)
sy.setDumpWriter(sysDump)
for i in range(100):
    print sy._injectedEncoder.decodeResult(sy.classify(sampleFromBitmap(bit0)))
    print sy._injectedEncoder.decodeResult(sy.classify(sampleFromBitmap(bit1)))
    print sy._injectedEncoder.decodeResult(sy.classify(sampleFromBitmap(bit2)))
    print sy._injectedEncoder.decodeResult(sy.classify(sampleFromBitmap(bit3)))

    sysLog.writeLine( '\nround #',i)
    sysDump.writeLine('\n\n\n\nBeginning round #',i)
        
    feedSamplesToSystem(sy,{bit0:3,bit1:2,bit2:1,bit3:4})
    #feedSamplesToSystem(sy,{bit2:1})
    sy._injectedEncoder.updateSystem(sy)
    sy.fullLoop()

    sysLog.writeLine('End of round',i)
    
    s = sysLog.readString()
    print s[:-1]#all but for last /n to fit the screen
    flog.write(s);  
    fdump.write(sysDump.readString())

    sysDump.removeOld()
    sysLog.removeOld() #remove me if you don't want logs to wipe after printed
        
flog.close()
fdump.close()

