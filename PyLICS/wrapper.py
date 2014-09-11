
#configuring path
import sys
import System
pyDir = ''
if System.IntPtr.Size * 8 == 32: # detect if we are running on 32bit process
    pyDir = System.Environment.GetEnvironmentVariable("ProgramFiles") + "\IronPython 2.7"
else:
    pyDir = System.Environment.GetEnvironmentVariable("ProgramFiles(x86)") + "\IronPython 2.7"

oldpath = sys.path
progDir = sys.path[1]
progDir = progDir[::-1]
while True:
    progDir = progDir[progDir.index("\\")+1:]
    if progDir[:3] == "raC" or progDir[:6] == "SCILyP":
        progDir = progDir[progDir.index("\\")+1:]
        break #car, pyLics, sorry
progDir = progDir[::-1]

sys.path = [progDir+"\\PyLICS",
            progDir+"\\PyLICS\\classifier",
            pyDir+"\\Lib",
            pyDir+"\\DLLs",
            pyDir,
            pyDir+'\\lib\\site-packages']
#adding .net modules
import clr
clr.AddReferenceByPartialName("System")
clr.AddReferenceByPartialName("System.Drawing")
clr.AddReferenceByPartialName("IronPython")
clr.AddReferenceByPartialName("Microsoft.Scripting")
from System.Drawing import Bitmap, Color


#wrapping classifier
import classifier.treeNode as treeNode

import classifier.samples as samples
import classifier.statements as statements
import classifier.system as system
import classifier.helpers as helpers
width = 6
height = 6


import random
import copy


def colorIntensity(color):
    return (color.R + color.G + color.B )/(3.*255.)
def sampleFromBitmap( bitmap):

    r = samples.sample([],[])
    imin=rmin = gmin = bmin = 1
    imax =rmax = gmax = bmax = 0
    isum = rsum = gsum = bsum = 0
    
    for i in range(width):
        for j in range(height):
            pixel = bitmap.GetPixel(i,j)
            intense = colorIntensity(pixel)
            
            #setattr(r, 'intensity_'+str(i)+'_'+str(j), intense)
            #setattr(r, 'colorR_'+str(i)+'_'+str(j), pixel.R)
            #setattr(r, 'colorG_'+str(i)+'_'+str(j), pixel.G)
            #setattr(r, 'colorB_'+str(i)+'_'+str(j), pixel.B)
            if intense< imin: imin = intense
            if intense> imax: imax = intense
            isum+= intense
            if pixel.R< rmin: rmin = pixel.R
            if pixel.R> imax: rmax = pixel.R
            rsum+= pixel.R
            if pixel.G< rmin: rmin = pixel.G
            if pixel.G> imax: rmax = pixel.G
            gsum+= pixel.G
            if pixel.B< rmin: rmin = pixel.B
            if pixel.B> imax: rmax = pixel.B
            bsum+= pixel.B
            
    setattr(r,'MinIntensity',imin)
    setattr(r,'MaxIntensity',imax)
    setattr(r,'AvgIntensity',isum/width/height)

    setattr(r,'MinBlue',bmin)
    setattr(r,'MaxBlue',bmax)
    setattr(r,'Blue',bsum/width/height)

    setattr(r,'MinGreen',gmin)
    setattr(r,'MaxGreen',gmax)
    setattr(r,'Green',gsum/width/height)

    setattr(r,'MinRed',rmin)
    setattr(r,'MaxRed',rmax)
    setattr(r,'Red',rsum/width/height)
    return r
def getSystem():

    en = encoder([],'weight')


    nums = ['MinIntensity','MaxIntensity','AvgIntensity','Red','Green','Blue']
    #nums +=['intensity_'+str(i)+'_'+str(j)    for i in range(width) for j in range(height)]
    #nums +=['colorR_'+str(i)+'_'+str(j)    for i in range(width) for j in range(height)]
    #nums +=['colorG_'+str(i)+'_'+str(j)    for i in range(width) for j in range(height)]
    #nums +=['colorB_'+str(i)+'_'+str(j)    for i in range(width) for j in range(height)] comment reason: this tree version has shown poor test results in performance per numStatement
    


    
    numparams= nums
    boolparams = []
    keyparams = list(en.boolNames)
    bit1 = Bitmap(width,height)
    for i in range(width):
        for j in range(height):
            bit1.SetPixel(i,j,Color.Black)

    bit2 = Bitmap(width,height)
    for i in range(width):
        for j in range(height):
            bit2.SetPixel(i,j,Color.White)
    slist = [sampleFromBitmap(bit2) for i in range(1)]
    for sample in slist: en.encode(sample,1)
    sadd = [sampleFromBitmap(bit1) for i in range(1)]
    for sample in sadd: en.encode(sample,1.1)
    slist += sadd   
    sys = system.system(keyparams,boolparams,numparams,slist,1500,100,7,majorant = True)
    sys.initialise()
    sys._injectedEncoder = en
    
    sys.setLogWriter(helpers.logWriter())


    #sys.compose()
    return sys
