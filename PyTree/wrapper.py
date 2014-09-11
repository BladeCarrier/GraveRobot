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

sys.path = [progDir+"\\PyTree",
            progDir+"\\PyTree\\classifier",
            pyDir+"\\Lib",
            pyDir+"\\DLLs",
            pyDir,
            pyDir+'\\lib\\site-packages']

import clr
clr.AddReferenceByPartialName("System")
clr.AddReferenceByPartialName("System.Drawing")
clr.AddReferenceByPartialName("IronPython")
clr.AddReferenceByPartialName("Microsoft.Scripting")

from System.Drawing import Bitmap, Color
import classifier.learning as learning
import classifier.treeNode as treeNode
import classifier.samples as samples
import classifier.statements as statements
import classifier.system as system

width = 6
height = 6
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
    slist = []
    sys = system.system(keyparams,boolparams,numparams,slist,100,1000,True)

    sys._injectedEncoder = en

    bit2 = Bitmap(width,height)
    for i in range(width):
        for j in range(height):
            bit2.SetPixel(i,j,Color.White)

    feedSampleToSystem(sys,bit2,1)
    #sys.compose()
    return sys
from math import log
def propagateNewKeyState(tree,state):
    tree.keyStatements.insert(0,state)
    tree.result.insert(0,0)
    if not tree.isTerminal:
        propagateNewKeyState(tree.childPositive,state)
        propagateNewKeyState(tree.childNegative,state)
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
        self.code={}#cannot use dict comprehensions for ironpython (at least with standard syntax)
        for i in range(len(self.values)):
            self.code[i] = self.values[i]
        self.item={}#cannot use dict comprehensions for ironpython (at least with standard syntax)
        for i in range(len(self.values)):
            self.item[self.values[i]] = i

    def addValue(self,val,system = None):
        if val in self.values:
            raise ValueError( 'Existent value')
        self.item[len(self.values)] = val
        self.code[val] =len(self.values)
        self.values.append(val)
        if roundup(log(len(self.values),2)) > self.numVars:
            self.numVars +=1
            self.boolNames.append(self.name+str(len(self.boolNames)))
            if system!=None:
                state=statements.get_statement(statements.op_takeValue,[self.boolNames[self.numVars -1]])
                system.keyStatements.insert(0,state)
                propagateNewKeyState(system.tree,state)
                for sample in system.samples:
                    setattr(sample,self.boolNames[-1],0)
        
                        
            
        
    def encode (self,sample,value, system = None):
        if value not in self.values:
            self.addValue(value,system)
        v= self.code[value]
        a = []
        cnt = 0
        while v >=1:
            a.append(v % 2)
            v/=2
            cnt +=1
        for i in range(cnt,self.numVars):
            a.append(0)
        for i in range(len(self.boolNames)):
            setattr(sample,self.boolNames[i],a[i])
    def decode(self, vals):
        #accepts the list of [result.boolNames[i] for i in boolnames]
        key = 0
        for i in range(len(vals)):
            if vals[-i-1]: key += 2**i#was -1-i debug
        return self.item[key]
    def decodeSample(self,sample):
        for name in self.boolNames:
            if name not in dir(sample):
                setattr(sample,name,0)
        values = [getattr(sample,name) for name in self.boolNames]
        return self.decode(values)
    def visualise(self):
        return str(self.code)+'\n'+str(self.item)+'\n'+str(self.boolNames)

def feedSampleToSystem(sys, bitmap, targetWeight):
    enc = sys._injectedEncoder
    sample =sampleFromBitmap(bitmap)
    enc.encode(sample,targetWeight,sys)
    sys.addSample(sample)

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