from math import log
def systemAddNewKeyState(system,state):
    for chromo in system.treesPool:
        treeAddNewKeyState(chromo.tree,state)
        chromo.keyStatements.add(state)
    system.keyStatements.append(state)
    
def treeAddNewKeyState(tree,state):
    tree.keyStatements.add(state)
    tree.result[state] = 0
    if not tree.isTerminal:
        treeAddNewKeyState(tree.childPositive,state)
        treeAddNewKeyState(tree.childNegative,state)
def roundup(x):
    v = int(x)
    if x>v:v+=1
    return v
class encoder:
    #encodes a discrete variable into several booleans
    def __init__(self,values,name):
        if len(values) <=1:
            self.numVars = 1
        else:
            self.numVars = roundup(log(len(values),2))
        self.values = values
        self.name = name
        self.boolNames = [name+str(i) for i in range(self.numVars)]
        self.code={i:values[i] for i in self.values}
        self.item={values[i]:i for i in self.values}

    def addValue(self,val):
        if val in self.values:
            raise ValueError( 'Existent value')
        self.item[len(self.values)] = val
        self.code[val] =len(self.values)
        self.values.append(val)
        if roundup(log(len(self.values),2)) > self.numVars:
            self.numVars +=1
            self.boolNames.append(self.name+str(len(self.boolNames)))
        
                        
            
    def updateSystem(self, system):
        '''propagates new states to the system and it's trees'''
        #why: samples addition and updates are done in parallel (in NeuroCar proj at least), so not doint this
        #may result in losing a bool name to update(crash almost inevitable) if it was added while this function was called.
        
        for name in self.boolNames:
            if name not in system.keyParams:
                state=statements.get_statement(statements.op_takeValue,[name])
                systemAddNewKeyState(system,state)
                system.keyParams.append(name)
    def updateChromo(self, chromo):
        '''propagates new states to the system and it's trees'''
        #why: samples addition and updates are done in parallel (in NeuroCar proj at least), so not doint this
        #may result in losing a bool name to update(crash almost inevitable) if it was added while this function was called.
        
        for name in self.boolNames:
            state=statements.get_statement(statements.op_takeValue,[name])
            if state not in chromo.keyStatements:
                treeAddNewKeyState(chromo.tree,state)
                chromo.keyStatements.add(state)

    def encode (self,sample,value):
        if value not in self.values:
            self.addValue(value)
        v= self.code[value]
        cnt = 0
        while v >=1:
            setattr(sample,self.boolNames[cnt],v%2)
            v//=2
            cnt +=1
        while cnt<self.numVars:
            setattr(sample,self.boolNames[cnt],0)
            cnt+=1
            
    def decodeResult(self, result):
        #accepts the list of [result.boolNames[i] for i in boolnames]
        key = 0
        for i in range(len(self.boolNames)):
            state = statements.get_takeValue(self.boolNames[i])
            if state not in result: continue
            if result[state]: key += 2**i
        return self.item[key]

    def decodeSample(self,sample):
        for name in self.boolNames:
            if name not in dir(sample):
                setattr(sample,name,0)
        vals = [getattr(sample,name) for name in self.boolNames]
        key =0
        for i in range(len(vals)):
            if vals[i]: key += 2**i
        return self.item[key]

    def visualise(self):
        return str(self.code)+'\n'+str(self.item)+'\n'+str(self.boolNames)

def feedSamplesToSystem(sys, targetWeightsDict):
    samplesWeightsDict = {sampleFromBitmap(bitmap):targetWeightsDict[bitmap] for bitmap in targetWeightsDict}
    enc = sys._injectedEncoder
    samples =[]
    for sample in samplesWeightsDict:
        enc.encode(sample,samplesWeightsDict[sample])
        samples.append(sample)
    sys.processSamples(samples)

def getTree():
    dich1 = statements.get_negation(statements.get_moreThan(statements.get_statement(statements.op_takeValue,['MinIntensity']),0.7))
    dich11 = statements.get_negation(statements.get_moreThan(statements.get_statement(statements.op_takeValue,['MinIntensity']),0.35))
    
    tree = treeNode.treeNode([],[])
    tree.dichotomy = dich1
    tree.isTerminal = False
    tree.childNegative = treeNode.treeNode([],[])
    tree.childNegative.result = [False,False]
    tree.childPositive = treeNode.treeNode([],[])
    tree.childPositive.dichotomy = dich11
    tree.childPositive.isTerminal = False
    tree.childPositive.childNegative = treeNode.treeNode([],[])
    tree.childPositive.childNegative.result = [True,False]
    tree.childPositive.childPositive = treeNode.treeNode([],[])
    tree.childPositive.childPositive.result = [True,True]

    return tree

sys.path = oldpath